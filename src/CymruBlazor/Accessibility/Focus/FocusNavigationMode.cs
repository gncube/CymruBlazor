namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Describes the type of keyboard navigation requested.
/// </summary>
[Obsolete("Nothing in CymruBlazor consumes this; it will be removed in 2.0.0.", error: false)]
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
