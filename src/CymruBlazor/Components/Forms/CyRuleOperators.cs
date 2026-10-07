namespace CymruBlazor.Components.Forms;

/// <summary>
/// The operator catalogue: the keys stored in <see cref="CyRuleCondition.OperatorKey"/>, which operators each
/// <see cref="CyRuleFieldType"/> offers, and how many values each takes.
/// </summary>
public static class CyRuleOperators
{
    /// <summary>Equal to. One value.</summary>
    public const string Eq = "eq";

    /// <summary>Not equal to. One value.</summary>
    public const string Ne = "ne";

    /// <summary>Text contains. One value.</summary>
    public const string Contains = "contains";

    /// <summary>Text starts with. One value.</summary>
    public const string StartsWith = "startswith";

    /// <summary>Text ends with. One value.</summary>
    public const string EndsWith = "endswith";

    /// <summary>Greater than. One value.</summary>
    public const string Gt = "gt";

    /// <summary>Greater than or equal to. One value.</summary>
    public const string Gte = "gte";

    /// <summary>Less than. One value.</summary>
    public const string Lt = "lt";

    /// <summary>Less than or equal to. One value.</summary>
    public const string Lte = "lte";

    /// <summary>Date is before. One value.</summary>
    public const string Before = "before";

    /// <summary>Date is after. One value.</summary>
    public const string After = "after";

    /// <summary>Between two values, inclusive. Two values.</summary>
    public const string Between = "between";

    /// <summary>Has no value. No value.</summary>
    public const string IsEmpty = "empty";

    /// <summary>Has a value. No value.</summary>
    public const string IsNotEmpty = "notempty";

    /// <summary>Boolean is yes. No value.</summary>
    public const string IsTrue = "true";

    /// <summary>Boolean is no. No value.</summary>
    public const string IsFalse = "false";

    private static readonly string[] TextOperators =
        [Eq, Ne, Contains, StartsWith, EndsWith, IsEmpty, IsNotEmpty];

    private static readonly string[] NumberOperators =
        [Eq, Ne, Gt, Gte, Lt, Lte, Between, IsEmpty, IsNotEmpty];

    private static readonly string[] DateOperators =
        [Eq, Ne, Before, After, Between, IsEmpty, IsNotEmpty];

    private static readonly string[] BooleanOperators =
        [IsTrue, IsFalse];

    private static readonly string[] ChoiceOperators =
        [Eq, Ne, IsEmpty, IsNotEmpty];

    /// <summary>The default operator keys for a field type, in display order.</summary>
    public static IReadOnlyList<string> For(CyRuleFieldType type) => type switch
    {
        CyRuleFieldType.Text => TextOperators,
        CyRuleFieldType.Number => NumberOperators,
        CyRuleFieldType.Date => DateOperators,
        CyRuleFieldType.Boolean => BooleanOperators,
        CyRuleFieldType.Choice => ChoiceOperators,
        CyRuleFieldType.Coded => ChoiceOperators,
        _ => TextOperators
    };

    /// <summary>
    /// The operator keys a field offers: its <see cref="CyRuleField.Operators"/> subset, or the type's defaults.
    /// </summary>
    /// <exception cref="InvalidOperationException">A key in the subset is not valid for the field's type.</exception>
    public static IReadOnlyList<string> Resolve(CyRuleField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var defaults = For(field.Type);
        if (field.Operators is null)
        {
            return defaults;
        }

        foreach (var key in field.Operators)
        {
            if (!defaults.Contains(key, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Field '{field.Key}' ({field.Type}) does not support the operator '{key}'. " +
                    $"Valid operators: {string.Join(", ", defaults)}.");
            }
        }

        return field.Operators;
    }

    /// <summary>How many values the operator takes: 0, 1 or 2.</summary>
    public static int Arity(string? operatorKey) => operatorKey switch
    {
        Between => 2,
        IsEmpty or IsNotEmpty or IsTrue or IsFalse => 0,
        null => 0,
        _ => 1
    };

    /// <summary>The visible label of an operator, from <paramref name="text"/> (English defaults when null).</summary>
    public static string Label(string operatorKey, CyRuleBuilderText? text = null)
    {
        var t = text ?? new CyRuleBuilderText();

        return operatorKey switch
        {
            Eq => t.OpEq,
            Ne => t.OpNe,
            Contains => t.OpContains,
            StartsWith => t.OpStartsWith,
            EndsWith => t.OpEndsWith,
            Gt => t.OpGt,
            Gte => t.OpGte,
            Lt => t.OpLt,
            Lte => t.OpLte,
            Before => t.OpBefore,
            After => t.OpAfter,
            Between => t.OpBetween,
            IsEmpty => t.OpIsEmpty,
            IsNotEmpty => t.OpIsNotEmpty,
            IsTrue => t.OpIsTrue,
            IsFalse => t.OpIsFalse,
            _ => operatorKey
        };
    }
}
