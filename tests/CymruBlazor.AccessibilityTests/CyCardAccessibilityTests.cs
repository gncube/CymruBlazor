using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyCardAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Header_And_Footer()
    {
        // Act
        var result = await ScanComponentAsync<CyCard>(parameters => parameters
            .Add(p => p.Header, "<strong>Cardiff and Vale clinic</strong>")
            .Add(p => p.ChildContent, "Next available appointment: Thursday, 9:15am.")
            .Add(p => p.Footer, "<a href=\"#\">View details</a>"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_As_A_Whole_Card_Link()
    {
        // Act
        var result = await ScanComponentAsync<CyCard>(parameters => parameters
            .Add(p => p.Header, "<strong>Cardiff and Vale clinic</strong>")
            .Add(p => p.ChildContent, "Next available appointment: Thursday, 9:15am.")
            .Add(p => p.Href, "/clinics/cardiff-vale"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ComponentElevation.None)]
    [InlineData(ComponentElevation.Small)]
    [InlineData(ComponentElevation.Large)]
    public async Task Should_Have_No_Violations_For_Each_Elevation(ComponentElevation elevation)
    {
        // Act
        var result = await ScanComponentAsync<CyCard>(parameters => parameters
            .Add(p => p.Elevation, elevation)
            .Add(p => p.ChildContent, "Plain card body."));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Card_Shapes_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - header/footer, whole-card link, and each elevation
        var markup = string.Join(
            Environment.NewLine,
            Render<CyCard>(parameters => parameters
                .Add(p => p.Header, "<strong>Cardiff and Vale clinic</strong>")
                .Add(p => p.ChildContent, "Next available appointment: Thursday, 9:15am.")
                .Add(p => p.Footer, "<a href=\"#\">View details</a>")).Markup,
            Render<CyCard>(parameters => parameters
                .Add(p => p.Header, "<strong>Swansea Bay clinic</strong>")
                .Add(p => p.ChildContent, "Next available appointment: Friday, 11:00am.")
                .Add(p => p.Href, "/clinics/swansea-bay")).Markup,
            Render<CyCard>(parameters => parameters
                .Add(p => p.Elevation, ComponentElevation.None)
                .Add(p => p.ChildContent, "Card with no elevation.")).Markup,
            Render<CyCard>(parameters => parameters
                .Add(p => p.Elevation, ComponentElevation.Small)
                .Add(p => p.ChildContent, "Card with small elevation.")).Markup,
            Render<CyCard>(parameters => parameters
                .Add(p => p.Elevation, ComponentElevation.Large)
                .Add(p => p.ChildContent, "Card with large elevation.")).Markup);

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
