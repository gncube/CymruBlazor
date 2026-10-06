using System.Text.RegularExpressions;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Validates user-supplied values that are written into a <c>style</c> attribute, so a parameter can
/// never smuggle in extra declarations (<c>;</c>), functions (<c>url(</c>, <c>calc(</c>) or expressions.
/// Same rule as <c>CyWorkspacePane.Width</c>: a plain length such as <c>12rem</c> or <c>40%</c>.
/// </summary>
internal static partial class CssLength
{
    [GeneratedRegex(@"^\d+(\.\d+)?(px|rem|em|ch|%|vw|vh)$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    /// <summary>Returns true when <paramref name="value"/> is null or a plain length.</summary>
    internal static bool IsValid(string? value) => value is null || Pattern().IsMatch(value);

    /// <summary>Throws <see cref="InvalidOperationException"/> when <paramref name="value"/> is not a plain length.</summary>
    internal static void Require(string? value, string component, string parameter)
    {
        if (!IsValid(value))
        {
            throw new InvalidOperationException(
                $"{component}.{parameter} must be a plain length such as '12rem', '40%' or '200px'. Received '{value}'.");
        }
    }
}
