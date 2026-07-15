namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Provides a high-level abstraction for focus management.
/// </summary>
public interface IFocusManager
{
    Task<FocusResult> FocusAsync(
        string elementId,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<FocusResult> RestoreFocusAsync(
        CancellationToken cancellationToken = default);
}
