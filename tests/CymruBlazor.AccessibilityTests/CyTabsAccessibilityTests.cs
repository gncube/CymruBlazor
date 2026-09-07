using Bunit;
using CymruBlazor.Components.Layout;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyTabsAccessibilityTests : AxeTestBase
{
    private static RenderFragment ThreePanels() => builder =>
    {
        builder.OpenComponent<CyTabPanel>(0);
        builder.AddComponentParameter(1, "TabId", "examples");
        builder.AddComponentParameter(2, "Title", "Examples");
        builder.AddComponentParameter(3, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Example usage goes here.")));
        builder.CloseComponent();

        builder.OpenComponent<CyTabPanel>(4);
        builder.AddComponentParameter(5, "TabId", "api");
        builder.AddComponentParameter(6, "Title", "API");
        builder.AddComponentParameter(7, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "API reference goes here.")));
        builder.CloseComponent();

        builder.OpenComponent<CyTabPanel>(8);
        builder.AddComponentParameter(9, "TabId", "accessibility");
        builder.AddComponentParameter(10, "Title", "Accessibility");
        builder.AddComponentParameter(11, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Accessibility notes go here.")));
        builder.CloseComponent();
    };

    [Fact]
    public async Task Should_Have_No_Violations_With_First_Tab_Active()
    {
        // Act
        var result = await ScanComponentAsync<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_A_Later_Tab_Active()
    {
        // Act
        var result = await ScanComponentAsync<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabId, "accessibility")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
