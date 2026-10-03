using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyCheckboxGroupAccessibilityTests : FormFieldAxeTestBase
{
    private static readonly CyOption<string>[] Items =
    [
        new("asthma", "Asthma") { Hint = "Including childhood asthma" },
        new("diabetes", "Diabetes"),
        new("none", "None of these") { Disabled = true }
    ];

    [Fact]
    public async Task Should_Have_No_Violations_With_Hints_And_Selection()
    {
        var model = new TestFormModel { Tags = ["diabetes"] };

        var result = await ScanComponentAsync<CyCheckboxGroup<string>>(parameters => parameters
            .Add(p => p.Label, "Do you have any of these conditions?")
            .Add(p => p.HintText, "Select all that apply")
            .Add(p => p.Items, Items)
            .Add(p => p.Value, model.Tags)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
            .Add(p => p.ValueExpression, () => model.Tags));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_In_Error_State()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContextWithError(model, nameof(TestFormModel.Tags), "Choose at least one");

        var result = await ScanComponentAsync<CyCheckboxGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Conditions")
            .Add(p => p.Required, true)
            .Add(p => p.Items, Items)
            .Add(p => p.Value, model.Tags)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
            .Add(p => p.ValueExpression, () => model.Tags));

        result.Violations.ShouldBeEmpty();
    }
}
