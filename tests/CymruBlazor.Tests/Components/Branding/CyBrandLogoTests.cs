using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Branding;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Branding;

public sealed class CyBrandLogoTests : TestContextBase
{
    [Fact]
    public void Should_Render_Mark_And_Wordmark_By_Default()
    {
        // Act
        var cut = Render<CyBrandLogo>();

        // Assert
        cut.FindAll("svg.cy-brand-logo__mark").Count.ShouldBe(1);
        cut.Find("span.cy-brand-logo__wordmark").TextContent.ShouldBe("CymruBlazor");
    }

    [Fact]
    public void Should_Render_As_Span_When_Href_Is_Not_Set()
    {
        // Act
        var cut = Render<CyBrandLogo>();

        // Assert
        cut.Find("*").TagName.ShouldBe("SPAN");
    }

    [Fact]
    public void Should_Render_As_Link_When_Href_Is_Set()
    {
        // Act
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Href, "/"));

        // Assert
        var anchor = cut.Find("a");
        anchor.GetAttribute("href").ShouldBe("/");
    }

    [Theory]
    [InlineData(BrandLogoVariant.Mark, true, false)]
    [InlineData(BrandLogoVariant.Wordmark, false, true)]
    [InlineData(BrandLogoVariant.Full, true, true)]
    public void Should_Render_Only_The_Requested_Variant_Parts(
        BrandLogoVariant variant,
        bool expectMark,
        bool expectWordmark)
    {
        // Act
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Assert
        cut.FindAll("svg.cy-brand-logo__mark").Count.ShouldBe(expectMark ? 1 : 0);
        cut.FindAll("span.cy-brand-logo__wordmark").Count.ShouldBe(expectWordmark ? 1 : 0);
    }

    [Fact]
    public void Should_Expose_Accessible_Label_For_Mark_Only_Variant()
    {
        // Act
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Variant, BrandLogoVariant.Mark)
            .Add(p => p.Text, "DHCW"));

        // Assert
        cut.Find("*").GetAttribute("aria-label").ShouldBe("DHCW");
    }

    [Fact]
    public void Should_Apply_Size_Modifier_Class()
    {
        // Act
        var cut = Render<CyBrandLogo>(parameters => parameters
            .Add(p => p.Size, ComponentSize.Large));

        // Assert
        cut.Find("*").ClassList.ShouldContain("cy-brand-logo--large");
    }
}
