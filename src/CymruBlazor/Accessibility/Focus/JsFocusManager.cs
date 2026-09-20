using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// The <see cref="IFocusManager"/> registered by <c>AddCymruBlazor()</c> since 1.3.0.
/// It really moves, restores and contains focus, using the on-demand
/// <c>cymru-overlay.js</c> ES module (no script tag required).
/// </summary>
/// <remarks>
/// Every call is best effort: when the browser or Blazor circuit has already gone away, or
/// the element does not exist, the method returns a failed <see cref="FocusResult"/> instead of
/// throwing. JavaScript is only reachable after the first render, so call it from
/// <c>OnAfterRenderAsync</c> (as <c>CyFocusTrap</c> does), never during prerendering.
/// </remarks>
public sealed partial class JsFocusManager(
    IJSRuntime js,
    ILogger<JsFocusManager> logger)
    : IFocusManager, IAsyncDisposable
{
    private Task<IJSObjectReference>? _module;

    /// <inheritdoc />
    public async Task<FocusResult> FocusAsync(
        string elementId,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(elementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);

        var effective = options ?? new FocusOptions();

        try
        {
            var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
            var focused = await module.InvokeAsync<bool>(
                "focusElement", cancellationToken, elementId, effective.PreventScroll, effective.RestorePreviousFocus)
                .ConfigureAwait(false);

            return focused ? new FocusResult(true) : new FocusResult(false, $"Element '{elementId}' could not be focused.");
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            LogInteropFailed(ex, nameof(FocusAsync));
            return new FocusResult(false, ex.Message);
        }
    }

    /// <inheritdoc />
    /// <remarks>Targets are resolved within the innermost active focus trap, or the whole page if there is none.</remarks>
    public async Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var effective = options ?? new FocusOptions();

        try
        {
            var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
            var focused = await module.InvokeAsync<bool>(
                "focusTarget", cancellationToken, target.ToString(), effective.PreventScroll)
                .ConfigureAwait(false);

            return focused ? new FocusResult(true) : new FocusResult(false, $"No focusable element for target '{target}'.");
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            LogInteropFailed(ex, nameof(FocusAsync));
            return new FocusResult(false, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<FocusResult> RestoreFocusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
            var restored = await module.InvokeAsync<bool>("restoreFocus", cancellationToken, true).ConfigureAwait(false);

            return restored ? new FocusResult(true) : new FocusResult(false, "There was no previously focused element to restore.");
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            LogInteropFailed(ex, nameof(RestoreFocusAsync));
            return new FocusResult(false, ex.Message);
        }
    }

    /// <summary>
    /// Traps <c>Tab</c>/<c>Shift+Tab</c> inside the element, optionally moving focus in now and restoring it on release.
    /// If the element does not exist, or JavaScript is unavailable, the returned handle is a harmless no-op.
    /// </summary>
    public async Task<IAsyncDisposable> TrapAsync(
        string elementId,
        FocusTrapOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(elementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);

        var effective = options ?? new FocusTrapOptions();

        try
        {
            var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
            var token = await module.InvokeAsync<int>(
                "activateTrapById",
                cancellationToken,
                elementId,
                new { autoFocus = effective.AutoFocus, restoreFocus = effective.RestoreFocus, preventScroll = effective.PreventScroll, mediaQuery = effective.MediaQuery })
                .ConfigureAwait(false);

            return token == 0 ? NoopHandle.Instance : new TrapHandle(this, token);
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            LogInteropFailed(ex, nameof(TrapAsync));
            return NoopHandle.Instance;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            var module = await _module.ConfigureAwait(false);
            await module.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // The browser is already gone; nothing to release.
        }
    }

    private Task<IJSObjectReference> GetModuleAsync(CancellationToken cancellationToken)
        => _module ??= OverlayInterop.ImportAsync(js, cancellationToken).AsTask();

    private async ValueTask ReleaseAsync(int token)
    {
        try
        {
            var module = await GetModuleAsync(CancellationToken.None).ConfigureAwait(false);
            await module.InvokeVoidAsync("releaseTrap", token).ConfigureAwait(false);
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            LogInteropFailed(ex, nameof(ReleaseAsync));
        }
    }

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Focus interop call '{Operation}' failed.")]
    private partial void LogInteropFailed(Exception exception, string operation);

    private sealed class TrapHandle(JsFocusManager owner, int token) : IAsyncDisposable
    {
        private int _released;

        public ValueTask DisposeAsync()
            => Interlocked.Exchange(ref _released, 1) == 0 ? owner.ReleaseAsync(token) : ValueTask.CompletedTask;
    }

    private sealed class NoopHandle : IAsyncDisposable
    {
        internal static readonly NoopHandle Instance = new();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
