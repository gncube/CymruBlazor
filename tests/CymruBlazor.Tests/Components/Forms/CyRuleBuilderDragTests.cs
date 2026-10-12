using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// The server-side half of <c>AllowDragReorder</c>: the markup the script needs and what the builder does with a
/// drop. The pointer gesture itself is covered in a real browser by <c>CyRuleBuilderDragBrowserTests</c>.
/// </summary>
public sealed class CyRuleBuilderDragTests : TestContextBase
{
    private const string SortableModule = "./_content/CymruBlazor/js/sortable-list.js";

    private static readonly IReadOnlyList<CyRuleField> Fields =
    [
        new CyRuleField { Key = "name", Label = "Name" },
        new CyRuleField { Key = "age", Label = "Age", Type = CyRuleFieldType.Number }
    ];

    private readonly BunitJSModuleInterop _sortable;
    private CyRuleGroup? _current;

    public CyRuleBuilderDragTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js").Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;
        _sortable = JSInterop.SetupModule(SortableModule);
        _sortable.Mode = JSRuntimeMode.Loose;
    }

    private static CyRuleCondition Cond(string id, string value) =>
        new() { Id = id, FieldKey = "name", OperatorKey = CyRuleOperators.Eq, Value = value };

    private static CyRuleGroup Group(string id, params CyRuleNode[] children) => new() { Id = id, Children = children };

    private IRenderedComponent<CyRuleBuilder> RenderBuilder(
        CyRuleGroup rule,
        bool drag = true,
        bool readOnly = false,
        CyRuleLimits? limits = null)
    {
        _current = rule;

        return Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, rule)
            .Add(c => c.AllowDragReorder, drag)
            .Add(c => c.ReadOnly, readOnly)
            .Add(c => c.Limits, limits)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<CyRuleGroup>(this, g => _current = g)));
    }

    private static async Task DropAsync(
        IRenderedComponent<CyRuleBuilder> cut,
        string source,
        int oldIndex,
        string target,
        int newIndex) =>
        await cut.InvokeAsync(() => cut.Instance.OnPointerDrop(target, oldIndex, newIndex, source));

    private static string Status(IRenderedComponent<CyRuleBuilder> cut) =>
        cut.Find("[role=status]").TextContent.Trim();

    private static string[] Ids(CyRuleGroup group) => [.. group.Children.Select(c => c.Id)];

    // ------------------------------------------------------------------ markup

    [Fact]
    public void Default_Markup_Has_No_Drag_Hooks_And_Loads_No_Script()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1")), drag: false);

        cut.FindAll("[data-cy-drag-handle], [data-cy-sortable], [data-cy-item], [data-cy-drop-zone]").Count.ShouldBe(0);
        cut.Markup.ShouldNotContain("cy-rule__grip");
        _sortable.Invocations.Count.ShouldBe(0);
    }

    [Fact]
    public void Drag_Adds_A_Grip_To_Every_Condition_And_Nested_Group_But_Not_The_Root()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1", Cond("b", "2"))));

        // a, g1 and b: three rows, three grips. The root group has none.
        cut.FindAll("[data-cy-drag-handle]").Count.ShouldBe(3);
        cut.Find("fieldset.cy-rule__group--root").QuerySelector(":scope > .cy-rule__bar [data-cy-drag-handle]").ShouldBeNull();
    }

    [Fact]
    public void The_Grip_Is_Hidden_From_Assistive_Technology_And_Cannot_Take_Focus()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1")));

        foreach (var grip in cut.FindAll("[data-cy-drag-handle]"))
        {
            grip.TagName.ShouldBe("SPAN");
            grip.GetAttribute("aria-hidden").ShouldBe("true");
            grip.HasAttribute("tabindex").ShouldBeFalse();
            grip.HasAttribute("role").ShouldBeFalse();
            grip.QuerySelectorAll("button, a, input").Length.ShouldBe(0);
        }
    }

    [Fact]
    public void Lists_And_Items_Carry_The_Ids_And_Indexes_The_Script_Reads()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1", Cond("b", "2"))));

        var lists = cut.FindAll("ul[data-cy-sortable]");
        lists.Count.ShouldBe(2);
        lists[0].GetAttribute("data-cy-list-id").ShouldBe("root");
        lists[1].GetAttribute("data-cy-list-id").ShouldBe("g1");
        lists.Select(l => l.GetAttribute("data-cy-group")).Distinct().Count().ShouldBe(1);

        var items = cut.FindAll("li[data-cy-item]");
        items.Count.ShouldBe(3);
        items[0].GetAttribute("data-cy-index").ShouldBe("0");
        items[1].GetAttribute("data-cy-index").ShouldBe("1");
        items[2].GetAttribute("data-cy-index").ShouldBe("0");
    }

    [Fact]
    public void An_Empty_Group_Is_A_Drop_Zone_That_Keeps_Its_Message()
    {
        var cut = RenderBuilder(Group("root", Group("g1")));

        var zone = cut.Find("[data-cy-drop-zone]");
        var list = zone.QuerySelector("ul[data-cy-sortable]");
        list.ShouldNotBeNull();
        list.GetAttribute("data-cy-list-id").ShouldBe("g1");
        list.GetAttribute("aria-hidden").ShouldBe("true");
        list.Children.Length.ShouldBe(0);
        zone.QuerySelector(".cy-rule__empty")!.TextContent.ShouldBe("No conditions in this group.");
    }

    [Fact]
    public void Without_Drag_An_Empty_Group_Is_Just_The_Message()
    {
        var cut = RenderBuilder(Group("root", Group("g1")), drag: false);

        cut.FindAll("[data-cy-drop-zone]").Count.ShouldBe(0);
        cut.FindAll(".cy-rule__empty").Count.ShouldBe(1);
    }

    [Fact]
    public void Read_Only_Shows_No_Grips_Even_With_Drag_Allowed()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1")), readOnly: true);

        cut.FindAll("[data-cy-drag-handle], [data-cy-sortable], [data-cy-item]").Count.ShouldBe(0);
        _sortable.Invocations.Count.ShouldBe(0);
    }

    [Fact]
    public void Drag_Registers_The_Lists_With_The_Script_After_Rendering()
    {
        RenderBuilder(Group("root", Cond("a", "1")));

        _sortable.VerifyInvoke("syncTree");
    }

    // ------------------------------------------------------------------ drops

    [Fact]
    public async Task Dropping_Within_A_Group_Reorders_It()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Cond("b", "2"), Cond("c", "3")));

        await DropAsync(cut, "root", 0, "root", 2);

        Ids(_current!).ShouldBe(["b", "c", "a"]);
        Status(cut).ShouldNotBe("It cannot be moved there.");
        cut.FindAll("li[data-cy-item]").Select(i => i.GetAttribute("data-cy-index")).ShouldBe(["0", "1", "2"]);
    }

    [Fact]
    public async Task Dropping_Into_Another_Group_Moves_The_Node_At_The_Given_Index()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1", Cond("b", "2"), Cond("c", "3"))));

        await DropAsync(cut, "root", 0, "g1", 1);

        Ids(_current!).ShouldBe(["g1"]);
        Ids((CyRuleGroup)_current!.Children[0]).ShouldBe(["b", "a", "c"]);
    }

    [Fact]
    public async Task Dropping_Into_An_Empty_Group_Moves_The_Node_There()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1")));

        await DropAsync(cut, "root", 0, "g1", 0);

        Ids(_current!).ShouldBe(["g1"]);
        Ids((CyRuleGroup)_current!.Children[0]).ShouldBe(["a"]);
        cut.FindAll("[data-cy-drop-zone]").Count.ShouldBe(0);
    }

    [Fact]
    public async Task Moving_A_Group_Out_Of_A_Group_Works_Like_Any_Other_Node()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1"), Group("g1", Group("g2", Cond("b", "2")))));

        await DropAsync(cut, "g1", 0, "root", 0);

        Ids(_current!).ShouldBe(["g2", "a", "g1"]);
    }

    [Fact]
    public async Task Dropping_A_Group_Into_Its_Own_Descendant_Is_Refused_And_Announced()
    {
        var rule = Group("root", Group("g1", Group("g2", Cond("b", "2"))));
        var cut = RenderBuilder(rule);

        await DropAsync(cut, "root", 0, "g2", 0);

        _current.ShouldBeSameAs(rule);
        Status(cut).ShouldBe("It cannot be moved there.");
    }

    [Fact]
    public async Task Dropping_Past_The_Depth_Limit_Is_Refused_And_Announced()
    {
        var rule = Group("root", Group("g1", Cond("a", "1")), Group("g2", Cond("b", "2")));
        var cut = RenderBuilder(rule, limits: new CyRuleLimits(MaxDepth: 2));

        // g2 would become a third level inside g1.
        await DropAsync(cut, "root", 1, "g1", 0);

        _current.ShouldBeSameAs(rule);
        Status(cut).ShouldBe("It cannot be moved there.");

        // A condition may still go in: it adds no level.
        await DropAsync(cut, "g2", 0, "g1", 0);

        Ids((CyRuleGroup)_current!.Children[0]).ShouldBe(["b", "a"]);
    }

    [Fact]
    public async Task A_Drop_With_Unknown_Ids_Or_A_Bad_Index_Changes_Nothing()
    {
        var rule = Group("root", Cond("a", "1"));
        var cut = RenderBuilder(rule);

        await DropAsync(cut, "nope", 0, "root", 0);
        await DropAsync(cut, "root", 0, "nope", 0);
        await DropAsync(cut, "root", 5, "root", 0);
        await DropAsync(cut, "root", -1, "root", 0);

        _current.ShouldBeSameAs(rule);
    }

    [Fact]
    public async Task A_Disabled_Builder_Ignores_Drops()
    {
        var rule = Group("root", Cond("a", "1"), Cond("b", "2"));
        _current = rule;
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, rule)
            .Add(c => c.AllowDragReorder, true)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<CyRuleGroup>(this, g => _current = g)));

        await DropAsync(cut, "root", 0, "root", 1);

        _current.ShouldBeSameAs(rule);
        cut.Find(".cy-rule__grip").ClassList.ShouldContain("cy-rule__grip--disabled");
    }

    [Fact]
    public async Task Disposing_Releases_The_Script_Registrations()
    {
        var cut = RenderBuilder(Group("root", Cond("a", "1")));

        await cut.Instance.DisposeAsync();

        _sortable.VerifyInvoke("disposeTree");
    }
}
