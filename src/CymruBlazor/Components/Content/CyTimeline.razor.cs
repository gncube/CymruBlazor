using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Displays chronological events, clinical histories, audit trails, and process milestones in an accessible timeline.
/// </summary>
/// <remarks>
/// <para>
/// Rendered as an ordered list (<c>&lt;ol&gt;</c>) containing <see cref="CyTimelineItem"/> milestones.
/// Supports both declarative child items and data-driven binding via <see cref="Items"/>.
/// </para>
/// <para>
/// Stem connectors between milestones are decorative and hidden from assistive technology, while milestones
/// preserve list numbering, time elements, and accessible heading structures.
/// </para>
/// </remarks>
public partial class CyTimeline : CyLayoutComponentBase
{
    private static readonly CyTimelineText s_defaultText = new();

    /// <summary>Accessible label describing the timeline for assistive technology.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Visual density of the timeline items. Defaults to <see cref="ComponentSize.Medium"/>.</summary>
    [Parameter]
    public ComponentSize Density { get; set; } = ComponentSize.Medium;

    /// <summary>Connecting stem line style. Defaults to <see cref="CyTimelineLineStyle.Solid"/>.</summary>
    [Parameter]
    public CyTimelineLineStyle LineStyle { get; set; } = CyTimelineLineStyle.Solid;

    /// <summary>Data-driven collection of timeline items.</summary>
    [Parameter]
    public IReadOnlyList<CyTimelineItemModel>? Items { get; set; }

    /// <summary>Custom template for rendering items when <see cref="Items"/> is used.</summary>
    [Parameter]
    public RenderFragment<CyTimelineItemModel>? ItemTemplate { get; set; }

    /// <summary>Localised phrases. Unset properties fall back to English defaults.</summary>
    [Parameter]
    public CyTimelineText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-timeline";

    private CyTimelineText Strings => Text ?? s_defaultText;

    private string? AriaLabelValue =>
        !string.IsNullOrWhiteSpace(Label) ? Label :
        (HasAriaAttribute() ? null : Strings.DefaultLabel);

    private bool HasAriaAttribute() =>
        AdditionalAttributes is not null &&
        (AdditionalAttributes.ContainsKey("aria-label") || AdditionalAttributes.ContainsKey("aria-labelledby"));

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-timeline--compact", Density == ComponentSize.Small)
            .AddClass("cy-timeline--dashed", LineStyle == CyTimelineLineStyle.Dashed)
            .AddClass("cy-timeline--dotted", LineStyle == CyTimelineLineStyle.Dotted)
            .AddClass(Class)
            .Build();
}
