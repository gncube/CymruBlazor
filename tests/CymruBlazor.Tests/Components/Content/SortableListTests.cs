using Xunit;
using Shouldly;
using Bunit;
using Moq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Accessibility;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Content;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Components.Content;

/// <summary>
/// Behaviour of <see cref="CySortableList{TItem}"/> that does not need a browser: the keyboard pick-up
/// model, the single-pointer alternatives (WCAG 2.5.7), reorder events and screen reader announcements.
/// The pointer gesture itself lives in <c>sortable-list.js</c> and is covered by the browser tests.
/// </summary>
public sealed class SortableListTests : TestContextBase
{
    private sealed record Question(int Id, string Name);

    private static readonly Question[] Questions =
    [
        new(1, "Disease status"),
        new(2, "Age at onset"),
        new(3, "Family history"),
    ];

    private readonly Mock<ILiveRegionRegistry> _liveRegions = new();
    private readonly List<string> _announcements = [];

    public SortableListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/sortable-list.js").Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;

        _liveRegions
            .Setup(r => r.PublishAsync(It.IsAny<LiveRegionAnnouncement>(), It.IsAny<CancellationToken>()))
            .Callback<LiveRegionAnnouncement, CancellationToken>((a, _) => _announcements.Add(a.Message))
            .Returns(ValueTask.CompletedTask);
    }

    private void UseLiveRegionMock() => Services.AddSingleton(_liveRegions.Object);

    private static RenderFragment<Question> Template => q => b => b.AddContent(0, q.Name);

    private IRenderedComponent<CySortableList<Question>> RenderList(
        IEnumerable<Question>? items = null,
        Action<ComponentParameterCollectionBuilder<CySortableList<Question>>>? extra = null,
        string label = "Questions") =>
        Render<CySortableList<Question>>(p =>
        {
            p.Add(c => c.Label, label)
             .Add(c => c.Items, items ?? Questions)
             .Add(c => c.ItemTemplate, Template)
             .Add(c => c.KeySelector, q => q.Id)
             .Add(c => c.ItemLabelSelector, q => q.Name);
            extra?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement Handle(IRenderedComponent<CySortableList<Question>> cut, int index) =>
        cut.FindAll("button[data-cy-drag-handle]")[index];

    // ----------------------------------------------------------------- structure

    [Fact]
    public void Should_Render_A_Labelled_List_With_One_Item_And_Handle_Per_Question()
    {
        var cut = RenderList();

        var list = cut.Find("ul[data-cy-sortable]");
        list.GetAttribute("aria-label").ShouldBe("Questions");
        cut.FindAll("li[data-cy-item]").Count.ShouldBe(3);
        cut.FindAll("button[data-cy-drag-handle]").Count.ShouldBe(3);
        cut.Find("li[data-cy-item]").TextContent.ShouldContain("Disease status");
    }

    [Fact]
    public void Should_Name_Each_Handle_After_Its_Item_And_Link_The_Instructions()
    {
        var cut = RenderList();

        var handle = Handle(cut, 1);
        handle.GetAttribute("aria-label").ShouldBe("Reorder Age at onset");
        handle.GetAttribute("aria-pressed").ShouldBe("false");

        var instructions = cut.Find($"#{handle.GetAttribute("aria-describedby")}");
        instructions.TextContent.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Should_Reject_Duplicate_Keys()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderList([new Question(1, "A"), new Question(1, "B")]));
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderList(label: ""));
    }

    [Fact]
    public void Should_Render_EmptyContent_When_There_Are_No_Items()
    {
        var cut = RenderList([], p => p.Add(c => c.EmptyContent, (RenderFragment)(b => b.AddContent(0, "Nothing here"))));

        cut.Find(".cy-sortable__empty").TextContent.ShouldBe("Nothing here");
    }

    [Fact]
    public void Should_Render_Item_Content_As_Escaped_Text()
    {
        var cut = RenderList([new Question(1, "<script>alert(1)</script>")]);

        cut.FindAll("li[data-cy-item] script").Count.ShouldBe(0);
        cut.Find("li[data-cy-item]").TextContent.ShouldContain("<script>alert(1)</script>");
    }

    // ----------------------------------------------------------------- WCAG 2.5.7: single-pointer alternatives

    [Fact]
    public void Should_Offer_Move_Up_And_Down_Buttons_That_Respect_The_Edges()
    {
        var cut = RenderList();

        var up = cut.FindAll("button[data-cy-action='up']");
        var down = cut.FindAll("button[data-cy-action='down']");

        up[0].HasAttribute("disabled").ShouldBeTrue();
        up[1].HasAttribute("disabled").ShouldBeFalse();
        down[2].HasAttribute("disabled").ShouldBeTrue();
        down[0].HasAttribute("disabled").ShouldBeFalse();
        down[0].GetAttribute("aria-label").ShouldBe("Move Disease status down");
    }

    [Fact]
    public void Should_Raise_OnReorder_When_Move_Down_Is_Pressed()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        cut.FindAll("button[data-cy-action='down']")[0].Click();

        args.ShouldNotBeNull();
        args.Item.Name.ShouldBe("Disease status");
        args.OldIndex.ShouldBe(0);
        args.NewIndex.ShouldBe(1);
        args.IsCrossList.ShouldBeFalse();
    }

    [Fact]
    public void Should_Raise_OnReorder_When_Move_Up_Is_Pressed()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        cut.FindAll("button[data-cy-action='up']")[2].Click();

        args!.OldIndex.ShouldBe(2);
        args.NewIndex.ShouldBe(1);
    }

    [Fact]
    public void Should_Not_Mutate_The_Items_It_Was_Given()
    {
        // The parent owns the data: the list reports the move and re-renders from whatever Items becomes.
        var source = Questions.ToList();
        var cut = RenderList(source);

        cut.FindAll("button[data-cy-action='down']")[0].Click();

        string.Join(",", source.Select(q => q.Id)).ShouldBe("1,2,3");
        cut.FindAll("li[data-cy-item]")[0].TextContent.ShouldContain("Disease status");
    }

    [Fact]
    public void Should_Re_Render_In_The_New_Order_When_The_Parent_Applies_The_Move()
    {
        var cut = RenderList();

        cut.Render(p => p.Add(c => c.Items, new[] { Questions[1], Questions[0], Questions[2] }));

        cut.FindAll("li[data-cy-item]")[0].TextContent.ShouldContain("Age at onset");
    }

    [Fact]
    public void Should_Hide_The_Move_Buttons_When_ShowMoveActions_Is_False()
    {
        var cut = RenderList(extra: p => p.Add(c => c.ShowMoveActions, false));

        cut.FindAll("button[data-cy-action]").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Disable_Every_Control_When_Disabled()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnReorder, EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        cut.FindAll("button[data-cy-drag-handle]").ShouldAllBe(b => b.HasAttribute("disabled"));
        cut.FindAll("button[data-cy-action]").ShouldAllBe(b => b.HasAttribute("disabled"));
        args.ShouldBeNull();
    }

    // ----------------------------------------------------------------- keyboard

    [Fact]
    public void Should_Pick_Up_With_Click_And_Show_The_Pressed_State()
    {
        var cut = RenderList();

        Handle(cut, 0).Click();

        Handle(cut, 0).GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find("ul").ClassList.ShouldContain("cy-sortable--picked");
    }

    [Fact]
    public void Should_Move_Down_With_ArrowDown_And_Drop_On_Second_Activation()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Nothing is committed until the drop.
        args.ShouldBeNull();

        Handle(cut, 0).Click();

        args.ShouldNotBeNull();
        args.OldIndex.ShouldBe(0);
        args.NewIndex.ShouldBe(1);
        cut.Find("ul").ClassList.ShouldNotContain("cy-sortable--picked");
    }

    [Fact]
    public void Should_Preview_The_Move_Through_CSS_Order_Without_Reordering_The_DOM()
    {
        var cut = RenderList();

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        var items = cut.FindAll("li[data-cy-item]");
        items[0].TextContent.ShouldContain("Disease status");
        items[0].GetAttribute("style")!.ShouldContain("order:1");
        items[1].GetAttribute("style")!.ShouldContain("order:0");
        items[2].GetAttribute("style")!.ShouldContain("order:2");
    }

    [Fact]
    public void Should_Clamp_At_The_Ends_Of_The_List()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Handle(cut, 0).Click();

        // Dropped where it started: nothing to report.
        args.ShouldBeNull();
    }

    [Fact]
    public void Should_Jump_To_The_End_With_End_Key()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "End" });
        Handle(cut, 0).Click();

        args!.NewIndex.ShouldBe(2);
    }

    [Fact]
    public void Should_Cancel_On_Escape_Without_Raising_OnReorder()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "Escape" });

        args.ShouldBeNull();
        Handle(cut, 0).GetAttribute("aria-pressed").ShouldBe("false");
        cut.FindAll("li[data-cy-item]").ShouldAllBe(li => li.GetAttribute("style") == null);
    }

    [Fact]
    public void Should_Cancel_When_The_Handle_Loses_Focus()
    {
        var cut = RenderList();

        Handle(cut, 0).Click();
        Handle(cut, 0).FocusOut();

        Handle(cut, 0).GetAttribute("aria-pressed").ShouldBe("false");
    }

    [Fact]
    public void Should_Ignore_Keys_On_A_Handle_That_Is_Not_Picked_Up()
    {
        CyReorderEventArgs<Question>? args = null;
        var cut = RenderList(extra: p => p.Add(c => c.OnReorder,
            EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, a => args = a)));

        Handle(cut, 1).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        args.ShouldBeNull();
        cut.Find("ul").ClassList.ShouldNotContain("cy-sortable--picked");
    }

    // ----------------------------------------------------------------- announcements

    [Fact]
    public void Should_Announce_Pick_Up_Move_And_Drop_Through_The_Live_Region_Registry()
    {
        UseLiveRegionMock();
        var cut = RenderList();

        Handle(cut, 0).Click();
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Handle(cut, 0).Click();

        _announcements.Count.ShouldBe(3);
        _announcements[0].ShouldBe("Picked up Disease status, position 1 of 3.");
        _announcements[1].ShouldBe("Moved Disease status to position 2 of 3.");
        _announcements[2].ShouldBe("Dropped Disease status at position 2 of 3.");
    }

    [Fact]
    public void Should_Announce_Cancellation()
    {
        UseLiveRegionMock();
        var cut = RenderList();

        Handle(cut, 1).Click();
        Handle(cut, 1).KeyDown(new KeyboardEventArgs { Key = "Escape" });

        _announcements.Last().ShouldBe("Reorder cancelled. Age at onset is back at position 2 of 3.");
    }

    [Fact]
    public void Should_Announce_A_Move_Made_With_The_Buttons()
    {
        UseLiveRegionMock();
        var cut = RenderList();

        cut.FindAll("button[data-cy-action='down']")[0].Click();

        _announcements.Single().ShouldBe("Moved Disease status to position 2 of 3.");
    }

    [Fact]
    public void Should_Publish_Polite_Announcements()
    {
        UseLiveRegionMock();
        var cut = RenderList();

        Handle(cut, 0).Click();

        _liveRegions.Verify(r => r.PublishAsync(
            It.Is<LiveRegionAnnouncement>(a => a.Politeness == CymruBlazor.Enums.LiveRegionPoliteness.Polite),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Should_Fall_Back_To_A_Local_Live_Region_When_No_Registry_Is_Registered()
    {
        var cut = RenderList();

        Handle(cut, 0).Click();

        var region = cut.Find("[aria-live='polite']");
        region.TextContent.ShouldBe("Picked up Disease status, position 1 of 3.");
    }

    [Fact]
    public void Should_Use_Consumer_Supplied_Text_For_Localisation()
    {
        UseLiveRegionMock();
        var cut = RenderList(extra: p => p.Add(c => c.Text, new CySortableListText
        {
            PickedUp = "Wedi codi {0}, safle {1} o {2}.",
            DragHandle = "Aildrefnu {0}",
        }));

        Handle(cut, 0).GetAttribute("aria-label").ShouldBe("Aildrefnu Disease status");
        Handle(cut, 0).Click();

        _announcements.Single().ShouldBe("Wedi codi Disease status, safle 1 o 3.");
    }

    // ----------------------------------------------------------------- cross-list moves

    private IRenderedComponent<CySortableList<Question>> RenderSection(
        string label,
        IEnumerable<Question> items,
        string? group,
        Action<CyReorderEventArgs<Question>>? onReorder = null) =>
        Render<CySortableList<Question>>(p =>
        {
            p.Add(c => c.Label, label)
             .Add(c => c.Group, group)
             .Add(c => c.Items, items)
             .Add(c => c.ItemTemplate, Template)
             .Add(c => c.KeySelector, q => q.Id)
             .Add(c => c.ItemLabelSelector, q => q.Name);

            if (onReorder is not null)
            {
                p.Add(c => c.OnReorder, EventCallback.Factory.Create<CyReorderEventArgs<Question>>(this, onReorder));
            }
        });

    [Fact]
    public void Should_Not_Offer_A_Move_To_Menu_When_No_Other_List_Shares_The_Group()
    {
        Services.AddCymruBlazor();

        var alone = RenderSection("Section A", [Questions[0]], group: "sections");

        alone.FindAll(".cy-menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Not_Offer_A_Move_To_Menu_Without_A_Group()
    {
        Services.AddCymruBlazor();

        var a = RenderSection("Section A", [Questions[0]], group: null);
        RenderSection("Section B", [Questions[1]], group: null);
        a.Render();

        a.FindAll(".cy-menu").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Move_An_Item_To_The_End_Of_A_Peer_List_From_The_Menu()
    {
        Services.AddCymruBlazor();

        var received = new List<CyReorderEventArgs<Question>>();
        var a = RenderSection("Section A", [Questions[0]], "sections");
        RenderSection("Section B", [Questions[1], Questions[2]], "sections", received.Add);

        // A rendered before its peer existed; re-render so it sees the registry entry.
        a.Render();

        a.Find(".cy-menu__trigger").Click();
        a.Find("[role='menuitem']").Click();

        var args = received.Single();
        args.Item.Name.ShouldBe("Disease status");
        args.IsCrossList.ShouldBeTrue();
        args.OldIndex.ShouldBe(0);
        args.NewIndex.ShouldBe(2);
        args.Group.ShouldBe("sections");
    }

    [Fact]
    public void Should_Name_The_Destination_List_In_The_Menu()
    {
        Services.AddCymruBlazor();

        var a = RenderSection("Section A", [Questions[0]], "sections");
        RenderSection("Section B", [Questions[1]], "sections");
        a.Render();

        a.Find(".cy-menu__trigger").Click();

        a.Find("[role='menuitem']").TextContent.Trim().ShouldBe("Section B");
    }

    // ----------------------------------------------------------------- target size

    [Fact]
    public void Handle_And_Move_Buttons_Should_Meet_The_Minimum_Target_Size()
    {
        const string selectorStart = ".cy-drag-handle,";

        var css = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "CymruBlazor", "wwwroot", "css", "components", "sortable-list.css"));

        var start = css.IndexOf(selectorStart, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);

        var block = css[start..css.IndexOf('}', start)];

        // 2.75rem = 44px, comfortably above the 24x24 CSS px floor of WCAG 2.5.8.
        block.ShouldContain("min-inline-size: 2.75rem");
        block.ShouldContain("min-block-size: 2.75rem");
        block.ShouldContain(".cy-sortable__action");
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
