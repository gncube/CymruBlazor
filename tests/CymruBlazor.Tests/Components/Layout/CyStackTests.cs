using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyStackTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Stack()
    {
        // Act
        var cut = Render<CyStack>();

        // Assert - Orientation defaults to Horizontal (enum value 0), which
        // does not add a --vertical modifier class.
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-stack");
        element.ClassList.ShouldContain("cy-gap-medium");
    }

    [Theory]
    [InlineData(Orientation.Horizontal, "cy-stack--horizontal")]
    [InlineData(Orientation.Vertical, "cy-stack--vertical")]
    public void Should_Apply_Correct_Orientation_Class(Orientation orientation, string expectedClass)
    {
        // Act
        var cut = Render<CyStack>(parameters => parameters.Add(p => p.Orientation, orientation));

        // Assert
        cut.Find("div").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Apply_Wrap_Class_When_Wrap_Is_True()
    {
        // Act
        var cut = Render<CyStack>(parameters => parameters.Add(p => p.Wrap, true));

        // Assert
        cut.Find("div").ClassList.ShouldContain("cy-stack--wrap");
    }

    [Theory]
    [InlineData(ComponentSize.Small, "cy-gap-small")]
    [InlineData(ComponentSize.Large, "cy-gap-large")]
    public void Should_Apply_Correct_Gap_Class(ComponentSize gap, string expectedClass)
    {
        // Act
        var cut = Render<CyStack>(parameters => parameters.Add(p => p.Gap, gap));

        // Assert
        cut.Find("div").ClassList.ShouldContain(expectedClass);
    }
}
