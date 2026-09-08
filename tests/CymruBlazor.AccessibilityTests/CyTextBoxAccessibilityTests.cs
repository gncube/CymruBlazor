using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
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
}
