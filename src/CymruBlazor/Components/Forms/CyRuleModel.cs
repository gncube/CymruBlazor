namespace CymruBlazor.Components.Forms;

/// <summary>How the children of a <see cref="CyRuleGroup"/> combine.</summary>
public enum CyRuleCombinator
{
    /// <summary>Every child must match.</summary>
    And,

    /// <summary>At least one child must match.</summary>
    Or
}

/// <summary>
/// A node in a rule: either a <see cref="CyRuleGroup"/> or a <see cref="CyRuleCondition"/>. Nodes are immutable
/// records: an edit produces a new tree, and a node keeps its <see cref="Id"/> across <c>with</c> edits so the
/// editor can keep focus and state on it.
/// </summary>
public abstract record CyRuleNode
{
    /// <summary>A stable identifier, unique within the tree. Generated when the node is created.</summary>
    public string Id { get; init; } = NewId();

    internal static string NewId() => "r" + Guid.NewGuid().ToString("N");
}

/// <summary>
/// A set of conditions and nested groups joined by <see cref="Combinator"/>, optionally negated.
/// </summary>
/// <remarks>
/// The library edits this model and describes it in words; it does not evaluate it, and it does not generate
/// SQL, LINQ or any other query. Translate the model on your server.
/// </remarks>
public sealed record CyRuleGroup : CyRuleNode
{
    /// <summary>How the children combine. Defaults to <see cref="CyRuleCombinator.And"/>.</summary>
    public CyRuleCombinator Combinator { get; init; } = CyRuleCombinator.And;

    /// <summary>Negates the whole group ("not all of", "none of").</summary>
    public bool Not { get; init; }

    /// <summary>The conditions and nested groups, in order.</summary>
    public IReadOnlyList<CyRuleNode> Children { get; init; } = [];
}

/// <summary>
/// One test: a field, an operator and (depending on the operator) one or two values.
/// </summary>
/// <remarks>
/// Value types by <see cref="CyRuleFieldType"/>: <c>Text</c>, <c>Choice</c> and <c>Coded</c> use
/// <see cref="string"/>; <c>Number</c> uses <see cref="decimal"/>; <c>Date</c> uses <see cref="DateOnly"/>;
/// <c>Boolean</c> needs no value (the operator says yes or no). <see cref="Value2"/> is used only by "between".
/// </remarks>
public sealed record CyRuleCondition : CyRuleNode
{
    /// <summary><see cref="CyRuleField.Key"/> of the chosen field, or <see langword="null"/> before one is chosen.</summary>
    public string? FieldKey { get; init; }

    /// <summary>Key of the chosen operator (see <see cref="CyRuleOperators"/>), or <see langword="null"/>.</summary>
    public string? OperatorKey { get; init; }

    /// <summary>The first (or only) value.</summary>
    public object? Value { get; init; }

    /// <summary>The second value of a "between" condition.</summary>
    public object? Value2 { get; init; }

    /// <summary>Display text of <see cref="Value"/> for a coded field whose option is not in a fixed list.</summary>
    public string? ValueLabel { get; init; }

    /// <summary>Display text of <see cref="Value2"/> for a coded field.</summary>
    public string? Value2Label { get; init; }
}
