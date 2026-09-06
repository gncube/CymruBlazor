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
}
