using System.Text.Json;
using Bunit;
using CymruBlazor.Components.Forms;
using Microsoft.Playwright;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// The pointer half of <c>CyRuleBuilder.AllowDragReorder</c> in a real browser: the builder's real markup and the
/// library's real <c>sortable-list.js</c>, driven with a real mouse. There is no Blazor circuit here, so the .NET
/// side is replaced by a recorder that captures each <c>OnPointerDrop</c> call; what the builder does with those
/// calls (reorder, move, refuse past the depth limit) is covered by <c>CyRuleBuilderDragTests</c> in the unit
/// test project.
/// </summary>
public sealed class CyRuleBuilderDragBrowserTests : AxeTestBase
{
    private static readonly IReadOnlyList<CyRuleField> Fields = [new CyRuleField { Key = "name", Label = "Name" }];

    public CyRuleBuilderDragBrowserTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js");
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js");
        JSInterop.SetupModule("./_content/CymruBlazor/js/sortable-list.js");
    }

    private static CyRuleCondition Cond(string id) =>
        new() { Id = id, FieldKey = "name", OperatorKey = CyRuleOperators.Eq, Value = id };

    private static CyRuleGroup Group(string id, params CyRuleNode[] children) => new() { Id = id, Children = children };

    /// <summary>Renders the builder, loads it in the browser and registers its lists with a recording stand-in for .NET.</summary>
    private async Task LoadAsync(CyRuleGroup rule, bool passSource = true)
    {
        var markup = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, rule)
            .Add(c => c.AllowDragReorder, true)).Markup;

        // Tall, so every row is on screen: a pointer near the viewport edge would start auto-scroll.
        await LoadHostedAsync(markup, "light", 1280, 1800);

        await Page.EvaluateAsync(
            """
            async (passSource) => {
              window.sortable = await import('/_content/CymruBlazor/js/sortable-list.js');
              window.calls = [];
              const dotNetRef = { invokeMethodAsync: (name, ...args) => { window.calls.push({ name, args }); return Promise.resolve(); } };
              window.sortable.syncTree(document.querySelector('.cy-rule'), dotNetRef,
                { group: 'rule', handleOnly: true, disabled: false, passSource });
            }
            """,
            passSource);
    }

    private static string CondGrip(string id) => $"[id$='-{id}-row'] [data-cy-drag-handle]";

    private static string GroupGrip(string id) => $"[id$='-{id}-group'] > .cy-rule__bar > [data-cy-drag-handle]";

    private static string CondItem(string id) => $"li[data-cy-item]:has(> [id$='-{id}-row'])";

    private static string GroupItem(string id) => $"li[data-cy-item]:has(> [id$='-{id}-group'])";

    private async Task<(double X, double Y)> PointAsync(string selector, double fractionDown)
    {
        var box = await Page.Locator(selector).First.BoundingBoxAsync();
        box.ShouldNotBeNull($"'{selector}' has no box");

        return (box.X + (box.Width / 2), box.Y + (box.Height * fractionDown));
    }

    /// <summary>Presses the grip, moves past the drag threshold, then to the point. Leaves the button down.</summary>
    private async Task DragToAsync(string grip, string over, double fractionDown)
    {
        var from = await PointAsync(grip, 0.5);
        var to = await PointAsync(over, fractionDown);

        await Page.Mouse.MoveAsync((float)from.X, (float)from.Y);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync((float)from.X + 12, (float)from.Y + 12, new MouseMoveOptions { Steps = 4 });
        await Page.Mouse.MoveAsync((float)to.X, (float)to.Y, new MouseMoveOptions { Steps = 12 });
    }

    private async Task DropAsync(string grip, string over, double fractionDown)
    {
        await DragToAsync(grip, over, fractionDown);
        await Page.Mouse.UpAsync();
    }

    private async Task<List<JsonElement[]>> CallsAsync()
    {
        var json = await Page.EvaluateAsync<JsonElement>("() => window.calls.map(c => [c.name, ...c.args])");

        return [.. json.EnumerateArray().Select(c => c.EnumerateArray().ToArray())];
    }

    private static void ShouldBeDrop(JsonElement[] call, string target, int oldIndex, int newIndex, string? source)
    {
        call[0].GetString().ShouldBe("OnPointerDrop");
        call[1].GetString().ShouldBe(target);
        call[2].GetInt32().ShouldBe(oldIndex);
        call[3].GetInt32().ShouldBe(newIndex);

        if (source is null)
        {
            call.Length.ShouldBe(4);
        }
        else
        {
            call.Length.ShouldBe(5);
            call[4].GetString().ShouldBe(source);
        }
    }

    [Fact]
    public async Task Dragging_A_Condition_Within_Its_Group_Reports_The_New_Index()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b"), Cond("c")));

        await DropAsync(CondGrip("a"), CondItem("c"), 0.9);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        ShouldBeDrop(calls[0], "root", 0, 2, "root");
    }

    [Fact]
    public async Task Dragging_Into_Another_Group_Reports_That_Group_And_The_Source()
    {
        await LoadAsync(Group("root", Cond("a"), Group("g1", Cond("b"), Cond("c"))));

        await DropAsync(CondGrip("a"), CondItem("c"), 0.25);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        ShouldBeDrop(calls[0], "g1", 0, 1, "root");
    }

    [Fact]
    public async Task Dragging_Out_Of_A_Group_Reports_The_Group_It_Started_In()
    {
        await LoadAsync(Group("root", Cond("a"), Group("g1", Cond("b"))));

        await DropAsync(CondGrip("b"), CondItem("a"), 0.25);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        ShouldBeDrop(calls[0], "root", 0, 0, "g1");
    }

    [Fact]
    public async Task Dropping_On_An_Empty_Group_Targets_That_Group()
    {
        await LoadAsync(Group("root", Cond("a"), Group("g1")));

        await DropAsync(CondGrip("a"), "[data-cy-drop-zone] .cy-rule__empty", 0.5);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        ShouldBeDrop(calls[0], "g1", 0, 0, "root");
    }

    [Fact]
    public async Task The_Drop_Zone_Of_An_Empty_Group_Shows_Where_The_Drop_Will_Land()
    {
        await LoadAsync(Group("root", Cond("a"), Group("g1")));

        await DragToAsync(CondGrip("a"), "[data-cy-drop-zone] .cy-rule__empty", 0.5);

        (await Page.Locator("ul[data-cy-list-id='g1'][data-cy-drop-target]").CountAsync()).ShouldBe(1);

        await Page.Mouse.UpAsync();
    }

    [Fact]
    public async Task A_Group_Cannot_Be_Dropped_Into_Itself_Or_Its_Own_Sub_Lists()
    {
        await LoadAsync(Group("root", Cond("a"), Group("g1", Group("g2", Cond("b")))));

        // Over b, which sits two lists deep inside the group being dragged.
        await DragToAsync(GroupGrip("g1"), CondItem("b"), 0.9);

        // Neither of the dragged group's own lists is offered...
        (await Page.Locator("[data-cy-drag-source] [data-cy-drop-target]").CountAsync()).ShouldBe(0);

        // ...and the only list that is offered is the one it came from.
        (await Page.Locator("ul[data-cy-list-id='root'][data-cy-drop-target]").CountAsync()).ShouldBe(1);

        // The floating copy is the group's header, not its whole subtree.
        (await Page.Locator("[data-cy-ghost] [data-cy-sortable]").CountAsync()).ShouldBe(0);

        await Page.Mouse.UpAsync();

        // Back in its own list below a: where it started, so nothing is reported.
        (await CallsAsync()).Count.ShouldBe(0);
    }

    [Fact]
    public async Task A_Group_Can_Still_Be_Reordered_Among_Its_Siblings()
    {
        await LoadAsync(Group("root", Group("g1", Cond("a")), Cond("b")));

        await DropAsync(GroupGrip("g1"), CondItem("b"), 0.9);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        ShouldBeDrop(calls[0], "root", 0, 1, "root");
    }

    [Fact]
    public async Task A_Press_Without_Movement_Starts_No_Drag()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b")));

        var at = await PointAsync(CondGrip("a"), 0.5);
        await Page.Mouse.MoveAsync((float)at.X, (float)at.Y);
        await Page.Mouse.DownAsync();
        await Page.Mouse.UpAsync();

        (await CallsAsync()).Count.ShouldBe(0);
        (await Page.Locator("[data-cy-ghost]").CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Pressing_Escape_During_A_Drag_Cancels_It()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b"), Cond("c")));

        await DragToAsync(CondGrip("a"), CondItem("c"), 0.9);
        await Page.Keyboard.PressAsync("Escape");
        await Page.Mouse.UpAsync();

        (await CallsAsync()).Count.ShouldBe(0);
        (await Page.Locator("[data-cy-ghost]").CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task A_List_That_Did_Not_Ask_For_The_Source_Gets_The_Original_Three_Arguments()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b"), Cond("c")), passSource: false);

        await DropAsync(CondGrip("a"), CondItem("c"), 0.9);

        var calls = await CallsAsync();
        calls.Count.ShouldBe(1);
        calls[0].Length.ShouldBe(4); // name + three arguments: what CySortableList.OnPointerDrop expects
        calls[0][1].GetString().ShouldBe("root");
    }

    [Fact]
    public async Task Registering_Again_After_A_Re_Render_Does_Not_Double_Report_A_Drop()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b"), Cond("c")));

        // The builder calls syncTree after every render.
        await Page.EvaluateAsync(
            """
            () => {
              const dotNetRef = { invokeMethodAsync: (name, ...args) => { window.calls.push({ name, args }); return Promise.resolve(); } };
              window.sortable.syncTree(document.querySelector('.cy-rule'), dotNetRef,
                { group: 'rule', handleOnly: true, disabled: false, passSource: true });
            }
            """);

        await DropAsync(CondGrip("a"), CondItem("c"), 0.9);

        (await CallsAsync()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Disposing_The_Tree_Stops_Dragging()
    {
        await LoadAsync(Group("root", Cond("a"), Cond("b")));

        await Page.EvaluateAsync("() => window.sortable.disposeTree(document.querySelector('.cy-rule'))");
        await DropAsync(CondGrip("a"), CondItem("b"), 0.9);

        (await CallsAsync()).Count.ShouldBe(0);
    }
}
