using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A multi-line text input field, for answers a single-line
/// <see cref="CyTextBox"/> is too short for. <c>TValue</c> is fixed to
/// <see cref="string"/>, matching <see cref="CyTextBox"/>.
/// </summary>
/// <remarks>
/// Per the NHS Wales Design System's own textarea guidance, a service
/// should prefer structured, closed questions (<see cref="CyRadioGroup{TValue}"/>,
/// <see cref="CyCheckbox"/>) where possible, and think through clinical/
/// safeguarding risk before using free text at all - this component does
/// not, and cannot, make that judgement for the consuming application.
/// </remarks>
public partial class CyTextArea : CyFormFieldComponentBase<string>
{
    /// <summary>
    /// The textarea's visible height, in rows. Make this proportional to
    /// the amount of text expected - a large textarea invites longer,
    /// less structured answers. Defaults to 5, matching the NHS Wales/
    /// NHS.UK/GOV.UK default.
    /// </summary>
    [Parameter]
    public int Rows { get; set; } = 5;

    /// <summary>Native <c>maxlength</c> attribute. Required when <see cref="ShowCharacterCount"/> is <see langword="true"/>.</summary>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Shows a live "characters remaining"/"characters too many" message
    /// below the field, updated as the user types and announced via
    /// <c>aria-live="polite"</c>. Throws <see cref="InvalidOperationException"/> if
    /// <see cref="MaxLength"/> is not set.
    /// </summary>
    [Parameter]
    public bool ShowCharacterCount { get; set; }

    /// <summary>English default for the singular "remaining" message. <c>{0}</c> is replaced with the count (always 1).</summary>
    [Parameter]
    public string CharacterRemainingFormat { get; set; } = "You have {0} character remaining";

    /// <summary>English default for the plural "remaining" message. <c>{0}</c> is replaced with the count.</summary>
    [Parameter]
    public string CharactersRemainingFormat { get; set; } = "You have {0} characters remaining";

    /// <summary>English default for the singular "too many" message. <c>{0}</c> is replaced with the count (always 1).</summary>
    [Parameter]
    public string CharacterOverLimitFormat { get; set; } = "You have {0} character too many";

    /// <summary>English default for the plural "too many" message. <c>{0}</c> is replaced with the count.</summary>
    [Parameter]
    public string CharactersOverLimitFormat { get; set; } = "You have {0} characters too many";

    private string CountId => $"{FieldId}-count";

    /// <summary>
    /// <see cref="CyFormFieldComponentBase{TValue}.ComputedAriaDescribedBy"/>
    /// plus the character-count region's id when it is shown - kept local
    /// to this component rather than added to the shared base, since no
    /// other field type has a live count region.
    /// </summary>
    private string? DescribedBy
    {
        get
        {
            var ids = new List<string>();

            if (ComputedAriaDescribedBy is { } described)
            {
                ids.Add(described);
            }

            if (ShowCharacterCount && MaxLength.HasValue)
            {
                ids.Add(CountId);
            }

            return ids.Count == 0 ? null : string.Join(' ', ids);
        }
    }

    private string CharacterCountMessage
    {
        get
        {
            var length = CurrentValueAsString?.Length ?? 0;
            var remaining = MaxLength!.Value - length;

            return remaining switch
            {
                1 => string.Format(CultureInfo.CurrentCulture, CharacterRemainingFormat, remaining),
                >= 0 => string.Format(CultureInfo.CurrentCulture, CharactersRemainingFormat, remaining),
                -1 => string.Format(CultureInfo.CurrentCulture, CharacterOverLimitFormat, -remaining),
                _ => string.Format(CultureInfo.CurrentCulture, CharactersOverLimitFormat, -remaining)
            };
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ShowCharacterCount && !MaxLength.HasValue)
        {
            throw new InvalidOperationException(
                $"{nameof(CyTextArea)}.{nameof(MaxLength)} must be set when {nameof(ShowCharacterCount)} is true.");
        }
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out string result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }
}
