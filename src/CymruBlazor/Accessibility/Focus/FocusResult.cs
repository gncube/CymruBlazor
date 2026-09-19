namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Result of a focus operation.
/// </summary>
/// <param name="Success">Whether the request was accepted. The default <see cref="FocusManager"/>
/// always returns <see langword="true"/> without moving focus, so this does not prove focus moved.</param>
/// <param name="Error">A description of the failure when <paramref name="Success"/> is false.</param>
public sealed record FocusResult(
    bool Success,
    string? Error = null);
