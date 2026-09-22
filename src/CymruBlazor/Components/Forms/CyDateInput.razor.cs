using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A three-field day/month/year date input, per roadmap decision D4:
/// built ahead of any calendar picker, following the GOV.UK/NHS.UK/NHS
/// Wales pattern confirmed against the DHCW component library (fetched
/// 2026-09-22) - three plain text fields (<c>type="text"
/// inputmode="numeric"</c>, not <c>type="number"</c> or a
/// <c>&lt;select&gt;</c> of months), each separately labelled "Day",
/// "Month" and "Year", with no automatic tabbing between them (which the
/// DHCW guidance explicitly warns against as confusing for keyboard
/// users). <c>TValue</c> is fixed to <see cref="DateOnly"/>? - nullable so
/// the field can start, and remain, empty until all three segments are
/// filled in, the same reasoning <see cref="CyTextBox"/> gives for fixing
/// its own <c>TValue</c> to <see cref="string"/> for this release.
/// </summary>
/// <remarks>
/// Segments are tracked as their own raw strings rather than parsed on
/// every keystroke, and only combined into <see cref="DateOnly"/> on
/// <c>change</c> (blur/tab-away, via <c>@bind:event="onchange"</c>) - not
/// <c>oninput</c> - so a user is never shown a validation error while
/// still mid-digit. An incomplete or out-of-range combination (e.g. day
/// 31, month 2) simply leaves the bound value <see langword="null"/>;
/// this component does not synthesise its own per-segment error text; a
/// <see langword="null"/> value with <c>Required</c> (or a
/// <c>DataAnnotations</c> attribute on the bound property) surfaces
/// through the same <c>EditContext</c> validation path as every other
/// CymruBlazor field. A known simplification, tracked in the backlog: the
/// rendered error state (<c>aria-invalid</c>) applies to all three
/// segments together, not to whichever specific segment is actually
/// wrong, as the DHCW reference markup does for a single-field error.
/// </remarks>
public partial class CyDateInput : CyFormFieldComponentBase<DateOnly?>
{
    private string _day = string.Empty;
    private string _month = string.Empty;
    private string _year = string.Empty;

    /// <summary>Visible label for the day field. Defaults to "Day".</summary>
    [Parameter]
    public string DayLabel { get; set; } = "Day";

    /// <summary>Visible label for the month field. Defaults to "Month".</summary>
    [Parameter]
    public string MonthLabel { get; set; } = "Month";

    /// <summary>Visible label for the year field. Defaults to "Year".</summary>
    [Parameter]
    public string YearLabel { get; set; } = "Year";

    /// <summary>
    /// Sets <c>autocomplete="bday-day"</c>/<c>"bday-month"</c>/<c>"bday-year"</c>
    /// on the three fields, letting browsers offer to fill in a
    /// previously-entered date of birth. Per DHCW/GOV.UK guidance, set
    /// this only when the field actually asks for a date of birth - not
    /// for an arbitrary date such as an appointment or expiry date.
    /// </summary>
    [Parameter]
    public bool AutocompleteDateOfBirth { get; set; }

    private string DayId => $"{FieldId}-day";

    private string MonthId => $"{FieldId}-month";

    private string YearId => $"{FieldId}-year";

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Refresh the three segments from an externally supplied/changed
        // Value (e.g. loading an existing record), but only when they
        // don't already represent it - so a re-render triggered by
        // something else on the page never clobbers a keystroke the user
        // is still in the middle of typing.
        if (Value is { } date && !SegmentsRepresent(date))
        {
            _day = date.Day.ToString(CultureInfo.InvariantCulture);
            _month = date.Month.ToString(CultureInfo.InvariantCulture);
            _year = date.Year.ToString(CultureInfo.InvariantCulture);
        }
    }

    private bool SegmentsRepresent(DateOnly date) =>
        TryBuildDate(out var current) && current == date;

    private Task RecomputeAsync()
    {
        CurrentValue = TryBuildDate(out var date) ? date : null;

        return Task.CompletedTask;
    }

    private bool TryBuildDate(out DateOnly date)
    {
        if (int.TryParse(_day, NumberStyles.None, CultureInfo.InvariantCulture, out var day) &&
            int.TryParse(_month, NumberStyles.None, CultureInfo.InvariantCulture, out var month) &&
            int.TryParse(_year, NumberStyles.None, CultureInfo.InvariantCulture, out var year))
        {
            try
            {
                date = new DateOnly(year, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                date = default;
                return false;
            }
        }

        date = default;
        return false;
    }

    /// <summary>
    /// This component's own markup never sets <c>CurrentValueAsString</c>
    /// (it composes <see cref="DateOnly"/> directly from the three
    /// segments in <see cref="RecomputeAsync"/>) - this override exists
    /// only to satisfy <see cref="Microsoft.AspNetCore.Components.Forms.InputBase{TValue}"/>'s
    /// abstract contract, for the unlikely case something else in a
    /// consuming app sets <c>CurrentValueAsString</c> directly. It accepts
    /// any format <see cref="DateOnly.TryParse(ReadOnlySpan{char},IFormatProvider?,out DateOnly)"/> does.
    /// </summary>
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out DateOnly? result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var parsed))
        {
            result = parsed;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = "Enter a valid date.";
        return false;
    }
}
