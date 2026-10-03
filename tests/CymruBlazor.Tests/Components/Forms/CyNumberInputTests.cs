using System.Globalization;
using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyNumberInputTests : FormFieldTestContext
{
    private IRenderedComponent<CyNumberInput<int?>> RenderInt(
        TestFormModel model,
        Action<ComponentParameterCollectionBuilder<CyNumberInput<int?>>>? configure = null,
        EditContext? editContext = null) =>
        Render<CyNumberInput<int?>>(parameters =>
        {
            if (editContext is not null)
            {
                parameters.AddCascadingValue(editContext);
            }

            parameters
                .Add(p => p.Label, "Count")
                .Add(p => p.Value, model.Count)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<int?>(this, v => model.Count = v))
                .Add(p => p.ValueExpression, () => model.Count);
            configure?.Invoke(parameters);
        });

    [Fact]
    public void Should_Render_Text_Input_With_Numeric_Inputmode_For_Integers()
    {
        var cut = RenderInt(new TestFormModel());

        var input = cut.Find("input");
        input.GetAttribute("type").ShouldBe("text");
        input.GetAttribute("inputmode").ShouldBe("numeric");
        input.GetAttribute("autocomplete").ShouldBe("off");
    }

    [Fact]
    public void Should_Use_Decimal_Inputmode_For_Decimals()
    {
        var model = new TestFormModel();

        var cut = Render<CyNumberInput<decimal?>>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.Value, model.Weight)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<decimal?>(this, v => model.Weight = v))
            .Add(p => p.ValueExpression, () => model.Weight));

        cut.Find("input").GetAttribute("inputmode").ShouldBe("decimal");
    }

    [Fact]
    public void Should_Omit_Inputmode_When_Negative_Values_Are_Allowed()
    {
        var cut = RenderInt(new TestFormModel(), p => p.Add(c => c.Min, -5m));

        cut.Find("input").HasAttribute("inputmode").ShouldBeFalse();
    }

    [Fact]
    public void Should_Render_Unit_And_Describe_The_Input_By_It()
    {
        var cut = RenderInt(new TestFormModel(), p => p.Add(c => c.Unit, "kg").Add(c => c.HintText, "Whole kilos"));

        var unit = cut.Find(".cy-input-affix__suffix");
        unit.TextContent.ShouldBe("kg");
        var describedBy = cut.Find("input").GetAttribute("aria-describedby")!.Split(' ');
        describedBy.First().ShouldBe(unit.Id);
        describedBy.Length.ShouldBe(2);
    }

    [Fact]
    public void Should_Write_Parsed_Value()
    {
        var model = new TestFormModel();
        var cut = RenderInt(model);

        cut.Find("input").Change("  7 ");

        model.Count.ShouldBe(7);
    }

    [Fact]
    public void Should_Write_Null_For_Empty_Nullable()
    {
        var model = new TestFormModel { Count = 3 };
        var cut = RenderInt(model);

        cut.Find("input").Change(string.Empty);

        model.Count.ShouldBeNull();
        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Standalone_Should_Show_Parse_Error_And_Not_Write_Value()
    {
        var model = new TestFormModel { Count = 4 };
        var cut = RenderInt(model);

        cut.Find("input").Change("abc");

        model.Count.ShouldBe(4);
        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe("Enter a number");
        cut.Find(".cy-field__error").GetAttribute("role").ShouldBe("alert");
        cut.Find("input").GetAttribute("aria-invalid").ShouldBe("true");
        cut.Find(".cy-field").ClassList.ShouldContain("cy-field--invalid");
        cut.Find("input").GetAttribute("aria-describedby")!.ShouldContain("-error");
    }

    [Fact]
    public void Standalone_Should_Clear_Error_On_Next_Valid_Entry()
    {
        var model = new TestFormModel();
        var cut = RenderInt(model);

        cut.Find("input").Change("abc");
        cut.Find("input").Change("5");

        cut.FindAll(".cy-field__error").ShouldBeEmpty();
        cut.Find(".cy-field").ClassList.ShouldNotContain("cy-field--invalid");
        model.Count.ShouldBe(5);
    }

    [Theory]
    [InlineData("11", "Enter a number that is 10 or less")]
    [InlineData("-1", "Enter a number that is 0 or more")]
    [InlineData("1.5", "Enter a number")]
    public void Should_Reject_Out_Of_Range_Or_Non_Integer(string typed, string message)
    {
        var cut = RenderInt(new TestFormModel(), p => p.Add(c => c.Min, 0m).Add(c => c.Max, 10m));

        cut.Find("input").Change(typed);

        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe(message);
    }

    [Fact]
    public void Should_Use_Custom_Messages()
    {
        var cut = RenderInt(new TestFormModel(), p => p
            .Add(c => c.Max, 10m)
            .Add(c => c.AboveMaxFormat, "Dim mwy na {0}"));

        cut.Find("input").Change("11");

        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe("Dim mwy na 10");
    }

    [Fact]
    public void Should_Enforce_Step_From_Min()
    {
        var model = new TestFormModel();
        var cut = Render<CyNumberInput<decimal?>>(parameters => parameters
            .Add(p => p.Label, "Dose")
            .Add(p => p.Min, 1m)
            .Add(p => p.Step, 0.5m)
            .Add(p => p.Culture, CultureInfo.InvariantCulture)
            .Add(p => p.Value, model.Weight)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<decimal?>(this, v => model.Weight = v))
            .Add(p => p.ValueExpression, () => model.Weight));

        cut.Find("input").Change("1.7");
        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe("Enter a number in steps of 0.5");

        cut.Find("input").Change("2.5");
        model.Weight.ShouldBe(2.5m);
    }

    [Fact]
    public void Should_Read_And_Format_In_The_Given_Culture()
    {
        var model = new TestFormModel { Weight = 2.5m };
        var german = CultureInfo.GetCultureInfo("de-DE");

        var cut = Render<CyNumberInput<decimal?>>(parameters => parameters
            .Add(p => p.Label, "Weight")
            .Add(p => p.Culture, german)
            .Add(p => p.Value, model.Weight)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<decimal?>(this, v => model.Weight = v))
            .Add(p => p.ValueExpression, () => model.Weight));

        cut.Find("input").GetAttribute("value").ShouldBe("2,5");
        cut.Find("input").Change("3,5");
        model.Weight.ShouldBe(3.5m);
    }

    [Fact]
    public void NonNullable_Should_Reject_Empty()
    {
        var value = 1;
        var model = new TestFormModel();

        var cut = Render<CyNumberInput<int>>(parameters => parameters
            .Add(p => p.Label, "Whole")
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => value = v))
            .Add(p => p.ValueExpression, () => value));

        cut.Find("input").Change(string.Empty);

        cut.Find(".cy-field__error").TextContent.Trim().ShouldBe("Enter a number");
        value.ShouldBe(1);
    }

    [Fact]
    public void In_EditForm_The_Message_Lives_On_The_EditContext_And_Shows_Once()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var cut = RenderInt(model, editContext: editContext);

        cut.Find("input").Change("zzz");

        cut.FindAll(".cy-field__error").Count.ShouldBe(1);
        editContext.GetValidationMessages().ShouldContain("Enter a number");

        cut.Find("input").Change("3");
        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Throw_For_Unsupported_Type()
    {
        var text = string.Empty;

        Should.Throw<InvalidOperationException>(() => Render<CyNumberInput<string>>(parameters => parameters
            .Add(p => p.Label, "x")
            .Add(p => p.Value, text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => text = v))
            .Add(p => p.ValueExpression, () => text)));
    }

    [Fact]
    public void Should_Throw_When_Min_Exceeds_Max()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderInt(new TestFormModel(), p => p.Add(c => c.Min, 5m).Add(c => c.Max, 1m)));
    }

    [Fact]
    public void Should_Throw_When_Step_Is_Not_Positive()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderInt(new TestFormModel(), p => p.Add(c => c.Step, 0m)));
    }
}
