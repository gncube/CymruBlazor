using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A content container following the NHS Wales card pattern - optional
/// header/footer regions, and an optional whole-card link (<see cref="Href"/>)
/// for the common "the entire card is a single link target" convention.
/// </summary>
public partial class CyCard : CyLayoutComponentBase
{
    /// <summary>
    /// Optional header content, rendered above the body.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Optional footer content, rendered below the body.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// When set, the whole card renders as a single <c>&lt;a&gt;</c>
    /// element rather than a <c>&lt;div&gt;</c>, so the entire card - not
    /// just some text inside it - is the link target. Do not additionally
    /// nest another link inside <see cref="CyLayoutComponentBase.ChildContent"/>
    /// when this is set; that produces invalid nested interactive content.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Controls the card's shadow/elevation. Defaults to
    /// <see cref="ComponentElevation.Small"/> to distinguish it from the
    /// surrounding page background.
    /// </summary>
    [Parameter]
    public ComponentElevation Elevation { get; set; } = ComponentElevation.Small;

    protected override string BaseCssClass => "cy-card";

    protected override string BuildCssClass()
    {
        var elevationSuffix = Elevation.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-card--elevation-{elevationSuffix}")
            .AddClass("cy-card--interactive", Href is not null)
            .Build();
    }
}
