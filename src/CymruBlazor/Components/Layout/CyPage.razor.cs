using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// The width policy for a page: centres the content and caps its width at one of four named sizes
/// (<see cref="CyPageWidth"/>), using the page-width tokens and the standard gutters.
/// It renders a plain <c>&lt;div&gt;</c>. The <c>&lt;main&gt;</c> landmark belongs to the layout (once per
/// page), not to this component, so several of these can be used without duplicating landmarks.
/// </summary>
public partial class CyPage : CyLayoutComponentBase
{
    /// <summary>Maximum content width. Defaults to <see cref="CyPageWidth.Default"/>.</summary>
    [Parameter]
    public CyPageWidth Width { get; set; } = CyPageWidth.Default;

    /// <summary>
    /// Removes the side gutters, for a page placed inside something that already pads it
    /// (a <see cref="CyContainer"/>, say).
    /// </summary>
    [Parameter]
    public bool RemovePadding { get; set; }

    protected override string BaseCssClass => "cy-page";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-page--{Width.ToString().ToLowerInvariant()}")
            .AddClass("cy-page--no-padding", RemovePadding)
            .Build();
}
