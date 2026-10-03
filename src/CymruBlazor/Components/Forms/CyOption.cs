namespace CymruBlazor.Components.Forms;

/// <summary>
/// One choice offered by a <see cref="CySelect{TValue}"/>,
/// <see cref="CyRadioGroup{TValue}"/> or <see cref="CyCheckboxGroup{TValue}"/>
/// through its <c>Items</c> parameter.
/// </summary>
/// <remarks>
/// A single shape (rather than <c>Items</c> plus per-component selector
/// delegates) keeps the three components' generic signatures exactly as they
/// were - adding a second type parameter to the existing
/// <c>CySelect&lt;TValue&gt;</c>/<c>CyRadioGroup&lt;TValue&gt;</c> would have been a
/// breaking change. Build the list from your own types with
/// <see cref="CyOptions.ToCyOptions{TItem, TValue}"/>.
/// </remarks>
/// <typeparam name="TValue">The type of the value the choice stands for.</typeparam>
/// <param name="Value">The value bound when this choice is selected.</param>
/// <param name="Text">The visible, programmatically associated label.</param>
public sealed record CyOption<TValue>(TValue Value, string Text)
{
    /// <summary>
    /// Optional supporting text for this choice. Rendered by
    /// <see cref="CyRadioGroup{TValue}"/> and <see cref="CyCheckboxGroup{TValue}"/>;
    /// a native <c>&lt;option&gt;</c> cannot carry one, so
    /// <see cref="CySelect{TValue}"/> ignores it.
    /// </summary>
    public string? Hint { get; init; }

    /// <summary>Whether this choice cannot be selected.</summary>
    public bool Disabled { get; init; }
}

/// <summary>
/// Helpers for building <see cref="CyOption{TValue}"/> lists.
/// </summary>
public static class CyOptions
{
    /// <summary>
    /// Projects any sequence into <see cref="CyOption{TValue}"/>s, e.g.
    /// <c>countries.ToCyOptions(c =&gt; c.Code, c =&gt; c.Name)</c>.
    /// </summary>
    /// <param name="items">The source sequence.</param>
    /// <param name="value">Selects the bound value.</param>
    /// <param name="text">Selects the visible label.</param>
    /// <param name="hint">Optionally selects per-choice supporting text.</param>
    /// <param name="disabled">Optionally selects whether the choice is disabled.</param>
    public static IReadOnlyList<CyOption<TValue>> ToCyOptions<TItem, TValue>(
        this IEnumerable<TItem> items,
        Func<TItem, TValue> value,
        Func<TItem, string> text,
        Func<TItem, string?>? hint = null,
        Func<TItem, bool>? disabled = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(text);

        return items
            .Select(item => new CyOption<TValue>(value(item), text(item))
            {
                Hint = hint?.Invoke(item),
                Disabled = disabled?.Invoke(item) ?? false
            })
            .ToList();
    }

    /// <summary>
    /// One <see cref="CyOption{TValue}"/> per member of an enum, in
    /// declaration order. The label defaults to the member's name; pass
    /// <paramref name="text"/> to supply display (or localised) text.
    /// </summary>
    public static IReadOnlyList<CyOption<TEnum>> FromEnum<TEnum>(Func<TEnum, string>? text = null)
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(member => new CyOption<TEnum>(member, text?.Invoke(member) ?? member.ToString()))
            .ToList();
}
