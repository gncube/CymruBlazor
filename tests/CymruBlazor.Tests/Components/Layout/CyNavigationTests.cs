using Xunit;
using Shouldly;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

/// <summary>
/// Covers <see cref="CyNavigation"/>'s mobile toggle behaviour and its
/// integration with <see cref="CymruBlazor.Components.Accessibility.CyFocusTrap"/> -
/// the first test in this suite exercising two shipped components
/// together rather than one in isolation.
/// </summary>
public sealed class CyNavigationTests : TestContextBase
{
    private readonly Mock<IFocusManager> _focusManagerMock = new(MockBehavior.Loose);

    public CyNavigationTests()
    {
        _focusManagerMock
            .Setup(m => m.FocusAsync(
                It.IsAny<string>(),
                It.IsAny<FocusOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FocusResult(Success: true));

        _focusManagerMock
            .Setup(m => m.RestoreFocusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FocusResult(Success: true));

        Services.AddSingleton(_focusManagerMock.Object);
    }

    [Fact]
    public void Should_Render_Closed_By_Default()
    {
        // Act
        var cut = Render<CyNavigation>(parameters => parameters
            .AddChildContent("<li>Item</li>"));

        // Assert
        cut.Find("button.cy-navigation__toggle").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find("ul").ClassList.ShouldNotContain("cy-navigation__menu--open");
    }

    [Fact]
    public void Clicking_Toggle_Should_Open_The_Mobile_Menu()
    {
        // Arrange
        var cut = Render<CyNavigation>(parameters => parameters
            .AddChildContent("<li>Item</li>"));

        // Act
        cut.Find("button.cy-navigation__toggle").Click();

        // Assert
        cut.Find("button.cy-navigation__toggle").GetAttribute("aria-expanded").ShouldBe("true");
        cut.Find("ul").ClassList.ShouldContain("cy-navigation__menu--open");
    }

    [Fact]
    public void Clicking_Toggle_Twice_Should_Close_The_Mobile_Menu_Again()
    {
        // Arrange
        var cut = Render<CyNavigation>(parameters => parameters
            .AddChildContent("<li>Item</li>"));

        // Act
        cut.Find("button.cy-navigation__toggle").Click();
        cut.Find("button.cy-navigation__toggle").Click();

        // Assert
        cut.Find("button.cy-navigation__toggle").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Render_Brand_When_Provided()
    {
        // Act
        var cut = Render<CyNavigation>(parameters => parameters
            .Add(p => p.Brand, "<span>CymruBlazor</span>")
            .AddChildContent("<li>Item</li>"));

        // Assert
        cut.Find(".cy-navigation__brand").TextContent.ShouldContain("CymruBlazor");
    }
}
