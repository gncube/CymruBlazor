using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyInsetTextAccessibilityTests : AxeTestBase
{
    private static RenderFragment InsetContent() => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, "It can take up to 3 working days for your referral to be processed.");
        builder.CloseElement();
    };

    [Fact]
    public async Task Should_Have_No_Violations_In_Default_Configuration()
    {
        var result = await ScanComponentAsync<CyInsetText>(parameters => parameters
            .Add(p => p.ChildContent, InsetContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Custom_Role_And_Colour()
    {
        var result = await ScanComponentAsync<CyInsetText>(parameters => parameters
            .Add(p => p.Role, "note")
            .Add(p => p.Colour, ComponentColour.Primary)
            .Add(p => p.ChildContent, InsetContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var cut = Render<CyInsetText>(parameters => parameters
            .Add(p => p.ChildContent, InsetContent()));

        var result = await ScanMarkupAsync(cut.Markup, theme);

        result.Violations.ShouldBeEmpty();
    }
}
