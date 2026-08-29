using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Site header - the page-level chrome bar (brand, primary content, and
/// trailing actions), following the same navy NHS Wales/DHCW header+footer
/// colour pairing as <see cref="CyFooter"/> by default. This is
/// deliberately a plain layout bar, not a navigation widget: put a
/// <see cref="CyNavigation"/> (which handles its own mobile collapse) or
/// plain links in <see cref="CyLayoutComponentBase.ChildContent"/> when
/// you need collapsible nav - CyHeader itself only arranges Brand,
/// ChildContent, and Actions into a row and applies the background/sticky
/// treatment.
/// </summary>
public partial class CyHeader : CyLayoutComponentBase
{
    /// <summary>
    /// Optional brand/logo content, rendered first (typically a
    /// <see cref="CyBrandLogo"/> or a <see cref="CyNavigation"/>'s own
    /// Brand, if you're composing the two).
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    /// <summary>
    /// Optional trailing content, rendered last and pushed to the end of
    /// the bar (typically icon buttons - search, theme toggle, account
    /// menu).
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Controls the header's background colour. Must be
    /// <see cref="ComponentColour.Primary"/> (the default - matches
    /// <see cref="CyFooter"/>'s default),
    /// <see cref="ComponentColour.Secondary"/>,
    /// <see cref="ComponentColour.Surface"/>, or
    /// <see cref="ComponentColour.Neutral"/>. Surface and Neutral render
    /// dark text instead of the light text the navy/secondary
    /// backgrounds need.
    /// </summary>
    [Parameter]
    public ComponentColour Background { get; set; } = ComponentColour.Primary;

    /// <summary>
    /// When <see langword="true"/>, the header sticks to the top of the
    /// viewport while the page scrolls (<c>position: sticky; top: 0</c>).
    /// </summary>
    [Parameter]
    public bool Sticky { get; set; }

    protected override string BaseCssClass => "cy-header";

    protected override string BuildCssClass()
    {
        var backgroundSuffix = Background.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-header--{backgroundSuffix}")
            .AddClass("cy-header--sticky", Sticky)
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Background is not (ComponentColour.Primary
            or ComponentColour.Secondary
            or ComponentColour.Surface
            or ComponentColour.Neutral))
        {
            throw new InvalidOperationException(
                $"{nameof(CyHeader)}.{nameof(Background)} must be Primary, Secondary, " +
                $"Surface, or Neutral. Received '{Background}'.");
        }
    }
}
