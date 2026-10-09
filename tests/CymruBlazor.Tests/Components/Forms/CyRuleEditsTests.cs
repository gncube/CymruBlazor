using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyRuleEditsTests
{
    // root
    //   c1
    //   g1 (Or)
    //     c2
    //     c3
    //   c4
    private static CyRuleGroup Sample() => new()
    {
        Id = "root",
        Children =
        [
            new CyRuleCondition { Id = "c1", FieldKey = "age" },
            new CyRuleGroup
            {
                Id = "g1",
                Combinator = CyRuleCombinator.Or,
                Children =
                [
                    new CyRuleCondition { Id = "c2" },
                    new CyRuleCondition { Id = "c3" }
                ]
            },
            new CyRuleCondition { Id = "c4" }
        ]
    };

    private static string Ids(CyRuleGroup group) => string.Join(",", group.Children.Select(c => c.Id));

    private static CyRuleGroup Group(CyRuleGroup root, string id) => (CyRuleGroup)CyRuleEdits.Find(root, id)!;

    // ---- queries

    [Fact]
    public void Find_Locates_Root_Child_And_Nested_Nodes()
    {
        var root = Sample();

        CyRuleEdits.Find(root, "root").ShouldBeSameAs(root);
        CyRuleEdits.Find(root, "c1").ShouldNotBeNull();
        CyRuleEdits.Find(root, "c3").ShouldNotBeNull();
        CyRuleEdits.Find(root, "nope").ShouldBeNull();
    }

    [Fact]
    public void FindParent_Returns_The_Containing_Group()
    {
        var root = Sample();

        CyRuleEdits.FindParent(root, "c1")!.Id.ShouldBe("root");
        CyRuleEdits.FindParent(root, "c3")!.Id.ShouldBe("g1");
        CyRuleEdits.FindParent(root, "root").ShouldBeNull();
        CyRuleEdits.FindParent(root, "nope").ShouldBeNull();
    }

    [Fact]
    public void Depth_Counts_The_Root_As_One_And_Is_Zero_For_Non_Groups()
    {
        var root = Sample();

        CyRuleEdits.Depth(root, "root").ShouldBe(1);
        CyRuleEdits.Depth(root, "g1").ShouldBe(2);
        CyRuleEdits.Depth(root, "c1").ShouldBe(0);
        CyRuleEdits.Depth(root, "nope").ShouldBe(0);
    }

    [Fact]
    public void LevelsBelow_And_CountConditions_Include_Nested_Groups()
    {
        var root = Sample();

        CyRuleEdits.LevelsBelow(root).ShouldBe(2);
        CyRuleEdits.LevelsBelow(Group(root, "g1")).ShouldBe(1);
        CyRuleEdits.CountConditions(root).ShouldBe(4);
        CyRuleEdits.CountConditions(Group(root, "g1")).ShouldBe(2);
    }

    // ---- add

    [Fact]
    public void AddCondition_Appends_And_Leaves_The_Original_Untouched()
    {
        var root = Sample();

        var next = CyRuleEdits.AddCondition(root, "root", new CyRuleCondition { Id = "new" });

        Ids(next).ShouldBe("c1,g1,c4,new");
        Ids(root).ShouldBe("c1,g1,c4");
    }

    [Fact]
    public void AddCondition_Inserts_At_An_Index_In_A_Nested_Group()
    {
        var next = CyRuleEdits.AddCondition(Sample(), "g1", new CyRuleCondition { Id = "new" }, index: 1);

        Ids(Group(next, "g1")).ShouldBe("c2,new,c3");
    }

    [Fact]
    public void AddCondition_Without_A_Condition_Adds_A_Blank_One()
    {
        var next = CyRuleEdits.AddCondition(Sample(), "root");

        var added = (CyRuleCondition)next.Children[^1];
        added.FieldKey.ShouldBeNull();
        added.OperatorKey.ShouldBeNull();
    }

    [Fact]
    public void AddCondition_Respects_MaxConditions_By_Returning_The_Same_Root()
    {
        var root = Sample();

        CyRuleEdits.AddCondition(root, "root", limits: new CyRuleLimits(MaxConditions: 4)).ShouldBeSameAs(root);
        CyRuleEdits.AddCondition(root, "root", limits: new CyRuleLimits(MaxConditions: 5)).ShouldNotBeSameAs(root);
    }

    [Fact]
    public void AddCondition_To_An_Unknown_Group_Is_A_No_Op()
    {
        var root = Sample();

        CyRuleEdits.AddCondition(root, "nope").ShouldBeSameAs(root);
        CyRuleEdits.AddCondition(root, "c1").ShouldBeSameAs(root); // a condition is not a group
    }

    [Fact]
    public void AddGroup_Adds_A_Group_Holding_One_Blank_Condition()
    {
        var next = CyRuleEdits.AddGroup(Sample(), "root");

        var added = next.Children[^1].ShouldBeOfType<CyRuleGroup>();
        added.Children.Count.ShouldBe(1);
        added.Children[0].ShouldBeOfType<CyRuleCondition>();
        CyRuleEdits.CountConditions(next).ShouldBe(5);
    }

    [Fact]
    public void AddGroup_Respects_MaxDepth()
    {
        var root = Sample();

        CyRuleEdits.AddGroup(root, "g1", limits: new CyRuleLimits(MaxDepth: 2)).ShouldBeSameAs(root);
        CyRuleEdits.AddGroup(root, "g1", limits: new CyRuleLimits(MaxDepth: 3)).ShouldNotBeSameAs(root);
    }

    [Fact]
    public void AddGroup_Respects_MaxConditions()
    {
        var root = Sample();

        CyRuleEdits.AddGroup(root, "root", limits: new CyRuleLimits(MaxConditions: 4)).ShouldBeSameAs(root);
    }

    // ---- remove

    [Fact]
    public void Remove_Takes_Out_A_Nested_Condition()
    {
        var next = CyRuleEdits.Remove(Sample(), "c2");

        Ids(Group(next, "g1")).ShouldBe("c3");
    }

    [Fact]
    public void Remove_Takes_Out_A_Group_With_Its_Children()
    {
        var next = CyRuleEdits.Remove(Sample(), "g1");

        Ids(next).ShouldBe("c1,c4");
        CyRuleEdits.CountConditions(next).ShouldBe(2);
    }

    [Fact]
    public void Remove_Cannot_Remove_The_Root_Or_An_Unknown_Node()
    {
        var root = Sample();

        CyRuleEdits.Remove(root, "root").ShouldBeSameAs(root);
        CyRuleEdits.Remove(root, "nope").ShouldBeSameAs(root);
    }

    [Fact]
    public void Remove_Respects_MinConditions()
    {
        var root = Sample();

        CyRuleEdits.Remove(root, "g1", new CyRuleLimits(MinConditions: 3)).ShouldBeSameAs(root);
        CyRuleEdits.Remove(root, "c1", new CyRuleLimits(MinConditions: 3)).ShouldNotBeSameAs(root);
    }

    // ---- duplicate

    [Fact]
    public void Duplicate_Copies_A_Condition_After_The_Original_With_A_New_Id()
    {
        var next = CyRuleEdits.Duplicate(Sample(), "c1");

        next.Children.Count.ShouldBe(4);
        next.Children[0].Id.ShouldBe("c1");
        next.Children[1].Id.ShouldNotBe("c1");
        ((CyRuleCondition)next.Children[1]).FieldKey.ShouldBe("age");
    }

    [Fact]
    public void Duplicate_Copies_A_Group_Deeply_With_New_Ids_Everywhere()
    {
        var next = CyRuleEdits.Duplicate(Sample(), "g1");

        var copy = next.Children[2].ShouldBeOfType<CyRuleGroup>();
        copy.Id.ShouldNotBe("g1");
        copy.Combinator.ShouldBe(CyRuleCombinator.Or);
        copy.Children.Select(c => c.Id).ShouldNotContain("c2");
        copy.Children.Select(c => c.Id).ShouldNotContain("c3");
        CyRuleEdits.CountConditions(next).ShouldBe(6);
    }

    [Fact]
    public void Duplicate_Respects_MaxConditions_And_Cannot_Duplicate_The_Root()
    {
        var root = Sample();

        CyRuleEdits.Duplicate(root, "g1", new CyRuleLimits(MaxConditions: 5)).ShouldBeSameAs(root);
        CyRuleEdits.Duplicate(root, "root").ShouldBeSameAs(root);
    }

    // ---- wrap and ungroup

    [Fact]
    public void Wrap_Puts_A_Condition_In_A_New_Group_In_Its_Place()
    {
        var next = CyRuleEdits.Wrap(Sample(), "c1");

        var wrapper = next.Children[0].ShouldBeOfType<CyRuleGroup>();
        wrapper.Id.ShouldNotBe("g1");
        Ids(wrapper).ShouldBe("c1");
        Ids(next).ShouldEndWith("g1,c4");
    }

    [Fact]
    public void Wrap_Respects_MaxDepth_For_Conditions_And_Groups()
    {
        var root = Sample();

        // c2 sits at depth 2; wrapping it would create depth 3.
        CyRuleEdits.Wrap(root, "c2", new CyRuleLimits(MaxDepth: 2)).ShouldBeSameAs(root);
        CyRuleEdits.Wrap(root, "c2", new CyRuleLimits(MaxDepth: 3)).ShouldNotBeSameAs(root);

        // g1 spans one level at depth 2; wrapping it needs depth 3.
        CyRuleEdits.Wrap(root, "g1", new CyRuleLimits(MaxDepth: 2)).ShouldBeSameAs(root);
    }

    [Fact]
    public void Wrap_Cannot_Wrap_The_Root()
    {
        var root = Sample();

        CyRuleEdits.Wrap(root, "root").ShouldBeSameAs(root);
    }

    [Fact]
    public void Ungroup_Splices_The_Children_Into_The_Parent_In_Place()
    {
        var next = CyRuleEdits.Ungroup(Sample(), "g1");

        Ids(next).ShouldBe("c1,c2,c3,c4");
    }

    [Fact]
    public void Ungroup_Ignores_The_Root_Conditions_And_Unknown_Ids()
    {
        var root = Sample();

        CyRuleEdits.Ungroup(root, "root").ShouldBeSameAs(root);
        CyRuleEdits.Ungroup(root, "c1").ShouldBeSameAs(root);
        CyRuleEdits.Ungroup(root, "nope").ShouldBeSameAs(root);
    }

    // ---- move and reorder

    [Fact]
    public void MoveTo_Moves_A_Condition_Into_A_Group_At_An_Index()
    {
        var next = CyRuleEdits.MoveTo(Sample(), "c1", "g1", 1);

        Ids(next).ShouldBe("g1,c4");
        Ids(Group(next, "g1")).ShouldBe("c2,c1,c3");
    }

    [Fact]
    public void MoveTo_Moves_A_Condition_Out_Of_A_Group_To_The_Root()
    {
        var next = CyRuleEdits.MoveTo(Sample(), "c3", "root", 0);

        Ids(next).ShouldBe("c3,c1,g1,c4");
        Ids(Group(next, "g1")).ShouldBe("c2");
    }

    [Fact]
    public void MoveTo_Index_Is_Taken_After_The_Node_Is_Removed_When_Staying_In_The_Same_Group()
    {
        var next = CyRuleEdits.MoveTo(Sample(), "c1", "root", 2);

        Ids(next).ShouldBe("g1,c4,c1");
    }

    [Fact]
    public void MoveTo_Clamps_An_Out_Of_Range_Index()
    {
        var next = CyRuleEdits.MoveTo(Sample(), "c1", "g1", 99);

        Ids(Group(next, "g1")).ShouldBe("c2,c3,c1");
    }

    [Fact]
    public void MoveTo_Refuses_To_Move_A_Group_Into_Itself_Or_A_Descendant()
    {
        var root = CyRuleEdits.AddGroup(Sample(), "g1");
        var inner = (CyRuleGroup)Group(root, "g1").Children[^1];

        CyRuleEdits.MoveTo(root, "g1", "g1", 0).ShouldBeSameAs(root);
        CyRuleEdits.MoveTo(root, "g1", inner.Id, 0).ShouldBeSameAs(root);
    }

    [Fact]
    public void MoveTo_Respects_MaxDepth_For_Groups()
    {
        var root = CyRuleEdits.AddGroup(Sample(), "root"); // a second group at depth 2
        var other = (CyRuleGroup)root.Children[^1];

        // g1 (one level) into other (depth 2) would sit at depth 3.
        CyRuleEdits.MoveTo(root, "g1", other.Id, 0, new CyRuleLimits(MaxDepth: 2)).ShouldBeSameAs(root);
        CyRuleEdits.MoveTo(root, "g1", other.Id, 0, new CyRuleLimits(MaxDepth: 3)).ShouldNotBeSameAs(root);
    }

    [Fact]
    public void MoveTo_Rejects_The_Root_Unknown_Nodes_And_Non_Group_Targets()
    {
        var root = Sample();

        CyRuleEdits.MoveTo(root, "root", "g1", 0).ShouldBeSameAs(root);
        CyRuleEdits.MoveTo(root, "nope", "g1", 0).ShouldBeSameAs(root);
        CyRuleEdits.MoveTo(root, "c1", "c4", 0).ShouldBeSameAs(root);
    }

    [Fact]
    public void Reorder_Moves_A_Child_Within_Its_Group()
    {
        var next = CyRuleEdits.Reorder(Sample(), "root", 0, 2);

        Ids(next).ShouldBe("g1,c4,c1");
    }

    [Fact]
    public void Reorder_Ignores_Bad_Indexes_And_Same_Position()
    {
        var root = Sample();

        CyRuleEdits.Reorder(root, "root", 0, 0).ShouldBeSameAs(root);
        CyRuleEdits.Reorder(root, "root", -1, 1).ShouldBeSameAs(root);
        CyRuleEdits.Reorder(root, "root", 0, 3).ShouldBeSameAs(root);
        CyRuleEdits.Reorder(root, "c1", 0, 1).ShouldBeSameAs(root);
    }

    // ---- update

    [Fact]
    public void SetCombinator_And_SetNot_Change_Only_That_Group()
    {
        var next = CyRuleEdits.SetNot(CyRuleEdits.SetCombinator(Sample(), "root", CyRuleCombinator.Or), "g1", true);

        next.Combinator.ShouldBe(CyRuleCombinator.Or);
        Group(next, "g1").Not.ShouldBeTrue();
        Group(next, "g1").Combinator.ShouldBe(CyRuleCombinator.Or);
        next.Not.ShouldBeFalse();
    }

    [Fact]
    public void Update_Keeps_The_Node_Id_And_Changes_A_Condition()
    {
        var next = CyRuleEdits.Update(Sample(), "c2", n => ((CyRuleCondition)n) with { FieldKey = "status" });

        var c2 = (CyRuleCondition)CyRuleEdits.Find(next, "c2")!;
        c2.FieldKey.ShouldBe("status");
        c2.Id.ShouldBe("c2");
    }

    [Fact]
    public void Update_Of_An_Unknown_Id_Returns_The_Same_Root()
    {
        var root = Sample();

        CyRuleEdits.Update(root, "nope", n => n).ShouldBeSameAs(root);
    }
}
