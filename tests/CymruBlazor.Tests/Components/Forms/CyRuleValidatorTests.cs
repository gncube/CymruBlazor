using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyRuleValidatorTests
{
    private static readonly IReadOnlyList<CyRuleField> Fields =
    [
        new() { Key = "age", Label = "Age", Type = CyRuleFieldType.Number },
        new() { Key = "name", Label = "Name", Type = CyRuleFieldType.Text },
        new() { Key = "dob", Label = "Date of birth", Type = CyRuleFieldType.Date },
        new() { Key = "smoker", Label = "Smoker", Type = CyRuleFieldType.Boolean },
        new()
        {
            Key = "status",
            Label = "Status",
            Type = CyRuleFieldType.Choice,
            Options = [new CyOption<string>("a", "Active")]
        }
    ];

    private static CyRuleCondition Cond(string? field, string? op, object? v = null, object? v2 = null) =>
        new() { Id = "x", FieldKey = field, OperatorKey = op, Value = v, Value2 = v2 };

    private static CyRuleIssueCode? Check(CyRuleCondition c) => CyRuleValidator.ValidateCondition(c, Fields);

    [Fact]
    public void A_Blank_Condition_Needs_A_Field_First()
    {
        Check(Cond(null, null)).ShouldBe(CyRuleIssueCode.NoField);
        Check(Cond(" ", "eq")).ShouldBe(CyRuleIssueCode.NoField);
    }

    [Fact]
    public void An_Unknown_Field_Is_Reported()
    {
        Check(Cond("gone", "eq", "x")).ShouldBe(CyRuleIssueCode.UnknownField);
    }

    [Fact]
    public void A_Field_Without_An_Operator_Needs_One()
    {
        Check(Cond("age", null)).ShouldBe(CyRuleIssueCode.NoOperator);
    }

    [Fact]
    public void An_Operator_The_Field_Does_Not_Offer_Is_Reported()
    {
        Check(Cond("name", "gt", "x")).ShouldBe(CyRuleIssueCode.UnknownOperator);
    }

    [Fact]
    public void A_One_Value_Operator_Needs_A_Value()
    {
        Check(Cond("name", "eq")).ShouldBe(CyRuleIssueCode.MissingValue);
        Check(Cond("name", "eq", "  ")).ShouldBe(CyRuleIssueCode.MissingValue);
        Check(Cond("name", "eq", "Ann")).ShouldBeNull();
    }

    [Fact]
    public void Number_Values_Must_Be_Numbers()
    {
        Check(Cond("age", "gt", "abc")).ShouldBe(CyRuleIssueCode.MissingValue);
        Check(Cond("age", "gt", 18m)).ShouldBeNull();
        Check(Cond("age", "gt", 18)).ShouldBeNull();
        Check(Cond("age", "gt", "18")).ShouldBeNull();
    }

    [Fact]
    public void Date_Values_Must_Be_Dates()
    {
        Check(Cond("dob", "before", "not a date")).ShouldBe(CyRuleIssueCode.MissingValue);
        Check(Cond("dob", "before", new DateOnly(2000, 1, 1))).ShouldBeNull();
    }

    [Fact]
    public void Operators_With_No_Value_Are_Complete_Without_One()
    {
        Check(Cond("name", "empty")).ShouldBeNull();
        Check(Cond("smoker", "true")).ShouldBeNull();
    }

    [Fact]
    public void Between_Needs_Both_Values_In_Order()
    {
        Check(Cond("age", "between", 1m)).ShouldBe(CyRuleIssueCode.MissingValue);
        Check(Cond("age", "between", 10m, 5m)).ShouldBe(CyRuleIssueCode.InvalidRange);
        Check(Cond("age", "between", 5m, 5m)).ShouldBeNull();
        Check(Cond("age", "between", 5m, 10m)).ShouldBeNull();
        Check(Cond("dob", "between", new DateOnly(2020, 1, 2), new DateOnly(2020, 1, 1)))
            .ShouldBe(CyRuleIssueCode.InvalidRange);
    }

    [Fact]
    public void Validate_Lists_One_Issue_Per_Incomplete_Condition_In_Document_Order_Including_Nested()
    {
        var root = new CyRuleGroup
        {
            Id = "root",
            Children =
            [
                new CyRuleCondition { Id = "ok", FieldKey = "name", OperatorKey = "empty" },
                new CyRuleGroup
                {
                    Id = "g",
                    Children = [new CyRuleCondition { Id = "bad1" }]
                },
                new CyRuleCondition { Id = "bad2", FieldKey = "age", OperatorKey = "gt" }
            ]
        };

        var issues = CyRuleValidator.Validate(root, Fields);

        string.Join(",", issues.Select(i => $"{i.NodeId}:{i.Code}")).ShouldBe("bad1:NoField,bad2:MissingValue");
    }

    [Fact]
    public void Validate_Returns_Nothing_For_A_Complete_Rule()
    {
        var root = new CyRuleGroup
        {
            Children = [new CyRuleCondition { FieldKey = "name", OperatorKey = "empty" }]
        };

        CyRuleValidator.Validate(root, Fields).ShouldBeEmpty();
    }
}
