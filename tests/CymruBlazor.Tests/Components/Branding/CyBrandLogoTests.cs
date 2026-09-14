using Bunit;
using CymruBlazor.Components.Branding;
using CymruBlazor.Components.Theming;
using CymruBlazor.Enums;
using CymruBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Branding;

public sealed class CyBrandLogoTests : BunitContext
{
    private readonly ThemeService _themeService;

    public CyBrandLogoTests()
    {
        _themeService = new ThemeService();
        Services.AddSingleton(_themeService);
    }

    [Fact]
    public void WhenVariantIsAutoAndThemeIsLightResolvesLightFullLogo()
    {
        _themeService.IsDark = false;

        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Auto)
            .Add(p => p.SymbolOnly, false));

        var img = cut.Find("img");
        img.GetAttribute("src").ShouldBe("images/logo-dhcw-light.svg");
        img.GetAttribute("class").ShouldContain("h-15");
    }

    [Fact]
    public void WhenSymbolOnlyIsTrueResolvesIconAsset()
    {
        _themeService.IsDark = false;

        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Auto)
            .Add(p => p.SymbolOnly, true));

        var img = cut.Find("img");
        img.GetAttribute("src").ShouldBe("images/icon-dhcw-light.svg");
        img.GetAttribute("class").ShouldContain("h-8");
    }

    [Fact]
    public void WhenVariantIsDarkResolvesDarkAssetRegardlessOfThemeService()
    {
        _themeService.IsDark = false;

        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Dark)
            .Add(p => p.SymbolOnly, false));

        var img = cut.Find("img");
        img.GetAttribute("src").ShouldBe("images/logo-dhcw-dark.svg");
    }

    [Fact]
    public void WhenThemeServiceChangesLogoReRenders()
    {
        _themeService.IsDark = false;

        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Auto)
            .Add(p => p.SymbolOnly, false));

        cut.Find("img").GetAttribute("src").ShouldBe("images/logo-dhcw-light.svg");

        _themeService.IsDark = true;
        _themeService.NotifyChanged();

        cut.Find("img").GetAttribute("src").ShouldBe("images/logo-dhcw-dark.svg");
    }
}
