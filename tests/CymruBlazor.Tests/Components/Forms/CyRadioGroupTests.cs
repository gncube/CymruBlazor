using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyRadioGroupTests : FormFieldTestContext
{
    private static RenderFragment EmailAndPhoneRadios() => builder =>
    {
        builder.OpenComponent<CyRadio>(0);
        builder.AddComponentParameter(1, "Value", "email");
        builder.AddComponentParameter(2, "Label", "Email");
        builder.CloseComponent();

        builder.OpenComponent<CyRadio>(3);
        builder.AddComponentParameter(4, "Value", "phone");
        builder.AddComponentParameter(5, "Label", "Phone");
        builder.CloseComponent();
    };

    [Fact]
    public void Should_Render_Fieldset_With_Legend_And_Radios_From_ChildContent()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios()));

        // Assert
        cut.Find("legend").TextContent.ShouldContain("Preferred contact method");

        var radios = cut.FindAll("input[type=radio]");
        radios.Count.ShouldBe(2);
        radios[0].GetAttribute("name").ShouldBe(radios[1].GetAttribute("name"));
    }

    [Fact]
    public void Should_Select_Radio_And_Raise_ValueChanged_On_Change()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var selected = string.Empty;

        // Act
        var cut = Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => selected = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios()));

        cut.Find("input[value=phone]").Change(true);

        // Assert
        selected.ShouldBe("phone");
        cut.Find("input[value=phone]").HasAttribute("checked").ShouldBeTrue();
        cut.Find("input[value=email]").HasAttribute("checked").ShouldBeFalse();
    }

    [Fact]
    public void Should_Throw_When_CyRadio_Used_Outside_CyRadioGroup()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyRadio>(parameters => parameters
                .Add(p => p.Value, "email")
                .Add(p => p.Label, "Email")));
    }

    [Fact]
    public async Task Should_Show_Validation_Error_On_Fieldset()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var messages = new ValidationMessageStore(editContext);

        var cut = Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Preferred contact method")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .Add(p => p.ChildContent, EmailAndPhoneRadios()));

        // Act
        messages.Add(new FieldIdentifier(model, nameof(TestFormModel.Choice)), "Choose a contact method");

        await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        cut.FindAll("input[type=radio]")
            .ShouldAllBe(radio => radio.GetAttribute("aria-invalid") == "true");
        cut.Find(".cy-field__error").TextContent.ShouldContain("Choose a contact method");
    }
}
