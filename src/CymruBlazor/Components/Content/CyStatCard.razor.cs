using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A key figure with a label, an optional unit, a trend and a short description - for dashboards and summaries.
/// </summary>
/// <remarks>
/// <para>
/// Label and value are always plain text (encoded). The trend is never conveyed by an arrow or colour alone:
/// <see cref="TrendText"/> is always rendered and a visually hidden "Increase:" / "Decrease:" / "No change:" prefix
/// is added for assistive technology. <see cref="TrendTone"/> only changes colour, so a rise can be shown as
/// good news (waiting times down) or bad (incidents up) without changing the words.
/// </para>
/// <para>
/// Set <see cref="Href"/> to make the whole card one link. Set <see cref="HeadingLevel"/> to expose the label
/// as a heading; leave it null to render a paragraph so the card does not disturb the page outline.
/// </para>
/// </remarks>
public partial class CyStatCard : CyLayoutComponentBase
{
    /// <summary>What the figure measures. Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>The figure, as text, so formatting (culture, rounding) stays with the caller.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Optional unit shown after the value, for example <c>days</c> or <c>%</c>.</summary>
    [Parameter]
    public string? Unit { get; set; }

    /// <summary>Optional short explanation shown under the figure.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Optional name of a built-in icon shown beside the label. Decorative.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Direction of change. Defaults to <see cref="CyTrend.None"/>.</summary>
    [Parameter]
    public CyTrend Trend { get; set; }

    /// <summary>
    /// Words describing the change, for example "4% fewer than last month". Required to show a trend: an arrow
    /// without words is not rendered.
    /// </summary>
    [Parameter]
    public string? TrendText { get; set; }

    /// <summary>
    /// Whether the change is good (<see cref="ComponentColour.Success"/>), bad (<see cref="ComponentColour.Danger"/>)
    /// or neutral (anything else, the default). Affects colour only.
    /// </summary>
    [Parameter]
    public ComponentColour TrendTone { get; set; } = ComponentColour.Neutral;

    /// <summary>Optional accent for the card's top edge. <see cref="ComponentColour.Unspecified"/> (the default) shows none.</summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Unspecified;

    /// <summary>When set, the whole card is a link to this URL.</summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>When true, the value is replaced by a skeleton and the card is marked <c>aria-busy</c>.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>The heading level (1 to 6) of the label, or null (the default) for a paragraph.</summary>
    [Parameter]
    public int? HeadingLevel { get; set; }

    /// <summary>Phrases added for assistive technology. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyStatCardText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-stat-card";

    private CyStatCardText Strings => Text ?? s_defaultText;

    private static readonly CyStatCardText s_defaultText = new();

    private bool HasTrendText => Trend != CyTrend.None && !string.IsNullOrWhiteSpace(TrendText);

    private string TrendModifier => TrendTone switch
    {
        ComponentColour.Success => "good",
        ComponentColour.Danger => "bad",
        _ => "neutral"
    };

    private string? TrendIconName => Trend switch
    {
        CyTrend.Up => "trend-up",
        CyTrend.Down => "trend-down",
        _ => null
    };

    private string? TrendPrefix => Trend switch
    {
        CyTrend.Up => Strings.Increase,
        CyTrend.Down => Strings.Decrease,
        CyTrend.Flat => Strings.NoChange,
        _ => null
    };

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-stat-card--link", Href is not null)
            .AddClass(AccentClass)
            .AddClass(Class)
            .Build();

    private string? AccentClass => Colour switch
    {
        ComponentColour.Unspecified => null,
        _ => $"cy-stat-card--{Colour.ToString().ToLowerInvariant()}"
    };

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyStatCard)}.{nameof(Label)} must not be empty: it says what the figure measures.");
        }

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyStatCard)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }
    }
}
