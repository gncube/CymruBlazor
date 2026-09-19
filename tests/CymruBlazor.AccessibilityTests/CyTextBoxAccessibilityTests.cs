using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyTextBoxAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Label_And_Hint()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.HintText, "As it appears on your NHS record")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

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
        var result = await ScanComponentAsync<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.Required, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    private string RenderTextBox(
        TestFormModel model, EditContext editContext, string? hint = null, bool required = false, bool disabled = false) =>
        Render<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.HintText, hint)
            .Add(p => p.Required, required)
            .Add(p => p.Disabled, disabled)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text)).Markup;

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
            RenderTextBox(plain, CreateEditContext(plain)),
            RenderTextBox(hinted, CreateEditContext(hinted), hint: "As it appears on your NHS record"),
            RenderTextBox(
                invalid,
                CreateEditContextWithError(invalid, nameof(TestFormModel.Text), "Enter your full name"),
                required: true),
            RenderTextBox(disabled, CreateEditContext(disabled), required: true, disabled: true));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
