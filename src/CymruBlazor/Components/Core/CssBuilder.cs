using System.Collections.Frozen;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Builds deterministic CSS class strings using an immutable fluent API.
/// </summary>
public sealed class CssBuilder
{
    private readonly FrozenSet<string> _classes;
    private readonly IReadOnlyList<string> _orderedClasses;

    private CssBuilder(
        FrozenSet<string> classes,
        IReadOnlyList<string> orderedClasses)
    {
        _classes = classes;
        _orderedClasses = orderedClasses;
    }

    /// <summary>
    /// Gets an empty CSS builder.
    /// </summary>
    public static CssBuilder Empty { get; } =
        new(
            FrozenSet<string>.Empty,
            Array.Empty<string>());

    /// <summary>
    /// Adds a CSS class.
    /// </summary>
    public CssBuilder AddClass(string? cssClass)
    {
        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return this;
        }

        cssClass = cssClass.Trim();

        if (_classes.Contains(cssClass))
        {
            return this;
        }

        var ordered = new List<string>(_orderedClasses.Count + 1);

        ordered.AddRange(_orderedClasses);

        ordered.Add(cssClass);

        var set = ordered.ToFrozenSet(StringComparer.Ordinal);

        return new CssBuilder(set, ordered);
    }

    /// <summary>
    /// Adds a CSS class when a condition is true.
    /// </summary>
    public CssBuilder AddClass(
        string? cssClass,
        bool condition)
    {
        return condition
            ? AddClass(cssClass)
            : this;
    }

    /// <summary>
    /// Adds a CSS class when the supplied condition evaluates to true.
    /// </summary>
    public CssBuilder AddClass(
        string? cssClass,
        Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        return condition()
            ? AddClass(cssClass)
            : this;
    }

    /// <summary>
    /// Adds multiple CSS classes separated by spaces.
    /// </summary>
    public CssBuilder AddClasses(string? cssClasses)
    {
        if (string.IsNullOrWhiteSpace(cssClasses))
        {
            return this;
        }

        var builder = this;

        foreach (var cssClass in cssClasses.Split(
                     ' ',
                     StringSplitOptions.RemoveEmptyEntries |
                     StringSplitOptions.TrimEntries))
        {
            builder = builder.AddClass(cssClass);
        }

        return builder;
    }

    /// <summary>
    /// Builds the CSS class string.
    /// </summary>
    public string Build()
    {
        return string.Join(' ', _orderedClasses);
    }

    /// <inheritdoc />
    public override string ToString() => Build();
}
