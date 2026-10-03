using System.Linq.Expressions;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// Label, hint, required marker and error around <em>any</em> control, with the
/// <c>for</c>/<c>id</c>/<c>aria-describedby</c> wiring done for you. It is how a
/// page that uses a raw <c>&lt;input&gt;</c>, a third-party editor or a
/// <c>&lt;select&gt;</c> gets the same accessible field chrome as
/// <see cref="CyTextBox"/> without adopting every CymruBlazor input.
/// </summary>
/// <remarks>
/// <para>
/// Needs no <c>EditContext</c>. Supply the error yourself with <see cref="Error"/>,
/// or - inside an <c>EditForm</c> - name the field with <see cref="For"/> and the
/// field shows (and clears) its validation messages as the form validates.
/// <see cref="Error"/> wins when both are present.
/// </para>
/// <example>
/// <code language="razor">
/// &lt;CyField Label="Weight" HintText="Without clothes" Error="@_error"&gt;
///     &lt;input @attributes="context.Attributes" class="cy-input" @bind="_weight" /&gt;
/// &lt;/CyField&gt;
/// </code>
/// </example>
/// <para>
/// Use <see cref="Group"/> when the content is several controls that answer one
/// question (a set of related checkboxes, say): the field then renders as a
/// <c>fieldset</c> with a <c>legend</c> instead of a <c>label</c>, and
/// <see cref="CyFieldContext.InputId"/> is not meaningful.
/// </para>
/// </remarks>
public partial class CyField : CyComponentBase, IDisposable
{
    private EditContext? _subscribedContext;
    private FieldIdentifier _field;
    private bool _hasField;
    private string? _contextError;
    private CyFieldContext _context = default!;

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>The field's visible name. Required: every field needs a programmatic label.</summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    /// <summary>Optional supporting text, shown under the label and linked via <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Marks the field as required (visual marker; the control gets
    /// <c>aria-required</c> if it splats <see cref="CyFieldContext.Attributes"/>).
    /// Enforcement is your validation rule's job, as everywhere in CymruBlazor.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// An error to show, set by you. Takes precedence over messages from
    /// <see cref="For"/>. <see langword="null"/> or empty shows none.
    /// </summary>
    [Parameter]
    public string? Error { get; set; }

    /// <summary>
    /// Names the model property this field edits, e.g.
    /// <c>For="@(() =&gt; model.Weight)"</c>, so its messages are read from the
    /// cascaded <see cref="EditContext"/>. Ignored when there is no <c>EditContext</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<object>>? For { get; set; }

    /// <summary>
    /// Renders as <c>fieldset</c>/<c>legend</c> for a set of controls that answer
    /// one question, instead of <c>div</c>/<c>label</c>.
    /// </summary>
    [Parameter]
    public bool Group { get; set; }

    /// <summary>
    /// The control. <c>context</c> carries the id and the ARIA attributes to put
    /// on it (see <see cref="CyFieldContext"/>).
    /// </summary>
    [Parameter]
    public RenderFragment<CyFieldContext>? ChildContent { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-field";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Group ? "cy-field--group" : null)
            .AddClass("cy-field--required", Required)
            .AddClass("cy-field--invalid", _context?.Invalid ?? false)
            .AddClass(Class)
            .Build();

    // Computed at render time (not the base class's cached CssClass) because
    // the invalid modifier depends on the error, which is only known after
    // parameters are validated and which can change on validation alone.
    private string FieldCssClass => BuildCssClass();

    private CyFieldContext FieldState => _context;

    /// <inheritdoc />
    protected override void OnParametersValidated()
    {
        Resubscribe();
        Rebuild();
    }

    private void Resubscribe()
    {
        var wanted = For is not null ? CascadedEditContext : null;

        if (!ReferenceEquals(wanted, _subscribedContext))
        {
            if (_subscribedContext is not null)
            {
                _subscribedContext.OnValidationStateChanged -= OnValidationStateChanged;
            }

            _subscribedContext = wanted;

            if (_subscribedContext is not null)
            {
                _subscribedContext.OnValidationStateChanged += OnValidationStateChanged;
            }
        }

        _hasField = For is not null && _subscribedContext is not null;
        _field = _hasField ? FieldIdentifier.Create(For!) : default;
    }

    private void Rebuild()
    {
        _contextError = _hasField
            ? string.Join(' ', _subscribedContext!.GetValidationMessages(_field))
            : null;

        var error = !string.IsNullOrWhiteSpace(Error)
            ? Error
            : (string.IsNullOrWhiteSpace(_contextError) ? null : _contextError);

        var inputId = Id;

        _context = new CyFieldContext(
            inputId: inputId,
            hintId: string.IsNullOrWhiteSpace(HintText) ? null : $"{inputId}-hint",
            errorId: error is null ? null : $"{inputId}-error",
            errorText: error,
            required: Required);

    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) =>
        _ = InvokeAsync(() =>
        {
            Rebuild();
            StateHasChanged();
        });

    /// <inheritdoc />
    public void Dispose()
    {
        if (_subscribedContext is not null)
        {
            _subscribedContext.OnValidationStateChanged -= OnValidationStateChanged;
            _subscribedContext = null;
        }

        GC.SuppressFinalize(this);
    }
}
