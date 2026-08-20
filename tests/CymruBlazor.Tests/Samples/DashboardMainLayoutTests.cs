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
/// Razor attribute - so this test renders the real component instead.
/// If any icon name anywhere in the rendered tree is invalid, the
/// `Render&lt;MainLayout&gt;()` call itself throws and every test below
/// fails, which is the primary regression guard.
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
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Page content"))));

        // Assert
        cut.Markup.ShouldContain("Page content");
    }

    [Fact]
    public void Should_Render_A_Valid_Icon_For_Every_Primary_Nav_And_Ward_Filter_Row()
    {
        // Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Page content"))));

        // Assert - 4 primary nav items + 5 ward filter rows, each
        // rendering one CyIcon (base class "cy-icon") if its Name
        // resolved successfully. Getting this far without an exception
        // already proves every name was valid; this additionally
        // confirms the expected number of icons actually rendered.
        cut.FindAll(".app-shell__nav-link svg.cy-icon").Count.ShouldBe(9);
    }

    [Theory]
    [InlineData(SidebarCollapseMode.Compact)]
    [InlineData(SidebarCollapseMode.IconOnly)]
    [InlineData(SidebarCollapseMode.Disabled)]
    [InlineData(SidebarCollapseMode.Hidden)]
    public void Should_Render_Without_Throwing_In_Every_CollapseMode(
        SidebarCollapseMode mode)
    {
        // Arrange/Act - MainLayout exposes a live "Sidebar" <select> for
        // this in the running app; drive the same underlying state here
        // by rendering, then changing the select, to exercise every
        // CollapseMode's icon rendering path (Compact/IconOnly render
        // every ward-filter icon at a different CSS size, but the same
        // underlying CyIcon.Name values).
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Page content"))));

        var select = cut.Find(".app-shell__collapse-mode-select");
        select.Change(mode.ToString());

        // Assert - no exception means every icon name remained valid
        // after switching modes (CollapseMode only changes CSS/whether
        // Brand renders - see CySidebar.razor.cs - it never changes
        // which icon names are used).
        cut.FindAll(".app-shell__nav-link svg.cy-icon").Count.ShouldBe(9);
    }
}
