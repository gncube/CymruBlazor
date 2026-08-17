using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A prominent page-top banner: title, optional subtitle, and optional
/// call-to-action content.
///
/// On <see cref="HeroBackground.Primary"/>/<see cref="HeroBackground.Accent"/>
/// (both dark backgrounds), the root element gets a
/// "cy-hero-banner--inverse" class which rescopes
/// <c>--cymru-color-text</c>/<c>--cymru-color-link</c> to their inverse
/// (white/near-white) equivalents for descendant content - see
/// components/navigation.css. This exists specifically so content placed
/// in <c>ChildContent</c> (buttons, links) is legible by default, rather
/// than requiring every consumer to hand-write a background-specific
/// override the way the Demo app's hero originally had to.
/// </summary>
public partial class CyHeroBanner : CyLayoutComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public HeroBackground Background { get; set; } = HeroBackground.Primary;

    protected override string BaseCssClass => "cy-hero-banner";

    protected override string BuildCssClass()
    {
        var backgroundSuffix = Background.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-hero-banner--{backgroundSuffix}")
            .AddClass("cy-hero-banner--inverse", Background is HeroBackground.Primary or HeroBackground.Accent)
            .Build();
    }
}
