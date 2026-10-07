using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyRuleOperatorsTests
{
    [Theory]
    [InlineData(CyRuleFieldType.Text, "eq,ne,contains,startswith,endswith,empty,notempty")]
    [InlineData(CyRuleFieldType.Number, "eq,ne,gt,gte,lt,lte,between,empty,notempty")]
    [InlineData(CyRuleFieldType.Date, "eq,ne,before,after,between,empty,notempty")]
    [InlineData(CyRuleFieldType.Boolean, "true,false")]
    [InlineData(CyRuleFieldType.Choice, "eq,ne,empty,notempty")]
    [InlineData(CyRuleFieldType.Coded, "eq,ne,empty,notempty")]
    public void Each_Type_Offers_Its_Default_Operators_In_Order(CyRuleFieldType type, string expected)
    {
        string.Join(",", CyRuleOperators.For(type)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("eq", 1)]
    [InlineData("contains", 1)]
    [InlineData("gt", 1)]
    [InlineData("before", 1)]
    [InlineData("between", 2)]
    [InlineData("empty", 0)]
    [InlineData("notempty", 0)]
    [InlineData("true", 0)]
    [InlineData("false", 0)]
    public void Arity_Is_The_Number_Of_Values(string key, int arity)
    {
        CyRuleOperators.Arity(key).ShouldBe(arity);
    }

    [Fact]
    public void Arity_Of_Null_Is_Zero()
    {
        CyRuleOperators.Arity(null).ShouldBe(0);
    }

    [Fact]
    public void Resolve_Returns_The_Defaults_When_The_Field_Has_No_Subset()
    {
        var field = new CyRuleField { Key = "age", Label = "Age", Type = CyRuleFieldType.Number };

        CyRuleOperators.Resolve(field).Count.ShouldBe(9);
    }

    [Fact]
    public void Resolve_Returns_A_Valid_Subset_As_Given()
    {
        var field = new CyRuleField
        {
            Key = "age",
            Label = "Age",
            Type = CyRuleFieldType.Number,
            Operators = [CyRuleOperators.Gt, CyRuleOperators.Between]
        };

        string.Join(",", CyRuleOperators.Resolve(field)).ShouldBe("gt,between");
    }

    [Fact]
    public void Resolve_Throws_For_An_Operator_The_Type_Does_Not_Support()
    {
        var field = new CyRuleField
        {
            Key = "name",
            Label = "Name",
            Type = CyRuleFieldType.Text,
            Operators = [CyRuleOperators.Gt]
        };

        var ex = Should.Throw<InvalidOperationException>(() => CyRuleOperators.Resolve(field));
        ex.Message.ShouldContain("name");
        ex.Message.ShouldContain("gt");
    }

    [Fact]
    public void Label_Uses_English_Defaults_And_Custom_Text()
    {
        CyRuleOperators.Label(CyRuleOperators.Gt).ShouldBe("is greater than");
        CyRuleOperators.Label(CyRuleOperators.IsTrue).ShouldBe("is yes");
        CyRuleOperators.Label(CyRuleOperators.Eq, new CyRuleBuilderText { OpEq = "yw" }).ShouldBe("yw");
        CyRuleOperators.Label("custom-key").ShouldBe("custom-key");
    }
}
