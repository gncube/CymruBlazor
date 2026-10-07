using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyComboboxAccessibilityTests : FormFieldAxeTestBase
{
    public CyComboboxAccessibilityTests()
    {
        // The combobox loads one small script on first render; the scan renders markup only, so stub it.
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js");
    }

    private static readonly IReadOnlyList<CyOption<string>> Boards =
    [
        new("a", "Aneurin Bevan University Health Board"),
        new("b", "Cardiff and Vale University Health Board") { Hint = "South East Wales" },
        new("c", "Hywel Dda University Health Board") { Disabled = true }
    ];

    private string RenderSingle(string value = "", bool disabled = false, bool required = false, bool error = false)
    {
        var model = new TestFormModel { Choice = value };
        var editContext = error
            ? CreateEditContextWithError(model, nameof(TestFormModel.Choice), "Choose a health board")
            : CreateEditContext(model);

        return Render<CyCombobox<string>>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Health board")
            .Add(c => c.HintText, "Start typing a name")
            .Add(c => c.Items, Boards)
            .Add(c => c.Disabled, disabled)
            .Add(c => c.Required, required)
            .Add(c => c.Value, model.Choice)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(c => c.ValueExpression, () => model.Choice)).Markup;
    }

    private string RenderMulti(bool error = false)
    {
        var model = new TestFormModel { Tags = ["a", "b"] };
        var editContext = error
            ? CreateEditContextWithError(model, nameof(TestFormModel.Tags), "Choose at least one")
            : CreateEditContext(model);

        return Render<CyMultiCombobox<string>>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Health boards")
            .Add(c => c.Items, Boards)
            .Add(c => c.Value, model.Tags)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
            .Add(c => c.ValueExpression, () => model.Tags)).Markup;
    }

    [Fact]
    public async Task Should_Have_No_Violations_Single_With_Label_And_Hint()
    {
        var result = await ScanMarkupAsync(RenderSingle());

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_Single_With_A_Value_Required_Invalid_And_Disabled()
    {
        var markup = string.Join(
            Environment.NewLine,
            RenderSingle("a"),
            RenderSingle(required: true, error: true),
            RenderSingle("b", disabled: true));

        var result = await ScanMarkupAsync(markup);

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_Multiple_With_Chips()
    {
        var result = await ScanMarkupAsync(string.Join(Environment.NewLine, RenderMulti(), RenderMulti(error: true)));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var markup = string.Join(
            Environment.NewLine,
            RenderSingle(),
            RenderSingle("a"),
            RenderSingle(required: true, error: true),
            RenderSingle("b", disabled: true),
            RenderMulti());

        var result = await ScanMarkupAsync(markup, theme);

        result.Violations.ShouldBeEmpty();
    }
}
