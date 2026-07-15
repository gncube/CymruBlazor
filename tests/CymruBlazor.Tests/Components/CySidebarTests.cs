using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components;

public class CySidebarTests : BunitContext
{
    [Fact]
    public void Sidebar_Should_Render_With_Default_Parameters()
    {
        // Act - Uses unified Bunit v2 Render<T>
        var cut = Render<CySidebar>(parameters => parameters
            .AddChildContent("Sidebar Content"));

        // Assert - Tag check is performed safely
        var element = cut.Find("aside");
        element.ShouldNotBeNull();
        element.InnerHtml.Trim().ShouldBe("Sidebar Content");

        var className = element.GetAttribute("class") ?? string.Empty;
        className.ShouldContain("cy-sidebar");
        className.ShouldContain("cy-sidebar--left");
        className.ShouldContain("cy-sidebar--md");
        className.ShouldNotContain("cy-sidebar--collapsed");
    }

    [Theory]
    [InlineData(SidebarPosition.Right, "cy-sidebar--right")]
    [InlineData(SidebarPosition.Left, "cy-sidebar--left")]
    public void Sidebar_Should_Apply_Correct_Position_Classes(SidebarPosition position, string expectedClass)
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Position, position));

        // Assert
        var classList = cut.Find("aside").GetAttribute("class") ?? string.Empty;
        classList.ShouldContain(expectedClass);
    }

    [Fact]
    public async Task ToggleAsync_Should_Update_State_And_Notify_Parent()
    {
        // Arrange
        var collapsedStateChanged = false;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Collapsed, false)
            .Add(p => p.CollapsedChanged, (bool val) => collapsedStateChanged = val));

        // Act - Accessing and invoking component state
        await cut.InvokeAsync(() => cut.Instance.ToggleAsync());

        // Assert
        cut.Instance.Collapsed.ShouldBeTrue();
        collapsedStateChanged.ShouldBeTrue();

        var classList = cut.Find("aside").GetAttribute("class") ?? string.Empty;
        classList.ShouldContain("cy-sidebar--collapsed");
    }
}
