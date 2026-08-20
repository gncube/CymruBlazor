
using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Services;
using CymruBlazor.Themes;
using CymruBlazor.Samples.Dashboard.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// Regression coverage for the invalid <c>CyIcon.Name</c> bug documented
/// in <c>plans/icon-bug-fix.md</c> (Option A): two ward-filter rows in
/// <c>samples/Dashboard/Layout/MainLayout.razor</c> used the icon
/// *domain* strings <c>"clinical"</c>/<c>"clinical-actions"</c> instead
/// of a registered icon name, which throws an <see cref="ArgumentException"/>
/// from <c>CyIcon.ValidateParameters()</c> at render time.
///
/// A static text scan over *.razor files would NOT have caught this
/// specific bug, because the bad value only ever flowed through a bound
/// C# field (<c>@filter.Icon</c>) rather than appearing as a literal
/// Razor attribute - so these tests render the real component instead.
///
/// If any icon name anywhere in the rendered navigation tree is invalid,
/// the <c>Render&lt;MainLayout&gt;()</c> call itself throws and the relevant
/// test fails, which is the primary regression guard.
/// </summary>
public sealed class DashboardMainLayoutTests : TestContextBase
{
    public DashboardMainLayoutTests()
    {
        // ThemeService constructed without an IJSRuntime - see
        // CyThemeProviderTests for the same, established pattern; JS
        // interop is optional and MainLayout doesn't need it to render.
        Services.AddSingleton<IThemeService>(new ThemeService());
    }

    [Fact]
    public void Should_Render_Without_Throwing()
    {
        // Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
                builder.AddContent(0, "Page content"))));

        // Assert
        cut.Markup.ShouldContain("Page content");
    }

    [Fact]
    public void Should_Render_A_Valid_Icon_For_Every_Navigation_Row()
    {
        // Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
                builder.AddContent(0, "Page content"))));

        // Assert
        // Render<MainLayout>() throws if any CyIcon.Name is invalid.
        // These assertions additionally verify that every expected
        // navigation row rendered its CyIcon.
        cut.FindAll(
            "nav[aria-label='Primary'] .app-shell__nav-link svg.cy-icon")
            .Count.ShouldBe(4);

        cut.FindAll(
            "nav[aria-label='Filter wards'] .app-shell__nav-link svg.cy-icon")
            .Count.ShouldBe(5);

        cut.FindAll(
            ".app-shell__nav-footer .app-shell__nav-link svg.cy-icon")
            .Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(SidebarCollapseMode.Compact)]
    [InlineData(SidebarCollapseMode.IconOnly)]
    [InlineData(SidebarCollapseMode.Disabled)]
    [InlineData(SidebarCollapseMode.Hidden)]
    public void Should_Render_Without_Throwing_In_Every_CollapseMode(
        SidebarCollapseMode mode)
    {
        // Arrange
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
                builder.AddContent(0, "Page content"))));

        // Act
        // MainLayout exposes a live "Sidebar" <select> for this in the
        // running app; drive the same underlying state here to exercise
        // every CollapseMode's rendering path.
        var select = cut.Find(".app-shell__collapse-mode-select");
        select.Change(mode.ToString());

        // Assert
        // An invalid CyIcon.Name causes component rendering to throw.
        // Reaching this point and retaining the page content therefore
        // verifies that icon rendering remains valid after changing
        // CollapseMode.
        cut.Markup.ShouldContain("Page content");
    }
}

