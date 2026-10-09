using System.Globalization;

namespace CymruBlazor.Components.Forms;

/// <summary>Describes a rule in plain language ("Age is greater than 18 and (Status is Active or Status is Pending)").</summary>
public static class CyRuleSummary
{
    /// <summary>The whole rule as one sentence, using the phrases in <paramref name="text"/> (English defaults when null).</summary>
    /// <param name="root">The rule.</param>
    /// <param name="fields">The fields, to turn keys into labels.</param>
    /// <param name="text">Phrases; <see langword="null"/> uses English.</param>
    /// <param name="culture">Formats numbers and dates; <see langword="null"/> uses the current culture.</param>
    public static string Describe(
        CyRuleGroup root,
        IReadOnlyList<CyRuleField> fields,
        CyRuleBuilderText? text = null,
        IFormatProvider? culture = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(fields);

        var t = text ?? new CyRuleBuilderText();
        var provider = culture ?? CultureInfo.CurrentCulture;

        if (CyRuleEdits.CountConditions(root) == 0)
        {
            return t.SummaryEmpty;
        }

        return DescribeGroup(root, fields, t, provider, isRoot: true);
    }

    /// <summary>The legend of a group: "All of the following", "Any of the following", and their negations.</summary>
    public static string GroupLegend(CyRuleGroup group, CyRuleBuilderText? text = null)
    {
        ArgumentNullException.ThrowIfNull(group);

        var t = text ?? new CyRuleBuilderText();

        return (group.Combinator, group.Not) switch
        {
            (CyRuleCombinator.And, false) => t.GroupAll,
            (CyRuleCombinator.Or, false) => t.GroupAny,
            (CyRuleCombinator.And, true) => t.GroupNotAll,
            _ => t.GroupNone
        };
    }

    /// <summary>One condition in words, or the "incomplete" phrase when it is not complete.</summary>
    public static string DescribeCondition(
        CyRuleCondition condition,
        IReadOnlyList<CyRuleField> fields,
        CyRuleBuilderText? text = null,
        IFormatProvider? culture = null)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(fields);

        return DescribeConditionCore(
            condition, fields, text ?? new CyRuleBuilderText(), culture ?? CultureInfo.CurrentCulture);
    }

    private static string DescribeGroup(
        CyRuleGroup group,
        IReadOnlyList<CyRuleField> fields,
        CyRuleBuilderText t,
        IFormatProvider provider,
        bool isRoot)
    {
        var parts = new List<string>();
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case CyRuleGroup nested when CyRuleEdits.CountConditions(nested) > 0:
                    parts.Add(DescribeGroup(nested, fields, t, provider, isRoot: false));
                    break;
                case CyRuleCondition condition:
                    parts.Add(DescribeConditionCore(condition, fields, t, provider));
                    break;
            }
        }

        if (parts.Count == 0)
        {
            return t.SummaryEmptyGroup;
        }

        var joiner = group.Combinator == CyRuleCombinator.And ? t.JoinAnd : t.JoinOr;
        var body = string.Join(joiner, parts);

        if (group.Not)
        {
            var inner = parts.Count > 1 ? string.Format(provider, t.GroupParens, body) : body;
            return string.Format(provider, t.NotFormat, inner);
        }

        return !isRoot && parts.Count > 1 ? string.Format(provider, t.GroupParens, body) : body;
    }

    private static string DescribeConditionCore(
        CyRuleCondition condition,
        IReadOnlyList<CyRuleField> fields,
        CyRuleBuilderText t,
        IFormatProvider provider)
    {
        if (CyRuleValidator.ValidateCondition(condition, fields) is not null)
        {
            return t.SummaryIncomplete;
        }

        var field = fields.First(f => string.Equals(f.Key, condition.FieldKey, StringComparison.Ordinal));
        var op = CyRuleOperators.Label(condition.OperatorKey!, t);

        return CyRuleOperators.Arity(condition.OperatorKey) switch
        {
            0 => string.Format(provider, t.Condition0, field.Label, op),
            1 => string.Format(provider, t.Condition1, field.Label, op,
                FormatValue(field, condition.Value, condition.ValueLabel, provider)),
            _ => string.Format(provider, t.Condition2, field.Label, op,
                FormatValue(field, condition.Value, condition.ValueLabel, provider),
                FormatValue(field, condition.Value2, condition.Value2Label, provider))
        };
    }

    private static string FormatValue(CyRuleField field, object? value, string? label, IFormatProvider provider)
    {
        switch (field.Type)
        {
            case CyRuleFieldType.Number when CyRuleValues.TryNumber(value, out var number):
                var text = number.ToString(provider);
                return string.IsNullOrWhiteSpace(field.Unit) ? text : $"{text} {field.Unit}";

            case CyRuleFieldType.Date when CyRuleValues.TryDate(value, out var date):
                return date.ToString("d", provider);

            case CyRuleFieldType.Choice:
                var raw = Convert.ToString(value, provider) ?? string.Empty;
                return field.Options?.FirstOrDefault(o => string.Equals(o.Value, raw, StringComparison.Ordinal))?.Text ?? raw;

            case CyRuleFieldType.Coded:
                return !string.IsNullOrWhiteSpace(label) ? label : Convert.ToString(value, provider) ?? string.Empty;

            default:
                return Convert.ToString(value, provider) ?? string.Empty;
        }
    }
}
