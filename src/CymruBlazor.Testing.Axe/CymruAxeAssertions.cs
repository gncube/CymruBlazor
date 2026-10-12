using Deque.AxeCore.Commons;

namespace CymruBlazor.Testing.Axe;

/// <summary>Assertions on the result of an axe-core scan.</summary>
public static class CymruAxeAssertions
{
    private const int MaxHtmlLength = 240;

    /// <summary>
    /// Throws <see cref="CymruAxeViolationException"/> if the scan found violations. The message names each rule, its
    /// impact, how to fix it, a link to the guidance and the markup of the elements affected.
    /// </summary>
    /// <param name="result">The result of a scan.</param>
    /// <exception cref="CymruAxeViolationException">The scan found one or more violations.</exception>
    public static void ShouldHaveNoViolations(this AxeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var violations = result.Violations ?? [];

        if (violations.Length > 0)
        {
            throw new CymruAxeViolationException(violations);
        }
    }

    /// <summary>Describes violations as readable text, one block per rule.</summary>
    /// <param name="violations">The violations to describe.</param>
    /// <returns>The text.</returns>
    public static string Describe(IReadOnlyList<AxeResultItem> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);

        var lines = new List<string>
        {
            FormattableString.Invariant($"The axe-core scan found {violations.Count} violation(s):")
        };

        foreach (var violation in violations)
        {
            lines.Add($"  - {violation.Id} ({violation.Impact}): {violation.Help}");

            if (!string.IsNullOrEmpty(violation.HelpUrl))
            {
                lines.Add($"    {violation.HelpUrl}");
            }

            foreach (var node in violation.Nodes ?? [])
            {
                var html = node.Html ?? string.Empty;
                lines.Add($"      {(html.Length > MaxHtmlLength ? html[..MaxHtmlLength] + "..." : html)}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
