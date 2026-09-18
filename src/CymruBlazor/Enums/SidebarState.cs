namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the display state of the <see cref="Layout.CySidebar"/> component.
/// </summary>
public enum SidebarState
{
    /// <summary>
    /// Fully expanded sidebar showing full labels and navigation tree.
    /// </summary>
    Expanded = 0,

    /// <summary>
    /// Compact sidebar display showing abbreviated content or icons.
    /// </summary>
    Compact = 1,

    /// <summary>
    /// Icon-only sidebar display showing exclusively primary navigation glyphs.
    /// </summary>
    IconOnly = 2,

    /// <summary>
    /// Fully collapsed and hidden from the normal layout flow.
    /// </summary>
    Hidden = 3
}
