using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Button;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Button;

public sealed class CyButtonTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_Button_By_Default()
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .AddChildContent("Save changes"));

        // Assert
        var element = cut.Find("button.cy-button");
        element.ClassList.ShouldContain("cy-button--primary");
        element.ClassList.ShouldContain("cy-button--medium");
        element.TextContent.ShouldContain("Save changes");
    }

    [Fact]
    public void Should_Render_As_Anchor_When_Href_Is_Set()
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Href, "/getting-started")
            .AddChildContent("Get started"));

        // Assert
        var element = cut.Find("a.cy-button");
        element.GetAttribute("href").ShouldBe("/getting-started");
    }

    [Fact]
    public void Should_Render_As_Button_When_Href_Is_Set_But_Disabled()
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Href, "/getting-started")
            .Add(p => p.Disabled, true)
            .AddChildContent("Get started"));

        // Assert
        cut.FindAll("a").Count.ShouldBe(0);
        cut.Find("button.cy-button").HasAttribute("disabled").ShouldBeTrue();
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-button--primary")]
    [InlineData(ComponentColour.Secondary, "cy-button--secondary")]
    [InlineData(ComponentColour.Tertiary, "cy-button--tertiary")]
    [InlineData(ComponentColour.Danger, "cy-button--danger")]
    public void Should_Apply_Variant_Css_Class(ComponentColour variant, string expectedClass)
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Button"));

        // Assert
        cut.Find("button.cy-button").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Reject_Unsupported_Variant()
    {
        // Act
        var act = () => Render<CyButton>(parameters => parameters
            .Add(p => p.Variant, ComponentColour.Info)
            .AddChildContent("Button"));

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ComponentSize.Small, "cy-button--small")]
    [InlineData(ComponentSize.Medium, "cy-button--medium")]
    [InlineData(ComponentSize.Large, "cy-button--large")]
    public void Should_Apply_Size_Css_Class(ComponentSize size, string expectedClass)
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Size, size)
            .AddChildContent("Button"));

        // Assert
        cut.Find("button.cy-button").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Disable_Button_And_Prevent_OnClick_When_Disabled()
    {
        // Arrange
        var clicked = false;

        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, () => clicked = true)
            .AddChildContent("Button"));

        // Act
        cut.Find("button").Click();

        // Assert
        cut.Find("button").HasAttribute("disabled").ShouldBeTrue();
        clicked.ShouldBeFalse();
    }

    [Fact]
    public void Should_Show_Spinner_And_Prevent_OnClick_When_Loading()
    {
        // Arrange
        var clicked = false;

        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Loading, true)
            .Add(p => p.OnClick, () => clicked = true)
            .AddChildContent("Button"));

        // Act
        cut.Find("button").Click();

        // Assert
        cut.Find(".cy-button__spinner").ShouldNotBeNull();
        cut.Find("button").HasAttribute("disabled").ShouldBeTrue();
        clicked.ShouldBeFalse();
    }

    [Fact]
    public void Should_Invoke_OnClick_When_Enabled()
    {
        // Arrange
        var clicked = false;

        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.OnClick, () => clicked = true)
            .AddChildContent("Button"));

        // Act
        cut.Find("button").Click();

        // Assert
        clicked.ShouldBeTrue();
    }
}
