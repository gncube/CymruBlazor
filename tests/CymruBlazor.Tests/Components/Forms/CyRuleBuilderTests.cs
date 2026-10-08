using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using AngleSharp.Dom;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>The behaviour of <see cref="CyRuleBuilder"/> that does not need a browser.</summary>
public sealed class CyRuleBuilderTests : TestContextBase
{
    private static readonly IReadOnlyList<CyRuleField> Fields =
    [
        new CyRuleField { Key = "name", Label = "Name" },
        new CyRuleField { Key = "city", Label = "City" },
        new CyRuleField { Key = "age", Label = "Age", Type = CyRuleFieldType.Number, Min = 0, Max = 120, Unit = "years" },
        new CyRuleField { Key = "dob", Label = "Date of birth", Type = CyRuleFieldType.Date },
        new CyRuleField
        {
            Key = "status",
            Label = "Status",
            Type = CyRuleFieldType.Choice,
            Options = [new CyOption<string>("a", "Active"), new CyOption<string>("p", "Pending")]
        },
        new CyRuleField
        {
            Key = "dx",
            Label = "Diagnosis",
            Type = CyRuleFieldType.Coded,
            ItemsProvider = (_, _) => Task.FromResult<IReadOnlyList<CyOption<string>>>([new CyOption<string>("x", "Asthma")])
        },
        new CyRuleField { Key = "smoker", Label = "Smoker", Type = CyRuleFieldType.Boolean }
    ];

    private readonly Mock<IFocusManager> _focus = new();
    private readonly List<string> _focused = [];
    private CyRuleGroup? _current;
    private int _changes;

    public CyRuleBuilderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js").Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;

