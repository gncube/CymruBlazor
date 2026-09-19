using Bunit;
using CymruBlazor.Components.Content;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyIconAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_When_Decorative()
    {
        // Arrange - no Label: decorative, paired with visible text in real usage
        var icon = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search"));

        // Act
        var result = await ScanMarkupAsync($"<span>{icon.Markup} Search</span>");

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Icon_Only_With_Label()
    {
        // Act - icon-only usage needs a Label, exactly like the header's
        // search/theme-toggle buttons that motivated CyIcon's Label param
        var result = await ScanComponentAsync<CyIcon>(parameters => parameters
            .Add(p => p.Name, "moon")
            .Add(p => p.Label, "Toggle dark mode"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_When_Labelled_And_Decorative_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange
        var decorative = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search"));
        var labelled = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "moon")
            .Add(p => p.Label, "Toggle dark mode"));

        // Act
        var result = await ScanMarkupAsync(
            $"<span>{decorative.Markup} Search</span> <span>{labelled.Markup}</span>", theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
