using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components;

public sealed class CyCenterTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Center()
    {
        // Act
        var cut = Render<CyCenter>();

        // Assert
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-center");
    }

    [Fact]
    public void Should_Enable_FullHeight()
    {
        // Act
        var cut = Render<CyCenter>(parameters => parameters.Add(p => p.FullHeight, true));

        // Assert
        cut.Find("div").ClassList.ShouldContain("cy-center--full-height");
    }
}
