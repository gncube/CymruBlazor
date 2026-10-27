using Bunit;
using Shouldly;
using Xunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyUserNavTests : TestContextBase
{
    public CyUserNavTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Should_Render_Unauthenticated_State_Button_By_Default()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false));

        cut.Find(".cy-user-nav").ClassList.ShouldContain("cy-user-nav--unauthenticated");
        var button = cut.Find("button.cy-user-nav__login");
        button.TextContent.ShouldContain("Log in");
        cut.FindAll(".cy-user-nav__trigger").Count.ShouldBe(0);
        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Use_Custom_Login_Text()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false)
            .Add(c => c.LoginText, "Sign in"));

        cut.Find(".cy-user-nav__login").TextContent.ShouldContain("Sign in");
    }

    [Fact]
    public void Should_Render_Link_When_LoginHref_Provided()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false)
            .Add(c => c.LoginHref, "/auth/login"));

        var link = cut.Find("a.cy-user-nav__login");
        link.GetAttribute("href").ShouldBe("/auth/login");
        link.TextContent.ShouldContain("Log in");
    }

    [Fact]
    public void Should_Invoke_OnLogin_When_Unauthenticated_Clicked()
    {
        var loggedIn = false;
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false)
            .Add(c => c.OnLogin, (MouseEventArgs e) => loggedIn = true));

        cut.Find("button.cy-user-nav__login").Click();
        loggedIn.ShouldBeTrue();
    }

    [Fact]
    public void Should_Render_Authenticated_Trigger_Pill()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Dr Jane Doe")
            .Add(c => c.Status, AvatarStatus.Online));

        cut.Find(".cy-user-nav").ClassList.ShouldContain("cy-user-nav--authenticated");

        var trigger = cut.Find("button.cy-user-nav__trigger");
        trigger.GetAttribute("aria-haspopup").ShouldBe("true");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        trigger.TextContent.ShouldContain("Dr Jane Doe");

        // Small avatar in trigger
        var avatar = cut.Find(".cy-user-nav__trigger .cy-avatar");
        avatar.ClassList.ShouldContain("cy-avatar--sm");

        // Status dot rendered
        cut.Find(".cy-user-nav__status-dot.cy-user-nav__status-dot--online").ShouldNotBeNull();

        // Chevron icon rendered
        cut.Find(".cy-user-nav__chevron").ShouldNotBeNull();

        // Menu closed initially
        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Toggle_Dropdown_Menu_When_Trigger_Clicked()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe"));

        var trigger = cut.Find("button.cy-user-nav__trigger");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");

        // Open menu
        trigger.Click();
        trigger = cut.Find("button.cy-user-nav__trigger");
        trigger.GetAttribute("aria-expanded").ShouldBe("true");

        var menu = cut.Find(".cy-user-nav__menu");
        menu.GetAttribute("aria-labelledby").ShouldBe(trigger.Id);

        // Close menu
        trigger.Click();
        trigger = cut.Find("button.cy-user-nav__trigger");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Profile_Header_Card_With_Large_Avatar_Name_Email_And_Status()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Gwen Davies")
            .Add(c => c.Email, "gwen.davies@cymru.nhs.uk")
            .Add(c => c.Status, AvatarStatus.Online)
            .Add(c => c.Open, true));

        var header = cut.Find(".cy-user-nav__profile-header");
        header.ShouldNotBeNull();

        // Large avatar
        var largeAvatar = header.QuerySelector(".cy-avatar--lg");
        largeAvatar.ShouldNotBeNull();

        // Name and email
        cut.Find(".cy-user-nav__profile-name").TextContent.ShouldBe("Gwen Davies");
        cut.Find(".cy-user-nav__profile-email").TextContent.ShouldBe("gwen.davies@cymru.nhs.uk");

        // Status chip
        var chip = cut.Find(".cy-user-nav__status-chip.cy-user-nav__status-chip--online");
        chip.TextContent.ShouldContain("Online");
    }

    [Fact]
    public void Should_Render_Custom_ProfileHeader_When_Provided()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true)
            .Add(c => c.ProfileHeader, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-profile-header");
                builder.AddContent(2, "Custom Header Content");
                builder.CloseElement();
            })));

        cut.Find(".custom-profile-header").TextContent.ShouldBe("Custom Header Content");
        cut.FindAll(".cy-user-nav__profile-details").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_MenuItems_Inside_Menu()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true)
            .Add(c => c.MenuItems, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "a");
                builder.AddAttribute(1, "href", "/profile");
                builder.AddAttribute(2, "class", "test-menu-link");
                builder.AddContent(3, "My Profile");
                builder.CloseElement();
            })));

        var link = cut.Find(".cy-user-nav__menu-items a.test-menu-link");
        link.GetAttribute("href").ShouldBe("/profile");
        link.TextContent.ShouldBe("My Profile");
    }

    [Fact]
    public void Should_Render_Default_Logout_Action_And_Trigger_Callback()
    {
        var loggedOut = false;
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true)
            .Add(c => c.LogoutText, "Sign out")
            .Add(c => c.OnLogout, () => loggedOut = true));

        var logoutBtn = cut.Find("button.cy-user-nav__logout-action");
        logoutBtn.TextContent.ShouldContain("Sign out");

        logoutBtn.Click();
        loggedOut.ShouldBeTrue();
        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Custom_LogoutContent_When_Provided()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true)
            .Add(c => c.LogoutContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "custom-logout-btn");
                builder.AddContent(2, "End Session");
                builder.CloseElement();
            })));

        cut.Find(".custom-logout-btn").TextContent.ShouldBe("End Session");
        cut.FindAll(".cy-user-nav__logout-action").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Dismiss_On_Escape_Key()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true));

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(1);

        // Escape on menu
        cut.Find(".cy-user-nav__menu").KeyDown("Escape");

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Dismiss_On_Tab_Key()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true));

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(1);

        cut.Find(".cy-user-nav__menu").KeyDown("Tab");

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Open_On_ArrowDown_Key_On_Trigger()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe"));

        cut.Find("button.cy-user-nav__trigger").KeyDown("ArrowDown");

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Dismiss_On_Outside_Pointer()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Open, true));

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(1);

        await cut.Instance.OnOutsidePointer();

        cut.FindAll(".cy-user-nav__menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Support_Welsh_Localisation_Text()
    {
        var welshText = new CyUserNavText
        {
            UserMenu = "Dewislen defnyddiwr",
            Login = "Mewngofnodi",
            Logout = "Allgofnodi",
            StatusOnline = "Ar-lein",
            StatusBusy = "Prysur"
        };

        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.Text, welshText)
            .Add(c => c.Status, AvatarStatus.Online)
            .Add(c => c.Open, true)
            .Add(c => c.OnLogout, () => { }));

        var trigger = cut.Find("button.cy-user-nav__trigger");
        trigger.GetAttribute("aria-label").ShouldBe("Dewislen defnyddiwr");

        var statusChip = cut.Find(".cy-user-nav__status-chip");
        statusChip.TextContent.ShouldContain("Ar-lein");

        var logoutBtn = cut.Find("button.cy-user-nav__logout-action");
        logoutBtn.TextContent.ShouldContain("Allgofnodi");
    }

    [Fact]
    public void Should_Attach_Click_Outside_When_User_Logs_In_After_First_Render()
    {
        var cut = Render<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false));

        // When not authenticated, no JS attach
        cut.Render(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe"));

        // Trigger is rendered
        cut.Find("button.cy-user-nav__trigger").ShouldNotBeNull();
    }
}
