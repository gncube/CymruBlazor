using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyFieldAccessibilityTests : FormFieldAxeTestBase
{
    private static RenderFragment<CyFieldContext> RawInput() => context => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, context.Attributes);
        builder.AddAttribute(2, "type", "text");
        builder.AddAttribute(3, "class", "cy-input");
        builder.CloseElement();
    };

    [Fact]
    public async Task Should_Have_No_Violations_Around_A_Raw_Input()
    {
        var result = await ScanComponentAsync<CyField>(parameters => parameters
            .Add(p => p.Label, "Postcode")
            .Add(p => p.HintText, "For example, CF10 3NQ")
            .Add(p => p.Required, true)
            .Add(p => p.ChildContent, RawInput()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_An_Error()
    {
        var result = await ScanComponentAsync<CyField>(parameters => parameters
            .Add(p => p.Label, "Postcode")
            .Add(p => p.Error, "Enter a postcode")
            .Add(p => p.ChildContent, RawInput()));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_As_A_Group()
    {
        var result = await ScanComponentAsync<CyField>(parameters => parameters
            .Add(p => p.Label, "Contact preference")
            .Add(p => p.Group, true)
            .Add(p => p.HintText, "Choose one")
            .Add(p => p.ChildContent, _ => builder =>
            {
                builder.OpenElement(0, "label");
                builder.OpenElement(1, "input");
                builder.AddAttribute(2, "type", "radio");
                builder.AddAttribute(3, "name", "contact");
                builder.CloseElement();
                builder.AddContent(4, " Email");
                builder.CloseElement();
            }));

        result.Violations.ShouldBeEmpty();
    }
}
