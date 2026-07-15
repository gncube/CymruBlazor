namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Result of a focus operation.
/// </summary>
public sealed record FocusResult(
    bool Success,
    string? Error = null);
