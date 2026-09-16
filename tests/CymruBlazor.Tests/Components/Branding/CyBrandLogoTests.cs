using Bunit;
using CymruBlazor.Components.Branding;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Branding;

/// <summary>
/// CyBrandLogo's theme handling is CSS-only (see branding.css,
/// "[data-theme]" selectors) rather than driven by <c>IThemeService</c>:
/// both the light and dark assets render into the DOM together, tagged
/// "cy-brand-logo__asset--light"/"--dark", and it's the ambient
/// "data-theme" attribute set by the theme provider that decides which
/// one is actually visible. This keeps the logo correct even before
/// Blazor/JS interop has finished initialising. These tests assert
/// against that real contract.
/// </summary>
public sealed class CyBrandLogoTests : TestContextBase
{
    [Fact]
    public void WhenNoLogoPathIsSuppliedRendersDefaultLightAndDarkAssetPair()
    {
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Auto)
            .Add(p => p.SymbolOnly, false));

        var lightImg = cut.Find("img.cy-brand-logo__asset--light");
        lightImg.GetAttribute("src").ShouldBe("images/logo-dhcw-light.svg");
        lightImg.GetAttribute("class")!.ShouldContain("h-15");

        var darkImg = cut.Find("img.cy-brand-logo__asset--dark");
        darkImg.GetAttribute("src").ShouldBe("images/logo-dhcw-dark.svg");
        darkImg.GetAttribute("class")!.ShouldContain("h-15");
    }

    [Fact]
    public void WhenSymbolOnlyIsTrueResolvesIconAssetPair()
    {
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Auto)
            .Add(p => p.SymbolOnly, true));

        var lightImg = cut.Find("img.cy-brand-logo__asset--light");
        lightImg.GetAttribute("src").ShouldBe("images/icon-dhcw-light.svg");
        lightImg.GetAttribute("class")!.ShouldContain("h-8");

        var darkImg = cut.Find("img.cy-brand-logo__asset--dark");
        darkImg.GetAttribute("src").ShouldBe("images/icon-dhcw-dark.svg");
        darkImg.GetAttribute("class")!.ShouldContain("h-8");
    }

    [Fact]
    public void WhenLogoPathIsSuppliedWithoutDarkLogoPathOnlyOneAssetRenders()
    {
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.LogoPath, "/images/logo-light.svg"));

        var img = cut.Find("img.cy-brand-logo__asset--light");
        img.GetAttribute("src").ShouldBe("/images/logo-light.svg");

        // No DarkLogoPath supplied and Variant isn't a themed one, so
        // EffectiveDarkLogoPath is null - no dark asset is rendered at
        // all, meaning the light asset stays visible under [data-theme]
        // dark automatically (see branding.css comments), without a
        // second image ever having existed in the DOM to hide.
        cut.FindAll("img.cy-brand-logo__asset--dark").Count.ShouldBe(0);
    }

    [Fact]
    public void WhenLogoPathAndDarkLogoPathAreBothSuppliedBothAssetsRender()
    {
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.LogoPath, "/images/logo-light.svg")
            .Add(p => p.DarkLogoPath, "/images/logo-dark.svg"));

        cut.Find("img.cy-brand-logo__asset--light").GetAttribute("src")
            .ShouldBe("/images/logo-light.svg");

        cut.Find("img.cy-brand-logo__asset--dark").GetAttribute("src")
            .ShouldBe("/images/logo-dark.svg");
    }

    [Fact]
    public void WhenNoLogoPathAndVariantIsFullRendersBuiltInLockupInstead()
    {
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Full)
            .Add(p => p.Text, "CymruBlazor"));

        // Full/Mark/Wordmark/Stacked are the built-in SVG lockup
        // variants, not image-asset theme modes - with no LogoPath
        // supplied, no <img> renders at all.
        cut.FindAll("img").Count.ShouldBe(0);
        cut.Find(".cy-brand-logo__wordmark").TextContent.ShouldBe("CymruBlazor");
    }
}
