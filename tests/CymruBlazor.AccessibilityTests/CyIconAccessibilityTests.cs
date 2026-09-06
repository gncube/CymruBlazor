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
}
