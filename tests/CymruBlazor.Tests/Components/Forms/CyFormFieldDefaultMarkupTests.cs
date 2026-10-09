using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// Structural snapshots (Implementation Plan item 0.5, TASK-107) of the six form fields that existed before
/// 1.8.0, so a later shared field-chrome refactor (R1) can prove it changes nothing. They pin element order,
/// tag names and static class lists. The root element's state classes, generated ids and values are not part
/// of the contract, so the root is compared by tag name and a few stable classes only.
/// </summary>
public sealed class CyFormFieldDefaultMarkupTests : FormFieldTestContext
{
    private static string Outline(IElement element, bool withClasses = true)
    {
        var classes = withClasses ? string.Join('.', element.ClassList) : string.Empty;
        var name = element.TagName.ToLowerInvariant() + (classes.Length > 0 ? "." + classes : string.Empty);
        var children = element.Children.Select(c => Outline(c)).ToList();

        return children.Count > 0 ? $"{name}[{string.Join(",", children)}]" : name;
    }

    private static EditContext WithError(TestFormModel model, string field)
    {
        var editContext = new EditContext(model);
        var store = new ValidationMessageStore(editContext);
        store.Add(editContext.Field(field), "Something is wrong");
        editContext.NotifyValidationStateChanged();
        return editContext;
    }

    // ------------------------------------------------------------------ CyTextBox

