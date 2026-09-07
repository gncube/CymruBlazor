using Bunit;
using CymruBlazor.Components.Content;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyAccordionAccessibilityTests : AxeTestBase
{
    private static RenderFragment TwoItems() => builder =>
    {
        builder.OpenComponent<CyAccordionItem>(0);
        builder.AddComponentParameter(1, "ItemId", "eligibility");
        builder.AddComponentParameter(2, "Title", "Eligibility criteria");
        builder.AddComponentParameter(3, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "You must be registered with a GP in Wales.")));
        builder.CloseComponent();

        builder.OpenComponent<CyAccordionItem>(4);
        builder.AddComponentParameter(5, "ItemId", "how-to-apply");
        builder.AddComponentParameter(6, "Title", "How to apply");
        builder.AddComponentParameter(7, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Apply online or by post.")));
        builder.CloseComponent();
    };

    [Fact]
    public async Task Should_Have_No_Violations_When_Collapsed()
    {
        // Act
        var result = await ScanComponentAsync<CyAccordion>(parameters => parameters
            .Add(p => p.ChildContent, TwoItems()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_An_Item_Is_Expanded()
    {
        // Act
        var result = await ScanComponentAsync<CyAccordion>(parameters => parameters
            .Add(p => p.DefaultExpandedItemId, "eligibility")
            .Add(p => p.ChildContent, TwoItems()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
