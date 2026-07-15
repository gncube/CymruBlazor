using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a collapsible, responsive sidebar layout component.
/// </summary>
public partial class CySidebar : LayoutComponentBase
{
    [Parameter]
    public SidebarPosition Position { get; set; } = SidebarPosition.Left;

    [Parameter]
    public SidebarWidth Width { get; set; } = SidebarWidth.Medium;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    protected override string BaseCssClass => "cy-sidebar";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-sidebar--left", Position == SidebarPosition.Left)
            .AddClass("cy-sidebar--right", Position == SidebarPosition.Right)
            .AddClass("cy-sidebar--sm", Width == SidebarWidth.Small)
            .AddClass("cy-sidebar--md", Width == SidebarWidth.Medium)
            .AddClass("cy-sidebar--lg", Width == SidebarWidth.Large)
            .AddClass("cy-sidebar--collapsed", Collapsed)
            .Build();

    /// <summary>
    /// Programmatically toggles the collapsed state of the sidebar.
    /// </summary>
    public async Task ToggleAsync()
    {
        Collapsed = !Collapsed;
        await CollapsedChanged.InvokeAsync(Collapsed);
        // Use direct synchronous state dispatching if working inside standard rendering scope
        StateHasChanged();
    }
}
