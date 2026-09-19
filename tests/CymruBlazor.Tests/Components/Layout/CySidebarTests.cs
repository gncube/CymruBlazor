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

#pragma warning disable CS0618
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
#pragma warning restore CS0618

    [Fact]
    public async Task CycleNextAsyncShouldCycleAcrossConfiguredStates()
    {
        var observedStates = new List<SidebarState>();
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Expanded)
            .Add(p => p.StateChanged, s => observedStates.Add(s)));

        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Compact);

        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.IconOnly);

        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Hidden);

        await cut.InvokeAsync(async () => await cut.Instance.CycleNextAsync());
        cut.Instance.State.ShouldBe(SidebarState.Expanded);

        observedStates.ShouldBe([SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden, SidebarState.Expanded]);
    }

    [Fact]
    public async Task ExpandAsyncShouldResetStateToPrimaryState()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Hidden));

        await cut.InvokeAsync(async () => await cut.Instance.ExpandAsync());

        cut.Instance.State.ShouldBe(SidebarState.Expanded);
        cut.Instance.EffectiveCollapsed.ShouldBeFalse();
    }

    [Fact]
    public async Task SetStateAsyncShouldAssignValidStateOnly()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact])
            .Add(p => p.State, SidebarState.Expanded));

        await cut.InvokeAsync(async () => await cut.Instance.SetStateAsync(SidebarState.Hidden));
        cut.Instance.State.ShouldBe(SidebarState.Expanded);

        await cut.InvokeAsync(async () => await cut.Instance.SetStateAsync(SidebarState.Compact));
        cut.Instance.State.ShouldBe(SidebarState.Compact);
    }

    [Fact]
    public void HiddenStateShouldRenderRevealHandleOutsideClippedLayout()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.State, SidebarState.Hidden));

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
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Hidden));

        await cut.Find(".cy-sidebar__reveal-handle").ClickAsync(new());

        cut.Instance.State.ShouldBe(SidebarState.Expanded);
        cut.FindAll(".cy-sidebar__reveal-handle").Count.ShouldBe(0);
    }

    [Fact]
    public async Task MobileOpenShouldRenderBackdropAndCloseButtonExclusively()
    {
        var mobileOpenState = true;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.MobileOpen, true)
            .Add(p => p.ShowMobileBackdrop, true)
            .Add(p => p.MobileOpenChanged, v => mobileOpenState = v));

        cut.FindAll(".cy-sidebar__backdrop").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__close").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(0);

        await cut.Find(".cy-sidebar__close").ClickAsync(new());

        mobileOpenState.ShouldBeFalse();
    }

    [Fact]
    public async Task ClickingBackdropShouldCloseMobileDrawer()
    {
        var mobileOpenState = true;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.MobileOpen, true)
            .Add(p => p.ShowMobileBackdrop, true)
            .Add(p => p.MobileOpenChanged, v => mobileOpenState = v));

        await cut.Find(".cy-sidebar__backdrop").ClickAsync(new());

        mobileOpenState.ShouldBeFalse();
    }

    private static readonly IReadOnlyList<SidebarState> FourStates =
        [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden];

    [Fact]
    public void ThreeOrMoreStatesShouldRenderDirectionalStepControlsInsteadOfSingleToggle()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Expanded));

        cut.FindAll(".cy-sidebar__toggle").Count.ShouldBe(0);

        // Expanded is the first state: it can only narrow.
        cut.FindAll(".cy-sidebar__step--narrow").Count.ShouldBe(1);
        cut.FindAll(".cy-sidebar__step--widen").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(SidebarState.Compact)]
    [InlineData(SidebarState.IconOnly)]
    public void RailStatesShouldRenderBothChevronsWithLabelsNamingTheirTarget(SidebarState state)
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, state));

        cut.FindAll(".cy-sidebar__step").Count.ShouldBe(2);

        var narrowLabel = state == SidebarState.Compact ? "Show icons only" : "Hide sidebar";
        var widenLabel = state == SidebarState.Compact ? "Expand sidebar" : "Show compact sidebar";

        cut.Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe(narrowLabel);
        cut.Find(".cy-sidebar__step--widen").GetAttribute("aria-label").ShouldBe(widenLabel);
    }

    [Theory]
    [InlineData(SidebarPosition.Left, "m15 18-6-6 6-6", "m9 18 6-6-6-6")]
    [InlineData(SidebarPosition.Right, "m9 18 6-6-6-6", "m15 18-6-6 6-6")]
    public void StepChevronsShouldPointTowardsAndAwayFromTheSidebarEdge(
        SidebarPosition position, string narrowPath, string widenPath)
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Position, position)
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Compact));

        cut.Find(".cy-sidebar__step--narrow svg path").GetAttribute("d").ShouldBe(narrowPath);
        cut.Find(".cy-sidebar__step--widen svg path").GetAttribute("d").ShouldBe(widenPath);
    }

    [Fact]
    public async Task NarrowAndWidenShouldStepWithoutWrapping()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Expanded));

        await cut.InvokeAsync(() => cut.Instance.WidenAsync());
        cut.Instance.State.ShouldBe(SidebarState.Expanded);

        await cut.InvokeAsync(() => cut.Instance.NarrowAsync());
        cut.Instance.State.ShouldBe(SidebarState.Compact);

        await cut.InvokeAsync(() => cut.Instance.NarrowAsync());
        await cut.InvokeAsync(() => cut.Instance.NarrowAsync());
        cut.Instance.State.ShouldBe(SidebarState.Hidden);

        await cut.InvokeAsync(() => cut.Instance.NarrowAsync());
        cut.Instance.State.ShouldBe(SidebarState.Hidden);

        await cut.InvokeAsync(() => cut.Instance.WidenAsync());
        cut.Instance.State.ShouldBe(SidebarState.IconOnly);
    }

    [Fact]
    public async Task ClickingStepChevronsShouldChangeState()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Compact));

        await cut.Find(".cy-sidebar__step--narrow").ClickAsync(new());
        cut.Instance.State.ShouldBe(SidebarState.IconOnly);

        await cut.Find(".cy-sidebar__step--widen").ClickAsync(new());
        cut.Instance.State.ShouldBe(SidebarState.Compact);
    }

    [Fact]
    public void CollapsedBrandShouldReplaceBrandInRailStatesOnly()
    {
        RenderFragment brand = b => b.AddContent(0, "Full");
        RenderFragment icon = b => b.AddContent(0, "Icon");

        var expanded = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Expanded)
            .Add(p => p.Brand, brand)
            .Add(p => p.CollapsedBrand, icon));
        expanded.Find(".cy-sidebar__brand").TextContent.ShouldBe("Full");

        foreach (var state in new[] { SidebarState.Compact, SidebarState.IconOnly })
        {
            var rail = Render<CySidebar>(parameters => parameters
                .Add(p => p.States, FourStates)
                .Add(p => p.State, state)
                .Add(p => p.Brand, brand)
                .Add(p => p.CollapsedBrand, icon));
            rail.Find(".cy-sidebar__brand").TextContent.ShouldBe("Icon");
        }
    }

    [Fact]
    public void ShowBrandWhenCollapsedFalseShouldLeaveOnlyTheChevronsInRailStates()
    {
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, FourStates)
            .Add(p => p.State, SidebarState.Compact)
            .Add(p => p.Brand, (RenderFragment)(b => b.AddContent(0, "Full")))
            .Add(p => p.ShowBrandWhenCollapsed, false));

        cut.FindAll(".cy-sidebar__brand").Count.ShouldBe(0);
        cut.FindAll(".cy-sidebar__step").Count.ShouldBe(2);
    }
}
