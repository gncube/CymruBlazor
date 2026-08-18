using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A dropdown selection field. Takes <c>&lt;option&gt;</c> elements as
/// <see cref="ChildContent"/> rather than an <c>Items</c> collection
/// parameter - this matches how the framework's own
/// <c>InputSelect&lt;TValue&gt;</c> works, which consumers are likely
/// already familiar with, and avoids this library imposing a specific
/// item-shape/display-text convention in its first release.
/// </summary>
public partial class CySelect<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] TValue> : CyFormFieldComponentBase<TValue>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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
