using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Centers its child content horizontally and/or vertically.
/// </summary>
public partial class CyCenter : LayoutComponentBase
{
    /// <summary>
    /// Centers content horizontally.
    /// </summary>
    [Parameter]
    public bool Horizontal { get; set; } = true;

    /// <summary>
    /// Centers content vertically.
    /// </summary>
    [Parameter]
    public bool Vertical { get; set; } = true;

    /// <summary>
    /// Expands to fill the available height.
    /// </summary>
    [Parameter]
    public bool FullHeight { get; set; }

    protected override string ComponentClass => "cy-center";

    protected override string CssClass =>
        CreateLayoutCss()
            .AddClass("cy-center--horizontal", Horizontal)
            .AddClass("cy-center--vertical", Vertical)
            .AddClass("cy-center--full-height", FullHeight)
            .Build();
}
