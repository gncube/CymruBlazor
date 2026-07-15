using Microsoft.AspNetCore.Components;

using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a responsive content container.
/// </summary>
public partial class CyContainer : LayoutComponentBase
{
    [Parameter]
    public ContainerSize Size { get; set; } = ContainerSize.Large;

    [Parameter]
    public bool RemovePadding { get; set; }

    protected override string ComponentClass => "cy-container";

    protected override string CssClass =>
        CreateLayoutCss()

            .AddClass("cy-container--sm",
                Size == ContainerSize.Small)

            .AddClass("cy-container--md",
                Size == ContainerSize.Medium)

            .AddClass("cy-container--lg",
                Size == ContainerSize.Large)

            .AddClass("cy-container--xl",
                Size == ContainerSize.ExtraLarge)

            .AddClass("cy-container--fluid",
                Size == ContainerSize.Fluid)

            .AddClass("cy-container--no-padding",
                RemovePadding)

            .Build();
}
