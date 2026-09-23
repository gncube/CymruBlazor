using Bunit;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CySpinnerAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Default_Label()
    {
        // Act
        var result = await ScanComponentAsync<CySpinner>();

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Visible_Label()
    {
        // Act
        var result = await ScanComponentAsync<CySpinner>(parameters => parameters
            .Add(p => p.Label, "Loading appointments")
            .Add(p => p.ShowLabel, true));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange
        var markup = string.Join(
            Environment.NewLine,
            Render<CySpinner>().Markup,
            Render<CySpinner>(parameters => parameters
                .Add(p => p.Colour, ComponentColour.Danger)
                .Add(p => p.Label, "Loading appointments")
                .Add(p => p.ShowLabel, true)).Markup);

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
