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
public partial class CyButton : CyInteractiveComponentBase
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
            .AddClass("cy-button--loading", Loading)
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
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled || Loading)
        {
            return;
        }

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }
    }
}
