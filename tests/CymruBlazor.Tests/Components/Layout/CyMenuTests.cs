using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyMenuTests : TestContextBase
{
    public CyMenuTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;
    }

    private static RenderFragment ThreeItems(Action? onSecond = null) => b =>
    {
        b.OpenComponent<CyMenuItem>(0);
        b.AddComponentParameter(1, nameof(CyMenuItem.Text), "Duplicate");
        b.CloseComponent();

        b.OpenComponent<CyMenuItem>(2);
        b.AddComponentParameter(3, nameof(CyMenuItem.Text), "Move");
        b.AddComponentParameter(4, nameof(CyMenuItem.OnClick), EventCallback.Factory.Create(new object(), () => onSecond?.Invoke()));
        b.CloseComponent();

        b.OpenComponent<CyMenuItem>(5);
        b.AddComponentParameter(6, nameof(CyMenuItem.Text), "Remove");
        b.AddComponentParameter(7, nameof(CyMenuItem.Destructive), true);
        b.CloseComponent();
    };

    private IRenderedComponent<CyMenu> RenderMenu(Action<ComponentParameterCollectionBuilder<CyMenu>>? extra = null) =>
        Render<CyMenu>(p =>
        {
            p.Add(c => c.Label, "More actions for Disease status")
             .Add(c => c.ChildContent, ThreeItems());
            extra?.Invoke(p);
        });

    [Fact]
    public void Should_Render_A_Closed_Menu_Button_With_Menu_Button_Semantics()
    {
        var cut = RenderMenu();

        var trigger = cut.Find("button.cy-menu__trigger");
        trigger.GetAttribute("aria-haspopup").ShouldBe("menu");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        trigger.GetAttribute("aria-label").ShouldBe("More actions for Disease status");
        cut.FindAll("[role='menu']").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyMenu>(p => p.Add(c => c.Label, "")));
    }

    [Fact]
    public void Should_Open_On_Click_And_Expose_Menu_Items()
    {
        var cut = RenderMenu();

        cut.Find("button.cy-menu__trigger").Click();

        var trigger = cut.Find("button.cy-menu__trigger");
        trigger.GetAttribute("aria-expanded").ShouldBe("true");
        trigger.GetAttribute("aria-controls").ShouldBe(cut.Find("[role='menu']").GetAttribute("id"));
        cut.FindAll("[role='menuitem']").Count.ShouldBe(3);
        cut.Find("[role='menu']").GetAttribute("aria-labelledby").ShouldBe(trigger.GetAttribute("id"));
    }

    [Fact]
    public void Should_Use_A_Roving_Tabindex_With_Exactly_One_Item_In_The_Tab_Order()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        var items = cut.FindAll("[role='menuitem']");
        items.Count(i => i.GetAttribute("tabindex") == "0").ShouldBe(1);
        items.Count(i => i.GetAttribute("tabindex") == "-1").ShouldBe(2);
    }

    [Fact]
    public void Should_Move_The_Active_Item_With_Arrow_Keys_And_Wrap()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        string Active() => cut.FindAll("[role='menuitem']")
            .Single(i => i.GetAttribute("tabindex") == "0").TextContent.Trim();

        Active().ShouldBe("Duplicate");

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Active().ShouldBe("Move");

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Active().ShouldBe("Remove");

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Active().ShouldBe("Duplicate");

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Active().ShouldBe("Remove");
    }

    [Fact]
    public void Should_Jump_To_First_And_Last_With_Home_And_End()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        string Active() => cut.FindAll("[role='menuitem']")
            .Single(i => i.GetAttribute("tabindex") == "0").TextContent.Trim();

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "End" });
        Active().ShouldBe("Remove");

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "Home" });
        Active().ShouldBe("Duplicate");
    }

    [Fact]
    public void Should_Close_On_Escape()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cut.FindAll("[role='menu']").Count.ShouldBe(0);
        cut.Find("button.cy-menu__trigger").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Open_From_The_Trigger_With_ArrowDown()
    {
        var cut = RenderMenu();

        cut.Find("button.cy-menu__trigger").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        cut.FindAll("[role='menuitem']").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Close_On_Tab()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "Tab" });

        cut.FindAll("[role='menu']").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Invoke_The_Item_Callback_And_Close()
    {
        var invoked = false;
        var cut = Render<CyMenu>(p => p
            .Add(c => c.Label, "Actions")
            .Add(c => c.ChildContent, ThreeItems(() => invoked = true)));
        cut.Find("button.cy-menu__trigger").Click();

        cut.FindAll("[role='menuitem']")[1].Click();

        invoked.ShouldBeTrue();
        cut.FindAll("[role='menu']").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Raise_OpenChanged()
    {
        var states = new List<bool>();
        var cut = RenderMenu(p => p.Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, states.Add)));

        cut.Find("button.cy-menu__trigger").Click();
        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        states.ShouldBe(new[] { true, false });
    }

    [Fact]
    public void Should_Skip_Disabled_Items_And_Not_Activate_Them()
    {
        var activated = false;
        var cut = Render<CyMenu>(p => p
            .Add(c => c.Label, "Actions")
            .Add(c => c.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<CyMenuItem>(0);
                b.AddComponentParameter(1, nameof(CyMenuItem.Text), "First");
                b.CloseComponent();
                b.OpenComponent<CyMenuItem>(2);
                b.AddComponentParameter(3, nameof(CyMenuItem.Text), "Disabled");
                b.AddComponentParameter(4, nameof(CyMenuItem.Disabled), true);
                b.AddComponentParameter(5, nameof(CyMenuItem.OnClick), EventCallback.Factory.Create(new object(), () => activated = true));
                b.CloseComponent();
                b.OpenComponent<CyMenuItem>(6);
                b.AddComponentParameter(7, nameof(CyMenuItem.Text), "Last");
                b.CloseComponent();
            })));
        cut.Find("button.cy-menu__trigger").Click();

        cut.Find("[role='menu']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        cut.FindAll("[role='menuitem']").Single(i => i.GetAttribute("tabindex") == "0").TextContent.Trim().ShouldBe("Last");

        var disabled = cut.FindAll("[role='menuitem']")[1];
        disabled.GetAttribute("aria-disabled").ShouldBe("true");
        disabled.Click();
        activated.ShouldBeFalse();
    }

    [Fact]
    public void Should_Name_A_Text_Trigger_By_Its_Visible_Text()
    {
        var cut = RenderMenu(p => p.Add(c => c.Text, "Actions"));

        var trigger = cut.Find("button.cy-menu__trigger");
        trigger.HasAttribute("aria-label").ShouldBeFalse();
        trigger.TextContent.ShouldContain("Actions");
    }

    [Fact]
    public void Should_Flag_Destructive_Items()
    {
        var cut = RenderMenu();
        cut.Find("button.cy-menu__trigger").Click();

        cut.Find(".cy-menu__item--destructive").TextContent.ShouldContain("Remove");
    }
}
