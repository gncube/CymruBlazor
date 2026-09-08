using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyCheckboxAccessibilityTests : FormFieldAxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_With_Label_And_Hint()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var result = await ScanComponentAsync<CyCheckbox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "I consent to being contacted about this appointment")
            .Add(p => p.HintText, "We'll only use this to confirm or reschedule")
            .Add(p => p.Value, model.Flag)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(p => p.ValueExpression, () => model.Flag));

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
        var result = await ScanComponentAsync<CyCheckbox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "I consent to being contacted about this appointment")
            .Add(p => p.Required, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, model.Flag)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(p => p.ValueExpression, () => model.Flag));

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
