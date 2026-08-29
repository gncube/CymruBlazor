using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyHeaderTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_Header_Element()
    {
        // Act
        var cut = Render<CyHeader>();

        // Assert
        cut.Find("header.cy-header").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Default_To_Primary_Background()
    {
        // Act
        var cut = Render<CyHeader>();

        // Assert
        cut.Find(".cy-header").ClassList.ShouldContain("cy-header--primary");
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-header--primary")]
    [InlineData(ComponentColour.Secondary, "cy-header--secondary")]
    [InlineData(ComponentColour.Surface, "cy-header--surface")]
    [InlineData(ComponentColour.Neutral, "cy-header--neutral")]
    public void Should_Apply_Background_Css_Class(ComponentColour background, string expectedClass)
    {
        // Act
        var cut = Render<CyHeader>(parameters => parameters
            .Add(p => p.Background, background));

        // Assert
        cut.Find(".cy-header").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Reject_Unsupported_Background()
    {
        // Act
        var act = () => Render<CyHeader>(parameters => parameters
            .Add(p => p.Background, ComponentColour.Danger));

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Should_Apply_Sticky_Class_When_Sticky_True()
    {
        // Act
        var cut = Render<CyHeader>(parameters => parameters
            .Add(p => p.Sticky, true));

        // Assert
        cut.Find(".cy-header").ClassList.ShouldContain("cy-header--sticky");
    }

    [Fact]
    public void Should_Not_Apply_Sticky_Class_By_Default()
    {
        // Act
        var cut = Render<CyHeader>();

        // Assert
        cut.Find(".cy-header").ClassList.ShouldNotContain("cy-header--sticky");
    }

    [Fact]
    public void Should_Render_Brand_When_Provided()
    {
        // Act
        var cut = Render<CyHeader>(parameters => parameters
            .Add(p => p.Brand, "<span>My Brand</span>"));

        // Assert
        cut.Find(".cy-header__brand").TextContent.ShouldContain("My Brand");
    }

    [Fact]
    public void Should_Not_Render_Brand_Wrapper_When_Not_Provided()
    {
        // Act
        var cut = Render<CyHeader>();

        // Assert
        cut.FindAll(".cy-header__brand").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_ChildContent_In_Content_Wrapper()
    {
        // Act
        var cut = Render<CyHeader>(parameters => parameters
            .AddChildContent("<a href=\"/\">Home</a>"));

        // Assert
        cut.Find(".cy-header__content a").TextContent.ShouldContain("Home");
    }

    [Fact]
    public void Should_Render_Actions_When_Provided()
    {
        // Act
        var cut = Render<CyHeader>(parameters => parameters
            .Add(p => p.Actions, "<button>Search</button>"));

        // Assert
        cut.Find(".cy-header__actions button").TextContent.ShouldContain("Search");
    }

    [Fact]
    public void Should_Not_Render_Actions_Wrapper_When_Not_Provided()
    {
        // Act
        var cut = Render<CyHeader>();

        // Assert
        cut.FindAll(".cy-header__actions").Count.ShouldBe(0);
    }
}
