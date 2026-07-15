using Microsoft.AspNetCore.Components;

using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a flexbox stack layout.
/// </summary>
public partial class CyStack : LayoutComponentBase
{
    [Parameter]
    public Orientation Orientation { get; set; }

    [Parameter]
    public ComponentSize Gap { get; set; } = ComponentSize.Medium;

    [Parameter]
    public AlignItems AlignItems { get; set; }

    [Parameter]
    public JustifyContent JustifyContent { get; set; }

    [Parameter]
    public bool Wrap { get; set; }

    protected override string ComponentClass => "cy-stack";

    protected override string CssClass =>
        CreateLayoutCss()

            .AddClass("cy-stack--horizontal",
                Orientation == Orientation.Horizontal)

            .AddClass("cy-stack--vertical",
                Orientation == Orientation.Vertical)

            .AddClass("cy-stack--wrap",
                Wrap)

            .AddClass($"cy-gap-{Gap.ToString().ToLowerInvariant()}")

            .AddClass($"cy-align-{AlignItems.ToString().ToLowerInvariant()}")

            .AddClass($"cy-justify-{JustifyContent.ToString().ToLowerInvariant()}")

            .Build();
}
