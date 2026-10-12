namespace CymruBlazor.Components.Forms;

/// <summary>
/// What a custom value editor (<see cref="CyRuleField.ValueEditor"/>) is given for one condition: the current
/// values, the ids and names to put on its controls, and the methods that store a new value.
/// </summary>
/// <remarks>
/// <para>
/// The editor draws every value the operator needs: <see cref="Arity"/> is 1 for most operators and 2 for
/// "between" (then <see cref="Value2"/> is the upper value). It is never shown for operators that take no value.
/// </para>
/// <para>
/// Put <see cref="ValueId"/> on the first control and <see cref="Value2Id"/> on the second so the builder's
/// messages and focus handling line up, and give each control a visible label or an <c>aria-label</c>
/// (<see cref="Label"/> and <see cref="Label2"/> are the localised defaults). Store values with
/// <see cref="SetValueAsync"/> and <see cref="SetValue2Async"/>, using the type the field's
/// <see cref="CyRuleField.Type"/> expects (see <see cref="CyRuleCondition"/>). Client-side checks are advisory.
/// </para>
/// <para>
/// If the editor throws while rendering, the builder shows a fixed message in its place and logs the exception;
/// the exception text is never shown to the user.
/// </para>
/// </remarks>
public sealed class CyRuleEditorContext
{
    private readonly Func<object?, string?, Task> _setValue;
    private readonly Func<object?, string?, Task> _setValue2;

    internal CyRuleEditorContext(
        CyRuleField field,
        string operatorKey,
        int arity,
        object? value,
        object? value2,
        string? valueLabel,
        string? value2Label,
        string valueId,
        string value2Id,
        string label,
        string label2,
        string? issueId,
        bool disabled,
        Func<object?, string?, Task> setValue,
        Func<object?, string?, Task> setValue2)
    {
        Field = field;
        OperatorKey = operatorKey;
        Arity = arity;
        Value = value;
        Value2 = value2;
        ValueLabel = valueLabel;
        Value2Label = value2Label;
        ValueId = valueId;
        Value2Id = value2Id;
        Label = label;
        Label2 = label2;
        IssueId = issueId;
        Disabled = disabled;
        _setValue = setValue;
        _setValue2 = setValue2;
    }

    /// <summary>The field being tested.</summary>
    public CyRuleField Field { get; }

    /// <summary>The chosen operator (see <see cref="CyRuleOperators"/>).</summary>
    public string OperatorKey { get; }

    /// <summary>How many values the operator needs here: 1, or 2 for "between".</summary>
    public int Arity { get; }

    /// <summary>The first (or only) value, or <see langword="null"/> when none has been entered.</summary>
    public object? Value { get; }

    /// <summary>The second value of a "between" condition.</summary>
    public object? Value2 { get; }

    /// <summary>The display text stored with <see cref="Value"/>, if any.</summary>
    public string? ValueLabel { get; }

    /// <summary>The display text stored with <see cref="Value2"/>, if any.</summary>
    public string? Value2Label { get; }

    /// <summary>The id to put on the control for <see cref="Value"/>.</summary>
    public string ValueId { get; }

    /// <summary>The id to put on the control for <see cref="Value2"/>.</summary>
    public string Value2Id { get; }

    /// <summary>The localised name for the first value ("Value").</summary>
    public string Label { get; }

    /// <summary>The localised name for the second value ("Upper value").</summary>
    public string Label2 { get; }

    /// <summary>
    /// The id of the condition's message when it is incomplete and messages are showing, otherwise
    /// <see langword="null"/>. The builder already links its own wrapper to it.
    /// </summary>
    public string? IssueId { get; }

    /// <summary>Whether the condition is incomplete and its message is showing (<see cref="IssueId"/> is set).</summary>
    public bool Invalid => IssueId is not null;

    /// <summary>Whether the builder is disabled. Disable your controls too.</summary>
    public bool Disabled { get; }

    /// <summary>
    /// Stores the first value. Pass <see langword="null"/> or blank text to clear it. <paramref name="label"/> is the
    /// text the plain-language summary shows for it, when that should differ from the value itself.
    /// </summary>
    public Task SetValueAsync(object? value, string? label = null) => _setValue(value, label);

    /// <summary>Stores the second value of a "between" condition. See <see cref="SetValueAsync"/>.</summary>
    public Task SetValue2Async(object? value, string? label = null) => _setValue2(value, label);
}
