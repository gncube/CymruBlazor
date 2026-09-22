using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyDateInputAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Label_And_Hint()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.HintText, "For example, 15 3 1984")
            .Add(p => p.AutocompleteDateOfBirth, true)
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

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
        var result = await ScanComponentAsync<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Required, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    private string RenderDateInput(
        TestFormModel model, EditContext editContext, string? hint = null, bool required = false, bool disabled = false) =>
        Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.HintText, hint)
            .Add(p => p.Required, required)
            .Add(p => p.Disabled, disabled)
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date)).Markup;

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
            RenderDateInput(plain, CreateEditContext(plain)),
            RenderDateInput(hinted, CreateEditContext(hinted), hint: "For example, 15 3 1984"),
            RenderDateInput(
                invalid,
                CreateEditContextWithError(invalid, nameof(TestFormModel.Date), "Enter your date of birth"),
                required: true),
            RenderDateInput(disabled, CreateEditContext(disabled), required: true, disabled: true));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
