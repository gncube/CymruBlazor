using CymruBlazor.Components.Core;
using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A single option within a <see cref="CyRadioGroup{TValue}"/>. Must be
/// used inside one - it reads the group's selected value, shared
/// <c>name</c> and validation state from the cascaded parent, and reports
/// selection back to it. This component's own <see cref="Value"/> is
/// always a string (matching a native radio input's <c>value</c>
/// attribute); the enclosing group is what converts the selected string
/// into its own bound type.
/// </summary>
public partial class CyRadio : CyComponentBase, IHasDisabledState
{
    [CascadingParameter]
    internal ICyRadioGroup? Parent { get; set; }

    /// <summary>The value submitted for this option - a plain string, as a native radio input's <c>value</c> always is.</summary>
    [Parameter, EditorRequired]
    public required string Value { get; set; }

    /// <summary>This option's visible, programmatically-associated label.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>Optional supporting text for this specific option, rendered below its label and associated via <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Disables this option specifically. A radio is also disabled when
    /// the enclosing <see cref="CyRadioGroup{TValue}"/> itself is
    /// disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    protected override string BaseCssClass => "cy-radio-item";

    private string HintId => $"{Id}-hint";

    private bool IsDisabled => Disabled || (Parent?.Disabled ?? false);

    private bool IsChecked =>
        Parent is not null && string.Equals(Parent.SelectedValue, Value, StringComparison.Ordinal);

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Parent is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyRadio)} must be used inside a {nameof(CyRadioGroup<object>)}.");
        }
    }

    private Task HandleChangeAsync()
    {
        if (!IsDisabled)
        {
            Parent?.Select(Value);
        }

        return Task.CompletedTask;
    }
}
