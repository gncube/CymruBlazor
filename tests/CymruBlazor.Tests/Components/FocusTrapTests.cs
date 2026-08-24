using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Tests.Accessibility;

/// <summary>
/// Unit tests for the <see cref="CyFocusTrap"/> component using bUnit, Moq, and Shouldly.
/// </summary>
public sealed class FocusTrapTests : BunitContext
{
    private readonly Mock<IFocusManager> _focusManagerMock = new(MockBehavior.Strict);

    public FocusTrapTests()
    {
        // 1. Register required core framework dependencies to satisfy CymruComponentBase
        Services.AddSingleton<IComponentIdGenerator, ComponentIdGenerator>();

        // 2. Setup mock behavior for the focused rendering lifecycle
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
    public void Should_Render_Child_Content()
    {
        // Act
        var cut = Render<CyFocusTrap>(p => p.AddChildContent("Hello"));

        // Assert[cite: 1]
        cut.Find("div").TextContent.ShouldBe("Hello");
    }

    [Fact]
    public void Should_Render_Css_Class()
    {
        // Act
        var cut = Render<CyFocusTrap>();

        // Assert[cite: 1]
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-focus-trap");
    }

    [Fact]
    public void Should_Render_TabIndex()
    {
        // Act
        var cut = Render<CyFocusTrap>();

        // Assert[cite: 1]
        var element = cut.Find("div");
        element.GetAttribute("tabindex").ShouldBe("-1");
    }
}
