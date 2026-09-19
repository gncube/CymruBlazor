using Microsoft.Extensions.Logging;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Default implementation of <see cref="IFocusManager"/>.
/// </summary>
/// <remarks>
/// <b>This is a logging placeholder.</b> Every method writes a Debug-level log
/// entry and returns <c>new FocusResult(true)</c>; none of them moves,
/// restores or contains focus in the browser, and no JavaScript is involved.
/// Register your own <see cref="IFocusManager"/> after <c>AddCymruBlazor()</c>
/// if you need real focus management. A functional implementation is planned
/// for 1.3.0.
/// </remarks>
public sealed partial class FocusManager(
    ILogger<FocusManager> logger)
    : IFocusManager
{
    /// <inheritdoc />
    /// <remarks>Logs the request and reports success. Does not focus anything.</remarks>
    public Task<FocusResult> FocusAsync(
        string elementId,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(elementId);

        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);

        LogFocusRequestedForElement(elementId);

        return Task.FromResult(new FocusResult(true));
    }

    /// <inheritdoc />
    /// <remarks>Logs the request and reports success. Does not focus anything.</remarks>
    public Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogFocusRequestedForTarget(target);

        return Task.FromResult(new FocusResult(true));
    }

    /// <inheritdoc />
    /// <remarks>Logs the request and reports success. Does not restore focus.</remarks>
    public Task<FocusResult> RestoreFocusAsync(
        CancellationToken cancellationToken = default)
    {
        LogRestorePreviousFocusRequested();

        return Task.FromResult(new FocusResult(true));
    }

    // Source-generated logging (CA1848/CA1873): the compiler emits
    // IsEnabled-guarded, allocation-free logging methods, avoiding the
    // params object?[] boxing and eager argument evaluation of the
    // LoggerExtensions.LogDebug(...) extension method calls this replaces.
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Focus requested for element '{ElementId}'.")]
    private partial void LogFocusRequestedForElement(string elementId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Focus requested for target '{Target}'.")]
    private partial void LogFocusRequestedForTarget(FocusTarget target);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Restore previous focus requested.")]
    private partial void LogRestorePreviousFocusRequested();
}
