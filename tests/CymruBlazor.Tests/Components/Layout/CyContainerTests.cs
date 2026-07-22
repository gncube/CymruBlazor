using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyContainerTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Container()
    {
        // Act
        var cut = Render<CyContainer>();

        // Assert
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-container");
        element.ClassList.ShouldContain("cy-container--lg");
    }

    [Theory]
    [InlineData(ContainerSize.Small, "cy-container--sm")]
    [InlineData(ContainerSize.Medium, "cy-container--md")]
    [InlineData(ContainerSize.Large, "cy-container--lg")]
    [InlineData(ContainerSize.ExtraLarge, "cy-container--xl")]
    [InlineData(ContainerSize.Fluid, "cy-container--fluid")]
    public void Should_Apply_Correct_Size_Class(ContainerSize size, string expectedClass)
    {
        // Act
        var cut = Render<CyContainer>(parameters => parameters.Add(p => p.Size, size));

        // Assert
        cut.Find("div").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Apply_No_Padding_Class_When_RemovePadding_Is_True()
    {
        // Act
        var cut = Render<CyContainer>(parameters => parameters.Add(p => p.RemovePadding, true));

        // Assert
        cut.Find("div").ClassList.ShouldContain("cy-container--no-padding");
    }

    [Fact]
    public void Should_Render_ChildContent()
    {
        // Act
        var cut = Render<CyContainer>(parameters => parameters
            .AddChildContent("<p>Hello CymruBlazor</p>"));

        // Assert
        cut.Markup.ShouldContain("Hello CymruBlazor");
    }
}
