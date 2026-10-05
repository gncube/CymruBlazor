using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A "check your answers" list: each <see cref="CySummaryRow"/> shows a key, its value and an optional change link,
/// following the NHS summary list pattern.
/// </summary>
/// <remarks>
/// Rendered as a description list (<c>dl</c>), so assistive technology announces each key with its value. Every
/// change link carries the key as visually hidden text ("Change <i>Date of birth</i>"), so links are distinguishable
/// when read out of context (WCAG 2.4.4). Below 40rem the key stacks above the value.
/// </remarks>
public partial class CySummaryList : CyLayoutComponentBase
{
    private static readonly CySummaryListText s_defaultText = new();

    /// <summary>Draw a line between rows. Defaults to true.</summary>
    [Parameter]
    public bool Borders { get; set; } = true;

    /// <summary>Draw the list inside a bordered card.</summary>
    [Parameter]
    public bool Card { get; set; }

    /// <summary>The phrases the list shows. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CySummaryListText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-summary-list";

    private CySummaryListText Strings => Text ?? s_defaultText;

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-summary-list--no-borders", !Borders)
            .AddClass("cy-summary-list--card", Card)
            .AddClass(Class)
            .Build();
}
