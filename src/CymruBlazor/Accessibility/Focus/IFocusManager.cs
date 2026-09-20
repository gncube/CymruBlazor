namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Provides a high-level abstraction for focus management.
/// </summary>
/// <remarks>
/// This is the extension point used by <c>CyFocusTrap</c> and the
/// <c>CySidebar</c> mobile drawer. Since 1.3.0 <c>AddCymruBlazor()</c>
/// registers <see cref="JsFocusManager"/>, which really moves, restores and
/// contains focus using a small on-demand JavaScript module (no script tag
/// needed). To replace it, register your own implementation after
/// <c>AddCymruBlazor()</c> (the last registration wins):
/// <code>
/// builder.Services.AddCymruBlazor();
/// builder.Services.AddScoped&lt;IFocusManager, MyFocusManager&gt;();
/// </code>
/// The older logging-only <see cref="FocusManager"/> remains available as a
/// no-op implementation, e.g. for tests.
/// </remarks>
public interface IFocusManager
{
    /// <summary>Requests focus for the element with the given id.</summary>
    /// <param name="elementId">The DOM <c>id</c> of the element to focus.</param>
    /// <param name="options">Optional focus behaviour.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> FocusAsync(
        string elementId,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Requests focus for a relative destination such as the first or last focusable element.</summary>
    /// <param name="target">The destination to focus.</param>
    /// <param name="options">Optional focus behaviour.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Requests that focus return to the element that had it before the last focus request.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> RestoreFocusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a focus trap on the element with the given id and returns a handle that
    /// releases it when disposed.
    /// </summary>
    /// <remarks>
    /// This is a default interface member so existing custom implementations keep compiling.
    /// The default does not contain <c>Tab</c>: it calls <see cref="FocusAsync(string, FocusOptions?, CancellationToken)"/>
    /// when <see cref="FocusTrapOptions.AutoFocus"/> is set and <see cref="RestoreFocusAsync"/> on release
    /// when <see cref="FocusTrapOptions.RestoreFocus"/> is set - exactly what <c>CyFocusTrap</c> did before 1.3.0.
    /// <see cref="JsFocusManager"/> overrides it to add Tab containment.
    /// </remarks>
    /// <param name="elementId">The DOM <c>id</c> of the container to trap focus in.</param>
    /// <param name="options">Optional trap behaviour.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A handle; disposing it releases the trap.</returns>
    async Task<IAsyncDisposable> TrapAsync(
        string elementId,
        FocusTrapOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var effective = options ?? new FocusTrapOptions();

        if (effective.AutoFocus)
        {
            await FocusAsync(
                elementId,
                new FocusOptions(effective.PreventScroll, effective.RestoreFocus),
                cancellationToken).ConfigureAwait(false);
        }

        return new FocusRestorer(this, effective.RestoreFocus);
    }
}
