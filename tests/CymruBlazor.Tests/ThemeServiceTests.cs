using System;
using System.Threading.Tasks;
using CymruBlazor.Services;
using CymruBlazor.Themes;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var service = new ThemeService();

        // Assert
        service.CurrentTheme.ShouldNotBeNull();
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light);
        service.CurrentTheme.Name.ShouldBe("Light");
        service.CurrentTheme.CssTheme.ShouldBe("light");

        service.AvailableThemes.Count.ShouldBe(4);
    }

    [Fact]
    public async Task SetThemeAsync_WithValidMode_UpdatesThemeAndRaisesEvent()
    {
        // Arrange
        var service = new ThemeService();
        ThemeChangedEventArgs? raisedArgs = null;
        service.ThemeChanged += (sender, e) => raisedArgs = e;

        // Act
        await service.SetThemeAsync(ThemeMode.Dark);

        // Assert
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Dark);
        service.CurrentTheme.CssTheme.ShouldBe("dark");

        raisedArgs.ShouldNotBeNull();
        raisedArgs.Theme.Mode.ShouldBe(ThemeMode.Dark);
    }

    [Fact]
    public async Task SetThemeAsync_WithSameMode_DoesNotRaiseEvent()
    {
        // Arrange
        var service = new ThemeService();
        var callCount = 0;
        service.ThemeChanged += (sender, e) => callCount++;

        // Act - Attempting to set to Light (which is already the default)
        await service.SetThemeAsync(ThemeMode.Light);

        // Assert
        callCount.ShouldBe(0);
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light);
    }

    [Fact]
    public async Task SetThemeAsync_WithInvalidMode_DoesNothing()
    {
        // Arrange
        var service = new ThemeService();
        var callCount = 0;
        service.ThemeChanged += (sender, e) => callCount++;

        // Use an undefined enum value
        var invalidMode = (ThemeMode)999;

        // Act
        await service.SetThemeAsync(invalidMode);

        // Assert
        callCount.ShouldBe(0);
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light); // Remains default
    }

    [Theory]
    [InlineData("dark", ThemeMode.Dark)]
    [InlineData("DARK", ThemeMode.Dark)] // Case-insensitivity verification
    [InlineData("high-contrast", ThemeMode.HighContrast)]
    [InlineData("light", ThemeMode.Light)]
    public async Task SetThemeAsync_WithValidCssThemeString_UpdatesThemeCorrectly(string cssTheme, ThemeMode expectedMode)
    {
        // Arrange
        var service = new ThemeService();

        // Start in Dark mode to ensure setting to Light actually causes a visible transition
        if (expectedMode == ThemeMode.Light)
        {
            await service.SetThemeAsync(ThemeMode.Dark);
        }

        // Act
        await service.SetThemeAsync(cssTheme);

        // Assert
        service.CurrentTheme.Mode.ShouldBe(expectedMode);
    }

    [Fact]
    public async Task SetThemeAsync_WithInvalidCssThemeString_DoesNotChangeTheme()
    {
        // Arrange
        var service = new ThemeService();
        var callCount = 0;
        service.ThemeChanged += (sender, e) => callCount++;

        // Act
        await service.SetThemeAsync("non-existent-theme-class");

        // Assert
        callCount.ShouldBe(0);
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light);
    }

    [Fact]
    public async Task ToggleDarkModeAsync_WhenCurrentlyLight_SwitchesToDark()
    {
        // Arrange
        var service = new ThemeService();
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light);

        // Act
        await service.ToggleDarkModeAsync();

        // Assert
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Dark);
    }

    [Fact]
    public async Task ToggleDarkModeAsync_WhenCurrentlyDark_SwitchesToLight()
    {
        // Arrange
        var service = new ThemeService();
        await service.SetThemeAsync(ThemeMode.Dark);

        // Act
        await service.ToggleDarkModeAsync();

        // Assert
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Light);
    }

    [Fact]
    public async Task ToggleDarkModeAsync_WhenCurrentlyHighContrast_SwitchesToDark()
    {
        // Arrange
        var service = new ThemeService();
        await service.SetThemeAsync(ThemeMode.HighContrast);

        // Act
        await service.ToggleDarkModeAsync();

        // Assert
        service.CurrentTheme.Mode.ShouldBe(ThemeMode.Dark);
    }

    [Fact]
    public async Task InitializeAsync_ReturnsCompletedTask()
    {
        // Arrange
        var service = new ThemeService();

        // Act & Assert
        await service.InitializeAsync();
    }

    [Fact]
    public async Task InitializeAsync_DoesNotThrow_ShouldlyStyle()
    {
        // Arrange
        var service = new ThemeService();

        // Act & Assert
        // Wrapping in a Func<Task> allows Shouldly to safely monitor the execution
        await Should.NotThrowAsync(async () =>
        {
            await service.InitializeAsync();
        });
    }
}
