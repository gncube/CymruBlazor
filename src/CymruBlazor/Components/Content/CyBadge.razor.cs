using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A small, static label for categorisation, status, or metadata - e.g. a
/// technology badge, a component category label, or a status pill.
///
/// Also covers the "tag" use case (a removable/interactive chip, as used
/// for active filters) via <see cref="Dismissible"/> - CymruBlazor does not
/// have a separate CyTag component, since the two only differ by whether a
/// dismiss affordance is present, not by structure or semantics.
/// </summary>
public partial class CyBadge : CyLayoutComponentBase, IHasColour
{
    /// <summary>
    /// Gets or sets the badge's semantic colour. Any value other than
    /// <see cref="ComponentColour.Unspecified"/> is supported.
    /// </summary>
    [Parameter]
    public ComponentColour Variant { get; set; } = ComponentColour.Neutral;

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Variant;

    /// <summary>
    /// When <see langword="true"/> (the default), renders as a fully
    /// rounded pill. When <see langword="false"/>, renders with the
    /// standard small corner radius used elsewhere in the library.
    /// </summary>
    [Parameter]
    public bool Pill { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, renders a dismiss ("x") button,
    /// turning the badge into a removable tag/chip.
    /// </summary>
    [Parameter]
    public bool Dismissible { get; set; }

    /// <summary>
    /// Accessible label for the dismiss button. Defaults to "Remove" -
    /// supply a more specific label (e.g. "Remove Cardiology filter")
    /// when multiple dismissible badges appear together, so screen
    /// reader users can distinguish between them.
    /// </summary>
    [Parameter]
    public string? DismissAriaLabel { get; set; }

    /// <summary>
    /// Raised when the dismiss button is activated. Like
    /// <see cref="CyAlert.OnDismiss"/>, CyBadge does not remove itself
    /// from the DOM - the parent owns whether/how the badge disappears.
    /// </summary>
    [Parameter]
    public EventCallback OnDismiss { get; set; }

    protected override string BaseCssClass => "cy-badge";

    protected override string BuildCssClass()
    {
        var variantSuffix = Variant.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-badge--{variantSuffix}")
            .AddClass("cy-badge--pill", Pill)
            .AddClass("cy-badge--dismissible", Dismissible)
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Variant is ComponentColour.Unspecified)
        {
            throw new InvalidOperationException(
                $"{nameof(CyBadge)}.{nameof(Variant)} must not be Unspecified.");
        }
    }

    private async Task HandleDismissAsync()
    {
        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }
    }
}
