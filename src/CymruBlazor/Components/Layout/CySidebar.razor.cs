using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a collapsible, responsive sidebar layout component.
/// </summary>
public partial class CySidebar : CyLayoutComponentBase
{
    [Parameter]
    public SidebarPosition Position { get; set; } = SidebarPosition.Left;

    [Parameter]
    public SidebarWidth Width { get; set; } = SidebarWidth.Medium;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>
    /// Configures which appearance the sidebar uses while
    /// <see cref="Collapsed"/> is <see langword="true"/> - see
    /// <see cref="Enums.SidebarCollapseMode"/> for the available states.
    /// Defaults to <see cref="Enums.SidebarCollapseMode.Compact"/> (an
    /// icon + short label rail).
    /// </summary>
    [Parameter]
    public SidebarCollapseMode CollapseMode { get; set; } = SidebarCollapseMode.Compact;

    /// <summary>
    /// Optional brand lockup (e.g. <c>&lt;CyBrandLogo /&gt;</c>) rendered
    /// at the top of the sidebar, above <c>ChildContent</c>/the
    /// rest of the sidebar's markup. Only rendered while the sidebar is
    /// fully expanded (<see cref="EffectiveCollapsed"/> is
    /// <see langword="false"/>) - once collapsed, in any
    /// <see cref="SidebarCollapseMode"/>, there is no longer room for it
    /// and it is omitted entirely (not just visually hidden), matching
    /// the reference NHS Wales sidebar design.
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    protected override string BaseCssClass => "cy-sidebar";

    /// <summary>
    /// The effective collapsed state actually applied to markup/CSS.
    /// <see cref="SidebarCollapseMode.Disabled"/> always renders
    /// expanded, regardless of what <see cref="Collapsed"/> is bound to.
    /// </summary>
    private bool EffectiveCollapsed =>
        CollapseMode != SidebarCollapseMode.Disabled && Collapsed;

    private string CollapseModeAttribute => CollapseMode switch
    {
        SidebarCollapseMode.Compact => "compact",
        SidebarCollapseMode.IconOnly => "icon-only",
        SidebarCollapseMode.Disabled => "disabled",
        _ => "hidden"
    };

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-sidebar--left", Position == SidebarPosition.Left)
            .AddClass("cy-sidebar--right", Position == SidebarPosition.Right)
            .AddClass("cy-sidebar--sm", Width == SidebarWidth.Small)
            .AddClass("cy-sidebar--md", Width == SidebarWidth.Medium)
            .AddClass("cy-sidebar--lg", Width == SidebarWidth.Large)
            .AddClass("cy-sidebar--compact", EffectiveCollapsed && CollapseMode == SidebarCollapseMode.Compact)
            .AddClass("cy-sidebar--icon-only", EffectiveCollapsed && CollapseMode == SidebarCollapseMode.IconOnly)
            .AddClass("cy-sidebar--collapsed", EffectiveCollapsed && CollapseMode == SidebarCollapseMode.Hidden)
            .Build();

    /// <summary>
    /// Programmatically toggles the collapsed state of the sidebar. A
    /// no-op when <see cref="CollapseMode"/> is
    /// <see cref="SidebarCollapseMode.Disabled"/>.
    /// </summary>
    public async Task ToggleAsync()
    {
        if (CollapseMode == SidebarCollapseMode.Disabled)
        {
            return;
        }

        Collapsed = !Collapsed;
        await CollapsedChanged.InvokeAsync(Collapsed);
        // Use direct synchronous state dispatching if working inside standard rendering scope
        StateHasChanged();
    }
}
