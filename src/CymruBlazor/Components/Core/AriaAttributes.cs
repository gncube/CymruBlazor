using System.Collections.Frozen;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Represents a collection of ARIA attributes.
/// </summary>
public sealed class AriaAttributes
{
    private readonly FrozenDictionary<string, object> _attributes;

    private AriaAttributes(FrozenDictionary<string, object> attributes)
    {
        _attributes = attributes;
    }

    /// <summary>
    /// Gets an empty collection.
    /// </summary>
    public static AriaAttributes Empty { get; } =
        new(FrozenDictionary<string, object>.Empty);

    /// <summary>
    /// Returns the attributes as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object> AsDictionary() => _attributes;

    public AriaAttributes AddLabel(string? value) =>
        Add("aria-label", value);

    public AriaAttributes AddLabelledBy(string? value) =>
        Add("aria-labelledby", value);

    public AriaAttributes AddDescription(string? value) =>
        Add("aria-description", value);

    public AriaAttributes AddDescribedBy(string? value) =>
        Add("aria-describedby", value);

    public AriaAttributes AddDisabled(bool disabled) =>
        disabled
            ? Add("aria-disabled", "true")
            : this;

    public AriaAttributes AddExpanded(bool? expanded)
    {
        if (!expanded.HasValue)
        {
            return this;
        }

        return Add(
            "aria-expanded",
            expanded.Value ? "true" : "false");
    }

    public AriaAttributes AddSelected(bool? selected)
    {
        if (!selected.HasValue)
        {
            return this;
        }

        return Add(
            "aria-selected",
            selected.Value ? "true" : "false");
    }

    public AriaAttributes AddChecked(bool? value)
    {
        if (!value.HasValue)
        {
            return this;
        }

        return Add(
            "aria-checked",
            value.Value ? "true" : "false");
    }

    public AriaAttributes AddCurrent(string? value) =>
        Add("aria-current", value);

    public AriaAttributes AddControls(string? id) =>
        Add("aria-controls", id);

    public AriaAttributes AddOwns(string? id) =>
        Add("aria-owns", id);

    public AriaAttributes AddHasPopup(string? popupType) =>
        Add("aria-haspopup", popupType);

    public AriaAttributes AddHidden(bool hidden) =>
        hidden
            ? Add("aria-hidden", "true")
            : this;

    public AriaAttributes AddInvalid(bool invalid) =>
        invalid
            ? Add("aria-invalid", "true")
            : this;

    public AriaAttributes AddRequired(bool required) =>
        required
            ? Add("aria-required", "true")
            : this;

    public AriaAttributes AddBusy(bool busy) =>
        busy
            ? Add("aria-busy", "true")
            : this;

    public AriaAttributes AddPressed(bool? pressed)
    {
        if (!pressed.HasValue)
        {
            return this;
        }

        return Add(
            "aria-pressed",
            pressed.Value ? "true" : "false");
    }

    public AriaAttributes AddRole(string? role) =>
        Add("role", role);

    private AriaAttributes Add(
        string name,
        object? value)
    {
        if (value is null)
        {
            return this;
        }

        if (value is string text &&
            string.IsNullOrWhiteSpace(text))
        {
            return this;
        }

        var dictionary = new Dictionary<string, object>(
            _attributes,
            StringComparer.Ordinal)
        {
            [name] = value
        };

        return new AriaAttributes(
            dictionary.ToFrozenDictionary(StringComparer.Ordinal));
    }
}
