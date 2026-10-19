using CymruBlazor.Contracts;
using CymruBlazor.Diagnostics;
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
///
/// <para>
/// <b>Standalone mode (1.8.0).</b> A field does not need an
/// <c>&lt;EditForm&gt;</c>: <see cref="InputBase{TValue}"/> itself tolerates a
/// missing cascaded <see cref="EditContext"/>, and every member here is
/// null-safe, so <c>&lt;CyTextBox @bind-Value="x" Label="..." /&gt;</c> works
/// on any page. Without an <c>EditContext</c> there is no validation store, so
/// the only error a field can show is one it raised itself (see
/// <see cref="LocalError"/>); everything else about the field is unchanged.
/// </para>
/// </summary>
public abstract class CyFormFieldComponentBase<TValue> : InputBase<TValue>, IHasDisabledState, IHasValidationState
{
    [Inject]
    private IComponentIdGenerator ComponentIdGenerator { get; set; } = default!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Gets the library diagnostics service, or the strict non-logging default when none is registered
    /// (the same fallback as <c>CyComponentBase</c>).
    /// </summary>
    private ICyDiagnostics FieldDiagnostics =>
        (ICyDiagnostics?)ServiceProvider?.GetService(typeof(ICyDiagnostics)) ?? CyDiagnostics.Default;

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
    /// deterministic id is generated on first access (see
    /// <see cref="EnsureId"/>) and cached back into this property - kept
    /// as a plain auto property (rather than a custom get/set pair) so
    /// Blazor's parameter-assignment reflection sees ordinary property
    /// semantics, per BL0007.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Same value as <see cref="Id"/> - exists as a clearer name at
    /// usage sites like <c>for</c>/<c>aria-describedby</c> wiring in
    /// derived components' markup.
    /// </summary>
    protected string FieldId => EnsureId();

    protected string HintId => $"{FieldId}-hint";

    protected string ErrorId => $"{FieldId}-error";

    /// <summary>
    /// An error the field raised itself, shown when there is no
    /// <see cref="EditContext"/> to carry it. Inside an <c>EditForm</c> a
    /// parse failure is reported through the <c>EditContext</c> by
    /// <see cref="InputBase{TValue}"/> and this stays <see langword="null"/>;
    /// outside one the framework discards the message, so a component whose
    /// parser produces user-facing messages (<see cref="CyNumberInput{TValue}"/>)
    /// records it here instead. Derived components must only set this while
    /// <see cref="EditContext"/> is <see langword="null"/>.
    /// </summary>
    protected string? LocalError { get; set; }

    /// <summary>
    /// The field's current validation messages: those the cascaded
    /// <see cref="InputBase{TValue}.EditContext"/> holds for this field (none
    /// when there is no <c>EditContext</c>), followed by <see cref="LocalError"/>.
    /// </summary>
    protected IEnumerable<string> ValidationMessages
    {
        get
        {
            var fromContext = EditContext is null
                ? Enumerable.Empty<string>()
                : EditContext.GetValidationMessages(FieldIdentifier);

            return LocalError is null ? fromContext : fromContext.Append(LocalError);
        }
    }

    /// <summary>
    /// <see cref="ValidationMessages"/> joined into the single string the
    /// field markup renders inside its error paragraph.
    /// </summary>
    protected string ValidationMessageText => string.Join(' ', ValidationMessages);

    /// <summary>
    /// Gets whether the field currently has one or more validation
    /// messages (see <see cref="ValidationMessages"/>). Safe to call with no
    /// <see cref="EditContext"/>.
    /// </summary>
    protected bool HasValidationError => ValidationMessages.Any();

    /// <summary>
    /// Gets the field's current validation state.
    /// </summary>
    protected ValidationState CurrentValidationState =>
        HasValidationError ? ValidationState.Invalid : ValidationState.Unspecified;

    /// <inheritdoc />
    ValidationState IHasValidationState.ValidationState => CurrentValidationState;

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

    /// <inheritdoc />
    /// <remarks>
    /// Reports <c>CY0005</c> (a warning in both diagnostics modes) when the field has no <see cref="Label"/> and no
    /// <c>aria-label</c> / <c>aria-labelledby</c>. Derived components that override this must call the base method.
    /// </remarks>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        CyAccessibleNameCheck.Check(FieldDiagnostics, GetType(), Label, AdditionalAttributes);
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
            // With an EditContext, InputBase's CssClass already supplies
            // "invalid"; standalone fields have no EditContext, so mirror it.
            .AddClass("cy-field--invalid", EditContext is null && HasValidationError)
            .Build();

    /// <summary>
    /// Renders the shared label chrome for this field (<c>&lt;label&gt;</c> with required indicator).
    /// </summary>
    /// <param name="labelClass">The CSS class for the label element (defaults to <c>cy-field__label</c>).</param>
    /// <param name="labelId">Optional HTML id for the label element.</param>
    protected RenderFragment RenderFieldLabel(string labelClass = "cy-field__label", string? labelId = null) => builder =>
    {
        builder.OpenElement(0, "label");
        builder.AddAttribute(1, "for", FieldId);
        if (!string.IsNullOrWhiteSpace(labelId))
        {
            builder.AddAttribute(2, "id", labelId);
        }
        builder.AddAttribute(3, "class", labelClass);
        builder.AddContent(4, Label);
        if (Required)
        {
            builder.OpenElement(5, "span");
            builder.AddAttribute(6, "class", "cy-field__required-indicator");
            builder.AddAttribute(7, "aria-hidden", "true");
            builder.AddContent(8, "*");
            builder.CloseElement();
        }
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the shared fieldset legend chrome for grouping fields (<c>&lt;legend&gt;</c> with required indicator).
    /// </summary>
    /// <param name="legendClass">The CSS class for the legend element (defaults to <c>cy-field__label</c>).</param>
    protected RenderFragment RenderFieldLegend(string legendClass = "cy-field__label") => builder =>
    {
        builder.OpenElement(0, "legend");
        builder.AddAttribute(1, "class", legendClass);
        builder.AddContent(2, Label);
        if (Required)
        {
            builder.OpenElement(3, "span");
            builder.AddAttribute(4, "class", "cy-field__required-indicator");
            builder.AddAttribute(5, "aria-hidden", "true");
            builder.AddContent(6, "*");
            builder.CloseElement();
        }
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the shared hint text paragraph when <see cref="HintText"/> is set.
    /// </summary>
    protected RenderFragment RenderFieldHint() => builder =>
    {
        if (!string.IsNullOrWhiteSpace(HintText))
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "id", HintId);
            builder.AddAttribute(2, "class", "cy-field__hint");
            builder.AddContent(3, HintText);
            builder.CloseElement();
        }
    };

    /// <summary>
    /// Renders the shared validation error paragraph when this field has a validation error.
    /// </summary>
    /// <param name="errorMessage">Optional override for the error message (defaults to <see cref="ValidationMessageText"/>).</param>
    protected RenderFragment RenderFieldError(string? errorMessage = null) => builder =>
    {
        if (HasValidationError)
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "id", ErrorId);
            builder.AddAttribute(2, "class", "cy-field__error");
            builder.AddAttribute(3, "role", "alert");
            builder.AddContent(4, errorMessage ?? ValidationMessageText);
            builder.CloseElement();
        }
    };

    private string EnsureId()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Id = ComponentIdGenerator.Create("cy-field");
        }

        return Id;
    }
}
