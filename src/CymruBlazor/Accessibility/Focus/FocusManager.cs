using Microsoft.Extensions.Logging;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Default implementation of IFocusManager.
/// </summary>
public sealed partial class FocusManager(
    ILogger<FocusManager> logger)
    : IFocusManager
{
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

    public Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogFocusRequestedForTarget(target);

        return Task.FromResult(new FocusResult(true));
    }

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
