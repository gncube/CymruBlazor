using Microsoft.AspNetCore.Components;

using System.Diagnostics.CodeAnalysis;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A single-line text input field. <c>TValue</c> is fixed to
/// <see cref="string"/> for this release - date-typed input is now built
/// the same way as a follow-up, see <see cref="CyDateInput"/>.
/// </summary>
/// <remarks>
/// Deliberately has no <c>type="number"</c> variant (roadmap D-numeric
/// decision, checked against DHCW's own text-input guidance, fetched
/// 2026-09-22): a native number input's spin buttons, locale-dependent
/// decimal separator, and its habit of silently discarding non-numeric
/// characters (losing a leading zero from a postcode or NHS number, for
/// example) make it unsuitable for most of what looks like "a number" in
/// a form. For a whole number, set <see cref="Type"/> to <c>"text"</c>
/// (the default) and <see cref="InputMode"/> to <c>"numeric"</c> instead -
/// this shows a numeric keypad on touch devices without any of a real
/// number input's parsing side effects.
/// </remarks>
public partial class CyTextBox : CyFormFieldComponentBase<string>
{
    /// <summary>
    /// The HTML input type. Kept as a plain string rather than an enum -
    /// HTML input types are open-ended (new ones are occasionally added
    /// to the spec), and a string avoids this library lagging behind.
    /// Common values: "text" (default), "email", "tel", "password",
    /// "search". See this class's remarks for why "number" is
    /// deliberately not a supported value.
    /// </summary>
    [Parameter]
    public string Type { get; set; } = "text";

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Native <c>inputmode</c> attribute - hints at which on-screen
    /// keyboard a touch device should show, without changing how the
    /// value is parsed (unlike <c>type</c>). Common values: <c>"numeric"</c>
    /// for whole numbers, <c>"decimal"</c>, <c>"tel"</c>, <c>"email"</c>.
    /// </summary>
    [Parameter]
    public string? InputMode { get; set; }

    /// <summary>
    /// Native <c>autocomplete</c> attribute, so browsers can offer to fill
    /// in previously-entered values. WCAG 2.2 SC 1.3.5 (Identify Input
    /// Purpose) expects this to be set for fields that ask for common
    /// personal data (name, address, phone number) - see the
    /// <see href="https://www.w3.org/TR/WCAG22/#input-purposes">
    /// WCAG-defined input purposes</see> for the full token list.
    /// </summary>
    [Parameter]
    public string? Autocomplete { get; set; }

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
