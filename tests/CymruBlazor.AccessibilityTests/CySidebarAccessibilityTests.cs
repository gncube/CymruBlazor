using CymruBlazor.Components.Content;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CySidebarAccessibilityTests : AxeTestBase
{
    private static RenderFragment DefaultBrandFragment() => builder =>
    {
        builder.AddContent(0, "Health Board");
    };

    private static RenderFragment DefaultNavItems() => builder =>
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "aria-label", "Main navigation");

        builder.OpenElement(2, "a");
        builder.AddAttribute(3, "href", "/overview");
        builder.AddAttribute(4, "class", "cy-sidebar__item");
        builder.OpenComponent<CyIcon>(5);
        builder.AddComponentParameter(6, "Name", "home");
        builder.AddComponentParameter(7, "Size", 18);
        builder.CloseComponent();
        builder.OpenElement(8, "span");
        builder.AddAttribute(9, "class", "cy-sidebar__label");
        builder.AddContent(10, "Overview");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(11, "a");
        builder.AddAttribute(12, "href", "/appointments");
        builder.AddAttribute(13, "class", "cy-sidebar__item");
        builder.OpenComponent<CyIcon>(14);
        builder.AddComponentParameter(15, "Name", "appointment");
        builder.AddComponentParameter(16, "Size", 18);
        builder.CloseComponent();
        builder.OpenElement(17, "span");
        builder.AddAttribute(18, "class", "cy-sidebar__label");
        builder.AddContent(19, "Appointments");
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement();
    };

    [Theory]
    [InlineData(SidebarState.Expanded)]
    [InlineData(SidebarState.Compact)]
    [InlineData(SidebarState.IconOnly)]
    public async Task VisibleSidebarStatesShouldHaveNoAccessibilityViolations(SidebarState state)
    {
        var result = await ScanComponentAsync<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden])
            .Add(p => p.State, state)
            .Add(p => p.Brand, DefaultBrandFragment())
            .Add(p => p.ChildContent, DefaultNavItems()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task HiddenStateWithRevealHandleShouldHaveNoAccessibilityViolations()
    {
        var result = await ScanComponentAsync<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden])
            .Add(p => p.State, SidebarState.Hidden)
            .Add(p => p.Brand, DefaultBrandFragment())
            .Add(p => p.ChildContent, DefaultNavItems()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task MobileDrawerOpenWithBackdropShouldHaveNoAccessibilityViolations()
    {
        var result = await ScanComponentAsync<CySidebar>(parameters => parameters
            .Add(p => p.MobileOpen, true)
            .Add(p => p.ShowMobileBackdrop, true)
            .Add(p => p.Brand, DefaultBrandFragment())
            .Add(p => p.ChildContent, DefaultNavItems()));

        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). One scan per
    // state and theme (not one markup per theme): each sidebar is its own
    // landmark, so several in one page would fail landmark-unique.
    [Theory]
    [InlineData(SidebarState.Expanded, "dark")]
    [InlineData(SidebarState.Compact, "dark")]
    [InlineData(SidebarState.IconOnly, "dark")]
    [InlineData(SidebarState.Hidden, "dark")]
    [InlineData(SidebarState.Expanded, "high-contrast")]
    [InlineData(SidebarState.Compact, "high-contrast")]
    [InlineData(SidebarState.IconOnly, "high-contrast")]
    [InlineData(SidebarState.Hidden, "high-contrast")]
    public async Task EveryStateShouldHaveNoAccessibilityViolationsInDarkAndHighContrastThemes(SidebarState state, string theme)
    {
        // Arrange - Hidden renders the reveal handle
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly, SidebarState.Hidden])
            .Add(p => p.State, state)
            .Add(p => p.Brand, DefaultBrandFragment())
            .Add(p => p.ChildContent, DefaultNavItems()));

        // Act
        var result = await ScanMarkupAsync(cut.Markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
