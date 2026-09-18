namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the display state of the sidebar layout component.
/// </summary>
public enum SidebarState
{
    /// <summary>
    /// Fully expanded sidebar displaying complete labels and child content.
    /// </summary>
    Expanded = 0,

    /// <summary>
    /// Compact sidebar display showing an icon and abbreviated label rail.
    /// </summary>
    Compact = 1,

    /// <summary>
    /// Narrow sidebar display showing navigation icons only.
    /// </summary>
    IconOnly = 2,

    /// <summary>
    /// Hidden sidebar collapsed completely out of layout flow.
    /// </summary>
    Hidden = 3
}
