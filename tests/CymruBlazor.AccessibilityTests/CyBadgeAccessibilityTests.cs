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
}
