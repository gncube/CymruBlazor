using Microsoft.Extensions.Logging;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Default implementation of IFocusManager.
/// </summary>
public sealed class FocusManager(
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

        logger.LogDebug(
            "Focus requested for element '{ElementId}'.",
            elementId);

        return Task.FromResult(new FocusResult(true));
    }

    public Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            "Focus requested for target '{Target}'.",
            target);

        return Task.FromResult(new FocusResult(true));
    }

    public Task<FocusResult> RestoreFocusAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Restore previous focus requested.");

        return Task.FromResult(new FocusResult(true));
    }
}
