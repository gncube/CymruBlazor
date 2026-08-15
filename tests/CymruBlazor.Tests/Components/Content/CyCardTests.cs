using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyCardTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_Div_When_Href_Is_Not_Set()
    {
        // Act
        var cut = Render<CyCard>(parameters => parameters
            .AddChildContent("<p>Body</p>"));

        // Assert
        cut.Nodes[0].NodeName.ToLowerInvariant().ShouldBe("div");
        cut.Find("div.cy-card").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Render_As_Anchor_When_Href_Is_Set()
    {
        // Act
        var cut = Render<CyCard>(parameters => parameters
            .Add(p => p.Href, "/components/cards")
            .AddChildContent("<p>Body</p>"));

        // Assert
        var element = cut.Find("a.cy-card");
        element.GetAttribute("href").ShouldBe("/components/cards");
        element.ClassList.ShouldContain("cy-card--interactive");
    }

    [Fact]
    public void Should_Render_Header_And_Footer_When_Provided()
    {
        // Act
        var cut = Render<CyCard>(parameters => parameters
            .Add(p => p.Header, "<h2>Title</h2>")
            .Add(p => p.Footer, "<span>Footer</span>")
            .AddChildContent("<p>Body</p>"));

        // Assert
        cut.Find(".cy-card__header").TextContent.ShouldContain("Title");
        cut.Find(".cy-card__body").TextContent.ShouldContain("Body");
        cut.Find(".cy-card__footer").TextContent.ShouldContain("Footer");
    }

    [Fact]
    public void Should_Not_Render_Header_Or_Footer_When_Not_Provided()
    {
        // Act
        var cut = Render<CyCard>(parameters => parameters
            .AddChildContent("<p>Body</p>"));

        // Assert
        cut.FindAll(".cy-card__header").Count.ShouldBe(0);
        cut.FindAll(".cy-card__footer").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(ComponentElevation.None, "cy-card--elevation-none")]
    [InlineData(ComponentElevation.Medium, "cy-card--elevation-medium")]
    [InlineData(ComponentElevation.Large, "cy-card--elevation-large")]
    public void Should_Apply_Correct_Elevation_Class(ComponentElevation elevation, string expectedClass)
    {
        // Act
        var cut = Render<CyCard>(parameters => parameters
            .Add(p => p.Elevation, elevation)
            .AddChildContent("<p>Body</p>"));

        // Assert
        cut.Find(".cy-card").ClassList.ShouldContain(expectedClass);
    }
}
