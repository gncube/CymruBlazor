using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyGridTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Grid()
    {
        // Act
        var cut = Render<CyGrid>();

        // Assert
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-grid");
        element.ClassList.ShouldContain("cy-grid--cols-auto");
        element.ClassList.ShouldContain("cy-gap-medium");
    }

    [Theory]
    [InlineData(GridColumns.One, "cy-grid--cols-1")]
    [InlineData(GridColumns.Two, "cy-grid--cols-2")]
    [InlineData(GridColumns.Twelve, "cy-grid--cols-12")]
    public void Should_Apply_Correct_Column_Class(GridColumns columns, string expectedClass)
    {
        // Act
        var cut = Render<CyGrid>(parameters => parameters.Add(p => p.Columns, columns));

        // Assert
        cut.Find("div").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Apply_Dense_And_AutoRows_Classes()
    {
        // Act
        var cut = Render<CyGrid>(parameters => parameters
            .Add(p => p.Dense, true)
            .Add(p => p.AutoRows, true));

        // Assert
        var classList = cut.Find("div").ClassList;
        classList.ShouldContain("cy-grid--dense");
        classList.ShouldContain("cy-grid--auto-rows");
    }
}
