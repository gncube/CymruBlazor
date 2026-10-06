using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyStepperTests : TestContextBase
{
    private static IReadOnlyList<CyStepItem> ThreeSteps() =>
    [
        new CyStepItem("Patient") { Description = "Who is the order for?" },
        new CyStepItem("Test"),
        new CyStepItem("Review")
    ];

    private IRenderedComponent<CyStepper> RenderStepper(Action<ComponentParameterCollectionBuilder<CyStepper>>? configure = null) =>
        Render<CyStepper>(p =>
        {
            p.Add(c => c.Label, "Order progress");
            p.Add(c => c.Steps, ThreeSteps());
            configure?.Invoke(p);
        });

    [Fact]
    public void Should_Render_A_Named_Navigation_Landmark_With_An_Ordered_List()
    {
        var cut = RenderStepper();

        cut.Find("nav.cy-stepper").GetAttribute("aria-label").ShouldBe("Order progress");
        cut.FindAll("ol.cy-stepper__list > li").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Mark_Only_The_Current_Step_With_Aria_Current()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        var current = cut.FindAll("[aria-current='step']");
        current.Count.ShouldBe(1);
        current[0].TextContent.ShouldContain("Test");
    }

    [Fact]
    public void Should_Work_Out_Status_From_The_Current_Index()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        var items = cut.FindAll(".cy-stepper__item");
        items[0].ClassList.ShouldContain("cy-stepper__item--complete");
        items[1].ClassList.ShouldContain("cy-stepper__item--current");
        items[2].ClassList.ShouldContain("cy-stepper__item--upcoming");
    }

    [Fact]
    public void Should_Speak_Status_As_Hidden_Text_Except_For_The_Current_Step()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        var items = cut.FindAll(".cy-stepper__item");
        items[0].QuerySelector(".u-sr-only")!.TextContent.ShouldContain("completed");
        items[1].QuerySelector(".u-sr-only").ShouldBeNull();
        items[2].QuerySelector(".u-sr-only")!.TextContent.ShouldContain("not started");
    }

    [Fact]
    public void Should_Flag_An_Error_Step_In_Words_And_Keep_The_Current_Mark()
    {
        var steps = new List<CyStepItem>
        {
            new("Patient") { Status = CyStepStatus.Error },
            new("Test")
        };
        var cut = Render<CyStepper>(p => p
            .Add(c => c.Label, "Progress")
            .Add(c => c.Steps, steps)
            .Add(c => c.Current, 0));

        var first = cut.FindAll(".cy-stepper__item")[0];
        first.ClassList.ShouldContain("cy-stepper__item--error");
        first.QuerySelector(".u-sr-only")!.TextContent.ShouldContain("has an error");
        first.QuerySelector("[aria-current='step']").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Render_Plain_Text_Steps_By_Default()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        cut.FindAll("button").Count.ShouldBe(0);
        cut.FindAll("a").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Make_Completed_And_Current_Steps_Buttons_When_Clickable()
    {
        var cut = RenderStepper(p => p.Add(c => c.Clickable, true).Add(c => c.Current, 1));

        cut.FindAll("button.cy-stepper__step").Count.ShouldBe(2);
        cut.FindAll(".cy-stepper__item")[2].QuerySelector("button").ShouldBeNull();
    }

    [Fact]
    public void Should_Allow_Upcoming_Steps_Only_With_AllowForwardNavigation()
    {
        var cut = RenderStepper(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.AllowForwardNavigation, true)
            .Add(c => c.Current, 0));

        cut.FindAll("button.cy-stepper__step").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Select_A_Step_And_Raise_Both_Callbacks_In_Order()
    {
        var log = new List<string>();
        var cut = RenderStepper(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.Current, 2)
            .Add(c => c.CurrentChanged, i => log.Add($"changed:{i}"))
            .Add(c => c.OnStepSelected, i => log.Add($"selected:{i}")));

        cut.FindAll("button.cy-stepper__step")[0].Click();

        log.Count.ShouldBe(2);
        log[0].ShouldBe("changed:0");
        log[1].ShouldBe("selected:0");
        cut.Find("[aria-current='step']").TextContent.ShouldContain("Patient");
    }

    [Fact]
    public void Should_Not_Make_A_Disabled_Step_Selectable()
    {
        var steps = new List<CyStepItem> { new("One") { Disabled = true }, new("Two") };
        var cut = Render<CyStepper>(p => p
            .Add(c => c.Label, "Progress")
            .Add(c => c.Steps, steps)
            .Add(c => c.Clickable, true)
            .Add(c => c.Current, 1));

        cut.FindAll("button.cy-stepper__step").Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Render_A_Link_For_A_Step_With_Href()
    {
        var steps = new List<CyStepItem> { new("One") { Href = "/order/1" }, new("Two") };
        var cut = Render<CyStepper>(p => p
            .Add(c => c.Label, "Progress")
            .Add(c => c.Steps, steps)
            .Add(c => c.Clickable, true)
            .Add(c => c.Current, 1));

        cut.Find("a.cy-stepper__step").GetAttribute("href").ShouldBe("/order/1");
    }

    [Fact]
    public void Should_Render_Markers_As_Numbers_Or_Icons_Hidden_From_Assistive_Technology()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        var markers = cut.FindAll(".cy-stepper__marker");
        markers.ShouldAllBe(m => m.GetAttribute("aria-hidden") == "true");
        markers[0].QuerySelector("svg").ShouldNotBeNull();
        markers[1].TextContent.Trim().ShouldBe("2");
        markers[2].TextContent.Trim().ShouldBe("3");
    }

    [Fact]
    public void Should_Render_A_Compact_Summary_For_The_Horizontal_Layout()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 1));

        cut.Find(".cy-stepper__summary").TextContent.ShouldBe("Step 2 of 3: Test");
    }

    [Fact]
    public void Should_Use_Localised_Summary_Text()
    {
        var cut = RenderStepper(p => p
            .Add(c => c.Current, 0)
            .Add(c => c.Text, new CyStepperText { StepOfFormat = "Cam {0} o {1}: {2}" }));

        cut.Find(".cy-stepper__summary").TextContent.ShouldBe("Cam 1 o 3: Patient");
    }

    [Fact]
    public void Should_Use_The_Vertical_Layout_Without_A_Summary()
    {
        var cut = RenderStepper(p => p.Add(c => c.Orientation, Orientation.Vertical));

        cut.Find(".cy-stepper").ClassList.ShouldContain("cy-stepper--vertical");
        cut.FindAll(".cy-stepper__summary").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Show_Descriptions()
    {
        RenderStepper().Find(".cy-stepper__description").TextContent.ShouldBe("Who is the order for?");
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyStepper>(p =>
            p.Add(c => c.Label, " ")
             .Add(c => c.Steps, ThreeSteps())));
    }

    [Fact]
    public void Should_Reject_A_Step_Without_A_Title()
    {
        var steps = new List<CyStepItem> { new(" ") };

        Should.Throw<InvalidOperationException>(() => Render<CyStepper>(p => p
            .Add(c => c.Label, "Progress")
            .Add(c => c.Steps, steps)));
    }

    [Fact]
    public void Should_Render_All_Steps_Complete_When_Current_Is_Past_The_End()
    {
        var cut = RenderStepper(p => p.Add(c => c.Current, 3));

        cut.FindAll(".cy-stepper__item--complete").Count.ShouldBe(3);
        cut.FindAll("[aria-current='step']").Count.ShouldBe(0);
        cut.FindAll(".cy-stepper__summary").Count.ShouldBe(0);
    }
}
