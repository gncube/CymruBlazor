using Bunit;
using CymruBlazor.Components.Data;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyTableAccessibilityTests : AxeTestBase
{
    private static readonly RenderFragment Rows = builder =>
    {
        builder.AddMarkupContent(0,
            "<thead><tr><th scope=\"col\">Specialty</th><th scope=\"col\">Waiting</th></tr></thead>" +
            "<tbody><tr><th scope=\"row\">Cardiology</th><td>132</td></tr></tbody>");
    };

    [Fact]
    public async Task Should_Have_No_Violations_With_Visible_Caption()
    {
        // Act
        var result = await ScanComponentAsync<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.ChildContent, Rows));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Visually_Hidden_Caption()
    {
        // Act
        var result = await ScanComponentAsync<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.CaptionVisuallyHidden, true)
            .Add(p => p.ChildContent, Rows));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Act
        var markup = Render<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.ChildContent, Rows)).Markup;

        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
