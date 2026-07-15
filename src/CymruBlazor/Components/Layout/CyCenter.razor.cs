using CymruBlazor.Components.Core;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Layout;

public partial class CyCenter : LayoutComponentBase
{
    [Parameter]
    public bool Horizontal { get; set; } = true;

    [Parameter]
    public bool Vertical { get; set; } = true;

    [Parameter]
    public bool FullHeight { get; set; }

    protected override string BaseCssClass => "cy-center";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-center--horizontal", Horizontal)
            .AddClass("cy-center--vertical", Vertical)
            .AddClass("cy-center--full-height", FullHeight)
            .Build();
}
