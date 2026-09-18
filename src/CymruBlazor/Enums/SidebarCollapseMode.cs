namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the display appearance and behaviour of the sidebar when collapsed.
/// </summary>
public enum SidebarCollapseMode
{
    /// <summary>
    /// Compact rail displaying an icon and abbreviated label.
    /// </summary>
    Compact = 0,

    /// <summary>
    /// Narrow rail displaying icons only.
    /// </summary>
    IconOnly = 1,

    /// <summary>
    /// Fully hidden sidebar collapsed out of the layout flow.
    /// </summary>
    Hidden = 2,

    /// <summary>
    /// Sidebar cannot be collapsed; toggle button is omitted.
    /// </summary>
    NonCollapsible = 3,

    /// <summary>
    /// Obsolete alias for <see cref="NonCollapsible"/>.
    /// </summary>
    [Obsolete("Use NonCollapsible instead.", error: false)]
    Disabled = NonCollapsible
}
