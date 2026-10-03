using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyCheckboxGroupTests : FormFieldTestContext
{
    private static readonly CyOption<string>[] Items =
    [
        new("a", "Alpha") { Hint = "First" },
        new("b", "Beta"),
        new("c", "Gamma") { Disabled = true }
    ];

    private IRenderedComponent<CyCheckboxGroup<string>> RenderGroup(
        TestFormModel model, EditContext? editContext = null, Action<ComponentParameterCollectionBuilder<CyCheckboxGroup<string>>>? configure = null) =>
        Render<CyCheckboxGroup<string>>(parameters =>
        {
            if (editContext is not null)
            {
                parameters.AddCascadingValue(editContext);
            }

            parameters
                .Add(p => p.Label, "Tags")
                .Add(p => p.Items, Items)
                .Add(p => p.Value, model.Tags)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
                .Add(p => p.ValueExpression, () => model.Tags);
            configure?.Invoke(parameters);
        });

    [Fact]
    public void Should_Render_Fieldset_With_Legend_And_One_Checkbox_Per_Item()
    {
        var cut = RenderGroup(new TestFormModel());

        cut.Find("fieldset legend").TextContent.ShouldContain("Tags");
        cut.FindAll("input[type=checkbox]").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Associate_Each_Label_With_Its_Input()
    {
        var cut = RenderGroup(new TestFormModel());

        var inputs = cut.FindAll("input[type=checkbox]");
        inputs.Select(i => i.Id).Distinct().Count().ShouldBe(3);
        foreach (var input in inputs)
        {
            cut.Find($"label[for='{input.Id}']").ShouldNotBeNull();
        }
    }

    [Fact]
    public void Should_Render_Item_Hint_Disabled_And_Checked_State()
    {
        var model = new TestFormModel { Tags = ["b"] };
        var cut = RenderGroup(model);

        var inputs = cut.FindAll("input[type=checkbox]");
        inputs[0].HasAttribute("checked").ShouldBeFalse();
        inputs[1].HasAttribute("checked").ShouldBeTrue();
        inputs[2].HasAttribute("disabled").ShouldBeTrue();
        cut.Find(".cy-checkbox-group__item-hint").TextContent.ShouldBe("First");
        inputs[0].GetAttribute("aria-describedby").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Not_Emit_Aria_Required_On_Group()
    {
        var cut = RenderGroup(new TestFormModel(), configure: p => p.Add(c => c.Required, true));

        cut.Markup.ShouldNotContain("aria-required");
        cut.Find(".cy-field__required-indicator").ShouldNotBeNull();
    }

    [Fact]
    public void Checking_Adds_In_Items_Order_And_Keeps_Unknown_Values()
    {
        var model = new TestFormModel { Tags = ["b", "zz"] };
        var cut = RenderGroup(model);

        cut.FindAll("input[type=checkbox]")[0].Change(true);

        model.Tags.ShouldBe(["a", "b", "zz"]);
    }

    [Fact]
    public void Unchecking_Removes_The_Value()
    {
        var model = new TestFormModel { Tags = ["a", "b"] };
        var cut = RenderGroup(model);

        cut.FindAll("input[type=checkbox]")[0].Change(false);

        model.Tags.ShouldBe(["b"]);
    }

    [Fact]
    public void Should_Treat_Null_Value_As_Empty()
    {
        var model = new TestFormModel();
        var cut = Render<CyCheckboxGroup<string>>(parameters => parameters
            .Add(p => p.Label, "Tags")
            .Add(p => p.Items, Items)
            .Add(p => p.Value, null)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
            .Add(p => p.ValueExpression, () => model.Tags));

        cut.FindAll("input[type=checkbox]")[1].Change(true);

        model.Tags.ShouldBe(["b"]);
    }

    [Fact]
    public void Should_Use_Custom_Comparer()
    {
        var model = new TestFormModel { Tags = ["A"] };
        var cut = RenderGroup(model, configure: p => p.Add(c => c.Comparer, StringComparer.OrdinalIgnoreCase));

        cut.FindAll("input[type=checkbox]")[0].HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Should_Show_EditContext_Error_And_Flag_Inputs_Invalid()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var store = new ValidationMessageStore(editContext);
        store.Add(editContext.Field(nameof(TestFormModel.Tags)), "Choose at least one");
        editContext.NotifyValidationStateChanged();

        var cut = RenderGroup(model, editContext);

        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe("Choose at least one");
        cut.FindAll("input[type=checkbox]").ShouldAllBe(i => i.GetAttribute("aria-invalid") == "true");
        cut.Find("fieldset").GetAttribute("aria-describedby")!.ShouldContain("-error");
    }

    [Fact]
    public void Should_Work_Without_EditContext()
    {
        var model = new TestFormModel();
        var cut = RenderGroup(model);

        cut.FindAll("input[type=checkbox]")[1].Change(true);

        model.Tags.ShouldBe(["b"]);
    }
}
