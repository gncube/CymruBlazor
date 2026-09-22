using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyTextAreaAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Label_And_Hint()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.HintText, "Include anything else the clinic should know")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Character_Count()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.MaxLength, 150)
            .Add(p => p.ShowCharacterCount, true)
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
        var result = await ScanComponentAsync<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
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
    private string RenderTextArea(
        TestFormModel model,
        EditContext editContext,
        string? hint = null,
        bool required = false,
        bool disabled = false,
        bool showCharacterCount = false) =>
        Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.HintText, hint)
            .Add(p => p.Required, required)
            .Add(p => p.Disabled, disabled)
            .Add(p => p.MaxLength, showCharacterCount ? 150 : null)
            .Add(p => p.ShowCharacterCount, showCharacterCount)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text)).Markup;

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_All_States_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - default, hint+count, error and disabled, each with its own model
        var plain = new TestFormModel();
        var counted = new TestFormModel();
        var invalid = new TestFormModel();
        var disabled = new TestFormModel();

        var markup = string.Join(
            Environment.NewLine,
            RenderTextArea(plain, CreateEditContext(plain)),
            RenderTextArea(
                counted,
                CreateEditContext(counted),
                hint: "Include anything else the clinic should know",
                showCharacterCount: true),
            RenderTextArea(
                invalid,
                CreateEditContextWithError(invalid, nameof(TestFormModel.Text), "Enter your notes"),
                required: true),
            RenderTextArea(disabled, CreateEditContext(disabled), required: true, disabled: true));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
