using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// An on/off switch for a setting that takes effect immediately (for example
/// "Required" or "Enabled" on a questionnaire row). Use <see cref="CyCheckbox"/>
/// instead when the choice is only submitted later with a form.
/// </summary>
/// <remarks>
/// <para>
/// Renders a native <c>&lt;button type="button" role="switch"&gt;</c> with
/// <c>aria-checked</c>, named by its <c>&lt;label&gt;</c> (<c>aria-labelledby</c>).
/// Because it is a real button, <c>Space</c> and <c>Enter</c> toggle it with no
/// script, and clicking the label toggles it too. The control is 44x24 CSS px,
/// above the WCAG 2.5.8 minimum target size.
/// </para>
/// <para>
/// Binds like every CymruBlazor field (<c>@bind-Value</c>, with or without an
/// <c>EditForm</c>). <see cref="OnChange"/> is an additional callback that is
/// raised after the value changes, for handlers that do not bind.
/// </para>
/// </remarks>
public partial class CySwitch : CyFormFieldComponentBase<bool>
{
    /// <summary>Raised with the new state each time the user toggles the switch.</summary>
    [Parameter]
    public EventCallback<bool> OnChange { get; set; }

    private string LabelId => $"{FieldId}-label";

    private async Task ToggleAsync()
    {
        if (Disabled)
        {
            return;
        }

        CurrentValue = !CurrentValue;

        if (OnChange.HasDelegate)
        {
            await OnChange.InvokeAsync(CurrentValue);
        }
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out bool result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotSupportedException(
            $"{nameof(CySwitch)} binds directly to its boolean value and does not parse a string representation.");
    }
}
