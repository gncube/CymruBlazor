namespace CymruBlazor.Components.Forms;

/// <summary>The kind of value a <see cref="CyRuleField"/> holds. It decides the operators and the value editor.</summary>
public enum CyRuleFieldType
{
    /// <summary>Free text (<see cref="string"/>).</summary>
    Text,

    /// <summary>A number (<see cref="decimal"/>).</summary>
    Number,

    /// <summary>A calendar date (<see cref="DateOnly"/>).</summary>
    Date,

    /// <summary>Yes or no. The operator carries the answer, so there is no value.</summary>
    Boolean,

    /// <summary>One of a fixed list (<see cref="CyRuleField.Options"/>).</summary>
    Choice,

    /// <summary>One term searched from a large coded list (<see cref="CyRuleField.ItemsProvider"/>).</summary>
    Coded
}

/// <summary>A field a rule can test.</summary>
public sealed record CyRuleField
{
    /// <summary>A stable key stored in <see cref="CyRuleCondition.FieldKey"/>. Must be unique in the field list.</summary>
    public required string Key { get; init; }

    /// <summary>The visible name of the field.</summary>
    public required string Label { get; init; }

    /// <summary>The kind of value. Defaults to <see cref="CyRuleFieldType.Text"/>.</summary>
    public CyRuleFieldType Type { get; init; } = CyRuleFieldType.Text;

    /// <summary>
    /// Restricts the operators to this subset of the type's defaults (keys from <see cref="CyRuleOperators"/>).
    /// <see langword="null"/> offers all of them. A key that is not valid for the type throws when the field is used.
    /// </summary>
    public IReadOnlyList<string>? Operators { get; init; }

    /// <summary>The choices of a <see cref="CyRuleFieldType.Choice"/> field.</summary>
    public IReadOnlyList<CyOption<string>>? Options { get; init; }

    /// <summary>The async search behind a <see cref="CyRuleFieldType.Coded"/> field (the same shape as <c>CyCombobox</c>).</summary>
    public Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<string>>>>? ItemsProvider { get; init; }

    /// <summary>Lower bound of a <see cref="CyRuleFieldType.Number"/> field.</summary>
    public decimal? Min { get; init; }

    /// <summary>Upper bound of a <see cref="CyRuleFieldType.Number"/> field.</summary>
    public decimal? Max { get; init; }

    /// <summary>Step of a <see cref="CyRuleFieldType.Number"/> field.</summary>
    public decimal? Step { get; init; }

    /// <summary>Unit shown after a <see cref="CyRuleFieldType.Number"/> value ("years", "mmol/L").</summary>
    public string? Unit { get; init; }
}
