using System.Diagnostics.CodeAnalysis;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A single checkbox field, <c>TValue</c> fixed to <see cref="bool"/>.
///
/// Unlike <see cref="CyTextBox"/>, this binds directly to
/// <c>CurrentValue</c> (via <c>@bind</c> on the checkbox's <c>checked</c>
/// property in the markup) rather than going through
/// <c>CurrentValueAsString</c>/<see cref="TryParseValueFromString"/> - a
/// checkbox's state is never actually a string, so
/// <see cref="TryParseValueFromString"/> is unreachable here. This matches
/// how the framework's own <c>InputCheckbox</c> is implemented.
/// </summary>
public partial class CyCheckbox : CyFormFieldComponentBase<bool>
{
    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out bool result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotSupportedException(
            $"{nameof(CyCheckbox)} binds directly to its boolean value and does not parse a string representation.");
    }
}
