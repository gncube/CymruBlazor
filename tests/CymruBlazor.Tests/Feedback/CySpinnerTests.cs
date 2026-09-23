using Bunit;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Feedback;

public sealed class CySpinnerTests : TestContextBase
{
    [Fact]
    public void Should_Render_With_Status_Role_And_Default_Label()
    {
        // Act
        var cut = Render<CySpinner>();

        // Assert
        var element = cut.Find("span.cy-spinner");
        element.GetAttribute("role").ShouldBe("status");
        element.ClassList.ShouldContain("cy-spinner--medium");
        element.ClassList.ShouldContain("cy-spinner--primary");
        cut.Find(".u-sr-only").TextContent.ShouldBe("Loading");
    }

    [Fact]
    public void Should_Use_Custom_Label_As_Sr_Only_Text_By_Default()
    {
        // Act
        var cut = Render<CySpinner>(parameters => parameters
            .Add(p => p.Label, "Loading ward occupancy"));

        // Assert
        cut.Find(".u-sr-only").TextContent.ShouldBe("Loading ward occupancy");
        cut.FindAll(".cy-spinner__label").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Show_Label_Visibly_When_ShowLabel_Is_True()
    {
        // Act
        var cut = Render<CySpinner>(parameters => parameters
            .Add(p => p.Label, "Loading results")
            .Add(p => p.ShowLabel, true));

        // Assert
        cut.Find(".cy-spinner__label").TextContent.ShouldBe("Loading results");
        cut.FindAll(".u-sr-only").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(ComponentSize.ExtraSmall, "cy-spinner--extrasmall")]
    [InlineData(ComponentSize.Small, "cy-spinner--small")]
    [InlineData(ComponentSize.Medium, "cy-spinner--medium")]
    [InlineData(ComponentSize.Large, "cy-spinner--large")]
    [InlineData(ComponentSize.ExtraLarge, "cy-spinner--extralarge")]
    public void Should_Apply_Size_Css_Class(ComponentSize size, string expectedClass)
    {
        // Act
        var cut = Render<CySpinner>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        cut.Find("span.cy-spinner").ClassList.ShouldContain(expectedClass);
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-spinner--primary")]
    [InlineData(ComponentColour.Danger, "cy-spinner--danger")]
    [InlineData(ComponentColour.Neutral, "cy-spinner--neutral")]
    public void Should_Apply_Colour_Css_Class(ComponentColour colour, string expectedClass)
    {
        // Act
        var cut = Render<CySpinner>(parameters => parameters
            .Add(p => p.Colour, colour));

        // Assert
        cut.Find("span.cy-spinner").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Throw_When_Size_Is_Unspecified()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CySpinner>(parameters => parameters
                .Add(p => p.Size, ComponentSize.Unspecified)));
    }

    [Fact]
    public void Should_Throw_When_Colour_Is_Unspecified()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CySpinner>(parameters => parameters
                .Add(p => p.Colour, ComponentColour.Unspecified)));
    }
}
