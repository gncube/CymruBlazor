namespace CymruBlazor.Components.Forms;

/// <summary>Why a condition is not complete.</summary>
public enum CyRuleIssueCode
{
    /// <summary>No field chosen.</summary>
    NoField,

    /// <summary>The chosen field is not in the field list.</summary>
    UnknownField,

    /// <summary>No operator chosen.</summary>
    NoOperator,

    /// <summary>The chosen operator is not offered for the field.</summary>
    UnknownOperator,

    /// <summary>A required value is missing or not of the right kind.</summary>
    MissingValue,

    /// <summary>The first value of "between" is greater than the second.</summary>
    InvalidRange
}

/// <summary>A problem with one condition.</summary>
/// <param name="NodeId">The condition's <see cref="CyRuleNode.Id"/>.</param>
/// <param name="Code">What is wrong.</param>
public sealed record CyRuleIssue(string NodeId, CyRuleIssueCode Code);

/// <summary>Checks that every condition of a rule is complete.</summary>
public static class CyRuleValidator
{
    /// <summary>One issue (the first found) for each incomplete condition anywhere in the tree, in document order.</summary>
    public static IReadOnlyList<CyRuleIssue> Validate(CyRuleGroup root, IReadOnlyList<CyRuleField> fields)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(fields);

        var issues = new List<CyRuleIssue>();
        Collect(root, fields, issues);
        return issues;
    }

    /// <summary>The first problem with a condition, or <see langword="null"/> when it is complete.</summary>
    public static CyRuleIssueCode? ValidateCondition(CyRuleCondition condition, IReadOnlyList<CyRuleField> fields)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(fields);

        if (string.IsNullOrWhiteSpace(condition.FieldKey))
        {
            return CyRuleIssueCode.NoField;
        }

        var field = fields.FirstOrDefault(f => string.Equals(f.Key, condition.FieldKey, StringComparison.Ordinal));
        if (field is null)
        {
            return CyRuleIssueCode.UnknownField;
        }

        if (string.IsNullOrWhiteSpace(condition.OperatorKey))
        {
            return CyRuleIssueCode.NoOperator;
        }

        if (!CyRuleOperators.Resolve(field).Contains(condition.OperatorKey, StringComparer.Ordinal))
        {
            return CyRuleIssueCode.UnknownOperator;
        }

        var arity = CyRuleOperators.Arity(condition.OperatorKey);
        if (arity >= 1 && !HasValue(field.Type, condition.Value))
        {
            return CyRuleIssueCode.MissingValue;
        }

        if (arity == 2)
        {
            if (!HasValue(field.Type, condition.Value2))
            {
                return CyRuleIssueCode.MissingValue;
            }

            if (IsReversed(field.Type, condition.Value, condition.Value2))
            {
                return CyRuleIssueCode.InvalidRange;
            }
        }

        return null;
    }

    private static void Collect(CyRuleGroup group, IReadOnlyList<CyRuleField> fields, List<CyRuleIssue> issues)
    {
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case CyRuleGroup nested:
                    Collect(nested, fields, issues);
                    break;
                case CyRuleCondition condition:
                    if (ValidateCondition(condition, fields) is { } code)
                    {
                        issues.Add(new CyRuleIssue(condition.Id, code));
                    }

                    break;
            }
        }
    }

    private static bool HasValue(CyRuleFieldType type, object? value) => type switch
    {
        CyRuleFieldType.Number => CyRuleValues.TryNumber(value, out _),
        CyRuleFieldType.Date => CyRuleValues.TryDate(value, out _),
        _ => !CyRuleValues.IsBlank(value)
    };

    private static bool IsReversed(CyRuleFieldType type, object? first, object? second) => type switch
    {
        CyRuleFieldType.Number =>
            CyRuleValues.TryNumber(first, out var a) && CyRuleValues.TryNumber(second, out var b) && a > b,
        CyRuleFieldType.Date =>
            CyRuleValues.TryDate(first, out var d1) && CyRuleValues.TryDate(second, out var d2) && d1 > d2,
        _ => false
    };
}
