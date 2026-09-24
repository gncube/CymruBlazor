using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// An indeterminate loading indicator (a "spinner"). Announces itself to
/// assistive technology via <c>role="status"</c> and an accessible name
/// (<see cref="Label"/>) rather than relying on motion or colour alone -
/// the same reasoning <see cref="CyProgress"/> applies for the
/// determinate case.
///
/// For a button's own inline busy state, keep using <c>CyButton.Loading</c>
/// (unchanged) - that spinner is tied to the button's own
/// <c>aria-busy</c>/<c>disabled</c> handling. Use <see cref="CySpinner"/>
/// wherever a loading state needs its own accessible name, e.g. a panel
/// or region that is (re)loading data.
/// </summary>
public partial class CySpinner : CyLayoutComponentBase, IHasSize, IHasColour
{
    /// <summary>
    /// Gets or sets the spinner's size.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <inheritdoc />
    ComponentSize IHasSize.Size => Size;

    /// <summary>
    /// Gets or sets the spinner's semantic colour. Any value other than
    /// <see cref="ComponentColour.Unspecified"/> is supported.
    /// </summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Primary;

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Colour;

    /// <summary>
    /// Accessible name announced to screen readers. Defaults to the
    /// English "Loading"; supply a translation (e.g. Welsh) for
    /// bilingual services.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// When <see langword="true"/>, <see cref="Label"/> is also shown
    /// visually next to the spinner. When <see langword="false"/> (the
    /// default), <see cref="Label"/> is only exposed to assistive
    /// technology via visually-hidden text - the common case where the
    /// surrounding UI already makes clear that something is loading.
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; }

    protected override string BaseCssClass => "cy-spinner";

    private string ComputedLabel =>
        string.IsNullOrWhiteSpace(Label) ? "Loading" : Label;

    protected override string BuildCssClass()
    {
        var sizeSuffix = Size.ToString().ToLowerInvariant();
        var colourSuffix = Colour.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-spinner--{sizeSuffix}")
            .AddClass($"cy-spinner--{colourSuffix}")
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Size is ComponentSize.Unspecified)
        {
            throw new InvalidOperationException(
                $"{nameof(CySpinner)}.{nameof(Size)} must not be Unspecified.");
        }

        if (Colour is ComponentColour.Unspecified)
        {
            throw new InvalidOperationException(
                $"{nameof(CySpinner)}.{nameof(Colour)} must not be Unspecified.");
        }
    }
}
