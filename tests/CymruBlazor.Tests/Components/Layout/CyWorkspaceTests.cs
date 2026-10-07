using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyWorkspaceTests : TestContextBase
{
    private static RenderFragment Panes => b =>
    {
        b.OpenComponent<CyWorkspacePane>(0);
        b.AddComponentParameter(1, nameof(CyWorkspacePane.Title), "Palette");
        b.AddComponentParameter(2, nameof(CyWorkspacePane.Collapsible), true);
        b.AddComponentParameter(3, nameof(CyWorkspacePane.Width), "16rem");
        b.AddComponentParameter(4, nameof(CyWorkspacePane.ChildContent),
            (RenderFragment)(c => c.AddContent(0, "Palette content")));
        b.CloseComponent();

        b.OpenComponent<CyWorkspacePane>(5);
        b.AddComponentParameter(6, nameof(CyWorkspacePane.Title), "Canvas");
        b.AddComponentParameter(7, nameof(CyWorkspacePane.Fill), true);
        b.AddComponentParameter(8, nameof(CyWorkspacePane.ChildContent),
            (RenderFragment)(c => c.AddContent(0, "Canvas content")));
        b.CloseComponent();
    };

    private IRenderedComponent<CyWorkspacePane> RenderPane(
        Action<ComponentParameterCollectionBuilder<CyWorkspacePane>>? extra = null) =>
        Render<CyWorkspacePane>(p =>
        {
            p.Add(c => c.Title, "Inspector")
             .Add(c => c.ChildContent, (RenderFragment)(b => b.AddContent(0, "Inspector content")));
            extra?.Invoke(p);
        });

    [Fact]
    public void Should_Render_Panes_Inside_The_Workspace()
    {
        var cut = Render<CyWorkspace>(p => p.Add(c => c.ChildContent, Panes));

        cut.Find("div.cy-workspace").ShouldNotBeNull();
        cut.FindAll("section.cy-workspace-pane").Count.ShouldBe(2);
    }

    [Fact]
    public void Should_Name_Each_Pane_By_Its_Title_Heading()
    {
        var cut = Render<CyWorkspace>(p => p.Add(c => c.ChildContent, Panes));

        foreach (var section in cut.FindAll("section.cy-workspace-pane"))
        {
            var heading = section.QuerySelector(".cy-workspace-pane__title")!;
            section.GetAttribute("aria-labelledby").ShouldBe(heading.GetAttribute("id"));
        }
    }

    [Fact]
    public void Should_Expand_The_Fill_Pane_And_Size_The_Others()
    {
        var cut = Render<CyWorkspace>(p => p.Add(c => c.ChildContent, Panes));

        var panes = cut.FindAll("section.cy-workspace-pane");
        var style = panes[0].GetAttribute("style")!;

        style.Replace(" ", string.Empty)
            .ShouldContain("--cy-pane-width:16rem");
        panes[0].ClassList.ShouldNotContain("cy-workspace-pane--fill");
        panes[1].ClassList.ShouldContain("cy-workspace-pane--fill");
        (panes[1].GetAttribute("style") ?? string.Empty).ShouldNotContain("--cy-pane-width");
    }

    [Fact]
    public void Should_Apply_The_Minimum_Width_As_A_Token_Driven_Variable()
    {
        var cut = RenderPane(p => p.Add(c => c.MinWidth, "14rem"));

        var style = cut.Find("section").GetAttribute("style")!;

        style.Replace(" ", string.Empty)
            .ShouldContain("--cy-pane-min-width:14rem");
    }

    [Theory]
    [InlineData("16rem")]
    [InlineData("320px")]
    [InlineData("30%")]
    [InlineData("12.5rem")]
    public void Should_Accept_Simple_CSS_Lengths(string width)
    {
        Should.NotThrow(() => RenderPane(p => p.Add(c => c.Width, width)));
    }

    // Width and MinWidth are written into a style attribute, so anything that is not a plain length is refused.
    [Theory]
    [InlineData("16rem; background:url(x)")]
    [InlineData("calc(100vw - 2rem)")]
    [InlineData("red")]
    [InlineData("")]
    public void Should_Reject_Values_That_Are_Not_Plain_Lengths(string width)
    {
        Should.Throw<InvalidOperationException>(() => RenderPane(p => p.Add(c => c.Width, width)));
        Should.Throw<InvalidOperationException>(() => RenderPane(p => p.Add(c => c.MinWidth, width)));
    }

    [Fact]
    public void Should_Reject_An_Empty_Title()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyWorkspacePane>(p => p.Add(c => c.Title, " ")));
    }

    // ----------------------------------------------------------------- collapsing

    [Fact]
    public void Should_Not_Render_A_Toggle_Unless_Collapsible()
    {
        RenderPane().FindAll(".cy-workspace-pane__toggle").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Expose_The_Toggle_State_And_The_Region_It_Controls()
    {
        var cut = RenderPane(p => p.Add(c => c.Collapsible, true));

        var toggle = cut.Find(".cy-workspace-pane__toggle");
        toggle.GetAttribute("aria-expanded").ShouldBe("true");
        toggle.GetAttribute("aria-label").ShouldBe("Collapse Inspector");
        toggle.GetAttribute("aria-controls").ShouldBe(cut.Find(".cy-workspace-pane__body").GetAttribute("id"));
    }

    [Fact]
    public void Should_Collapse_And_Expand_When_The_Toggle_Is_Pressed()
    {
        var cut = RenderPane(p => p.Add(c => c.Collapsible, true));

        cut.Find(".cy-workspace-pane__toggle").Click();

        cut.Find("section").ClassList.ShouldContain("cy-workspace-pane--collapsed");
        cut.Find(".cy-workspace-pane__body").HasAttribute("hidden").ShouldBeTrue();
        cut.Find(".cy-workspace-pane__toggle").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find(".cy-workspace-pane__toggle").GetAttribute("aria-label").ShouldBe("Expand Inspector");

        cut.Find(".cy-workspace-pane__toggle").Click();

        cut.Find("section").ClassList.ShouldNotContain("cy-workspace-pane--collapsed");
        cut.Find(".cy-workspace-pane__body").HasAttribute("hidden").ShouldBeFalse();
    }

    [Fact]
    public void Should_Keep_Collapsed_Content_In_The_DOM_So_Component_State_Survives()
    {
        var cut = RenderPane(p => p
            .Add(c => c.Collapsible, true)
            .Add(c => c.Collapsed, true));

        cut.Find(".cy-workspace-pane__body").TextContent.ShouldContain("Inspector content");
    }

    [Fact]
    public void Should_Raise_CollapsedChanged()
    {
        var states = new List<bool>();
        var cut = RenderPane(p => p
            .Add(c => c.Collapsible, true)
            .Add(c => c.CollapsedChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        cut.Find(".cy-workspace-pane__toggle").Click();

        states.Count.ShouldBe(1);
        states[0].ShouldBeTrue();
    }

    [Fact]
    public void Should_Ignore_Collapsed_When_Not_Collapsible()
    {
        var cut = RenderPane(p => p.Add(c => c.Collapsed, true));

        cut.Find("section").ClassList.ShouldNotContain("cy-workspace-pane--collapsed");
        cut.Find(".cy-workspace-pane__body").HasAttribute("hidden").ShouldBeFalse();
    }

    [Fact]
    public void Should_Hide_Header_Actions_While_Collapsed()
    {
        var cut = RenderPane(p => p
            .Add(c => c.Collapsible, true)
            .Add(c => c.Collapsed, true)
            .Add(c => c.Actions, (RenderFragment)(b => b.AddContent(0, "Add"))));

        cut.FindAll(".cy-workspace-pane__actions").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Use_Consumer_Supplied_Toggle_Labels()
    {
        var cut = RenderPane(p => p
            .Add(c => c.Collapsible, true)
            .Add(c => c.CollapseLabelFormat, "Cuddio {0}"));

        cut.Find(".cy-workspace-pane__toggle").GetAttribute("aria-label").ShouldBe("Cuddio Inspector");
    }

    [Fact]
    public void Should_Meet_The_Minimum_Target_Size_For_The_Toggle()
    {
        var css = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "CymruBlazor", "wwwroot", "css", "components", "editing.css"));

        var start = css.IndexOf(".cy-workspace-pane__toggle {", StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var block = css[start..css.IndexOf('}', start)];

        block.ShouldContain("min-inline-size: 2.75rem");
        block.ShouldContain("min-block-size: 2.75rem");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CymruBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not locate CymruBlazor.slnx above '{AppContext.BaseDirectory}'.");
    }
}
