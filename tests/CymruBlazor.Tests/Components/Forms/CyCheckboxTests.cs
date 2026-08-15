using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyCheckboxTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Checkbox_Input_With_Label_After_It()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyCheckbox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "I agree to the terms")
            .Add(p => p.Value, model.Flag)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(p => p.ValueExpression, () => model.Flag));

        // Assert
        var input = cut.Find("input[type=checkbox]");
        var label = cut.Find("label");

        input.GetAttribute("id").ShouldBe(label.GetAttribute("for"));
        label.TextContent.ShouldContain("I agree to the terms");
    }

    [Fact]
    public void Should_Reflect_Initial_Checked_State()
    {
        // Arrange
        var model = new TestFormModel { Flag = true };
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyCheckbox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Subscribe")
            .Add(p => p.Value, model.Flag)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(p => p.ValueExpression, () => model.Flag));

        // Assert
        cut.Find("input[type=checkbox]").HasAttribute("checked").ShouldBeTrue();
    }
}
