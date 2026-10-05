using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CySegmentedControlTests : TestContextBase
{
    private static IReadOnlyList<CyOption<string>> Views() =>
    [
        new CyOption<string>("list", "List"),
        new CyOption<string>("grid", "Grid"),
        new CyOption<string>("map", "Map") { Disabled = true }
    ];

    private IRenderedComponent<CySegmentedControl<string>> RenderControl(Action<ComponentParameterCollectionBuilder<CySegmentedControl<string>>>? configure = null) =>
        Render<CySegmentedControl<string>>(p =>
        {
            p.Add(c => c.Label, "View");
            p.Add(c => c.Items, Views());
            configure?.Invoke(p);
        });

    [Fact]
    public void Should_Render_A_Named_Radio_Group()
    {
        var cut = RenderControl();

        var group = cut.Find("[role='radiogroup']");
        group.GetAttribute("aria-label").ShouldBe("View");
        cut.FindAll("input[type='radio']").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Share_One_Radio_Name_Across_Options()
    {
        var cut = RenderControl();

        var names = cut.FindAll("input[type='radio']").Select(i => i.GetAttribute("name")).Distinct().ToList();
        names.Count.ShouldBe(1);
        names[0].ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Should_Use_A_Supplied_Name()
    {
        var cut = RenderControl(p => p.Add(c => c.Name, "view-mode"));

        cut.FindAll("input[type='radio']").ShouldAllBe(i => i.GetAttribute("name") == "view-mode");
    }

    [Fact]
    public void Should_Check_Only_The_Selected_Option()
    {
        var cut = RenderControl(p => p.Add(c => c.Value, "grid"));

        var radios = cut.FindAll("input[type='radio']");
        radios[0].HasAttribute("checked").ShouldBeFalse();
        radios[1].HasAttribute("checked").ShouldBeTrue();
        cut.FindAll(".cy-segmented__option")[1].ClassList.ShouldContain("cy-segmented__option--selected");
    }

    [Fact]
    public void Should_Raise_ValueChanged_When_An_Option_Is_Chosen()
    {
        string? chosen = null;
        var cut = RenderControl(p => p
            .Add(c => c.Value, "list")
            .Add(c => c.ValueChanged, v => chosen = v));

        cut.FindAll("input[type='radio']")[1].Change(true);

        chosen.ShouldBe("grid");
    }

    [Fact]
    public void Should_Disable_Individual_Options()
    {
        var cut = RenderControl();

        var radios = cut.FindAll("input[type='radio']");
        radios[0].HasAttribute("disabled").ShouldBeFalse();
        radios[2].HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void Should_Disable_Every_Option_When_Disabled()
    {
        var cut = RenderControl(p => p.Add(c => c.Disabled, true));

        cut.FindAll("input[type='radio']").ShouldAllBe(i => i.HasAttribute("disabled"));
        cut.Find(".cy-segmented").ClassList.ShouldContain("cy-segmented--disabled");
    }

    [Fact]
    public void Should_Apply_Size_And_FullWidth_Classes()
    {
        var cut = RenderControl(p => p.Add(c => c.Size, ComponentSize.Small).Add(c => c.FullWidth, true));

        cut.Find(".cy-segmented").ClassList.ShouldContain("cy-segmented--sm");
        cut.Find(".cy-segmented").ClassList.ShouldContain("cy-segmented--full");
    }

    [Fact]
    public void Should_Render_Option_Text_As_Encoded_Labels()
    {
        var items = new List<CyOption<string>> { new("a", "<b>A</b>") };
        var cut = Render<CySegmentedControl<string>>(p => p
            .Add(c => c.Label, "View")
            .Add(c => c.Items, items));

        cut.FindAll("b").Count.ShouldBe(0);
        cut.Find(".cy-segmented__label").TextContent.ShouldBe("<b>A</b>");
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() => RenderControl(p => p.Add(c => c.Label, " ")));
    }

    [Fact]
    public void Should_Render_Without_Items()
    {
        var cut = Render<CySegmentedControl<int>>(p => p.Add(c => c.Label, "Empty"));

        cut.FindAll("input").Count.ShouldBe(0);
    }
}
