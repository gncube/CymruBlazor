using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyTextBoxTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Label_Associated_With_Input_Via_For_Id()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");

        label.TextContent.ShouldContain("Full name");
        input.GetAttribute("id").ShouldBe(label.GetAttribute("for"));
    }

    [Fact]
    public void Should_Render_Required_Indicator_When_Required()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.Required, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        cut.Find("input").GetAttribute("aria-required").ShouldBe("true");
        cut.FindAll(".cy-field__required-indicator").Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Render_Hint_And_Associate_Via_Aria_Describedby()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.HintText, "As shown on your passport")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        var input = cut.Find("input");
        var hint = cut.Find(".cy-field__hint");

        hint.TextContent.ShouldContain("As shown on your passport");
        input.GetAttribute("aria-describedby").ShouldBe(hint.GetAttribute("id"));
    }

    [Fact]
    public async Task Should_Show_Validation_Error_When_Field_Is_Invalid()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var messages = new ValidationMessageStore(editContext);

        var cut = Render<CyTextBox>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Full name")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Act
        messages.Add(new FieldIdentifier(model, nameof(TestFormModel.Text)), "Enter your full name");

       await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        var input = cut.Find("input");

        input.GetAttribute("aria-invalid").ShouldBe("true");
        cut.Find(".cy-field__error")
            .TextContent
            .ShouldContain("Enter your full name");
    }
}
