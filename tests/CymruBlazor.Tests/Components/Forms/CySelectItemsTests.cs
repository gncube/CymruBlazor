using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>1.8.0 (Phase B5): the <c>Items</c> API on CySelect and CyRadioGroup, plus the option helpers.</summary>
public sealed class CySelectItemsTests : FormFieldTestContext
{
    private IRenderedComponent<CySelect<string>> RenderSelect(
        TestFormModel model, IEnumerable<CyOption<string>>? items, string? placeholder = null, RenderFragment? children = null) =>
        Render<CySelect<string>>(parameters => parameters
            .AddCascadingValue(CreateEditContext(model))
            .Add(p => p.Label, "Country")
            .Add(p => p.Items, items)
            .Add(p => p.Placeholder, placeholder)
            .Add(p => p.ChildContent, children)
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice));

    [Fact]
    public void Should_Render_One_Option_Per_Item_With_Value_And_Text()
    {
        var cut = RenderSelect(new TestFormModel(), [new("wales", "Wales"), new("eng", "England")]);

        var options = cut.FindAll("option");
        options.Select(o => o.GetAttribute("value")).ShouldBe(["wales", "eng"]);
        options.Select(o => o.TextContent).ShouldBe(["Wales", "England"]);
    }

    [Fact]
    public void Should_Render_Placeholder_First_With_Empty_Value()
    {
        var cut = RenderSelect(new TestFormModel(), [new("wales", "Wales")], placeholder: "Choose a country");

        var first = cut.FindAll("option")[0];
        first.TextContent.ShouldBe("Choose a country");
        (first.GetAttribute("value") ?? string.Empty).ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Not_Render_Placeholder_When_Not_Set()
    {
        var cut = RenderSelect(new TestFormModel(), [new("wales", "Wales")]);

        cut.FindAll("option").Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Disable_Items_Marked_Disabled()
    {
        var cut = RenderSelect(new TestFormModel(), [new("a", "A"), new("b", "B") { Disabled = true }]);

        var options = cut.FindAll("option");
        options[0].HasAttribute("disabled").ShouldBeFalse();
        options[1].HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void Should_Render_Items_Before_ChildContent()
    {
        RenderFragment children = b =>
        {
            b.OpenElement(0, "option");
            b.AddAttribute(1, "value", "z");
            b.AddContent(2, "Zed");
            b.CloseElement();
        };

        var cut = RenderSelect(new TestFormModel(), [new("a", "A")], children: children);

        cut.FindAll("option").Select(o => o.TextContent).ShouldBe(["A", "Zed"]);
    }

    [Fact]
    public void Should_Bind_Selected_Item_Value()
    {
        var model = new TestFormModel();
        var cut = RenderSelect(model, [new("wales", "Wales"), new("eng", "England")]);

        cut.Find("select").Change("eng");

        model.Choice.ShouldBe("eng");
    }

    [Fact]
    public void Should_Round_Trip_Enum_Values()
    {
        var chosen = DayOfWeek.Monday;
        var model = new TestFormModel();

        var cut = Render<CySelect<DayOfWeek>>(parameters => parameters
            .AddCascadingValue(CreateEditContext(model))
            .Add(p => p.Label, "Day")
            .Add(p => p.Items, CyOptions.FromEnum<DayOfWeek>())
            .Add(p => p.Value, chosen)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DayOfWeek>(this, v => chosen = v))
            .Add(p => p.ValueExpression, () => chosen));

        cut.FindAll("option").Count.ShouldBe(7);
        cut.Find("select").Change("Friday");
        chosen.ShouldBe(DayOfWeek.Friday);
    }

    [Fact]
    public void RadioGroup_Should_Render_One_Radio_Per_Item_With_Hint_And_Disabled()
    {
        var model = new TestFormModel();
        IEnumerable<CyOption<string>> items =
        [
            new("a", "Alpha") { Hint = "First" },
            new("b", "Beta") { Disabled = true }
        ];

        var cut = Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(CreateEditContext(model))
            .Add(p => p.Label, "Pick one")
            .Add(p => p.Items, items)
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice));

        var radios = cut.FindAll("input[type=radio]");
        radios.Count.ShouldBe(2);
        radios[0].HasAttribute("disabled").ShouldBeFalse();
        radios[1].HasAttribute("disabled").ShouldBeTrue();
        cut.Markup.ShouldContain("First");
    }

    [Fact]
    public void RadioGroup_Should_Bind_Selected_Item()
    {
        var model = new TestFormModel();

        var cut = Render<CyRadioGroup<string>>(parameters => parameters
            .AddCascadingValue(CreateEditContext(model))
            .Add(p => p.Label, "Pick one")
            .Add(p => p.Items, new CyOption<string>[] { new("a", "Alpha"), new("b", "Beta") })
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice));

        cut.FindAll("input[type=radio]")[1].Change("b");

        model.Choice.ShouldBe("b");
    }

    [Fact]
    public void ToCyOptions_Projects_Value_Text_Hint_And_Disabled()
    {
        var source = new[] { (Id: 1, Name: "one", Note: (string?)"n"), (Id: 2, Name: "two", Note: null) };

        var options = source.ToCyOptions(s => s.Id, s => s.Name, s => s.Note, s => s.Id == 2);

        options.Count.ShouldBe(2);
        options[0].ShouldBe(new CyOption<int>(1, "one") { Hint = "n" });
        options[1].Disabled.ShouldBeTrue();
    }

    [Fact]
    public void FromEnum_Uses_Names_Unless_Text_Is_Supplied()
    {
        CyOptions.FromEnum<DayOfWeek>()[0].Text.ShouldBe("Sunday");
        CyOptions.FromEnum<DayOfWeek>(d => d.ToString().ToUpperInvariant())[1].Text.ShouldBe("MONDAY");
    }
}
