namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Represents a request to move focus.
/// </summary>
public sealed record FocusRequest(
    string ElementId,
    FocusOptions? Options = null);
