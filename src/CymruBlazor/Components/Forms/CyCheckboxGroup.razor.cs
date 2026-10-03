using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A group of checkboxes bound to a <em>set</em> of values, rendered as a native
/// <c>&lt;fieldset&gt;</c>/<c>&lt;legend&gt;</c>. Use it for "select all that
/// apply" questions; a single yes/no question is a <see cref="CyCheckbox"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Binding.</b> The bound value is an <c>IEnumerable&lt;TValue&gt;</c>
/// (<c>@bind-Value</c>). Each change produces a new collection, in the order of
/// <see cref="Items"/> (values the group does not offer are kept, after the
/// offered ones), so the property you bind must be assignable from
/// <c>IEnumerable&lt;TValue&gt;</c>. To bind a <c>List&lt;T&gt;</c> or
/// <c>HashSet&lt;T&gt;</c> property, pass <c>Value</c> and <c>ValueChanged</c>
/// separately and convert in the handler.
/// </para>
/// <para>
/// <b>Validation.</b> "Required" here is a visual marker only, as on every
/// CymruBlazor field; enforce "at least one" with your own validation rule. No
/// <c>aria-required</c> is emitted because the <c>group</c> role does not
/// support it.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of each selectable value.</typeparam>
public partial class CyCheckboxGroup<TValue> : CyFormFieldComponentBase<IEnumerable<TValue>>
{
    /// <summary>The checkboxes to offer, one per <see cref="CyOption{TValue}"/>.</summary>
    [Parameter]
    public IEnumerable<CyOption<TValue>>? Items { get; set; }

    /// <summary>
    /// How selected values are matched against the options. Defaults to
    /// <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    [Parameter]
    public IEqualityComparer<TValue>? Comparer { get; set; }

    private IEqualityComparer<TValue> EffectiveComparer => Comparer ?? EqualityComparer<TValue>.Default;

    private string ItemId(int index) => $"{FieldId}-{index}";

    private bool IsSelected(TValue value)
    {
        var current = CurrentValue;

        if (current is null)
        {
            return false;
        }

        var comparer = EffectiveComparer;
        return current.Any(selected => comparer.Equals(selected, value));
    }

    private Task ToggleAsync(TValue value, bool isChecked)
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        var comparer = EffectiveComparer;
        var current = (CurrentValue ?? []).ToList();
        var present = current.Any(selected => comparer.Equals(selected, value));

        if (isChecked && !present)
        {
            current.Add(value);
        }
        else if (!isChecked && present)
        {
            current.RemoveAll(selected => comparer.Equals(selected, value));
        }
        else
        {
            return Task.CompletedTask;
        }

        var offered = (Items ?? []).Select(option => option.Value).ToList();

        // Offered values in Items order, then anything selected that the group
        // does not offer (so a round-trip never silently drops model data).
        var ordered = offered
            .Where(candidate => current.Any(selected => comparer.Equals(selected, candidate)))
            .Concat(current.Where(selected => !offered.Any(candidate => comparer.Equals(selected, candidate))))
            .ToList();

        CurrentValue = ordered;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Unreachable: the group binds to its collection directly and never
    /// parses a single string, mirroring <see cref="CyCheckbox"/>.
    /// </summary>
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out IEnumerable<TValue> result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotSupportedException(
            $"{nameof(CyCheckboxGroup<int>)} binds directly to its collection and does not parse a string representation.");
    }
}
