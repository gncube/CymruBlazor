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
}
