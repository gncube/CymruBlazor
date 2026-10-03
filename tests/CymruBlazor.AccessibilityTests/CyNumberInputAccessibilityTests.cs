using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyNumberInputAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Unit_And_Hint()
    {
        var model = new TestFormModel();

        var result = await ScanComponentAsync<CyNumberInput<decimal?>>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.HintText, "Without clothes")
            .Add(p => p.Unit, "kg")
            .Add(p => p.Min, 0m)
            .Add(p => p.Value, model.Weight)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<decimal?>(this, v => model.Weight = v))
            .Add(p => p.ValueExpression, () => model.Weight));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Standalone_And_In_Error()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContextWithError(model, nameof(TestFormModel.Count), "Enter a number");

        var result = await ScanComponentAsync<CyNumberInput<int?>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Number of doses")
            .Add(p => p.Required, true)
            .Add(p => p.Value, model.Count)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int?>(this, v => model.Count = v))
            .Add(p => p.ValueExpression, () => model.Count));

        result.Violations.ShouldBeEmpty();
    }
}
