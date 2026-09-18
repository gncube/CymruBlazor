using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Tests.Components.Layout;

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

    [Fact]
    public void Sidebar_Should_Default_To_Compact_CollapseMode()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters.Add(p => p.Collapsed, true));

        // Assert
        cut.Find("*").ClassList.ShouldContain("cy-sidebar--compact");
        cut.Find("*").GetAttribute("data-collapse-mode").ShouldBe("compact");
    }

    [Fact]
    public void Sidebar_Should_Apply_Compact_Class_When_CollapseMode_Is_Compact()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Compact)
            .Add(p => p.Collapsed, true));

        // Assert
        var classes = cut.Find("*").ClassList;
        classes.ShouldContain("cy-sidebar--compact");
        classes.ShouldNotContain("cy-sidebar--icon-only");
        classes.ShouldNotContain("cy-sidebar--collapsed");
    }

    [Fact]
    public void Sidebar_Should_Apply_IconOnly_Class_When_CollapseMode_Is_IconOnly()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.IconOnly)
            .Add(p => p.Collapsed, true));

        // Assert
        var classes = cut.Find("*").ClassList;
        classes.ShouldContain("cy-sidebar--icon-only");
        classes.ShouldNotContain("cy-sidebar--compact");
    }

    [Fact]
    public void Sidebar_Should_Apply_Collapsed_Class_When_CollapseMode_Is_Hidden()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Hidden)
            .Add(p => p.Collapsed, true));

        // Assert - the original "completely hidden" (width: 0) behaviour
        var classes = cut.Find("*").ClassList;
        classes.ShouldContain("cy-sidebar--collapsed");
        classes.ShouldNotContain("cy-sidebar--compact");
        classes.ShouldNotContain("cy-sidebar--icon-only");
    }

    [Fact]
    public void Sidebar_Should_Never_Render_Collapsed_When_CollapseMode_Is_Disabled()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Disabled)
            .Add(p => p.Collapsed, true));

        // Assert
        var classes = cut.Find("*").ClassList;
        classes.ShouldNotContain("cy-sidebar--collapsed");
        classes.ShouldNotContain("cy-sidebar--compact");
        classes.ShouldNotContain("cy-sidebar--icon-only");
    }

    [Fact]
    public async Task ToggleAsync_Should_Be_NoOp_When_CollapseMode_Is_Disabled()
    {
        // Arrange
        var eventFired = false;

        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Disabled)
            .Add(p => p.Collapsed, false)
            .Add(p => p.CollapsedChanged, _ => eventFired = true));

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.ToggleAsync());

        // Assert
        cut.Instance.Collapsed.ShouldBeFalse();
        eventFired.ShouldBeFalse();
    }

    [Fact]
    public async Task ToggleAsync_Should_Flip_Collapsed_When_CollapseMode_Is_Compact()
    {
        // Arrange
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Compact)
            .Add(p => p.Collapsed, false));

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.ToggleAsync());

        // Assert
        cut.Instance.Collapsed.ShouldBeTrue();
    }

    [Fact]
    public void Should_Render_Brand_When_Expanded()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Collapsed, false)
            .Add(p => p.Brand, (RenderFragment)(builder => builder.AddContent(0, "Logo")))
            .Add(p => p.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Nav"))));

        // Assert
        cut.Find(".cy-sidebar__brand").TextContent.ShouldBe("Logo");
    }

    [Theory]
    [InlineData(SidebarCollapseMode.Compact)]
    [InlineData(SidebarCollapseMode.IconOnly)]
    [InlineData(SidebarCollapseMode.Hidden)]
    public void Should_Still_Render_Brand_When_Collapsed(SidebarCollapseMode mode)
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, mode)
            .Add(p => p.Collapsed, true)
            .Add(p => p.Brand, (RenderFragment)(builder => builder.AddContent(0, "Logo"))));

        // Assert
        cut.Find(".cy-sidebar__brand").TextContent.ShouldBe("Logo");
    }

    [Fact]
    public void Should_Render_Toggle_Button_Alongside_Brand_When_Expanded()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Collapsed, false)
            .Add(p => p.Brand, (RenderFragment)(builder => builder.AddContent(0, "Logo"))));

        // Assert
        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(1);
        cut.Find(".cy-sidebar__header").TextContent.ShouldContain("Logo");
    }

    [Theory]
    [InlineData(SidebarCollapseMode.Compact)]
    [InlineData(SidebarCollapseMode.IconOnly)]
    [InlineData(SidebarCollapseMode.Hidden)]
    public void Should_Still_Render_Toggle_Button_When_Collapsed(SidebarCollapseMode mode)
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, mode)
            .Add(p => p.Collapsed, true));

        // Assert
        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__brand").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Not_Render_Toggle_Button_When_CollapseMode_Is_Disabled_And_No_Brand()
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.CollapseMode, SidebarCollapseMode.Disabled)
            .Add(p => p.Collapsed, false));

        // Assert
        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(0);
        cut.FindAll(".cy-sidebar__header").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(SidebarPosition.Left, false, "m15 18-6-6 6-6")]
    [InlineData(SidebarPosition.Left, true, "m9 18 6-6-6-6")]
    [InlineData(SidebarPosition.Right, false, "m9 18 6-6-6-6")]
    [InlineData(SidebarPosition.Right, true, "m15 18-6-6 6-6")]
    public void Toggle_Button_Icon_Points_Towards_The_Expand_Direction(
        SidebarPosition position, bool collapsed, string expectedPathData)
    {
        // Act
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Position, position)
            .Add(p => p.Collapsed, collapsed));

        cut.Find(".cy-sidebar__toggle svg path").GetAttribute("d")
            .ShouldBe(expectedPathData);
    }

    [Fact]
    public async Task Clicking_Toggle_Button_Should_Flip_Collapsed()
    {
        // Arrange
        var collapsedValue = false;

        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Collapsed, false)
            .Add(p => p.CollapsedChanged, value => collapsedValue = value));

        // Act
        await cut.Find(".cy-sidebar__toggle").ClickAsync(new());

        // Assert
        collapsedValue.ShouldBeTrue();
    }

    [Fact]
    public async Task CycleNextAsyncShouldCycleAcrossConfiguredStates()
    {
        // Arrange - 4-state cycling (TEST-001)
        var observedStates = new List<SidebarState>();
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Expanded)
            .Add(p => p.StateChanged, s => observedStates.Add(s)));

        // Act & Assert cycle 1: Expanded -> Compact
        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Compact);

        // Act & Assert cycle 2: Compact -> IconOnly
        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.IconOnly);

        // Act & Assert cycle 3: IconOnly -> Hidden
        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Hidden);

        // Act & Assert cycle 4: Hidden -> Expanded (wrap around)
        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Expanded);

        observedStates.ShouldBe([SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden, SidebarState.Expanded]);
    }

    [Fact]
    public async Task ExpandAsyncShouldResetStateToPrimaryState()
    {
        // Arrange - (TEST-004)
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Hidden));

        // Act
        await cut.InvokeAsync(async () => await cut.Instance.ExpandAsync());

        // Assert
        cut.Instance.State.ShouldBe(SidebarState.Expanded);
        cut.Instance.EffectiveCollapsed.ShouldBeFalse();
    }

    [Fact]
    public async Task SetStateAsyncShouldAssignValidStateOnly()
    {
        // Arrange
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact])
            .Add(p => p.State, SidebarState.Expanded));

        // Act - Attempt invalid state not in States
        await cut.InvokeAsync(async () => await cut.Instance.SetStateAsync(SidebarState.Hidden));
        cut.Instance.State.ShouldBe(SidebarState.Expanded);

        // Act - Attempt valid state
        await cut.InvokeAsync(async () => await cut.Instance.SetStateAsync(SidebarState.Compact));
        cut.Instance.State.ShouldBe(SidebarState.Compact);
    }

    [Fact]
    public void HiddenStateShouldRenderRevealHandleOutsideClippedLayout()
    {
        // Arrange - (TEST-003)
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.State, SidebarState.Hidden));

        // Assert
        var revealHandle = cut.Find(".cy-sidebar__reveal-handle");
        revealHandle.ShouldNotBeNull();
        revealHandle.GetAttribute("aria-label").ShouldBe("Reveal sidebar");

        var header = cut.Find(".cy-sidebar__header");
        header.GetAttribute("aria-hidden").ShouldBe("true");
        header.GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public async Task RevealHandleClickShouldInvokeExpandAsync()
    {
        // Arrange - (TEST-004)
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Hidden));

        // Act
        await cut.Find(".cy-sidebar__reveal-handle").ClickAsync(new());

        // Assert
        cut.Instance.State.ShouldBe(SidebarState.Expanded);
        cut.FindAll(".cy-sidebar__reveal-handle").Count.ShouldBe(0);
    }

    [Fact]
    public async Task MobileOpenShouldRenderBackdropAndCloseButtonExclusively()
    {
        // Arrange - (TEST-005)
        var mobileOpenState = true;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.MobileOpen, true)
            .Add(p => p.ShowMobileBackdrop, true)
            .Add(p => p.MobileOpenChanged, v => mobileOpenState = v));

        // Assert presence of mobile drawer elements
        cut.FindAll(".cy-sidebar__backdrop").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__close").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(0);

        // Act - Click mobile close button
        await cut.Find(".cy-sidebar__close").ClickAsync(new());

        // Assert
        mobileOpenState.ShouldBeFalse();
    }

    [Fact]
    public async Task ClickingBackdropShouldCloseMobileDrawer()
    {
        // Arrange
        var mobileOpenState = true;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.MobileOpen, true)
            .Add(p => p.ShowMobileBackdrop, true)
            .Add(p => p.MobileOpenChanged, v => mobileOpenState = v));

        // Act
        await cut.Find(".cy-sidebar__backdrop").ClickAsync(new());

        // Assert
        mobileOpenState.ShouldBeFalse();
    }
}
