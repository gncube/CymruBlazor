namespace CymruBlazor.Enums;

/// <summary>
/// Configures which collapsed appearance a <c>CySidebar</c> uses when
/// <c>Collapsed</c> is <see langword="true"/>. <c>Collapsed</c> itself
/// remains the single on/off switch (bound to a toggle button the
/// consumer renders, typically inside <c>CySidebar.Brand</c> or
/// <c>ChildContent</c>) - <c>CollapseMode</c> only decides what the
/// "collapsed" side of that switch looks like. When
/// <c>Collapsed="false"</c> the sidebar always renders fully expanded
/// (its <c>CySidebar.Brand</c> lockup and item labels/section headings
/// visible), regardless of <c>CollapseMode</c>.
/// </summary>
public enum SidebarCollapseMode
{
    /// <summary>
    /// Collapses to a narrow rail that keeps a short text label
    /// stacked beneath each item's icon. The <c>CySidebar.Brand</c>
    /// lockup is not rendered while collapsed - there isn't room for it
    /// once section headings and item labels have shrunk to this width.
    /// Consumers mark up each item's label with class
    /// <c>cy-sidebar__label</c> and each item's row with
    /// <c>cy-sidebar__item</c> to get the icon-above-label layout for
    /// free; wrap group headings in <c>cy-sidebar__heading</c> so they
    /// hide automatically at this width.
    /// </summary>
    Compact = 0,

    /// <summary>
    /// Collapses to the narrowest possible rail: icons only, with item
    /// labels and the <c>CySidebar.Brand</c> lockup both hidden. Labels
    /// marked with <c>cy-sidebar__label</c> are hidden visually but
    /// remain in the accessible tree (not <c>aria-hidden</c>) so
    /// screen reader users still get the item's name.
    /// </summary>
    IconOnly = 1,

    /// <summary>
    /// The sidebar can never collapse - <c>Collapsed</c> is ignored (the
    /// sidebar always renders at full width, <c>Brand</c> lockup and all)
    /// and <c>ToggleAsync()</c> is a no-op. Use this for a permanent,
    /// structural primary-navigation rail that should never be hidden.
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// The sidebar collapses to zero width and hides its content
    /// entirely (<c>overflow: hidden</c>) - the most extreme "completely
    /// hidden" state, distinct from <see cref="Compact"/>/<see cref="IconOnly"/>
    /// which keep a visible rail. This was CySidebar's original (and
    /// only) collapse behaviour before <c>CollapseMode</c> was
    /// introduced.
    /// </summary>
    Hidden = 3
}