    [Fact]
    public void TextBox_Default_Is_A_Label_Then_An_Input()
    {
        var model = new TestFormModel();

        var cut = Render<CyTextBox>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.Value, model.Text)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(c => c.ValueExpression, () => model.Text));

        var root = cut.Find("div.cy-field");
        Outline(root, withClasses: false).ShouldBe("div[label.cy-field__label,input.cy-textbox]");
    }

    [Fact]
    public void TextBox_With_Hint_Required_And_Error_Keeps_Its_Order()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Text));

        var cut = Render<CyTextBox>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Name")
            .Add(c => c.HintText, "As on your card")
            .Add(c => c.Required, true)
            .Add(c => c.Value, model.Text)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(c => c.ValueExpression, () => model.Text));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[label.cy-field__label[span.cy-field__required-indicator],p.cy-field__hint,input.cy-textbox,p.cy-field__error]");

        var input = cut.Find("input");
        input.GetAttribute("aria-invalid").ShouldBe("true");
        input.GetAttribute("aria-required").ShouldBe("true");
        input.GetAttribute("aria-describedby")!.ShouldContain(cut.Find(".cy-field__error").Id!);
        cut.Find("label").GetAttribute("for").ShouldBe(input.Id);
    }

    // ------------------------------------------------------------------ CyTextArea

    [Fact]
    public void TextArea_Default_Is_A_Label_Then_A_Textarea()
    {
        var model = new TestFormModel();

        var cut = Render<CyTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Value, model.Text)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(c => c.ValueExpression, () => model.Text));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[label.cy-field__label,textarea.cy-textarea]");
    }

    [Fact]
    public void TextArea_With_Count_And_Error_Keeps_Its_Order()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Text));

        var cut = Render<CyTextArea>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Notes")
            .Add(c => c.MaxLength, 100)
            .Add(c => c.ShowCharacterCount, true)
            .Add(c => c.Value, model.Text)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(c => c.ValueExpression, () => model.Text));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[label.cy-field__label,textarea.cy-textarea,p.cy-field__hint.cy-textarea__count,p.cy-field__error]");
    }

    // ------------------------------------------------------------------ CySelect

    [Fact]
    public void Select_With_Items_And_Placeholder_Renders_A_Placeholder_Option_First()
    {
        var model = new TestFormModel();

        var cut = Render<CySelect<string>>(p => p
            .Add(c => c.Label, "Board")
            .Add(c => c.Placeholder, "Choose")
            .Add(c => c.Items, new[] { new CyOption<string>("a", "Alpha"), new CyOption<string>("b", "Beta") })
            .Add(c => c.Value, model.Choice)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(c => c.ValueExpression, () => model.Choice));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[label.cy-field__label,select.cy-select[option,option,option]]");
        cut.FindAll("option").Select(o => o.GetAttribute("value")).ShouldBe(new string?[] { "", "a", "b" });
    }

    [Fact]
    public void Select_With_Error_Appends_The_Error_After_The_Select()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Choice));

        var cut = Render<CySelect<string>>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Board")
            .Add(c => c.Items, new[] { new CyOption<string>("a", "Alpha") })
            .Add(c => c.Value, model.Choice)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(c => c.ValueExpression, () => model.Choice));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[label.cy-field__label,select.cy-select[option],p.cy-field__error]");
    }

    // ------------------------------------------------------------------ CyCheckbox

    [Fact]
    public void Checkbox_Default_Is_An_Input_Then_Its_Label_Inside_One_Wrapper()
    {
        var model = new TestFormModel();

        var cut = Render<CyCheckbox>(p => p
            .Add(c => c.Label, "I agree")
            .Add(c => c.Value, model.Flag)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(c => c.ValueExpression, () => model.Flag));

        var root = cut.Find("div.cy-field");
        root.ClassList.ShouldContain("cy-field--checkbox");
        Outline(root, withClasses: false).ShouldBe("div[div.cy-checkbox[input.cy-checkbox__input,label.cy-checkbox__label]]");
    }

    [Fact]
    public void Checkbox_With_Hint_And_Error_Keeps_Its_Order()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Flag));

        var cut = Render<CyCheckbox>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "I agree")
            .Add(c => c.HintText, "Required to continue")
            .Add(c => c.Value, model.Flag)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(c => c.ValueExpression, () => model.Flag));

        Outline(cut.Find("div.cy-field"), withClasses: false)
            .ShouldBe("div[div.cy-checkbox[input.cy-checkbox__input,label.cy-checkbox__label],p.cy-field__hint,p.cy-field__error]");
    }

    // ------------------------------------------------------------------ CyRadioGroup

    [Fact]
    public void RadioGroup_Is_A_Fieldset_With_A_Legend_And_A_Radios_Wrapper()
    {
        var model = new TestFormModel();

        var cut = Render<CyRadioGroup<string>>(p => p
            .Add(c => c.Label, "Choice")
            .Add(c => c.Items, new[] { new CyOption<string>("a", "Alpha"), new CyOption<string>("b", "Beta") })
            .Add(c => c.Value, model.Choice)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(c => c.ValueExpression, () => model.Choice));

        var root = cut.Find("fieldset.cy-radio-group");
        root.ClassList.ShouldContain("cy-field");
        root.Children.Select(c => c.TagName.ToLowerInvariant() + "." + string.Join('.', c.ClassList))
            .ShouldBe(new[] { "legend.cy-field__label", "div.cy-radios" });
        cut.FindAll(".cy-radios input[type=radio]").Count.ShouldBe(2);
    }

    [Fact]
    public void RadioGroup_With_Hint_And_Error_Keeps_Its_Order()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Choice));

        var cut = Render<CyRadioGroup<string>>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Choice")
            .Add(c => c.HintText, "Pick one")
            .Add(c => c.Items, new[] { new CyOption<string>("a", "Alpha") })
            .Add(c => c.Value, model.Choice)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(c => c.ValueExpression, () => model.Choice));

        cut.Find("fieldset.cy-radio-group").Children
            .Select(c => c.TagName.ToLowerInvariant() + "." + string.Join('.', c.ClassList))
            .ShouldBe(new[] { "legend.cy-field__label", "p.cy-field__hint", "div.cy-radios", "p.cy-field__error" });
    }

    // ------------------------------------------------------------------ CyDateInput

    [Fact]
    public void DateInput_Is_A_Group_Fieldset_With_Day_Month_And_Year_Items()
    {
        var model = new TestFormModel();

        var cut = Render<CyDateInput>(p => p
            .Add(c => c.Label, "Date of birth")
            .Add(c => c.Value, model.Date)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(c => c.ValueExpression, () => model.Date));

        var root = cut.Find("fieldset.cy-date-input-group");
        root.GetAttribute("role").ShouldBe("group");
        Outline(root, withClasses: false).ShouldBe(
            "fieldset[legend.cy-field__label,div.cy-date-input[" +
            "div.cy-date-input__item[label.cy-date-input__label,input.cy-date-input__input.cy-date-input__input--day]," +
            "div.cy-date-input__item[label.cy-date-input__label,input.cy-date-input__input.cy-date-input__input--month]," +
            "div.cy-date-input__item[label.cy-date-input__label,input.cy-date-input__input.cy-date-input__input--year]]]");
    }

    [Fact]
    public void DateInput_With_Hint_And_Error_Keeps_Its_Order()
    {
        var model = new TestFormModel();
        var editContext = WithError(model, nameof(TestFormModel.Date));

        var cut = Render<CyDateInput>(p => p
            .AddCascadingValue(editContext)
            .Add(c => c.Label, "Date of birth")
            .Add(c => c.HintText, "For example, 27 3 2007")
            .Add(c => c.Value, model.Date)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(c => c.ValueExpression, () => model.Date));

        cut.Find("fieldset.cy-date-input-group").Children
            .Select(c => c.TagName.ToLowerInvariant() + "." + string.Join('.', c.ClassList))
            .ShouldBe(new[] { "legend.cy-field__label", "p.cy-field__hint", "div.cy-date-input", "p.cy-field__error" });
    }
}
