using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CySelectAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Options()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CySelect<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred clinic")
            .Add(p => p.HintText, "Choose your nearest health board")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .AddChildContent(
                "<option value=\"\">Select a clinic</option>"
                + "<option value=\"cardiff\">Cardiff and Vale</option>"
                + "<option value=\"swansea\">Swansea Bay</option>"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    private string RenderSelect(
        TestFormModel model, EditContext editContext, string? hint = null, bool required = false, bool disabled = false) =>
        Render<CySelect<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred clinic")
            .Add(p => p.HintText, hint)
            .Add(p => p.Required, required)
            .Add(p => p.Disabled, disabled)
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .AddChildContent(
                "<option value=\"\">Select a clinic</option>"
                + "<option value=\"cardiff\">Cardiff and Vale</option>"
                + "<option value=\"swansea\">Swansea Bay</option>")).Markup;

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
            RenderSelect(plain, CreateEditContext(plain)),
            RenderSelect(hinted, CreateEditContext(hinted), hint: "Choose your nearest health board"),
            RenderSelect(
                invalid,
                CreateEditContextWithError(invalid, nameof(TestFormModel.Choice), "Select a clinic"),
                required: true),
            RenderSelect(disabled, CreateEditContext(disabled), required: true, disabled: true));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
