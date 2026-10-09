using System.Globalization;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyRuleSummaryTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private static readonly IReadOnlyList<CyRuleField> Fields =
    [
        new() { Key = "age", Label = "Age", Type = CyRuleFieldType.Number },
        new() { Key = "weight", Label = "Weight", Type = CyRuleFieldType.Number, Unit = "kg" },
        new() { Key = "dob", Label = "Date of birth", Type = CyRuleFieldType.Date },
        new() { Key = "smoker", Label = "Smoker", Type = CyRuleFieldType.Boolean },
        new()
        {
            Key = "status",
            Label = "Status",
            Type = CyRuleFieldType.Choice,
            Options = [new CyOption<string>("act", "Active"), new CyOption<string>("pen", "Pending")]
        },
        new() { Key = "term", Label = "Diagnosis", Type = CyRuleFieldType.Coded }
    ];

    private static CyRuleCondition Cond(string field, string op, object? v = null, object? v2 = null) =>
        new() { FieldKey = field, OperatorKey = op, Value = v, Value2 = v2 };

    private static string Describe(CyRuleGroup g, CyRuleBuilderText? text = null) =>
        CyRuleSummary.Describe(g, Fields, text, Culture);

    [Fact]
    public void A_Rule_With_No_Conditions_Says_So()
    {
        Describe(new CyRuleGroup()).ShouldBe("No conditions yet.");
        Describe(new CyRuleGroup { Children = [new CyRuleGroup()] }).ShouldBe("No conditions yet.");
    }

    [Fact]
    public void One_Condition_Reads_Field_Operator_Value()
    {
        var g = new CyRuleGroup { Children = [Cond("age", "gt", 18m)] };

        Describe(g).ShouldBe("Age is greater than 18");
    }

    [Fact]
    public void An_And_Group_Joins_With_And_And_An_Or_Group_With_Or()
    {
        var and = new CyRuleGroup { Children = [Cond("age", "gt", 18m), Cond("smoker", "true")] };
        var or = and with { Combinator = CyRuleCombinator.Or };

        Describe(and).ShouldBe("Age is greater than 18 and Smoker is yes");
        Describe(or).ShouldBe("Age is greater than 18 or Smoker is yes");
    }

    [Fact]
    public void A_Nested_Group_Of_Several_Parts_Is_Parenthesised()
    {
        var g = new CyRuleGroup
        {
            Children =
            [
                Cond("age", "gt", 18m),
                new CyRuleGroup
                {
                    Combinator = CyRuleCombinator.Or,
                    Children = [Cond("status", "eq", "act"), Cond("status", "eq", "pen")]
                }
            ]
        };

        Describe(g).ShouldBe("Age is greater than 18 and (Status is Active or Status is Pending)");
    }

    [Fact]
    public void A_Nested_Group_Of_One_Part_Is_Not_Parenthesised()
    {
        var g = new CyRuleGroup
        {
            Children = [Cond("age", "gt", 18m), new CyRuleGroup { Children = [Cond("smoker", "true")] }]
        };

        Describe(g).ShouldBe("Age is greater than 18 and Smoker is yes");
    }

    [Fact]
    public void A_Negated_Group_Is_Prefixed_With_Not_And_Parenthesised_When_It_Has_Several_Parts()
    {
        var several = new CyRuleGroup
        {
            Not = true,
            Children = [Cond("age", "gt", 18m), Cond("smoker", "true")]
        };
        var one = new CyRuleGroup { Not = true, Children = [Cond("smoker", "true")] };

        Describe(several).ShouldBe("not (Age is greater than 18 and Smoker is yes)");
        Describe(one).ShouldBe("not Smoker is yes");
    }

    [Fact]
    public void Between_Reads_Both_Values()
    {
        var g = new CyRuleGroup { Children = [Cond("age", "between", 18m, 65m)] };

        Describe(g).ShouldBe("Age is between 18 and 65");
    }

    [Fact]
    public void A_Number_Unit_Follows_The_Value()
    {
        var g = new CyRuleGroup { Children = [Cond("weight", "lt", 60m)] };

        Describe(g).ShouldBe("Weight is less than 60 kg");
    }

    [Fact]
    public void A_Date_Uses_The_Given_Culture()
    {
        var g = new CyRuleGroup { Children = [Cond("dob", "before", new DateOnly(2026, 10, 7))] };

        Describe(g).ShouldBe("Date of birth is before 10/07/2026");
        CyRuleSummary.Describe(g, Fields, null, CultureInfo.GetCultureInfo("en-GB"))
            .ShouldBe("Date of birth is before 07/10/2026");
    }

    [Fact]
    public void A_Choice_Value_Shows_The_Option_Text_And_Falls_Back_To_The_Raw_Value()
    {
        Describe(new CyRuleGroup { Children = [Cond("status", "eq", "act")] }).ShouldBe("Status is Active");
        Describe(new CyRuleGroup { Children = [Cond("status", "eq", "zzz")] }).ShouldBe("Status is zzz");
    }

    [Fact]
    public void A_Coded_Value_Shows_Its_Label_When_There_Is_One()
    {
        var withLabel = Cond("term", "eq", "I10") with { ValueLabel = "Hypertension" };

        Describe(new CyRuleGroup { Children = [withLabel] }).ShouldBe("Diagnosis is Hypertension");
        Describe(new CyRuleGroup { Children = [Cond("term", "eq", "I10")] }).ShouldBe("Diagnosis is I10");
    }

    [Fact]
    public void An_Incomplete_Condition_Uses_The_Incomplete_Phrase()
    {
        var g = new CyRuleGroup { Children = [Cond("age", "gt"), Cond("smoker", "true")] };

        Describe(g).ShouldBe("(incomplete condition) and Smoker is yes");
    }

    [Fact]
    public void An_Empty_Nested_Group_Is_Left_Out_When_There_Are_Other_Conditions()
    {
        var g = new CyRuleGroup { Children = [Cond("smoker", "true"), new CyRuleGroup()] };

        Describe(g).ShouldBe("Smoker is yes");
    }

    [Fact]
    public void Custom_Text_Can_Reorder_Words_And_Change_Joiners()
    {
        var text = new CyRuleBuilderText
        {
            Condition1 = "{1} {0}: {2}",
            JoinAnd = " AC ",
            OpGt = "mwy na"
        };
        var g = new CyRuleGroup { Children = [Cond("age", "gt", 18m), Cond("smoker", "true")] };

        Describe(g, text).ShouldBe("mwy na Age: 18 AC Smoker is yes");
    }

    [Theory]
    [InlineData(CyRuleCombinator.And, false, "All of the following")]
    [InlineData(CyRuleCombinator.Or, false, "Any of the following")]
    [InlineData(CyRuleCombinator.And, true, "Not all of the following")]
    [InlineData(CyRuleCombinator.Or, true, "None of the following")]
    public void GroupLegend_Names_Each_Combination(CyRuleCombinator combinator, bool not, string expected)
    {
        var g = new CyRuleGroup { Combinator = combinator, Not = not };

        CyRuleSummary.GroupLegend(g).ShouldBe(expected);
    }
}
