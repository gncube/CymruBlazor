using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyTextAreaTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Label_And_Rows()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.Rows, 8)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        var label = cut.Find("label");
        var textarea = cut.Find("textarea");

        label.TextContent.ShouldContain("Additional notes");
        textarea.GetAttribute("id").ShouldBe(label.GetAttribute("for"));
        textarea.GetAttribute("rows").ShouldBe("8");
    }

    [Fact]
    public void Should_Default_To_Five_Rows()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        cut.Find("textarea").GetAttribute("rows").ShouldBe("5");
    }

    [Fact]
    public void Should_Not_Render_Character_Count_Without_MaxLength()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.ShowCharacterCount, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        cut.FindAll(".cy-textarea__count").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Plural_Remaining_Message()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.MaxLength, 150)
            .Add(p => p.ShowCharacterCount, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        var count = cut.Find(".cy-textarea__count");

        count.GetAttribute("aria-live").ShouldBe("polite");
        count.TextContent.ShouldContain("You have 150 characters remaining");
        cut.Find("textarea").GetAttribute("aria-describedby")!.ShouldContain(count.GetAttribute("id")!);
    }

    [Fact]
    public void Should_Render_Singular_Remaining_Message()
    {
        // Arrange
        var model = new TestFormModel { Text = new string('a', 149) };
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.MaxLength, 150)
            .Add(p => p.ShowCharacterCount, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        cut.Find(".cy-textarea__count").TextContent.ShouldContain("You have 1 character remaining");
    }

    [Fact]
    public void Should_Render_Over_Limit_Message_When_Bound_Value_Already_Exceeds_MaxLength()
    {
        // Arrange - a pre-existing over-limit value (e.g. loaded from a
        // record saved under an older, longer limit), not something a
        // user could type past the native `maxlength` attribute.
        var model = new TestFormModel { Text = new string('a', 152) };
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.MaxLength, 150)
            .Add(p => p.ShowCharacterCount, true)
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Assert
        cut.Find(".cy-textarea__count").TextContent.ShouldContain("You have 2 characters too many");
    }

    [Fact]
    public async Task Should_Show_Validation_Error_When_Field_Is_Invalid()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var messages = new ValidationMessageStore(editContext);

        var cut = Render<CyTextArea>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Additional notes")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        // Act
        messages.Add(new FieldIdentifier(model, nameof(TestFormModel.Text)), "Enter your notes");

        await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        var textarea = cut.Find("textarea");

        textarea.GetAttribute("aria-invalid").ShouldBe("true");
        cut.Find(".cy-field__error").TextContent.ShouldContain("Enter your notes");
    }
}
