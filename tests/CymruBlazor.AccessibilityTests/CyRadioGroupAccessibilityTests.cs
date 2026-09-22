using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyRadioGroupAccessibilityTests : FormFieldAxeTestBase
{
    private static RenderFragment EmailAndPhoneRadios() => builder =>
    {
        builder.OpenComponent<CyRadio>(0);
        builder.AddComponentParameter(1, "Value", "email");
        builder.AddComponentParameter(2, "Label", "Email");
        builder.AddComponentParameter(3, "HintText", "We'll only use this to confirm or reschedule");
        builder.CloseComponent();

        builder.OpenComponent<CyRadio>(4);
        builder.AddComponentParameter(5, "Value", "phone");
        builder.AddComponentParameter(6, "Label", "Phone");
        builder.CloseComponent();
    };

    [Fact]
    public async Task Should_Have_No_Violations_With_Label_And_Hint()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.HintText, "We may use this to get in touch about your appointment")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Required_And_Disabled()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.Required, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios()));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    private string RenderRadioGroup(
        TestFormModel model, EditContext editContext, string? hint = null, bool required = false, bool disabled = false) =>
        Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.HintText, hint)
            .Add(p => p.Required, required)
            .Add(p => p.Disabled, disabled)
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios())).Markup;

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_All_States_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - default, hint, error and disabled, each with its own model
        var plain = new TestFormModel();
        var hinted = new TestFormModel();
        var invalid = new TestFormModel();
        var disabled = new TestFormModel();

        var markup = string.Join(
            Environment.NewLine,
            RenderRadioGroup(plain, CreateEditContext(plain)),
            RenderRadioGroup(hinted, CreateEditContext(hinted), hint: "We may use this to get in touch about your appointment"),
            RenderRadioGroup(
                invalid,
                CreateEditContextWithError(invalid, nameof(TestFormModel.Choice), "Choose a contact method"),
                required: true),
            RenderRadioGroup(disabled, CreateEditContext(disabled), required: true, disabled: true));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
