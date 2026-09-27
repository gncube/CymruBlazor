namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Result of keyboard navigation processing.
/// </summary>
[Obsolete("Nothing in CymruBlazor consumes this; it will be removed in 2.0.0.", error: false)]
public sealed record KeyboardNavigationResult(
    FocusNavigationMode NavigationMode,
    bool PreventDefault = true);
