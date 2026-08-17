using Microsoft.AspNetCore.Components;

using System.Diagnostics.CodeAnalysis;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A single-line text input field. <c>TValue</c> is fixed to
/// <see cref="string"/> for this release - numeric/date-typed inputs are
/// a natural follow-up built the same way, not a blocker for this one.
/// </summary>
public partial class CyTextBox : CyFormFieldComponentBase<string>
{
    /// <summary>
    /// The HTML input type. Kept as a plain string rather than an enum -
    /// HTML input types are open-ended (new ones are occasionally added
    /// to the spec), and a string avoids this library lagging behind.
    /// Common values: "text" (default), "email", "tel", "password",
    /// "search".
    /// </summary>
    [Parameter]
    public string Type { get; set; } = "text";

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public int? MaxLength { get; set; }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out string result,
        out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }
}
