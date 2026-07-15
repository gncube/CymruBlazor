using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;

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
    protected override string ComponentClass => "cy-sidebar";

    protected override string CssClass =>
        CreateLayoutCss()
            // Position mapping
            .AddClass("cy-sidebar--left", Position == SidebarPosition.Left)
            .AddClass("cy-sidebar--right", Position == SidebarPosition.Right)

            // Width mapping
            .AddClass("cy-sidebar--sm", Width == SidebarWidth.Small)
            .AddClass("cy-sidebar--md", Width == SidebarWidth.Medium)
            .AddClass("cy-sidebar--lg", Width == SidebarWidth.Large)

            // State mapping
            .AddClass("cy-sidebar--collapsed", Collapsed)
            .Build();

    /// <summary>
    /// Programmatically toggles the collapsed state of the sidebar.
    /// </summary>
    public async Task ToggleAsync()
    {
        Collapsed = !Collapsed;
        await CollapsedChanged.InvokeAsync(Collapsed);
        await InvokeAsync(StateHasChanged);
    }
}

/// <summary>
/// Minimal fluent helper supporting the class generation pattern.
/// </summary>
public class ClassBuilder(string baseClass)
{
    private readonly List<string> _classes = [baseClass];

    public ClassBuilder AddClass(string className, bool condition)
    {
        if (condition)
        {
            _classes.Add(className);
        }
        return this;
    }

    public string Build() => string.Join(" ", _classes);
}
