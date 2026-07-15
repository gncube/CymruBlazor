using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

public partial class CyCluster : LayoutComponentBase
{
    [Parameter]
    public ComponentSize Gap { get; set; } = ComponentSize.Medium;

    [Parameter]
    public AlignItems AlignItems { get; set; } = AlignItems.Center;

    [Parameter]
    public JustifyContent JustifyContent { get; set; }

    [Parameter]
    public bool Wrap { get; set; } = true;

    protected override string BaseCssClass => "cy-cluster";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-cluster--wrap", Wrap)
            .AddClass($"cy-gap-{Gap.ToString().ToLowerInvariant()}")
            .AddClass($"cy-align-{AlignItems.ToString().ToLowerInvariant()}")
            .AddClass($"cy-justify-{JustifyContent.ToString().ToLowerInvariant()}")
            .Build();
}
