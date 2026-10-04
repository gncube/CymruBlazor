using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CySwitchTests : FormFieldTestContext
{
    private IRenderedComponent<CySwitch> RenderSwitch(
        TestFormModel model,
        Action<ComponentParameterCollectionBuilder<CySwitch>>? extra = null) =>
        Render<CySwitch>(parameters =>
        {
            parameters
                .Add(p => p.Label, "Required")
                .Add(p => p.Value, model.Flag)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => model.Flag = v))
                .Add(p => p.ValueExpression, () => model.Flag);
            extra?.Invoke(parameters);
        });

    [Fact]
    public void Should_Render_A_Switch_Button_Named_By_Its_Label()
    {
        var cut = RenderSwitch(new TestFormModel());

        var control = cut.Find("button[role='switch']");
        control.GetAttribute("type").ShouldBe("button");

        var label = cut.Find("label");
        label.TextContent.ShouldContain("Required");
        control.GetAttribute("aria-labelledby").ShouldBe(label.GetAttribute("id"));
        label.GetAttribute("for").ShouldBe(control.GetAttribute("id"));
    }

    [Theory]
    [InlineData(false, "false")]
    [InlineData(true, "true")]
    public void Should_Reflect_The_Initial_State_In_AriaChecked(bool initial, string expected)
    {
        var cut = RenderSwitch(new TestFormModel { Flag = initial });

        cut.Find("[role='switch']").GetAttribute("aria-checked").ShouldBe(expected);
    }

    [Fact]
    public void Should_Toggle_AriaChecked_And_Bound_Value_On_Click()
    {
        var model = new TestFormModel();
        var cut = RenderSwitch(model);

        cut.Find("[role='switch']").Click();

        model.Flag.ShouldBeTrue();
        cut.Find("[role='switch']").GetAttribute("aria-checked").ShouldBe("true");

        cut.Find("[role='switch']").Click();

        model.Flag.ShouldBeFalse();
        cut.Find("[role='switch']").GetAttribute("aria-checked").ShouldBe("false");
    }

    // Space and Enter on a <button> raise a click in the browser; the component relies on that
    // native behaviour instead of handling keys itself, so a click is the keyboard path under test.
    [Fact]
    public void Should_Not_Intercept_Keys_So_The_Native_Button_Behaviour_Applies()
    {
        var cut = RenderSwitch(new TestFormModel());

        cut.Find("[role='switch']").HasAttribute("onkeydown").ShouldBeFalse();
        cut.Find("[role='switch']").TagName.ShouldBe("BUTTON");
    }

    [Fact]
    public void Should_Raise_OnChange_With_The_New_Value()
    {
        bool? received = null;
        var cut = RenderSwitch(new TestFormModel(), p =>
            p.Add(c => c.OnChange, EventCallback.Factory.Create<bool>(this, v => received = v)));

        cut.Find("[role='switch']").Click();

        received.ShouldBe(true);
    }

    [Fact]
    public void Should_Not_Toggle_Or_Raise_Events_When_Disabled()
    {
        var model = new TestFormModel();
        var raised = false;
        var cut = RenderSwitch(model, p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnChange, EventCallback.Factory.Create<bool>(this, _ => raised = true)));

        cut.Find("[role='switch']").HasAttribute("disabled").ShouldBeTrue();
        cut.Find("[role='switch']").Click();

        model.Flag.ShouldBeFalse();
        raised.ShouldBeFalse();
    }

    [Fact]
    public void Should_Describe_The_Switch_By_Its_Hint()
    {
        var cut = RenderSwitch(new TestFormModel(), p => p.Add(c => c.HintText, "Answers cannot be skipped."));

        var hint = cut.Find(".cy-field__hint");
        cut.Find("[role='switch']").GetAttribute("aria-describedby")!.ShouldContain(hint.GetAttribute("id")!);
    }

    [Fact]
    public void Should_Work_Without_An_EditContext()
    {
        var cut = RenderSwitch(new TestFormModel());

        Should.NotThrow(() => cut.Find("[role='switch']").Click());
    }
}
