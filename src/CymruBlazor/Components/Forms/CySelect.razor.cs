using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A dropdown selection field. Options come from <see cref="Items"/> (a list of
/// <see cref="CyOption{TValue}"/>, added in 1.8.0), from hand-written
/// <c>&lt;option&gt;</c> elements in <see cref="ChildContent"/> (the original
/// shape, matching the framework's <c>InputSelect&lt;TValue&gt;</c>), or both:
/// <see cref="Items"/> render first, then <see cref="ChildContent"/>.
/// </summary>
public partial class CySelect<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] TValue> : CyFormFieldComponentBase<TValue>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The options to render, as <c>&lt;option&gt;</c> elements ahead of any
    /// <see cref="ChildContent"/>. Values are formatted with the same
    /// conversion the selection is parsed with, so they round-trip for
    /// strings, numbers, enums and their nullable forms.
    /// </summary>
    [Parameter]
    public IEnumerable<CyOption<TValue>>? Items { get; set; }

    /// <summary>
    /// When set (and <see cref="Items"/> is used), renders a leading option with
    /// an empty value and this text, e.g. "Choose a country". Selecting it
    /// binds <c>default</c>/<see langword="null"/> for nullable
    /// <typeparamref name="TValue"/>s; for a non-nullable one the empty value
    /// fails to parse, which surfaces as a validation message, so use it with
    /// nullable types or a required-field rule.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.CurrentCulture, out var parsedValue))
        {
            result = parsedValue!;
            validationErrorMessage = null;
            return true;
        }

        result = default;
        validationErrorMessage = "The selected value is not valid.";
        return false;
    }
}
