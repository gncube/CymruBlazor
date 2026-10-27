using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyWarningCalloutAccessibilityTests : AxeTestBase
{
    private static RenderFragment CalloutContent() => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, "Stay away from school, nursery or work until 48 hours after you've stopped vomiting or having diarrhoea.");
        builder.CloseElement();
    };

    [Fact]
    public async Task Should_Have_No_Violations_In_Default_Configuration()
    {
        var result = await ScanComponentAsync<CyWarningCallout>(parameters => parameters
            .Add(p => p.Heading, "School, nursery or work")
            .Add(p => p.ChildContent, CalloutContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Custom_Role_And_HeadingLevel()
    {
        var result = await ScanComponentAsync<CyWarningCallout>(parameters => parameters
            .Add(p => p.Heading, "Important notice")
            .Add(p => p.HeadingLevel, 2)
            .Add(p => p.Role, "note")
            .Add(p => p.Severity, ComponentColour.Danger)
            .Add(p => p.ChildContent, CalloutContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var cut = Render<CyWarningCallout>(parameters => parameters
            .Add(p => p.Heading, "School, nursery or work")
            .Add(p => p.ChildContent, CalloutContent()));

        var result = await ScanMarkupAsync(cut.Markup, theme);

        result.Violations.ShouldBeEmpty();
    }
}
