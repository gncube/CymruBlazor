using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// A high-performance utility component that visually hides content
/// while keeping it accessible to screen readers using 'sr-only' styling.
/// </summary>
public partial class CyScreenReaderOnly : CymruComponentBase
{
    private ElementReference _elementRef;

    /// <summary>
    /// Content to be visually hidden but read by screen readers.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string BaseCssClass => "sr-only";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();
}
