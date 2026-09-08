using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
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
}
