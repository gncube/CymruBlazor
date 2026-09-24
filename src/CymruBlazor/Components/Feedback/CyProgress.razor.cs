using System.Globalization;
using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// A progress indicator built on the native <c>&lt;progress&gt;</c>
/// element - the same "prefer the platform" reasoning
/// <see cref="CymruBlazor.Components.Accessibility.CyDialog"/> applies for modals. A native
/// <c>&lt;progress&gt;</c> is exposed to assistive technology as
/// <c>role="progressbar"</c> automatically, with <c>aria-valuenow</c>/
/// <c>aria-valuemin</c>/<c>aria-valuemax</c> computed by the browser from
/// its <c>value</c>/<c>max</c> attributes, so this component does not
/// re-implement that ARIA by hand.
///
/// Renders determinate (<see cref="Value"/> set) or indeterminate
/// (<see cref="Value"/> left <see langword="null"/>) depending on whether
/// the underlying quantity is currently known - e.g. "42 of 48 beds
/// occupied" is determinate, "checking bed availability" is
/// indeterminate.
/// </summary>
public partial class CyProgress : CyComponentBase, IHasColour, IHasSize
{
    /// <summary>
    /// The current value, between 0 and <see cref="Max"/>. Leave
    /// <see langword="null"/> to render an indeterminate progress bar
    /// (the underlying quantity is not yet known - e.g. still loading).
    /// </summary>
    [Parameter]
    public double? Value { get; set; }

    /// <summary>
    /// The value <see cref="Value"/> is measured against. Defaults to
    /// 100, so <see cref="Value"/> can be supplied directly as a
    /// percentage.
    /// </summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>
    /// Gets or sets the progress bar's semantic colour. Any value other
    /// than <see cref="ComponentColour.Unspecified"/> is supported.
    /// </summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Primary;

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Colour;

    /// <summary>
    /// Gets or sets the progress bar's size (its block-size/thickness).
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <inheritdoc />
    ComponentSize IHasSize.Size => Size;

    /// <summary>
    /// Optional visible label rendered above the bar (e.g. "Medical Ward
    /// A"). When set, it is also used as the progress bar's accessible
    /// name via <c>aria-labelledby</c> - do not also set
    /// <see cref="AriaLabel"/> in that case.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Accessible name for the progress bar, used when there is no
    /// visible <see cref="Label"/> (e.g. a compact progress bar with the
    /// context given by surrounding text instead). Exactly one of
    /// <see cref="Label"/> or <see cref="AriaLabel"/> must be supplied,
    /// so the progress bar always has an accessible name.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// When <see langword="true"/>, shows the computed percentage (e.g.
    /// "42%") next to <see cref="Label"/>. Ignored when
    /// <see cref="ValueText"/> is set, or when the bar is indeterminate.
    /// </summary>
    [Parameter]
    public bool ShowValueText { get; set; }

    /// <summary>
    /// Overrides the displayed and announced value text (e.g. "18 of 20
    /// beds") in place of the computed percentage. Implies
    /// <see cref="ShowValueText"/>.
    /// </summary>
    [Parameter]
    public string? ValueText { get; set; }

    protected override string BaseCssClass => "cy-progress";

    private bool IsIndeterminate => Value is null;

    private string LabelId => $"{Id}-label";

    private double ClampedValue =>
        Value is null ? 0 : Math.Clamp(Value.Value, 0, Max);

    private string ClampedValueAttr =>
        ClampedValue.ToString(CultureInfo.InvariantCulture);

    private string MaxAttr =>
        Max.ToString(CultureInfo.InvariantCulture);

    private int PercentRounded =>
        Max <= 0 ? 0 : (int)Math.Round(ClampedValue / Max * 100, MidpointRounding.AwayFromZero);

    private string? ComputedValueText =>
        ValueText ?? (ShowValueText && !IsIndeterminate
            ? PercentRounded.ToString(CultureInfo.InvariantCulture) + "%"
            : null);

    private bool ShouldRenderHeader =>
        !string.IsNullOrWhiteSpace(Label) || ComputedValueText is not null;

    protected override string BuildCssClass()
    {
        var colourSuffix = Colour.ToString().ToLowerInvariant();
        var sizeSuffix = Size.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-progress--{colourSuffix}")
            .AddClass($"cy-progress--{sizeSuffix}")
            .AddClass("cy-progress--indeterminate", IsIndeterminate)
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Colour is ComponentColour.Unspecified)
        {
            throw new InvalidOperationException(
                $"{nameof(CyProgress)}.{nameof(Colour)} must not be Unspecified.");
        }

        if (Size is ComponentSize.Unspecified)
        {
            throw new InvalidOperationException(
                $"{nameof(CyProgress)}.{nameof(Size)} must not be Unspecified.");
        }

        if (Max <= 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyProgress)}.{nameof(Max)} must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(Label) && string.IsNullOrWhiteSpace(AriaLabel))
        {
            throw new InvalidOperationException(
                $"{nameof(CyProgress)} requires either {nameof(Label)} or {nameof(AriaLabel)}, " +
                "so the progress bar always has an accessible name.");
        }

        if (!string.IsNullOrWhiteSpace(Label) && !string.IsNullOrWhiteSpace(AriaLabel))
        {
            throw new InvalidOperationException(
                $"Specify either {nameof(CyProgress)}.{nameof(Label)} or {nameof(AriaLabel)}, not both.");
        }
    }
}
