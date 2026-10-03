using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyFieldTests : FormFieldTestContext
{
    private static RenderFragment<CyFieldContext> RawInput() => context => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, context.Attributes);
        builder.AddAttribute(2, "class", "cy-input");
        builder.CloseElement();
    };

    [Fact]
    public void Should_Wire_Label_Hint_Error_And_Aria_To_The_Child()
    {
        var cut = Render<CyField>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.HintText, "In kilos")
            .Add(p => p.Required, true)
            .Add(p => p.Error, "Too heavy")
            .Add(p => p.ChildContent, RawInput()));

        var input = cut.Find("input");
        cut.Find("label").GetAttribute("for").ShouldBe(input.Id);
        input.Id.ShouldNotBeNullOrEmpty();
        input.GetAttribute("aria-describedby").ShouldBe($"{input.Id}-hint {input.Id}-error");
        input.GetAttribute("aria-invalid").ShouldBe("true");
        input.GetAttribute("aria-required").ShouldBe("true");
        cut.Find(".cy-field__error").TextContent.ShouldBe("Too heavy");
        cut.Find(".cy-field__error").GetAttribute("role").ShouldBe("alert");
        cut.Find(".cy-field").ClassList.ShouldContain("cy-field--invalid");
        cut.Find(".cy-field").ClassList.ShouldContain("cy-field--required");
    }

    [Fact]
    public void Should_Emit_No_Aria_When_There_Is_Nothing_To_Say()
    {
        var cut = Render<CyField>(parameters => parameters
            .Add(p => p.Label, "Plain")
            .Add(p => p.ChildContent, RawInput()));

        var input = cut.Find("input");
        input.HasAttribute("aria-describedby").ShouldBeFalse();
        input.HasAttribute("aria-invalid").ShouldBeFalse();
        input.HasAttribute("aria-required").ShouldBeFalse();
        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Group_Should_Render_Fieldset_And_Legend()
    {
        var cut = Render<CyField>(parameters => parameters
            .Add(p => p.Label, "Choices")
            .Add(p => p.Group, true)
            .Add(p => p.HintText, "Pick")
            .Add(p => p.Error, "Required")
            .Add(p => p.ChildContent, _ => b => b.AddContent(0, "x")));

        cut.Find("fieldset legend").TextContent.ShouldContain("Choices");
        cut.FindAll("label").ShouldBeEmpty();
        cut.Find("fieldset").GetAttribute("aria-describedby").ShouldNotBeNull();
        cut.Find("fieldset").ClassList.ShouldContain("cy-field--group");
    }

    [Fact]
    public void For_Should_Show_And_Clear_EditContext_Messages_As_Validation_Changes()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var store = new ValidationMessageStore(editContext);

        var cut = Render<CyField>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Weight")
            .Add(p => p.For, () => model.Weight!)
            .Add(p => p.ChildContent, RawInput()));

        cut.FindAll(".cy-field__error").ShouldBeEmpty();

        cut.InvokeAsync(() =>
        {
            store.Add(editContext.Field(nameof(TestFormModel.Weight)), "Enter a weight");
            editContext.NotifyValidationStateChanged();
        });
        cut.Find(".cy-field__error").TextContent.ShouldBe("Enter a weight");
        cut.Find("input").GetAttribute("aria-invalid").ShouldBe("true");

        cut.InvokeAsync(() =>
        {
            store.Clear();
            editContext.NotifyValidationStateChanged();
        });
        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Error_Parameter_Wins_Over_EditContext_Messages()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        new ValidationMessageStore(editContext).Add(editContext.Field(nameof(TestFormModel.Weight)), "from context");

        var cut = Render<CyField>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Weight")
            .Add(p => p.Error, "mine")
            .Add(p => p.For, () => model.Weight!)
            .Add(p => p.ChildContent, RawInput()));

        cut.Find(".cy-field__error").TextContent.ShouldBe("mine");
    }

    [Fact]
    public void For_Without_EditContext_Is_Ignored()
    {
        var model = new TestFormModel();

        var cut = Render<CyField>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.For, () => model.Weight!)
            .Add(p => p.ChildContent, RawInput()));

        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Dispose_Unsubscribes_From_The_EditContext()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        var cut = Render<CyField>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Weight")
            .Add(p => p.For, () => model.Weight!)
            .Add(p => p.ChildContent, RawInput()));

        cut.Dispose();

        Should.NotThrow(editContext.NotifyValidationStateChanged);
    }

    [Fact]
    public void Should_Apply_Custom_Class_And_Pass_Through_Attributes()
    {
        var cut = Render<CyField>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.Class, "extra")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { ["data-test"] = "f" })
            .Add(p => p.ChildContent, RawInput()));

        cut.Find(".cy-field").ClassList.ShouldContain("extra");
        cut.Find(".cy-field").GetAttribute("data-test").ShouldBe("f");
    }
}
