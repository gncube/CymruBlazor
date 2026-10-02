using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Button;

/// <summary>
/// A button that triggers actions or submits forms. Renders as a native
/// <c>&lt;button&gt;</c> by default, or as an <c>&lt;a&gt;</c> when
/// <see cref="Href"/> is set and the button is not <see cref="CyInteractiveComponentBase.Disabled"/>,
/// so it can be used for both actions and navigation.
/// </summary>
public partial class CyButton : CyInteractiveComponentBase, IHasSize, IHasColour
{
    /// <summary>
    /// Content to render inside the button.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the button's visual style. Must be
    /// <see cref="ComponentColour.Primary"/>, <see cref="ComponentColour.Secondary"/>,
    /// <see cref="ComponentColour.Tertiary"/>, or <see cref="ComponentColour.Danger"/>.
    /// </summary>
    [Parameter]
    public ComponentColour Variant { get; set; } = ComponentColour.Primary;

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Variant;

    /// <summary>
    /// Gets or sets the button's size. Must be <see cref="ComponentSize.Small"/>,
    /// <see cref="ComponentSize.Medium"/>, or <see cref="ComponentSize.Large"/>.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// When <see langword="true"/>, shows a loading spinner and disables
    /// interaction without changing layout width.
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// When set, and the button is not <see cref="CyInteractiveComponentBase.Disabled"/>, renders the
    /// button as an <c>&lt;a&gt;</c> pointing at this URL instead of a
    /// <c>&lt;button&gt;</c>.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the native <c>type</c> attribute used when rendering as
    /// a <c>&lt;button&gt;</c> (ignored when <see cref="Href"/> is set).
    /// </summary>
    [Parameter]
    public string Type { get; set; } = "button";

    /// <summary>
    /// Raised when the button is activated. Not raised while
    /// <see cref="CyInteractiveComponentBase.Disabled"/> or
    /// <see cref="Loading"/> is <see langword="true"/>, and not raised at
    /// all when rendered as a navigation link (the browser handles
    /// navigation instead).
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Optional icon name (see <see cref="CymruBlazor.Icons.IconRegistry"/>) drawn
    /// next to the text. The icon is decorative; the button's accessible name
    /// comes from its text (or, with <see cref="IconOnly"/>, from
    /// <see cref="CyInteractiveComponentBase.AriaLabel"/>).
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Which side of the text the <see cref="Icon"/> is on. Defaults to
    /// <see cref="ButtonIconPlacement.Start"/>.
    /// </summary>
    [Parameter]
    public ButtonIconPlacement IconPlacement { get; set; } = ButtonIconPlacement.Start;

    /// <summary>
    /// Shows only the <see cref="Icon"/>. The button then needs an accessible
    /// name from one of <see cref="CyInteractiveComponentBase.AriaLabel"/>,
    /// <see cref="CyInteractiveComponentBase.AriaLabelledBy"/>, or
    /// <see cref="ChildContent"/> (which is kept for assistive technology but
    /// visually hidden). Without one the button would be announced as just
    /// "button"; that is an error in <c>Strict</c> diagnostics mode and a
    /// logged warning in <c>Lenient</c> mode.
    /// </summary>
    [Parameter]
    public bool IconOnly { get; set; }

    private bool HasIcon => !string.IsNullOrWhiteSpace(Icon);

    private int IconPixelSize => Size switch
    {
        ComponentSize.Small => 16,
        ComponentSize.Large => 24,
        _ => 20,
    };

    /// <summary>
    /// When <see langword="true"/>, the button puts itself into its
    /// <see cref="Loading"/> state for as long as the <see cref="OnClick"/>
    /// handler is running, so a double-click (or an impatient second tap)
    /// cannot run an <c>async</c> handler twice. Off by default so existing
    /// handlers that rely on being re-entrant keep working.
    /// </summary>
    [Parameter]
    public bool AutoLoading { get; set; }

    private bool _handling;

    // Loading is a consumer-controlled flag; _handling is the AutoLoading
    // flag. Either one blocks clicks and shows the busy affordances.
    private bool IsBusy => Loading || _handling;

    // CssClass is rebuilt only when parameters change, but _handling flips
    // between parameter sets - so overlay the loading modifier at render time.
    private string EffectiveCssClass =>
        _handling && !Loading ? $"{CssClass} cy-button--loading" : CssClass;

    protected override string BaseCssClass => "cy-button";

    protected override string BuildCssClass()
    {
        var variantSuffix = Variant.ToString().ToLowerInvariant();
        var sizeSuffix = Size.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-button--{variantSuffix}")
            .AddClass($"cy-button--{sizeSuffix}")
            .AddClass("cy-button--loading", IsBusy)
            .AddClass("cy-button--icon-only", IconOnly)
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Variant is not (ComponentColour.Primary
            or ComponentColour.Secondary
            or ComponentColour.Tertiary
            or ComponentColour.Danger))
        {
            throw new InvalidOperationException(
                $"{nameof(CyButton)}.{nameof(Variant)} must be Primary, Secondary, " +
                $"Tertiary, or Danger. Received '{Variant}'.");
        }

        if (Size is not (ComponentSize.Small
            or ComponentSize.Medium
            or ComponentSize.Large))
        {
            throw new InvalidOperationException(
                $"{nameof(CyButton)}.{nameof(Size)} must be Small, Medium, or Large. " +
                $"Received '{Size}'.");
        }

        if (IconOnly)
        {
            if (!HasIcon)
            {
                ReportMisuse("CY0003",
                    $"{nameof(CyButton)}.{nameof(IconOnly)} is set but {nameof(Icon)} is not, so the button would render nothing.");
            }

            if (string.IsNullOrWhiteSpace(AriaLabel)
                && string.IsNullOrWhiteSpace(AriaLabelledBy)
                && ChildContent is null)
            {
                ReportMisuse("CY0004",
                    $"{nameof(CyButton)} is icon-only but has no accessible name. Set {nameof(AriaLabel)} " +
                    $"(or {nameof(AriaLabelledBy)}, or supply {nameof(ChildContent)} for screen readers).");
            }
        }
    }

    private void ReportMisuse(string code, string message)
    {
        if (Diagnostics.IsStrict)
        {
            throw new InvalidOperationException(message);
        }

        Diagnostics.Warn(code, message);
    }


    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled || IsBusy)
        {
            return;
        }

        if (!OnClick.HasDelegate)
        {
            return;
        }

        if (!AutoLoading)
        {
            await OnClick.InvokeAsync(args);
            return;
        }

        _handling = true;
        // Re-render now (not only when the handler finishes) so the button
        // is visibly/programmatically disabled while the handler awaits.
        StateHasChanged();

        try
        {
            await OnClick.InvokeAsync(args);
        }
        finally
        {
            _handling = false;
        }
    }
}
