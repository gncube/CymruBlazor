using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// 1.8.0 (Phase B3): the fields work with no cascaded <c>EditContext</c>.
/// Spike 0.4 showed <c>InputBase</c> tolerates a missing one in .NET 10; the
/// library's own <c>HasValidationError</c> used to throw.
/// </summary>
public sealed class StandaloneFieldTests : FormFieldTestContext
{
    [Fact]
    public void CyTextBox_Renders_Without_EditContext()
    {
        var model = new TestFormModel();

        var cut = Render<CyTextBox>(parameters => parameters
            .Add(p => p.Label, "Full name")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        var input = cut.Find("input");
        input.GetAttribute("aria-invalid").ShouldBe("false");
        cut.FindAll(".cy-field__error").ShouldBeEmpty();
    }

    [Fact]
    public void CyTextBox_Writes_Typed_Value_Back_Without_EditContext()
    {
        var model = new TestFormModel();

        var cut = Render<CyTextBox>(parameters => parameters
            .Add(p => p.Label, "Full name")
            .Add(p => p.Value, model.Text)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Text = v))
            .Add(p => p.ValueExpression, () => model.Text));

        cut.Find("input").Change("Ann");

        model.Text.ShouldBe("Ann");
    }

    [Fact]
    public void CyCheckbox_Renders_Without_EditContext()
    {
        var model = new TestFormModel();

        var cut = Render<CyCheckbox>(parameters => parameters
            .Add(p => p.Label, "I agree")
            .Add(p => p.Value, model.Flag)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
            .Add(p => p.ValueExpression, () => model.Flag));

        cut.Find("input[type=checkbox]").ShouldNotBeNull();
    }

    [Fact]
    public void CyDateInput_Renders_Without_EditContext()
    {
        var model = new TestFormModel();

        var cut = Render<CyDateInput>(parameters => parameters
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, new DateOnly(2000, 1, 2))
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        cut.FindAll("input").Count.ShouldBeGreaterThan(0);
    }
}
