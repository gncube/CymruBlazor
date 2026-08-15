using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components;

public sealed class CySidebarTests : TestContextBase
{
    [Fact]
    public void Sidebar_Should_Render_With_Default_Parameters()
    {
        // Act
        var cut = Render<CySidebar>();

        // Assert - Fallback to a root wildcard selector if the markup tag varies
        var rootElement = cut.Find("*");
        rootElement.ClassList.ShouldContain("cy-sidebar");
        rootElement.ClassList.ShouldContain("cy-sidebar--left");
    }

    [Theory]
    [InlineData(SidebarPosition.Left, "cy-sidebar--left")]
    [InlineData(SidebarPosition.Right, "cy-sidebar--right")]
    public void Sidebar_Should_Apply_Correct_Position_Classes(SidebarPosition position, string expectedClass)
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters.Add(p => p.Position, position));

        // Assert
        cut.Find("*").ClassList.ShouldContain(expectedClass);
    }
}
