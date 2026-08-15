using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyHeroBannerTests : TestContextBase
{
    [Fact]
    public void Should_Render_Title_As_H1()
    {
        // Act
        var cut = Render<CyHeroBanner>(parameters => parameters
            .Add(p => p.Title, "CymruBlazor"));

        // Assert
        cut.Find("h1").TextContent.ShouldContain("CymruBlazor");
    }

    [Theory]
    [InlineData(HeroBackground.Primary, true)]
    [InlineData(HeroBackground.Accent, true)]
    [InlineData(HeroBackground.Plain, false)]
    public void Should_Apply_Inverse_Class_Only_For_Dark_Backgrounds(HeroBackground background, bool expectInverse)
    {
        // Act
        var cut = Render<CyHeroBanner>(parameters => parameters
            .Add(p => p.Title, "CymruBlazor")
            .Add(p => p.Background, background));

        // Assert
        var hasInverse = cut.Find("section").ClassList.Contains("cy-hero-banner--inverse");
        hasInverse.ShouldBe(expectInverse);
    }

    [Fact]
    public void Should_Render_ChildContent_Actions_When_Provided()
    {
        // Act
        var cut = Render<CyHeroBanner>(parameters => parameters
            .Add(p => p.Title, "CymruBlazor")
            .AddChildContent("<button>Get started</button>"));

        // Assert
        cut.Find(".cy-hero-banner__actions button").TextContent.ShouldContain("Get started");
    }
}
