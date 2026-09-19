using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyBadgeAccessibilityTests : AxeTestBase
{
    [Theory]
    [InlineData(ComponentColour.Primary)]
    [InlineData(ComponentColour.Neutral)]
    [InlineData(ComponentColour.Warning)]
    [InlineData(ComponentColour.Surface)]
    public async Task Should_Have_No_Violations_For_Each_Variant(ComponentColour variant)
    {
        // Act
        var result = await ScanComponentAsync<CyBadge>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Cardiology"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Dismissible()
    {
        // Act
        var result = await ScanComponentAsync<CyBadge>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.DismissAriaLabel, "Remove Cardiology filter")
            .AddChildContent("Cardiology"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Variants_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - the four variants under test above, plus a dismissible one
        ComponentColour[] variants =
            [ComponentColour.Primary, ComponentColour.Neutral, ComponentColour.Warning, ComponentColour.Surface];

        var markup = string.Join(
            Environment.NewLine,
            variants.Select(variant => Render<CyBadge>(parameters => parameters
                .Add(p => p.Variant, variant)
                .AddChildContent("Cardiology")).Markup)
            .Append(Render<CyBadge>(parameters => parameters
                .Add(p => p.Dismissible, true)
                .Add(p => p.DismissAriaLabel, "Remove Cardiology filter")
                .AddChildContent("Cardiology")).Markup));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