        _focus
            .Setup(f => f.FocusAsync(It.IsAny<string>(), It.IsAny<FocusOptions?>(), It.IsAny<CancellationToken>()))
            .Callback<string, FocusOptions?, CancellationToken>((id, _, _) => _focused.Add(id))
            .ReturnsAsync(new FocusResult(true));
        Services.AddSingleton(_focus.Object);
    }

    private IRenderedComponent<CyRuleBuilder> RenderBuilder(
        CyRuleGroup? rule,
        Action<ComponentParameterCollectionBuilder<CyRuleBuilder>>? configure = null)
    {
        _current = rule;

        return Render<CyRuleBuilder>(p =>
        {
            p.Add(c => c.Label, "Eligibility rule")
                .Add(c => c.Fields, Fields)
                .Add(c => c.Value, rule)
                .Add(c => c.ValueChanged, EventCallback.Factory.Create<CyRuleGroup>(this, g =>
                {
                    _current = g;
                    _changes++;
                }));
            configure?.Invoke(p);
        });
    }

    private static CyRuleCondition Cond(string field, string op, object? value = null) =>
        new() { FieldKey = field, OperatorKey = op, Value = value };

    private static CyRuleGroup Rule(params CyRuleNode[] children) => new() { Children = children };

    private static IElement ButtonWithText(IRenderedComponent<CyRuleBuilder> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text);

    private static IElement ByLabel(IRenderedComponent<CyRuleBuilder> cut, string label) =>
        cut.Find($"[aria-label='{label}']");

    private static string Status(IRenderedComponent<CyRuleBuilder> cut) =>
        cut.Find("[role=status]").TextContent.Replace(' ', ' ').Trim();

    [Fact]
    public void Empty_Rule_Shows_Group_Legend_Empty_Message_And_Add_Buttons()
    {
        var cut = RenderBuilder(null);

        cut.Find("fieldset legend").TextContent.ShouldBe("All of the following");
        cut.Find(".cy-rule__empty").TextContent.ShouldBe("No conditions in this group.");
        ButtonWithText(cut, "Add condition").ShouldNotBeNull();
        ButtonWithText(cut, "Add group").ShouldNotBeNull();
        cut.Find("[role=group]").GetAttribute("aria-label").ShouldBe("Eligibility rule");
    }

    [Fact]
    public void Conditions_Are_Named_By_Position_And_Listed_In_A_Named_List()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a"), Cond("city", "eq", "b")));

        cut.FindAll("[aria-label='Condition 1 of 2']").Count.ShouldBe(1);
        cut.FindAll("[aria-label='Condition 2 of 2']").Count.ShouldBe(1);
        cut.Find("ul.cy-rule__list").GetAttribute("aria-label").ShouldBe("Conditions: All of the following");
    }

    [Fact]
    public void Add_Condition_Appends_A_Blank_Condition_Announces_And_Focuses_Its_Field()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        ButtonWithText(cut, "Add condition").Click();

        _current!.Children.Count.ShouldBe(2);
        var added = (CyRuleCondition)_current.Children[1];
        added.FieldKey.ShouldBeNull();
        Status(cut).ShouldBe("Condition 2 added.");
        cut.WaitForAssertion(() => _focused.Any(id => id.EndsWith($"{added.Id}-field", StringComparison.Ordinal)).ShouldBeTrue());
    }

    [Fact]
    public void Add_Group_Adds_A_Nested_Group_With_One_Condition()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        ButtonWithText(cut, "Add group").Click();

        var nested = _current!.Children[1].ShouldBeOfType<CyRuleGroup>();
        nested.Children.Count.ShouldBe(1);
        Status(cut).ShouldBe("Group added.");
        cut.FindAll("fieldset").Count.ShouldBe(2);
    }

    [Fact]
    public void Changing_The_Combinator_Updates_The_Model_And_The_Legend()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        cut.FindAll("input[type=radio]")[1].Change(true);

        _current!.Combinator.ShouldBe(CyRuleCombinator.Or);
        cut.Find("fieldset legend").TextContent.ShouldBe("Any of the following");
    }

    [Fact]
    public void Negating_A_Group_Changes_The_Legend()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        cut.Find("button[role=switch]").Click();

        _current!.Not.ShouldBeTrue();
        cut.Find("fieldset legend").TextContent.ShouldBe("Not all of the following");
    }

    [Fact]
    public void Choosing_A_Field_Offers_The_Operators_Of_Its_Type()
    {
        var cut = RenderBuilder(Rule(new CyRuleCondition()));
        cut.FindAll("select")[1].HasAttribute("disabled").ShouldBeTrue();

        cut.FindAll("select")[0].Change("age");

        var options = cut.FindAll("select")[1].QuerySelectorAll("option").Select(o => o.TextContent).ToList();
        options.ShouldContain("is between");
        options.ShouldNotContain("contains");
        ((CyRuleCondition)_current!.Children[0]).FieldKey.ShouldBe("age");
    }

    [Fact]
    public void Changing_To_A_Field_Of_Another_Type_Resets_Operator_And_Value_And_Announces_It()
    {
        var cut = RenderBuilder(Rule(Cond("age", "gte", 5m)));

        cut.FindAll("select")[0].Change("name");

        var condition = (CyRuleCondition)_current!.Children[0];
        condition.FieldKey.ShouldBe("name");
        condition.OperatorKey.ShouldBeNull();
        condition.Value.ShouldBeNull();
        Status(cut).ShouldBe("Field changed. The operator and value were reset.");
    }

    [Fact]
    public void Changing_To_Another_Text_Field_Keeps_Operator_And_Value()
    {
        var cut = RenderBuilder(Rule(Cond("name", "contains", "Jo")));

        cut.FindAll("select")[0].Change("city");

        var condition = (CyRuleCondition)_current!.Children[0];
        condition.OperatorKey.ShouldBe("contains");
        condition.Value.ShouldBe("Jo");
        Status(cut).ShouldBeEmpty();
    }

    [Fact]
    public void Between_Shows_Two_Value_Editors_And_Empty_Shows_None()
    {
        var cut = RenderBuilder(Rule(new CyRuleCondition { FieldKey = "age", OperatorKey = "between" }));
        cut.FindAll("input[type=text]").Count.ShouldBe(2);

        var none = RenderBuilder(Rule(new CyRuleCondition { FieldKey = "age", OperatorKey = "empty" }));
        none.FindAll("input[type=text]").Count.ShouldBe(0);
    }

    [Fact]
    public void Operator_With_No_Value_Clears_The_Stored_Value()
    {
        var cut = RenderBuilder(Rule(Cond("age", "gte", 5m)));

        cut.FindAll("select")[1].Change("empty");

        var condition = (CyRuleCondition)_current!.Children[0];
        condition.OperatorKey.ShouldBe("empty");
        condition.Value.ShouldBeNull();
    }

    [Fact]
    public void Typing_Text_And_Numbers_Stores_Typed_Values()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq")));
        cut.Find("input[type=text]").Change("Jo");
        ((CyRuleCondition)_current!.Children[0]).Value.ShouldBe("Jo");

        var numbers = RenderBuilder(Rule(Cond("age", "gte")));
        numbers.Find("input[type=text]").Change("42");
        ((CyRuleCondition)_current!.Children[0]).Value.ShouldBe(42m);
    }

    [Fact]
    public void Choosing_From_A_Choice_Field_Stores_The_Key_And_Its_Label()
    {
        var cut = RenderBuilder(Rule(Cond("status", "eq")));

        cut.FindAll("select")[2].Change("p");

        var condition = (CyRuleCondition)_current!.Children[0];
        condition.Value.ShouldBe("p");
        condition.ValueLabel.ShouldBe("Pending");
    }

    [Fact]
    public void A_Coded_Value_Shows_Its_Stored_Label()
    {
        var cut = RenderBuilder(Rule(new CyRuleCondition { FieldKey = "dx", OperatorKey = "eq", Value = "x", ValueLabel = "Asthma" }));

        cut.Find("input[role=combobox]").GetAttribute("value").ShouldBe("Asthma");
    }

    [Fact]
    public void Remove_Moves_Focus_To_The_Next_Condition_Then_The_Previous_Then_The_Add_Button()
    {
        var first = Cond("name", "eq", "a");
        var second = Cond("city", "eq", "b");
        var cut = RenderBuilder(Rule(first, second));

        ByLabel(cut, "Remove condition 1").Click();
        _current!.Children.Count.ShouldBe(1);
        Status(cut).ShouldBe("Condition removed.");
        cut.WaitForAssertion(() => _focused.Last().ShouldEndWith($"{second.Id}-field"));

        ByLabel(cut, "Remove condition 1").Click();
        _current.Children.Count.ShouldBe(0);
        cut.WaitForAssertion(() => _focused.Last().ShouldEndWith($"{_current.Id}-add"));
    }

    [Fact]
    public void Removing_The_Last_Condition_Focuses_The_Previous_One()
    {
        var first = Cond("name", "eq", "a");
        var second = Cond("city", "eq", "b");
        var cut = RenderBuilder(Rule(first, second));

        ByLabel(cut, "Remove condition 2").Click();

        cut.WaitForAssertion(() => _focused.Last().ShouldEndWith($"{first.Id}-field"));
    }

    [Fact]
    public void Removing_A_Group_Asks_First_And_Keeps_It_When_Declined()
    {
        var confirm = new Mock<ICyConfirmService>();
        confirm.Setup(c => c.ConfirmAsync(It.IsAny<CyConfirmOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Services.AddSingleton(confirm.Object);
        var cut = RenderBuilder(Rule(new CyRuleGroup { Children = [Cond("name", "eq", "a")] }));

        cut.Find("button[aria-label^='Remove group']").Click();

        _changes.ShouldBe(0);
        confirm.Verify(c => c.ConfirmAsync(It.Is<CyConfirmOptions>(o => o.Destructive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Removing_A_Group_Removes_It_When_Confirmed()
    {
        var confirm = new Mock<ICyConfirmService>();
        confirm.Setup(c => c.ConfirmAsync(It.IsAny<CyConfirmOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Services.AddSingleton(confirm.Object);
        var cut = RenderBuilder(Rule(Cond("age", "gte", 1m), new CyRuleGroup { Children = [Cond("name", "eq", "a")] }));

        cut.Find("button[aria-label^='Remove group']").Click();

        _current!.Children.Count.ShouldBe(1);
        Status(cut).ShouldBe("Group removed.");
    }

    [Fact]
    public void Removing_A_Group_Without_A_Confirm_Service_Just_Removes_It()
    {
        var cut = RenderBuilder(Rule(Cond("age", "gte", 1m), new CyRuleGroup { Children = [Cond("name", "eq", "a")] }));

        cut.Find("button[aria-label^='Remove group']").Click();

        _current!.Children.Count.ShouldBe(1);
    }

    [Fact]
    public void Move_Buttons_Reorder_And_The_Edges_Are_Disabled()
    {
        var a = Cond("name", "eq", "a");
        var b = Cond("city", "eq", "b");
        var cut = RenderBuilder(Rule(a, b));

        ByLabel(cut, "Move condition 1 up").HasAttribute("disabled").ShouldBeTrue();
        ByLabel(cut, "Move condition 2 down").HasAttribute("disabled").ShouldBeTrue();

        ByLabel(cut, "Move condition 2 up").Click();

        _current!.Children[0].Id.ShouldBe(b.Id);
        _current.Children[1].Id.ShouldBe(a.Id);
        Status(cut).ShouldBe("Moved to: Top level.");
        cut.WaitForAssertion(() => _focused.Last().ShouldEndWith($"{b.Id}-down"));
    }

    [Fact]
    public void Row_Menu_Duplicates_A_Condition()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        cut.Find("button[aria-label='More actions for condition 1']").Click();
        cut.FindAll("[role=menuitem]").First(i => i.TextContent.Trim() == "Duplicate").Click();

        _current!.Children.Count.ShouldBe(2);
        ((CyRuleCondition)_current.Children[1]).Value.ShouldBe("a");
        Status(cut).ShouldBe("Duplicated.");
    }

    [Fact]
    public void Row_Menu_Moves_A_Condition_Into_Another_Group()
    {
        var moving = Cond("name", "eq", "a");
        var nested = new CyRuleGroup { Children = [Cond("city", "eq", "b")] };
        var cut = RenderBuilder(Rule(moving, nested));

        cut.Find("button[aria-label='More actions for condition 1']").Click();
        cut.FindAll("[role=menuitem]").First(i => i.TextContent.Contains("Group 1", StringComparison.Ordinal)).Click();

        _current!.Children.Count.ShouldBe(1);
        ((CyRuleGroup)_current.Children[0]).Children[^1].Id.ShouldBe(moving.Id);
    }

    [Fact]
    public void Row_Menu_Wraps_A_Condition_In_A_Group_And_Ungroup_Reverses_It()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        cut.Find("button[aria-label='More actions for condition 1']").Click();
        cut.FindAll("[role=menuitem]").First(i => i.TextContent.Trim() == "Wrap in group").Click();
        _current!.Children[0].ShouldBeOfType<CyRuleGroup>();

        cut.Find("button[aria-label='More actions for group 1']").Click();
        cut.FindAll("[role=menuitem]").First(i => i.TextContent.Trim() == "Ungroup").Click();
        _current.Children[0].ShouldBeOfType<CyRuleCondition>();
    }

    [Fact]
    public void Reaching_The_Condition_Limit_Disables_Adding_And_Explains_Why()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")), p => p.Add(c => c.Limits, new CyRuleLimits(MaxConditions: 1)));

        var add = ButtonWithText(cut, "Add condition");
        add.HasAttribute("disabled").ShouldBeTrue();
        var hint = cut.Find(".cy-rule__hint");
        hint.TextContent.ShouldBe("The limit of 1 conditions has been reached.");
        add.GetAttribute("aria-describedby").ShouldBe(hint.Id);
    }

    [Fact]
    public void Reaching_The_Depth_Limit_Disables_Add_Group_Only()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")), p => p.Add(c => c.Limits, new CyRuleLimits(MaxDepth: 1)));

        ButtonWithText(cut, "Add group").HasAttribute("disabled").ShouldBeTrue();
        ButtonWithText(cut, "Add condition").HasAttribute("disabled").ShouldBeFalse();
        cut.Find(".cy-rule__hint").TextContent.ShouldBe("Groups cannot be nested any deeper.");
    }

    [Fact]
    public void Minimum_Conditions_Disables_Remove_And_Explains_Why()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")), p => p.Add(c => c.Limits, new CyRuleLimits(MinConditions: 1)));

        var remove = ByLabel(cut, "Remove condition 1");
        remove.HasAttribute("disabled").ShouldBeTrue();
        cut.Find(".cy-rule__hint").TextContent.ShouldBe("At least 1 condition is needed.");
    }

    [Fact]
    public void Summary_Describes_The_Rule_And_Can_Be_Hidden()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "Jo"), Cond("age", "gte", 18m)));
        cut.Find(".cy-rule__summary-text").TextContent.ShouldBe("Name is Jo and Age is at least 18 years");

        var hidden = RenderBuilder(Rule(Cond("name", "eq", "Jo")), p => p.Add(c => c.ShowSummary, false));
        hidden.FindAll(".cy-rule__summary").ShouldBeEmpty();
    }

    [Fact]
    public void Read_Only_Shows_Text_And_No_Controls()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "Jo")), p => p.Add(c => c.ReadOnly, true));

        cut.FindAll("button").ShouldBeEmpty();
        cut.FindAll("select").ShouldBeEmpty();
        cut.Find(".cy-rule__readonly").TextContent.ShouldBe("Name is Jo");
    }

    [Fact]
    public void Disabled_Disables_Every_Control()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "Jo")), p => p.Add(c => c.Disabled, true));

        cut.FindAll("select").ShouldAllBe(s => s.HasAttribute("disabled"));
        ButtonWithText(cut, "Add condition").HasAttribute("disabled").ShouldBeTrue();
        ByLabel(cut, "Remove condition 1").HasAttribute("disabled").ShouldBeTrue();
        ButtonWithText(cut, "Add condition").Click();
        _changes.ShouldBe(0);
    }

    [Fact]
    public void Issues_Are_Hidden_Until_Asked_For()
    {
        var cut = RenderBuilder(Rule(new CyRuleCondition()));

        cut.FindAll(".cy-rule__issue").ShouldBeEmpty();
        cut.Instance.IsValid.ShouldBeFalse();

        var shown = RenderBuilder(Rule(new CyRuleCondition()), p => p.Add(c => c.ShowIssues, true));
        var issue = shown.Find(".cy-rule__issue");
        issue.TextContent.Trim().ShouldBe("Choose a field.");
        shown.Find("[aria-label='Condition 1 of 1']").GetAttribute("aria-describedby").ShouldBe(issue.Id);
    }

    [Fact]
    public async Task ValidateAsync_Shows_Issues_Focuses_The_First_And_Returns_False()
    {
        var bad = new CyRuleCondition();
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a"), bad));

        var valid = await cut.InvokeAsync(() => cut.Instance.ValidateAsync());

        valid.ShouldBeFalse();
        cut.Find(".cy-rule__issue").TextContent.Trim().ShouldBe("Choose a field.");
        cut.WaitForAssertion(() => _focused.Last().ShouldEndWith($"{bad.Id}-field"));
    }

    [Fact]
    public async Task ValidateAsync_Returns_True_For_A_Complete_Rule()
    {
        var cut = RenderBuilder(Rule(Cond("name", "eq", "a")));

        (await cut.InvokeAsync(() => cut.Instance.ValidateAsync())).ShouldBeTrue();
    }

    [Fact]
    public void Validation_Callback_Fires_When_The_Set_Of_Issues_Changes()
    {
        IReadOnlyList<CyRuleIssue>? last = null;
        var cut = RenderBuilder(
            Rule(Cond("name", "eq", "a")),
            p => p.Add(c => c.OnValidationChanged, EventCallback.Factory.Create<IReadOnlyList<CyRuleIssue>>(this, i => last = i)));

        ButtonWithText(cut, "Add condition").Click();

        last.ShouldNotBeNull();
        last.Count.ShouldBe(1);
        last[0].Code.ShouldBe(CyRuleIssueCode.NoField);
    }

    [Fact]
    public void Text_Replaces_The_English_Phrases()
    {
        var cut = RenderBuilder(null, p => p.Add(c => c.Text, new CyRuleBuilderText { AddCondition = "Ychwanegu amod" }));

        ButtonWithText(cut, "Ychwanegu amod").ShouldNotBeNull();
    }

    [Fact]
    public void Works_Inside_An_EditForm_Without_Joining_Its_EditContext()
    {
        var model = new TestModel();
        var rule = Rule(
            Cond("name", "eq", "a"),
            Cond("age", "gte", 1m),
            new CyRuleCondition { FieldKey = "dob", OperatorKey = "before", Value = new DateOnly(2000, 1, 1) },
            Cond("status", "eq", "a"),
            Cond("dx", "eq", "x"));

        var cut = Render<CyRuleBuilder>(p => p
            .AddCascadingValue(new EditContext(model))
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, rule));

        cut.FindAll("select").Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Labels_And_Values_Are_Encoded_Not_Interpreted_As_Markup()
    {
        var hostile = "<img src=x onerror=alert(1)>";
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, [new CyRuleField { Key = "a", Label = hostile }])
            .Add(c => c.Value, Rule(Cond("a", "eq", hostile))));

        cut.FindAll("img").ShouldBeEmpty();

        var summary = cut.Find(".cy-rule__summary-text");

        summary.TextContent.ShouldContain(hostile);
        summary.Children.ShouldBeEmpty();
    }

    [Fact]
    public void Empty_Label_Duplicate_Field_Keys_And_Bad_Limits_Are_Rejected()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyRuleBuilder>(p => p.Add(c => c.Label, " ")));

        Should.Throw<InvalidOperationException>(() => Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, [new CyRuleField { Key = "a", Label = "A" }, new CyRuleField { Key = "a", Label = "B" }])));

        Should.Throw<InvalidOperationException>(() => Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Limits, new CyRuleLimits(MaxDepth: 0))));
    }

    private sealed class TestModel
    {
        public string? Name { get; set; }
    }
}
