using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyToolbarTests : TestContextBase
{
    private const string EditingModule = "./_content/CymruBlazor/js/cymru-editing.js";

    private readonly BunitJSModuleInterop _module;

    public CyToolbarTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(EditingModule);
        _module.Mode = JSRuntimeMode.Loose;
    }

    private static RenderFragment Buttons => b =>
    {
        b.OpenElement(0, "button");
        b.AddContent(1, "Undo");
        b.CloseElement();
        b.OpenElement(2, "button");
        b.AddContent(3, "Redo");
        b.CloseElement();
    };

    [Fact]
    public void Should_Render_A_Toolbar_With_An_Accessible_Name()
    {
        var cut = Render<CyToolbar>(p => p
            .Add(c => c.Label, "Designer actions")
            .Add(c => c.ChildContent, Buttons));

        var toolbar = cut.Find("[role='toolbar']");
        toolbar.GetAttribute("aria-label").ShouldBe("Designer actions");
        toolbar.GetAttribute("aria-orientation").ShouldBe("horizontal");
        toolbar.ClassList.ShouldContain("cy-toolbar");
        toolbar.QuerySelectorAll("button").Length.ShouldBe(2);
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyToolbar>(p => p.Add(c => c.Label, " ")));
    }

    [Fact]
    public void Should_Report_Vertical_Orientation()
    {
        var cut = Render<CyToolbar>(p => p
            .Add(c => c.Label, "Tools")
            .Add(c => c.Orientation, Orientation.Vertical));

        var toolbar = cut.Find("[role='toolbar']");
        toolbar.GetAttribute("aria-orientation").ShouldBe("vertical");
        toolbar.ClassList.ShouldContain("cy-toolbar--vertical");
    }

    [Theory]
    [InlineData(StickyPosition.Top, "cy-toolbar--sticky-top")]
    [InlineData(StickyPosition.Bottom, "cy-toolbar--sticky-bottom")]
    public void Should_Apply_The_Sticky_Modifier(StickyPosition position, string expected)
    {
        var cut = Render<CyToolbar>(p => p
            .Add(c => c.Label, "Tools")
            .Add(c => c.Sticky, position));

        cut.Find("[role='toolbar']").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Should_Not_Be_Sticky_By_Default()
    {
        var cut = Render<CyToolbar>(p => p.Add(c => c.Label, "Tools"));

        cut.Find("[role='toolbar']").ClassList.ShouldNotContain("cy-toolbar--sticky-top");
        cut.Find("[role='toolbar']").ClassList.ShouldNotContain("cy-toolbar--sticky-bottom");
    }

    [Fact]
    public void Should_Attach_The_Roving_Focus_Script_After_Render()
    {
        var cut = Render<CyToolbar>(p => p.Add(c => c.Label, "Tools").Add(c => c.ChildContent, Buttons));

        cut.WaitForAssertion(() => _module.VerifyInvoke("attachToolbar"));
    }

    [Fact]
    public void Should_Keep_Rendering_When_The_Script_Is_Unavailable()
    {
        // No module set up and strict mode: the import fails, the toolbar must still render.
        using var strict = new CyToolbarTestsNoScript();
        var cut = strict.RenderToolbar();

        cut.Find("[role='toolbar']").ShouldNotBeNull();
    }

    private sealed class CyToolbarTestsNoScript : TestContextBase
    {
        public IRenderedComponent<CyToolbar> RenderToolbar()
        {
            JSInterop.Mode = JSRuntimeMode.Strict;
            return Render<CyToolbar>(p => p.Add(c => c.Label, "Tools"));
        }
    }
}
