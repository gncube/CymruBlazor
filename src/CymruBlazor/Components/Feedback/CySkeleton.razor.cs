using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Localisation;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// A placeholder shown while content loads: bars of text, a block or a circle.
/// </summary>
/// <remarks>
/// <para>
/// The shapes are decorative and hidden from assistive technology. The component adds one visually
/// hidden "Loading" text so a screen reader user is told something is on its way. When you show
/// several skeletons together, set <see cref="Announce"/> to <see langword="false"/> on all but one, or
/// on all of them and wrap the group in a region with <c>role="status"</c> and <c>aria-busy="true"</c>
/// that carries the message. Remove the skeletons (and <c>aria-busy</c>) when the real content arrives.
/// </para>
/// <para>
/// <see cref="Width"/> and <see cref="Height"/> accept plain lengths only (for example <c>12rem</c> or
/// <c>40%</c>); anything else throws, because the values are written into a <c>style</c> attribute.
/// The shimmer stops under <c>prefers-reduced-motion</c>.
/// </para>
/// </remarks>
public partial class CySkeleton : CyComponentBase
{
    private const int MaxLines = 20;

    /// <summary>The cascaded step-2 localiser (ADR-0002), if any. See <see cref="Label"/>.</summary>
    [CascadingParameter]
    public ICyLocalizer? Localizer { get; set; }

    /// <summary>The placeholder shape. Defaults to <see cref="CySkeletonShape.Text"/>.</summary>
    [Parameter]
    public CySkeletonShape Shape { get; set; } = CySkeletonShape.Text;

    /// <summary>
    /// Number of text bars (1 to 20) when <see cref="Shape"/> is <see cref="CySkeletonShape.Text"/>. The last of
    /// several bars is shorter, like the end of a paragraph.
    /// </summary>
    [Parameter]
    public int Lines { get; set; } = 1;

    /// <summary>
    /// Width as a plain length (for example <c>12rem</c>). Text and rectangle default to the full width,
    /// a circle to <c>3rem</c>.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Height as a plain length. A rectangle defaults to <c>8rem</c> and a circle to its width.
    /// Ignored for text, whose bar height follows the font size.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>Whether the shimmer animation runs. Defaults to true; it is always off under reduced motion.</summary>
    [Parameter]
    public bool Animated { get; set; } = true;

    /// <summary>
    /// Whether to render the visually hidden loading text. Set to false when a surrounding region announces
    /// the loading state once for the whole group.
    /// </summary>
    [Parameter]
    public bool Announce { get; set; } = true;

    /// <summary>The visually hidden loading text. Defaults to the localised spinner label ("Loading").</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-skeleton";

    private int LineCount => Math.Clamp(Lines, 1, MaxLines);

    private string LoadingText =>
        string.IsNullOrWhiteSpace(Label) ? (Localizer?.Strings.SpinnerLabel ?? "Loading") : Label;

    private string LineClass(int index) =>
        LineCount > 1 && index == LineCount - 1
            ? "cy-skeleton__line cy-skeleton__line--last"
            : "cy-skeleton__line";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass($"cy-skeleton--{Shape.ToString().ToLowerInvariant()}")
            .AddClass("cy-skeleton--animated", Animated)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override string BuildCssStyle() =>
        StyleBuilder.Empty
            .AddStyle("--cy-skeleton-width", Width)
            .AddStyle("--cy-skeleton-height", Height)
            .AddStyle(Style)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        CssLength.Require(Width, nameof(CySkeleton), nameof(Width));
        CssLength.Require(Height, nameof(CySkeleton), nameof(Height));

        if (Lines is < 1 or > MaxLines)
        {
            throw new InvalidOperationException(
                $"{nameof(CySkeleton)}.{nameof(Lines)} must be between 1 and {MaxLines}. Received '{Lines}'.");
        }
    }
}
