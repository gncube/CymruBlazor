using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Provides the common foundation for interactive CymruBlazor components.
///
/// This base class extends <see cref="CyComponentBase"/> with
/// functionality shared by interactive controls, including disabled state,
/// accessibility metadata, and keyboard navigation configuration.
///
/// Form binding, validation, and JavaScript interop are intentionally
/// implemented by more specialised derived classes.
/// </summary>
public abstract class CyInteractiveComponentBase : CyComponentBase
{
    /// <summary>
    /// Gets or sets whether the component is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the accessible label.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the accessible description.
    /// </summary>
    [Parameter]
    public string? AriaDescription { get; set; }

    /// <summary>
    /// Gets or sets the accessible labelled-by reference.
    /// </summary>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// Gets or sets the accessible described-by reference.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets the tab order.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    /// <summary>
    /// Gets the effective tab index.
    /// Disabled controls are removed from the tab sequence.
    /// </summary>
    protected virtual int ComputedTabIndex =>
        Disabled ? -1 : TabIndex ?? 0;

    /// <summary>
    /// Gets whether the component can receive focus.
    /// </summary>
    protected virtual bool CanReceiveFocus => !Disabled;

    /// <summary>
    /// Gets the computed ARIA disabled value.
    /// </summary>
    protected virtual string? AriaDisabled =>
        Disabled ? "true" : null;

    /// <summary>
    /// Called when the user activates the component using the keyboard.
    /// </summary>
    /// <param name="args">
    /// Keyboard event arguments.
    /// </param>
    protected virtual Task OnKeyboardActivateAsync(
        KeyboardEventArgs args)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (!string.IsNullOrWhiteSpace(AriaLabel) &&
            !string.IsNullOrWhiteSpace(AriaLabelledBy))
        {
            throw new InvalidOperationException(
                "Specify either AriaLabel or AriaLabelledBy, but not both.");
        }

        if (!string.IsNullOrWhiteSpace(AriaDescription) &&
            !string.IsNullOrWhiteSpace(AriaDescribedBy))
        {
            throw new InvalidOperationException(
                "Specify either AriaDescription or AriaDescribedBy, but not both.");
        }
    }
}
