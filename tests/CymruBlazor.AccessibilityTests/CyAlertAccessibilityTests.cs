using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyAlertAccessibilityTests : AxeTestBase
{
    [Theory]
    [InlineData(ComponentColour.Info)]
    [InlineData(ComponentColour.Success)]
    [InlineData(ComponentColour.Warning)]
    [InlineData(ComponentColour.Danger)]
    public async Task Should_Have_No_Violations_For_Each_Severity(ComponentColour severity)
    {
        // Act
        var result = await ScanComponentAsync<CyAlert>(parameters => parameters
            .Add(p => p.Severity, severity)
            .Add(p => p.Title, "Appointment confirmed")
            .AddChildContent("Your appointment has been confirmed for Tuesday at 10:30am."));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Dismissible()
    {
        // Act
        var result = await ScanComponentAsync<CyAlert>(parameters => parameters
            .Add(p => p.Severity, ComponentColour.Warning)
            .Add(p => p.Dismissible, true)
            .AddChildContent("This appointment is outside normal clinic hours."));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_Without_A_Title()
    {
        // Act
        var result = await ScanComponentAsync<CyAlert>(parameters => parameters
            .Add(p => p.Severity, ComponentColour.Info)
            .AddChildContent("A short, title-less status message."));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Severities_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - four severities, all dismissible
        ComponentColour[] severities =
            [ComponentColour.Info, ComponentColour.Success, ComponentColour.Warning, ComponentColour.Danger];

        var markup = string.Join(
            Environment.NewLine,
            severities.Select(severity => Render<CyAlert>(parameters => parameters
                .Add(p => p.Severity, severity)
                .Add(p => p.Title, "Appointment confirmed")
                .Add(p => p.Dismissible, true)
                .AddChildContent("Your appointment has been confirmed for Tuesday at 10:30am.")).Markup));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
