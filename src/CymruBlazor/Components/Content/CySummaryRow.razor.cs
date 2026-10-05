using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// One row of a <see cref="CySummaryList"/>. Must be placed inside a <see cref="CySummaryList"/>.
/// </summary>
/// <remarks>
/// <see cref="Value"/> is rendered as encoded text; use <c>ChildContent</c> for anything richer, which wins over
/// <see cref="Value"/>. A row with neither shows the localised "Not provided". Set <see cref="ChangeHref"/> for a link
/// or <see cref="OnChange"/> for a button; the link wins when both are set.
/// </remarks>
public partial class CySummaryRow : CyLayoutComponentBase
{
    private static readonly CySummaryListText s_defaultText = new();

    /// <summary>The question or field name. Required.</summary>
    [Parameter, EditorRequired]
    public required string Key { get; set; }

    /// <summary>The answer, as text.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>URL of the page where the answer can be changed. Renders a link.</summary>
    [Parameter]
    public string? ChangeHref { get; set; }

    /// <summary>Raised when the change button is pressed. Renders a button when <see cref="ChangeHref"/> is not set.</summary>
    [Parameter]
    public EventCallback OnChange { get; set; }

    /// <summary>Overrides the visible text of the change action (the default comes from the list's text).</summary>
    [Parameter]
    public string? ChangeText { get; set; }

    /// <summary>Visually hidden text read after the change action. Defaults to <see cref="Key"/>.</summary>
    [Parameter]
    public string? HiddenText { get; set; }

    /// <summary>The text cascaded by the parent <see cref="CySummaryList"/>. Not for application code.</summary>
    [CascadingParameter(Name = "CySummaryListText")]
    public CySummaryListText? ListText { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-summary-list__row";

    private CySummaryListText Strings => ListText ?? s_defaultText;

    private string ChangeLabel => string.IsNullOrWhiteSpace(ChangeText) ? Strings.Change : ChangeText;

    private string HiddenTextValue => string.IsNullOrWhiteSpace(HiddenText) ? Key : HiddenText;

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new InvalidOperationException(
                $"{nameof(CySummaryRow)}.{nameof(Key)} must not be empty: it names the answer.");
        }
    }
}
