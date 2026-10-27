using Bunit;
using CymruBlazor.Components.Content;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyDetailsAccessibilityTests : AxeTestBase
{
    private static RenderFragment DetailsContent() => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, "An NHS number is a 10 digit number, like 485 777 3456.");
        builder.CloseElement();
        builder.OpenElement(2, "p");
        builder.AddContent(3, "You can find your NHS number on any letter the NHS has sent you.");
        builder.CloseElement();
    };

    [Fact]
    public async Task Should_Have_No_Violations_When_Closed()
    {
        var result = await ScanComponentAsync<CyDetails>(parameters => parameters
            .Add(p => p.Summary, "Where can I find my NHS number?")
            .Add(p => p.ChildContent, DetailsContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Open()
    {
        var result = await ScanComponentAsync<CyDetails>(parameters => parameters
            .Add(p => p.Summary, "Where can I find my NHS number?")
            .Add(p => p.Open, true)
            .Add(p => p.ChildContent, DetailsContent()));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var cut = Render<CyDetails>(parameters => parameters
            .Add(p => p.Summary, "Where can I find my NHS number?")
            .Add(p => p.Open, true)
            .Add(p => p.ChildContent, DetailsContent()));

        var result = await ScanMarkupAsync(cut.Markup, theme);

        result.Violations.ShouldBeEmpty();
    }
}
