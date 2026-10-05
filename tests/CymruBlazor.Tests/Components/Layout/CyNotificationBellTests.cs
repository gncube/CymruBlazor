using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyNotificationBellTests : TestContextBase
{
    [Fact]
    public void Should_Render_A_Button_Named_Notifications_When_There_Are_None()
    {
        var cut = Render<CyNotificationBell>();

        var button = cut.Find("button.cy-bell__control");
        button.GetAttribute("type").ShouldBe("button");
        button.GetAttribute("aria-label").ShouldBe("Notifications");
        cut.FindAll(".cy-bell__badge").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Include_The_Count_In_The_Accessible_Name()
    {
        var cut = Render<CyNotificationBell>(p => p.Add(c => c.Count, 3));

        cut.Find("button").GetAttribute("aria-label").ShouldBe("Notifications, 3 unread");
    }

    [Fact]
    public void Should_Hide_The_Visible_Badge_From_Assistive_Technology()
    {
        var cut = Render<CyNotificationBell>(p => p.Add(c => c.Count, 3));

        var badge = cut.Find(".cy-bell__badge");
        badge.TextContent.ShouldBe("3");
        badge.GetAttribute("aria-hidden").ShouldBe("true");
        cut.Find(".cy-bell").ClassList.ShouldContain("cy-bell--unread");
    }

    [Fact]
    public void Should_Cap_The_Visible_Count_At_Max()
    {
        var cut = Render<CyNotificationBell>(p => p.Add(c => c.Count, 250));

        cut.Find(".cy-bell__badge").TextContent.ShouldBe("99+");
        cut.Find("button").GetAttribute("aria-label").ShouldBe("Notifications, 250 unread");
    }

    [Fact]
    public void Should_Use_A_Custom_Label_And_Localised_Format()
    {
        var cut = Render<CyNotificationBell>(p => p
            .Add(c => c.Count, 2)
            .Add(c => c.Label, "Hysbysiadau")
            .Add(c => c.Text, new CyNotificationBellText { UnreadFormat = "{0}, {1} heb eu darllen" }));

        cut.Find("button").GetAttribute("aria-label").ShouldBe("Hysbysiadau, 2 heb eu darllen");
    }

    [Fact]
    public void Should_Raise_OnClick()
    {
        var clicks = 0;
        var cut = Render<CyNotificationBell>(p => p.Add(c => c.OnClick, () => clicks++));

        cut.Find("button").Click();

        clicks.ShouldBe(1);
    }

    [Fact]
    public void Should_Render_Aria_Expanded_Only_When_Expanded_Is_Set()
    {
        Render<CyNotificationBell>().Find("button").HasAttribute("aria-expanded").ShouldBeFalse();
        Render<CyNotificationBell>(p => p.Add(c => c.Expanded, true)).Find("button").GetAttribute("aria-expanded").ShouldBe("true");
        Render<CyNotificationBell>(p => p.Add(c => c.Expanded, false)).Find("button").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Render_A_Link_When_Href_Is_Set()
    {
        var cut = Render<CyNotificationBell>(p => p.Add(c => c.Href, "/notifications").Add(c => c.Count, 1));

        cut.FindAll("button").Count.ShouldBe(0);
        var link = cut.Find("a.cy-bell__control");
        link.GetAttribute("href").ShouldBe("/notifications");
        link.GetAttribute("aria-label").ShouldBe("Notifications, 1 unread");
    }

    [Fact]
    public void Should_Announce_The_Count_Politely_Only_When_Asked()
    {
        Render<CyNotificationBell>(p => p.Add(c => c.Count, 2)).FindAll("[role='status']").Count.ShouldBe(0);

        var cut = Render<CyNotificationBell>(p => p.Add(c => c.Count, 2).Add(c => c.Announce, true));
        cut.Find("[role='status']").TextContent.ShouldBe("Notifications, 2 unread");
    }

    [Fact]
    public void Should_Reject_A_Negative_Count_Or_A_Max_Below_One()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyNotificationBell>(p => p.Add(c => c.Count, -1)));
        Should.Throw<InvalidOperationException>(() => Render<CyNotificationBell>(p => p.Add(c => c.Max, 0)));
    }
}
