namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Result of keyboard navigation processing.
/// </summary>
public sealed record KeyboardNavigationResult(
    FocusNavigationMode NavigationMode,
    bool PreventDefault = true);
