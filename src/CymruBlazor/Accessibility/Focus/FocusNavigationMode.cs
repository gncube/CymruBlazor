namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Describes the type of keyboard navigation requested.
/// </summary>
public enum FocusNavigationMode
{
    None = 0,

    Next,
    Previous,

    First,
    Last,

    Left,
    Right,
    Up,
    Down,

    Activate,
    Cancel
}
