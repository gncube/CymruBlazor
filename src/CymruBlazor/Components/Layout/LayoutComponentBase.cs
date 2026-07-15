using Microsoft.AspNetCore.Components;

using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Base class for layout components.
/// </summary>
public abstract class LayoutComponentBase : CymruComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    // [Parameter]
    // public string Tag { get; set; } = "div";

    protected CssBuilder CreateLayoutCss() =>
        CssBuilder.Empty
            .AddClass(ComponentClass)
            .AddClass(Class);

    protected StyleBuilder CreateLayoutStyle() =>
        StyleBuilder.Empty
            .AddStyle(Style);

    protected override string CssStyle =>
        CreateLayoutStyle().Build();

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        // ArgumentException.ThrowIfNullOrWhiteSpace(Tag);
    }
}
