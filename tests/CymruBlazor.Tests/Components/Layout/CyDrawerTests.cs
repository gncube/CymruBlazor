using Xunit;
using Shouldly;
using Bunit;
using Moq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

/// <summary>
/// The modal drawer is a native <c>&lt;dialog&gt;</c> opened by <c>cymru-overlay.js</c> (ADR-0001), which is
/// what contains Tab, makes the page inert and returns focus to the opener; bUnit has no browser, so these tests
/// assert the hand-off to the module and the markup. The trap itself is covered by the Playwright suite.
/// </summary>
public sealed class CyDrawerTests : TestContextBase
{
    private const string OverlayModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _overlay;
    private readonly Mock<IFocusManager> _focusManager = new(MockBehavior.Loose);

    public CyDrawerTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _overlay = JSInterop.SetupModule(OverlayModulePath);
        _overlay.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton(_focusManager.Object);
    }

    private static RenderFragment Body => b => b.AddContent(0, "Drawer body");

    private IRenderedComponent<CyDrawer> RenderDrawer(
        bool open,
        Action<ComponentParameterCollectionBuilder<CyDrawer>>? extra = null) =>
        Render<CyDrawer>(p =>
        {
            p.Add(c => c.Title, "Question settings")
             .Add(c => c.Open, open)
             .Add(c => c.ChildContent, Body);
            extra?.Invoke(p);
        });

    // ----------------------------------------------------------------- modal

    [Fact]
    public void Should_Render_A_Dialog_Named_By_Its_Title_When_Modal()
    {
        var cut = RenderDrawer(open: true);

        var dialog = cut.Find("dialog.cy-drawer");
        var title = cut.Find(".cy-drawer__title");
        dialog.GetAttribute("aria-labelledby").ShouldBe(title.GetAttribute("id"));
        title.TextContent.Trim().ShouldBe("Question settings");
        cut.Find(".cy-drawer__body").TextContent.ShouldContain("Drawer body");
    }

    [Fact]
    public void Should_Render_No_Panel_Content_While_Closed()
    {
        var cut = RenderDrawer(open: false);

        cut.FindAll(".cy-drawer__panel").Count.ShouldBe(0);
        cut.Find("dialog").HasAttribute("aria-labelledby").ShouldBeFalse();
    }

    [Fact]
    public void Should_Hand_The_Dialog_To_The_Overlay_Module_When_Opened()
    {
        var cut = RenderDrawer(open: false);
        _overlay.VerifyNotInvoke("showDialog");

        cut.Render(p => p.Add(c => c.Open, true));

        cut.WaitForAssertion(() => _overlay.VerifyInvoke("showDialog"));
    }

    [Fact]
    public void Should_Release_The_Overlay_And_Raise_OnClosed_When_Closed_By_The_Parent()
    {
        // Loose-mode module calls return 0 for the token, so drive the token through a real value.
        _overlay.Setup<int>("showDialog", _ => true).SetResult(7);

        var closed = 0;
        var cut = RenderDrawer(open: true, p => p.Add(c => c.OnClosed, EventCallback.Factory.Create(this, () => closed++)));

        cut.Render(p => p.Add(c => c.Open, false));

        cut.WaitForAssertion(() => _overlay.VerifyInvoke("disposeDialog"));
        closed.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Close_When_The_Native_Dialog_Reports_A_Close()
    {
        var states = new List<bool>();
        var cut = RenderDrawer(open: true, p => p.Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        await cut.InvokeAsync(() => cut.Instance.OnNativeClosed());

        states.Count.ShouldBe(1);
        states[0].ShouldBeFalse();
    }

    [Fact]
    public void Should_Close_From_The_Close_Button()
    {
        var states = new List<bool>();
        var cut = RenderDrawer(open: true, p => p.Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        cut.Find("button.cy-drawer__close").Click();

        states.Count.ShouldBe(1);
        states[0].ShouldBeFalse();
    }

    [Fact]
    public void Should_Give_The_Close_Button_An_Accessible_Name()
    {
        var cut = RenderDrawer(open: true);
        cut.Find("button.cy-drawer__close").GetAttribute("aria-label").ShouldNotBeNullOrWhiteSpace();

        var custom = RenderDrawer(open: true, p => p.Add(c => c.CloseLabel, "Cau'r panel"));
        custom.Find("button.cy-drawer__close").GetAttribute("aria-label").ShouldBe("Cau'r panel");
    }

    [Fact]
    public void Should_Omit_The_Close_Button_When_Not_Dismissible()
    {
        var cut = RenderDrawer(open: true, p => p.Add(c => c.Dismissible, false));

        cut.FindAll("button.cy-drawer__close").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_The_Footer_Only_When_Provided()
    {
        RenderDrawer(open: true).FindAll(".cy-drawer__footer").Count.ShouldBe(0);

        var cut = RenderDrawer(open: true, p => p.Add(c => c.Footer, (RenderFragment)(b => b.AddContent(0, "Save"))));

        cut.Find(".cy-drawer__footer").TextContent.ShouldBe("Save");
    }

    // ----------------------------------------------------------------- sides, sizes, validation

    [Theory]
    [InlineData(DrawerSide.Right, "cy-drawer--right")]
    [InlineData(DrawerSide.Left, "cy-drawer--left")]
    public void Should_Apply_The_Side_Modifier(DrawerSide side, string expected)
    {
        var cut = RenderDrawer(open: true, p => p.Add(c => c.Side, side));

        cut.Find(".cy-drawer").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData(ComponentSize.Small, "cy-drawer--sm")]
    [InlineData(ComponentSize.Medium, "cy-drawer--md")]
    [InlineData(ComponentSize.Large, "cy-drawer--lg")]
    public void Should_Apply_The_Size_Modifier(ComponentSize size, string expected)
    {
        var cut = RenderDrawer(open: true, p => p.Add(c => c.Size, size));

        cut.Find(".cy-drawer").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Should_Reject_An_Empty_Title()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyDrawer>(p => p.Add(c => c.Title, " ")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Should_Reject_An_Invalid_HeadingLevel(int level)
    {
        Should.Throw<InvalidOperationException>(() => RenderDrawer(open: true, p => p.Add(c => c.HeadingLevel, level)));
    }

    // ----------------------------------------------------------------- non-modal

    [Fact]
    public void Should_Render_An_Aside_Only_While_Open_When_Not_Modal()
    {
        var closed = RenderDrawer(open: false, p => p.Add(c => c.Modal, false));
        closed.FindAll("aside").Count.ShouldBe(0);
        closed.FindAll("dialog").Count.ShouldBe(0);

        var open = RenderDrawer(open: true, p => p.Add(c => c.Modal, false));
        var aside = open.Find("aside.cy-drawer--inline");
        aside.GetAttribute("tabindex").ShouldBe("-1");
        aside.GetAttribute("aria-labelledby").ShouldBe(open.Find(".cy-drawer__title").GetAttribute("id"));
    }

    [Fact]
    public void Should_Move_Focus_Into_The_Non_Modal_Drawer_When_It_Opens()
    {
        var cut = RenderDrawer(open: true, p => p.Add(c => c.Modal, false));

        var id = cut.Find("aside").GetAttribute("id")!;
        cut.WaitForAssertion(() => _focusManager.Verify(
            m => m.FocusAsync(id, It.IsAny<FocusOptions?>(), It.IsAny<CancellationToken>()),
            Times.Once));
    }

    [Fact]
    public void Should_Restore_Focus_When_The_Non_Modal_Drawer_Closes()
    {
        var cut = RenderDrawer(open: true, p => p.Add(c => c.Modal, false));

        cut.Render(p => p.Add(c => c.Open, false));

        cut.WaitForAssertion(() => _focusManager.Verify(m => m.RestoreFocusAsync(It.IsAny<CancellationToken>()), Times.Once));
    }

    [Fact]
    public void Should_Close_The_Non_Modal_Drawer_On_Escape()
    {
        var states = new List<bool>();
        var cut = RenderDrawer(open: true, p => p
            .Add(c => c.Modal, false)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        cut.Find("aside").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        states.Count.ShouldBe(1);
        states[0].ShouldBeFalse();
    }

    [Fact]
    public void Should_Ignore_Escape_When_Not_Dismissible()
    {
        var states = new List<bool>();
        var cut = RenderDrawer(open: true, p => p
            .Add(c => c.Modal, false)
            .Add(c => c.Dismissible, false)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        cut.Find("aside").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        states.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Not_Trap_Focus_In_The_Non_Modal_Drawer()
    {
        // A non-modal drawer leaves the page usable, so it must not ask the focus manager to contain Tab.
        RenderDrawer(open: true, p => p.Add(c => c.Modal, false));

        _focusManager.Verify(
            m => m.TrapAsync(It.IsAny<string>(), It.IsAny<FocusTrapOptions?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
