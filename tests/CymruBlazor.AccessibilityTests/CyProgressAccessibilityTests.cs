using Bunit;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyProgressAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_When_Determinate()
    {
        // Act
        var result = await ScanComponentAsync<CyProgress>(parameters => parameters
            .Add(p => p.Value, 42)
            .Add(p => p.Label, "Medical Ward A")
            .Add(p => p.ShowValueText, true));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Indeterminate()
    {
        // Act
        var result = await ScanComponentAsync<CyProgress>(parameters => parameters
            .Add(p => p.Value, (double?)null)
            .Add(p => p.Label, "Checking bed availability"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_AriaLabel_Only()
    {
        // Act
        var result = await ScanComponentAsync<CyProgress>(parameters => parameters
            .Add(p => p.Value, 75)
            .Add(p => p.AriaLabel, "Upload progress"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_Each_Colour_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange
        ComponentColour[] colours =
            [ComponentColour.Primary, ComponentColour.Success, ComponentColour.Warning, ComponentColour.Danger];

        var markup = string.Join(
            Environment.NewLine,
            colours.Select(colour => Render<CyProgress>(parameters => parameters
                .Add(p => p.Value, 60)
                .Add(p => p.Label, $"{colour} ward")
                .Add(p => p.Colour, colour)).Markup));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
