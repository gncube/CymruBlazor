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
/// a <see langword="null"/> value with <c>Required</c> (or a
/// <c>DataAnnotations</c> attribute on the bound property) surfaces
/// through the same <c>EditContext</c> validation path as every other
/// CymruBlazor field.
/// <para>
/// Once <c>EditContext</c> flags the field invalid, <c>aria-invalid</c> is
/// applied to whichever segment(s) are actually at fault - not all three
/// together - and, when a segment's own content is what's wrong (out of its
/// own range, or a structurally-impossible day/month combination such as 31
/// February), an additional segment-specific sentence
/// (<see cref="DayRangeErrorMessage"/>, <see cref="MonthRangeErrorMessage"/>,
/// <see cref="YearRangeErrorMessage"/>, <see cref="InvalidDateErrorMessage"/>)
/// is appended to the shared error message so the one thing that's wrong is
/// named specifically. When the segments are individually well-formed and
/// combine into a real date, yet the field is still invalid (e.g. a custom
/// "date must be in the past" rule), there is nothing segment-specific to
/// blame, so all three fall back to <c>aria-invalid="true"</c> together, as
/// before.
/// </para>
/// </remarks>
/// before.
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

    /// <summary>
    /// Appended to the error message when the day segment is filled in but out
    /// of its own valid range (not 1-31). Defaults to English; override for Welsh.
    /// </summary>
    [Parameter]
    public string DayRangeErrorMessage { get; set; } = "Day must be a number between 1 and 31.";

    /// <summary>
    /// Appended to the error message when the month segment is filled in but out
    /// of its own valid range (not 1-12). Defaults to English; override for Welsh.
    /// </summary>
    [Parameter]
    public string MonthRangeErrorMessage { get; set; } = "Month must be a number between 1 and 12.";

    /// <summary>
    /// Appended to the error message when the year segment is filled in but is
    /// not a 4-digit number. Defaults to English; override for Welsh.
    /// </summary>
    [Parameter]
    public string YearRangeErrorMessage { get; set; } = "Year must be a 4-digit number.";

    /// <summary>
    /// Appended to the error message when day, month and year are each
    /// individually in range but do not combine into a real date (e.g. 31
    /// February). Defaults to English; override for Welsh.
    /// </summary>
    [Parameter]
    public string InvalidDateErrorMessage { get; set; } = "Enter a real date - that day does not exist in that month.";

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

    private static bool SegmentInOwnRange(string value, int min, int max) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) &&
        parsed >= min && parsed <= max;

    /// <summary>
    /// Which segment(s) are at fault for the current validation error, computed only once
    /// <see cref="CyFormFieldComponentBase{TValue}.HasValidationError"/> is true. A segment that is
    /// empty or outside its own numeric range is blamed individually; if all three are individually
    /// in range but do not combine into a real date, Day and Month share the blame (Year rarely
    /// causes an otherwise-valid day/month to become impossible). If all three are fine on their own
    /// terms and combine into a real date, whatever failed validation isn't attributable to one
    /// segment (e.g. a custom "must be in the past" rule), so all three share it, as before.
    /// </summary>
    private (bool Day, bool Month, bool Year) InvalidSegments()
    {
        if (!HasValidationError)
        {
            return (false, false, false);
        }

        var dayInOwnRange = SegmentInOwnRange(_day, 1, 31);
        var monthInOwnRange = SegmentInOwnRange(_month, 1, 12);
        var yearInOwnRange = SegmentInOwnRange(_year, 1, 9999);

        if (!dayInOwnRange || !monthInOwnRange || !yearInOwnRange)
        {
            return (!dayInOwnRange, !monthInOwnRange, !yearInOwnRange);
        }

        return TryBuildDate(out _) ? (true, true, true) : (true, true, false);
    }

    private string BuildErrorMessage()
    {
        var messages = EditContext.GetValidationMessages(FieldIdentifier).ToList();

        var (dayInvalid, monthInvalid, yearInvalid) = InvalidSegments();
        var dayInOwnRange = SegmentInOwnRange(_day, 1, 31);
        var monthInOwnRange = SegmentInOwnRange(_month, 1, 12);
        var yearInOwnRange = SegmentInOwnRange(_year, 1, 9999);

        if (dayInvalid && !dayInOwnRange && !string.IsNullOrWhiteSpace(_day))
        {
            messages.Add(DayRangeErrorMessage);
        }

        if (monthInvalid && !monthInOwnRange && !string.IsNullOrWhiteSpace(_month))
        {
            messages.Add(MonthRangeErrorMessage);
        }

        if (yearInvalid && !yearInOwnRange && !string.IsNullOrWhiteSpace(_year))
        {
            messages.Add(YearRangeErrorMessage);
        }

        if (dayInvalid && monthInvalid && !yearInvalid && dayInOwnRange && monthInOwnRange)
        {
            messages.Add(InvalidDateErrorMessage);
        }

        return string.Join(' ', messages);
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
