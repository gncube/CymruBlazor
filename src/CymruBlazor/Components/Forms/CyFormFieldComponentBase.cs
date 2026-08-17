using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// Shared foundation for CymruBlazor form field components
/// (<see cref="CyTextBox"/>, <see cref="CySelect{TValue}"/>,
/// <see cref="CyCheckbox"/>).
///
/// Derives from the framework's own <see cref="InputBase{TValue}"/> rather
/// than <c>CyComponentBase</c>, to get <c>EditContext</c>/
/// <c>FieldIdentifier</c>/<c>CurrentValue</c> plumbing and validation-state
/// CSS classes for free instead of reimplementing them. Because C# doesn't
/// support multiple inheritance, this intentionally does not share a base
/// with the rest of the component library - see
/// plan/plan-next-release-components.md section 1.2 for the reasoning.
/// Id generation and CSS class composition are duplicated here in minimal
/// form to keep the same conventions as <c>CyComponentBase</c>-derived
/// components.
/// </summary>
public abstract class CyFormFieldComponentBase<TValue> : InputBase<TValue>
{
    private string _id = string.Empty;

    [Inject]
    private IComponentIdGenerator ComponentIdGenerator { get; set; } = default!;

    /// <summary>
    /// The field's visible label. Required - every CymruBlazor form field
    /// must have a programmatically associated label. There is no
    /// "placeholder as label" option, since that's a well-known
    /// accessibility failure (placeholder text disappears once the user
    /// starts typing, and isn't reliably announced the same way a label
    /// is by every screen reader).
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Optional supporting hint text, rendered below the label and
    /// associated with the input via <c>aria-describedby</c>.
    /// </summary>
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Marks the field as visually/programmatically required
    /// (<c>aria-required</c> + a visual indicator). This does not perform
    /// validation itself - actual required-ness enforcement is the
    /// consuming app's <c>DataAnnotations</c>/<c>EditContext</c> concern,
    /// matching how validation works everywhere else in Blazor forms.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the field is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the field wrapper.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id of the input element. If not supplied, a
    /// deterministic id is generated on first use.
    /// </summary>
    [Parameter]
    public string Id
    {
        get => EnsureId();
        set => _id = value;
    }

    /// <summary>
    /// Same value as <see cref="Id"/> - exists as a clearer name at
    /// usage sites like <c>for</c>/<c>aria-describedby</c> wiring in
    /// derived components' markup.
    /// </summary>
    protected string FieldId => EnsureId();

    protected string HintId => $"{FieldId}-hint";

    protected string ErrorId => $"{FieldId}-error";

    /// <summary>
    /// Gets whether the field currently has one or more validation
    /// messages, per the cascaded <see cref="InputBase{TValue}.EditContext"/>.
    /// </summary>
    protected bool HasValidationError =>
        EditContext.GetValidationMessages(FieldIdentifier).Any();

    /// <summary>
    /// Gets the field's current validation state.
    /// </summary>
    protected ValidationState CurrentValidationState =>
        HasValidationError ? ValidationState.Invalid : ValidationState.Unspecified;

    /// <summary>
    /// The space-separated ids this field's input should be described by
    /// (hint text and/or the validation error message, whichever are
    /// present), for wiring up <c>aria-describedby</c>.
    /// </summary>
    protected string? ComputedAriaDescribedBy
    {
        get
        {
            var ids = new List<string>();

            if (!string.IsNullOrWhiteSpace(HintText))
            {
                ids.Add(HintId);
            }

            if (HasValidationError)
            {
                ids.Add(ErrorId);
            }

            return ids.Count == 0 ? null : string.Join(' ', ids);
        }
    }

    /// <summary>
    /// Composes a field wrapper's CSS classes: the supplied base class,
    /// InputBase's own validation-state class (<see cref="InputBase{TValue}.CssClass"/> -
    /// e.g. "valid modified" / "invalid"), <see cref="Class"/>, and a
    /// "cy-field--required" modifier when applicable.
    /// </summary>
    protected string BuildFieldCssClass(string baseClass) =>
        CssBuilder.Empty
            .AddClass(baseClass)
            .AddClass(CssClass)
            .AddClass(Class)
            .AddClass("cy-field--required", Required)
            .Build();

    private string EnsureId()
    {
        if (string.IsNullOrWhiteSpace(_id))
        {
            _id = ComponentIdGenerator.Create("cy-field");
        }

        return _id;
    }
}
