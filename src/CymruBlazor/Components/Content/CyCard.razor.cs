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

    /// <summary>
    /// Optional card title, rendered as a heading at <see cref="HeadingLevel"/> inside the header region
    /// (above any <see cref="Header"/> content). Required when <see cref="Collapsible"/> is set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Heading level (1 to 6) used for <see cref="Title"/>. Defaults to 3; choose the level that keeps the
    /// page outline sequential. The visual size does not change with the level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 3;

    /// <summary>Inner spacing. <see cref="CyCardPadding.Default"/> keeps the earlier look.</summary>
    [Parameter]
    public CyCardPadding Padding { get; set; } = CyCardPadding.Default;

    /// <summary>Visual treatment. <see cref="CyCardVariant.Raised"/> keeps the earlier look.</summary>
    [Parameter]
    public CyCardVariant Variant { get; set; } = CyCardVariant.Raised;

    /// <summary>
    /// Makes the title a disclosure button that shows and hides the body (no script: a button with
    /// <c>aria-expanded</c> and <c>aria-controls</c> over a <c>hidden</c> body). Requires <see cref="Title"/>
    /// and cannot be combined with <see cref="Href"/> (a button inside a link is invalid).
    /// </summary>
    [Parameter]
    public bool Collapsible { get; set; }

    /// <summary>Whether a <see cref="Collapsible"/> card is open. Two-way bindable; defaults to open.</summary>
    [Parameter]
    public bool Expanded { get; set; } = true;

    /// <summary>Raised when the user opens or closes a <see cref="Collapsible"/> card.</summary>
    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    private bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private bool HasHeaderRegion => Header is not null || HasTitle;

    private string HeadingTag => $"h{HeadingLevel}";

    private string? BodyId => Collapsible ? $"{Id}-body" : null;

    private async Task ToggleAsync()
    {
        Expanded = !Expanded;
        await ExpandedChanged.InvokeAsync(Expanded);
    }

    protected override void ValidateParameters()
    {
        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyCard)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }

        if (Collapsible && !HasTitle)
        {
            throw new InvalidOperationException(
                $"{nameof(CyCard)}.{nameof(Collapsible)} needs a {nameof(Title)}: the title is the button that opens and closes the card.");
        }

        if (Collapsible && Href is not null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyCard)}.{nameof(Collapsible)} cannot be combined with {nameof(Href)}: a button inside a link is invalid interactive nesting.");
        }
    }

    protected override string BaseCssClass => "cy-card";

    protected override string BuildCssClass()
    {
        var elevationSuffix = Elevation.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-card--elevation-{elevationSuffix}")
            .AddClass("cy-card--interactive", Href is not null)
            .AddClass("cy-card--padding-compact", Padding == CyCardPadding.Compact)
            .AddClass("cy-card--padding-none", Padding == CyCardPadding.None)
            .AddClass("cy-card--outlined", Variant == CyCardVariant.Outlined)
            .AddClass("cy-card--flat", Variant == CyCardVariant.Flat)
            .AddClass("cy-card--collapsible", Collapsible)
            .Build();
    }
}
