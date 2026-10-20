using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A single milestone or activity record within a <see cref="CyTimeline"/>.
/// </summary>
public partial class CyTimelineItem : CyLayoutComponentBase
{
    /// <summary>The title or headline for the event.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Custom template for rendering the milestone headline.</summary>
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    /// <summary>
    /// Heading level for the title (1 to 6). Defaults to 3 (<c>&lt;h3&gt;</c>).
    /// When null or outside 1..6, renders a <c>&lt;p class="cy-timeline__title"&gt;</c>.
    /// </summary>
    [Parameter]
    public int? HeadingLevel { get; set; } = 3;

    /// <summary>Human-readable time or date string (e.g. "14:30, 20 Oct 2026").</summary>
    [Parameter]
    public string? Time { get; set; }

    /// <summary>Machine-readable timestamp for the &lt;time&gt; element's datetime attribute.</summary>
    [Parameter]
    public DateTimeOffset? DateTime { get; set; }

    /// <summary>Short text summary or description of the event.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Actor or attribution line (e.g. "Dr. Jane Evans" or "Pathology Department").</summary>
    [Parameter]
    public string? Actor { get; set; }

    /// <summary>Semantic colour for the marker node. Defaults to <see cref="ComponentColour.Neutral"/>.</summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Neutral;

    /// <summary>Name of an icon from the registry (e.g. "check", "clock", "alert-circle").</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Custom template for the marker node in place of the default icon or dot.</summary>
    [Parameter]
    public RenderFragment? MarkerTemplate { get; set; }

    /// <summary>Optional status badge text shown beside the title.</summary>
    [Parameter]
    public string? BadgeText { get; set; }

    /// <summary>Semantic colour for the status badge. Defaults to <see cref="ComponentColour.Neutral"/>.</summary>
    [Parameter]
    public ComponentColour BadgeColour { get; set; } = ComponentColour.Neutral;

    /// <summary>Action buttons or links placed at the bottom of the item.</summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Explicitly flags this item as the final entry, hiding its connecting stem line.
    /// When null, the last child in CSS is automatically handled.
    /// </summary>
    [Parameter]
    public bool? IsLast { get; set; }

    /// <summary>Cascaded parent timeline context.</summary>
    [CascadingParameter]
    public CyTimeline? Timeline { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-timeline__item";

    private bool ShowStem => IsLast != true;

    private int IconSize => Timeline?.Density == ComponentSize.Small ? 14 : 16;

    private string MarkerClass =>
        Colour != ComponentColour.Unspecified
            ? $"cy-timeline__marker--{Colour.ToString().ToLowerInvariant()}"
            : "cy-timeline__marker--neutral";

    private string? DateTimeString => DateTime?.ToString("o", CultureInfo.InvariantCulture);

    private ComponentColour BadgeVariant =>
        BadgeColour != ComponentColour.Neutral && BadgeColour != ComponentColour.Unspecified
            ? BadgeColour
            : (Colour != ComponentColour.Unspecified ? Colour : ComponentColour.Neutral);

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass($"cy-timeline__item--{Colour.ToString().ToLowerInvariant()}", Colour != ComponentColour.Neutral && Colour != ComponentColour.Unspecified)
            .AddClass("cy-timeline__item--last", IsLast == true)
            .AddClass(Class)
            .Build();

    private RenderFragment RenderHeading() => builder =>
    {
        var tag = HeadingLevel is >= 1 and <= 6 ? $"h{HeadingLevel}" : "p";
        builder.OpenElement(0, tag);
        builder.AddAttribute(1, "class", "cy-timeline__title");
        if (TitleTemplate is not null)
        {
            builder.AddContent(2, TitleTemplate);
        }
        else
        {
            builder.AddContent(2, Title);
        }
        builder.CloseElement();
    };
}
