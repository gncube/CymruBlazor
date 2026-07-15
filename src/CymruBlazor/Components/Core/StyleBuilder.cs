using System.Collections.Frozen;
using System.Text;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Builds deterministic inline CSS style strings using an immutable fluent API.
///
/// Duplicate CSS properties are automatically overwritten, with the most
/// recently added value taking precedence.
/// </summary>
public sealed class StyleBuilder
{
    private readonly FrozenDictionary<string, string> _styles;
    private readonly IReadOnlyList<string> _orderedKeys;

    private StyleBuilder(
        FrozenDictionary<string, string> styles,
        IReadOnlyList<string> orderedKeys)
    {
        _styles = styles;
        _orderedKeys = orderedKeys;
    }

    /// <summary>
    /// Gets an empty style builder.
    /// </summary>
    public static StyleBuilder Empty { get; } =
        new(
            FrozenDictionary<string, string>.Empty,
            Array.Empty<string>());

    /// <summary>
    /// Adds a CSS property.
    /// </summary>
    public StyleBuilder AddStyle(
        string? property,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(property) ||
            string.IsNullOrWhiteSpace(value))
        {
            return this;
        }

        property = property.Trim();
        value = value.Trim();

        var orderedKeys = new List<string>(_orderedKeys);

        if (!orderedKeys.Contains(property, StringComparer.Ordinal))
        {
            orderedKeys.Add(property);
        }

        var dictionary = new Dictionary<string, string>(
            _styles,
            StringComparer.Ordinal)
        {
            [property] = value
        };

        return new StyleBuilder(
            dictionary.ToFrozenDictionary(StringComparer.Ordinal),
            orderedKeys);
    }

    /// <summary>
    /// Adds a CSS property when the supplied condition is true.
    /// </summary>
    public StyleBuilder AddStyle(
        string? property,
        string? value,
        bool condition)
    {
        return condition
            ? AddStyle(property, value)
            : this;
    }

    /// <summary>
    /// Adds a CSS property when the supplied condition evaluates to true.
    /// </summary>
    public StyleBuilder AddStyle(
        string? property,
        string? value,
        Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        return condition()
            ? AddStyle(property, value)
            : this;
    }

    /// <summary>
    /// Adds an existing inline style string.
    /// </summary>
    public StyleBuilder AddStyle(string? style)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            return this;
        }

        var builder = this;

        foreach (var declaration in style.Split(
                     ';',
                     StringSplitOptions.RemoveEmptyEntries |
                     StringSplitOptions.TrimEntries))
        {
            var separator = declaration.IndexOf(':');

            if (separator < 1)
            {
                continue;
            }

            var property = declaration[..separator];
            var value = declaration[(separator + 1)..];

            builder = builder.AddStyle(property, value);
        }

        return builder;
    }

    /// <summary>
    /// Builds the inline style string.
    /// </summary>
    public string Build()
    {
        if (_orderedKeys.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var key in _orderedKeys)
        {
            if (!_styles.TryGetValue(key, out var value))
            {
                continue;
            }

            builder
                .Append(key)
                .Append(':')
                .Append(value)
                .Append(';');
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public override string ToString() => Build();
}
