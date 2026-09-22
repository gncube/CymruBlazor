using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A group of mutually exclusive options, rendered as a native
/// <c>&lt;fieldset&gt;</c>/<c>&lt;legend&gt;</c> containing one
/// <see cref="CyRadio"/> per option. Radios are supplied as
/// <see cref="ChildContent"/> rather than an items collection - this
/// matches how <see cref="CySelect{TValue}"/> takes its own
/// <c>&lt;option&gt;</c> elements as content, for the same reasons (no
/// item-shape/display-text convention imposed, and consumers can put
/// arbitrary markup - a hint, a divider - between options).
///
/// Each <see cref="CyRadio"/> reports its selection up as a plain string
/// (its own <c>Value</c>); this component is the one place that string is
/// converted to <typeparamref name="TValue"/>, via the same
/// <see cref="Microsoft.AspNetCore.Components.BindConverter"/> call
/// <c>CySelect{TValue}</c> uses for its string-valued options.
/// </summary>
public partial class CyRadioGroup<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] TValue>
    : CyFormFieldComponentBase<TValue>, ICyRadioGroup
{
    /// <summary>
    /// The group's options, as one <see cref="CyRadio"/> per option. May
    /// also contain other markup (e.g. a divider) between them.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    string ICyRadioGroup.GroupName => FieldId;

    string? ICyRadioGroup.SelectedValue => CurrentValueAsString;

    bool ICyRadioGroup.Disabled => Disabled;

    bool ICyRadioGroup.HasError => HasValidationError;

    bool ICyRadioGroup.Required => Required;

    /// <summary>
    /// Called by whichever child <see cref="CyRadio"/> the user selected.
    /// Setting <see cref="Microsoft.AspNetCore.Components.Forms.InputBase{TValue}"/>'s
    /// own <c>CurrentValueAsString</c> (rather than a plain field) routes
    /// through <see cref="TryParseValueFromString"/> and raises
    /// <c>ValueChanged</c>/<c>EditContext.NotifyFieldChanged</c> exactly as
    /// a bound native input would. This component - not the individual
    /// <see cref="CyRadio"/> - owns the state, so the explicit
    /// <see cref="ComponentBase.StateHasChanged"/> call re-renders every
    /// radio in the group (each recomputes its own checked state from
    /// <see cref="ICyRadioGroup.SelectedValue"/>), the same pattern
    /// <c>CyAccordion.Toggle</c> uses for its own child items.
    /// </summary>
    void ICyRadioGroup.Select(string value)
    {
        CurrentValueAsString = value;
        StateHasChanged();
    }

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
