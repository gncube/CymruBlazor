using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

public partial class CyContainer : LayoutComponentBase
{
    [Parameter]
    public ContainerSize Size { get; set; } = ContainerSize.Large;

    [Parameter]
    public bool RemovePadding { get; set; }

    protected override string BaseCssClass => "cy-container";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-container--sm", Size == ContainerSize.Small)
            .AddClass("cy-container--md", Size == ContainerSize.Medium)
            .AddClass("cy-container--lg", Size == ContainerSize.Large)
            .AddClass("cy-container--xl", Size == ContainerSize.ExtraLarge)
            .AddClass("cy-container--fluid", Size == ContainerSize.Fluid)
            .AddClass("cy-container--no-padding", RemovePadding)
            .Build();
}
