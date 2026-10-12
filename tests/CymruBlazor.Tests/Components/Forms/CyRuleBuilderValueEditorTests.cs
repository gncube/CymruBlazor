using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary><see cref="CyRuleField.ValueEditor"/>: a consumer's own editor in place of the built-in one.</summary>
public sealed class CyRuleBuilderValueEditorTests : TestContextBase
{
    private CyRuleGroup? _current;
    private CyRuleEditorContext? _context;
    private int _renders;

    public CyRuleBuilderValueEditorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js").Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js").Mode = JSRuntimeMode.Loose;

        var focus = new Mock<IFocusManager>();
        focus
            .Setup(f => f.FocusAsync(It.IsAny<string>(), It.IsAny<FocusOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FocusResult(true));
        Services.AddSingleton(focus.Object);
    }

    /// <summary>A plain text input per value: <c>data-editor</c> marks it, a change stores the typed text as a number.</summary>
    private RenderFragment<CyRuleEditorContext> HeartRateEditor => context => builder =>
    {
        _context = context;
        _renders++;

        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "id", context.ValueId);
        builder.AddAttribute(2, "data-editor", "first");
        builder.AddAttribute(3, "aria-label", context.Label);
        builder.AddAttribute(4, "data-invalid", context.Invalid ? "true" : "false");
        builder.AddAttribute(5, "value", context.Value?.ToString());
        builder.AddAttribute(6, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
            this, e => context.SetValueAsync(decimal.Parse(e.Value!.ToString()!, System.Globalization.CultureInfo.InvariantCulture), "label-one")));
        builder.CloseElement();

        if (context.Arity == 2)
        {
            builder.OpenElement(7, "input");
            builder.AddAttribute(8, "id", context.Value2Id);
            builder.AddAttribute(9, "data-editor", "second");
            builder.AddAttribute(10, "aria-label", context.Label2);
            builder.AddAttribute(11, "value", context.Value2?.ToString());
            builder.AddAttribute(12, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
                this, e => context.SetValue2Async(decimal.Parse(e.Value!.ToString()!, System.Globalization.CultureInfo.InvariantCulture))));
            builder.CloseElement();
        }
    };

    private static RenderFragment<CyRuleEditorContext> Throwing(bool inOuterLambda, string message) => context =>
    {
        if (inOuterLambda)
        {
            throw new InvalidOperationException(message);
        }

        return builder => throw new InvalidOperationException(message);
    };

    private static IReadOnlyList<CyRuleField> FieldsWith(RenderFragment<CyRuleEditorContext>? editor) =>
    [
        new CyRuleField { Key = "name", Label = "Name" },
        new CyRuleField { Key = "hr", Label = "Heart rate", Type = CyRuleFieldType.Number, ValueEditor = editor }
    ];

    private IRenderedComponent<CyRuleBuilder> RenderRule(
        CyRuleGroup rule,
        IReadOnlyList<CyRuleField> fields,
        bool showIssues = false)
    {
        _current = rule;

        return Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Eligibility rule")
            .Add(c => c.Fields, fields)
            .Add(c => c.Value, rule)
            .Add(c => c.ShowIssues, showIssues)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<CyRuleGroup>(this, g => _current = g)));
    }

    private static CyRuleGroup RuleOf(string op, object? value = null, object? value2 = null) => new()
    {
        Children = [new CyRuleCondition { FieldKey = "hr", OperatorKey = op, Value = value, Value2 = value2 }]
    };

    private CyRuleCondition OnlyCondition() => (CyRuleCondition)_current!.Children.Single();

    [Fact]
    public void A_Custom_Editor_Replaces_The_Built_In_One_And_Is_Grouped_And_Named()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(HeartRateEditor));

        var custom = cut.Find(".cy-rule__control--custom");
        custom.GetAttribute("role").ShouldBe("group");
        custom.GetAttribute("aria-label").ShouldBe("Value");
        custom.QuerySelectorAll("[data-editor]").Length.ShouldBe(1);
        custom.QuerySelector("[data-editor=first]")!.GetAttribute("value").ShouldBe("60");
        cut.FindAll(".cy-rule__control--value").Count.ShouldBe(1);
    }

    [Fact]
    public void The_Context_Carries_The_Operator_Ids_And_Localised_Names()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(HeartRateEditor));

        _context.ShouldNotBeNull();
        _context.Field.Key.ShouldBe("hr");
        _context.OperatorKey.ShouldBe(CyRuleOperators.Gt);
        _context.Arity.ShouldBe(1);
        _context.Label.ShouldBe("Value");
        _context.Label2.ShouldBe("Upper value");
        _context.Disabled.ShouldBeFalse();
        cut.Find("[data-editor=first]").Id.ShouldBe(_context.ValueId);
        _context.ValueId.ShouldEndWith("-value");
        _context.Value2Id.ShouldEndWith("-value2");
    }

    [Fact]
    public void Storing_A_Value_Edits_The_Rule_With_The_Value_And_Its_Label()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt), FieldsWith(HeartRateEditor));

        cut.Find("[data-editor=first]").Change("72");

        OnlyCondition().Value.ShouldBe(72m);
        OnlyCondition().ValueLabel.ShouldBe("label-one");
    }

    [Fact]
    public void Between_Gives_The_Editor_Arity_Two_And_A_Way_To_Store_The_Second_Value()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Between, 50m), FieldsWith(HeartRateEditor));

        _context!.Arity.ShouldBe(2);
        cut.FindAll("[data-editor]").Count.ShouldBe(2);
        cut.Find("[data-editor=second]").GetAttribute("aria-label").ShouldBe("Upper value");

        cut.Find("[data-editor=second]").Change("90");

        OnlyCondition().Value2.ShouldBe(90m);
        OnlyCondition().Value.ShouldBe(50m);
    }

    [Theory]
    [InlineData(CyRuleOperators.IsEmpty)]
    [InlineData(CyRuleOperators.IsNotEmpty)]
    public void An_Operator_With_No_Value_Does_Not_Show_The_Editor(string op)
    {
        var cut = RenderRule(RuleOf(op), FieldsWith(HeartRateEditor));

        cut.FindAll(".cy-rule__control--custom").ShouldBeEmpty();
        cut.FindAll("[data-editor]").ShouldBeEmpty();
    }

    [Fact]
    public void A_Field_Without_An_Editor_Keeps_The_Built_In_One()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(null));

        cut.FindAll(".cy-rule__control--custom").ShouldBeEmpty();
        cut.FindAll("input").Any(i => i.GetAttribute("id")?.EndsWith("-value", StringComparison.Ordinal) == true).ShouldBeTrue();
    }

    [Fact]
    public void An_Incomplete_Condition_Marks_The_Context_Invalid_And_Links_The_Group_To_The_Message()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt), FieldsWith(HeartRateEditor), showIssues: true);

        _context!.Invalid.ShouldBeTrue();
        _context.IssueId.ShouldNotBeNull();
        cut.Find("[data-editor=first]").GetAttribute("data-invalid").ShouldBe("true");
        cut.Find(".cy-rule__control--custom").GetAttribute("aria-describedby").ShouldBe(_context.IssueId);
        cut.Find($"#{_context.IssueId}").TextContent.ShouldContain("value");
    }

    [Fact]
    public void A_Complete_Condition_Is_Not_Marked_Invalid()
    {
        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(HeartRateEditor), showIssues: true);

        _context!.Invalid.ShouldBeFalse();
        cut.Find(".cy-rule__control--custom").HasAttribute("aria-describedby").ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_Editor_That_Throws_Shows_The_Fixed_Message_And_Never_The_Exception_Text(bool inOuterLambda)
    {
        const string secret = "SECRET-DETAIL-12345";

        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(Throwing(inOuterLambda, secret)));

        cut.Markup.ShouldContain("This value editor could not be shown.");
        cut.Markup.ShouldNotContain(secret);
        cut.Markup.ShouldNotContain("InvalidOperationException");
    }

    [Fact]
    public void A_Failed_Editor_Leaves_The_Rest_Of_The_Builder_Working()
    {
        var fields = new List<CyRuleField>
        {
            new() { Key = "name", Label = "Name" },
            new() { Key = "hr", Label = "Heart rate", Type = CyRuleFieldType.Number, ValueEditor = Throwing(true, "boom") }
        };
        var rule = new CyRuleGroup
        {
            Children =
            [
                new CyRuleCondition { FieldKey = "hr", OperatorKey = CyRuleOperators.Gt, Value = 60m },
                new CyRuleCondition { FieldKey = "name", OperatorKey = CyRuleOperators.Eq, Value = "Ann" }
            ]
        };

        var cut = RenderRule(rule, fields);

        cut.FindAll(".cy-rule__control--custom").Count.ShouldBe(1);
        var nameBox = cut.FindAll("input").First(i => i.GetAttribute("value") == "Ann");
        nameBox.Change("Bea");

        ((CyRuleCondition)_current!.Children[1]).Value.ShouldBe("Bea");
    }

    [Fact]
    public void Changing_The_Operator_Gives_A_Failed_Editor_A_Fresh_Chance()
    {
        var calls = 0;
        RenderFragment<CyRuleEditorContext> flaky = context => builder =>
        {
            calls++;
            if (context.OperatorKey == CyRuleOperators.Gt)
            {
                throw new InvalidOperationException("only for gt");
            }

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "data-editor", "first");
            builder.CloseElement();
        };

        var cut = RenderRule(RuleOf(CyRuleOperators.Gt, 60m), FieldsWith(flaky));
        cut.Markup.ShouldContain("This value editor could not be shown.");

        var operatorSelect = cut.FindAll("select").First(s => s.GetAttribute("id")?.EndsWith("-operator", StringComparison.Ordinal) == true);
        operatorSelect.Change(CyRuleOperators.Lt);

        cut.Markup.ShouldNotContain("This value editor could not be shown.");
        cut.FindAll("[data-editor=first]").Count.ShouldBe(1);
        calls.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void The_Fallback_Text_Can_Be_Replaced_For_Welsh()
    {
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rheol")
            .Add(c => c.Fields, FieldsWith(Throwing(true, "x")))
            .Add(c => c.Value, RuleOf(CyRuleOperators.Gt, 60m))
            .Add(c => c.Text, new CyRuleBuilderText { EditorFailed = "Ni ellid dangos y golygydd gwerth hwn." }));

        cut.Markup.ShouldContain("Ni ellid dangos y golygydd gwerth hwn.");
    }

    [Fact]
    public void A_Disabled_Builder_Tells_The_Editor()
    {
        Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, FieldsWith(HeartRateEditor))
            .Add(c => c.Value, RuleOf(CyRuleOperators.Gt, 60m))
            .Add(c => c.Disabled, true));

        _context!.Disabled.ShouldBeTrue();
    }

    [Fact]
    public void A_Read_Only_Builder_Does_Not_Render_The_Editor()
    {
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, FieldsWith(HeartRateEditor))
            .Add(c => c.Value, RuleOf(CyRuleOperators.Gt, 60m))
            .Add(c => c.ReadOnly, true));

        cut.FindAll("[data-editor]").ShouldBeEmpty();
        _renders.ShouldBe(0);
    }
}
