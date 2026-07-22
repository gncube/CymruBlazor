using Xunit;
using Shouldly;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Theming;
using CymruBlazor.Services;
using CymruBlazor.Themes;

namespace CymruBlazor.Tests.Components.Theming;

public sealed class CyThemeProviderTests : TestContextBase
{
    public CyThemeProviderTests()
    {
        // ThemeService constructed without an IJSRuntime - exercises the
        // fully-functional-without-JS-interop path (see ThemeService's
        // class summary). This keeps the test focused on CyThemeProvider's
        // own rendering/subscription behaviour rather than JS interop.
        Services.AddSingleton<IThemeService>(new ThemeService());
    }

    [Fact]
    public void Should_Render_ChildContent()
    {
        // Act
        var cut = Render<CyThemeProvider>(parameters => parameters
            .AddChildContent("<p>Hello</p>"));

        // Assert
        cut.Markup.ShouldContain("Hello");
    }

    [Fact]
    public void Should_Apply_Current_Theme_As_Data_Attribute()
    {
        // Act
        var cut = Render<CyThemeProvider>(parameters => parameters
            .AddChildContent("<p>Hello</p>"));

        // Assert
        var element = cut.Find("div");
        element.GetAttribute("data-theme").ShouldBe("light");
    }

    [Fact]
    public async Task Should_Rerender_With_New_Theme_When_ThemeChanged_Fires()
    {
        // Arrange
        var themeService = Services.GetRequiredService<IThemeService>();

        var cut = Render<CyThemeProvider>(parameters => parameters
            .AddChildContent("<p>Hello</p>"));

        // Act
        await themeService.SetThemeAsync(ThemeMode.Dark);

        // Assert
        cut.WaitForState(() =>
            cut.Find("div").GetAttribute("data-theme") == "dark");
    }

    [Fact]
    public async Task Should_Apply_InitialTheme_On_First_Render()
    {
        // Act
        var cut = Render<CyThemeProvider>(parameters => parameters
            .Add(p => p.InitialTheme, ThemeMode.HighContrast)
            .AddChildContent("<p>Hello</p>"));

        // Assert
        cut.WaitForState(() =>
            cut.Find("div").GetAttribute("data-theme") == "high-contrast");

        await Task.CompletedTask;
    }
}
