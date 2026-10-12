using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Testing;

/// <summary>
/// Structural accessibility checks on rendered markup, with no browser: the problems that show up in the HTML itself,
/// not in the CSS.
/// </summary>
/// <remarks>
/// <para>
/// The checks are: every <c>id</c> is unique; every id named by <c>aria-labelledby</c>, <c>aria-describedby</c> or a
/// <c>label for</c> exists; every form control and button has an accessible name; and heading levels never skip
/// downwards into the page (an <c>h2</c> after an <c>h4</c> is fine, an <c>h4</c> straight after an <c>h2</c> is not).
/// Elements that are hidden (<c>hidden</c> or <c>aria-hidden="true"</c>, or inside such an element) are ignored.
/// </para>
/// <para>
/// They look only at the markup you pass. A reference to an element outside it (a live region in your layout, for
/// example) is reported, so check a whole page or layout, or accept those reports for a single component. These checks
/// complement axe-core; they do not replace it, because they cannot see colour, contrast, focus or layout.
/// </para>
/// </remarks>
public static class CymruMarkupChecks
{
    private static readonly string[] ReferenceAttributes = ["aria-labelledby", "aria-describedby"];

    private static readonly string[] NamedRoles =
    [
        "button", "switch", "checkbox", "radio", "combobox", "textbox", "slider", "spinbutton"
    ];

    private static readonly string[] UnnamedInputTypes = ["hidden", "submit", "reset", "button", "image"];

    private static readonly string ControlSelector =
        "input, select, textarea, button, " + string.Join(", ", NamedRoles.Select(role => $"[role='{role}']"));

    /// <summary>Finds the problems in a rendered component's markup. An empty list means none were found.</summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="component">The rendered component.</param>
    /// <returns>One readable message per problem, in document order within each check.</returns>
    public static IReadOnlyList<string> FindMarkupProblems<TComponent>(this IRenderedComponent<TComponent> component)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(component);

        return FindProblems(component.Markup);
    }

    /// <summary>Finds the problems in a piece of markup. An empty list means none were found.</summary>
    /// <param name="markup">HTML: a fragment or a whole page.</param>
    /// <returns>One readable message per problem, in document order within each check.</returns>
    public static IReadOnlyList<string> FindProblems(string markup)
    {
        ArgumentNullException.ThrowIfNull(markup);

        var document = new HtmlParser().ParseDocument(markup);
        var problems = new List<string>();

        CheckIds(document, problems);
        CheckReferences(document, problems);
        CheckNames(document, problems);
        CheckHeadings(document, problems);

        return problems;
    }

    /// <summary>Fails with every problem listed when the component's markup has any.</summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="component">The rendered component.</param>
    /// <exception cref="CymruMarkupAssertionException">The markup has at least one problem.</exception>
    public static void ShouldHaveNoMarkupProblems<TComponent>(this IRenderedComponent<TComponent> component)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(component);

        ShouldHaveNoProblems(component.Markup);
    }

    /// <summary>Fails with every problem listed when the markup has any.</summary>
    /// <param name="markup">HTML: a fragment or a whole page.</param>
    /// <exception cref="CymruMarkupAssertionException">The markup has at least one problem.</exception>
    public static void ShouldHaveNoProblems(string markup)
    {
        var problems = FindProblems(markup);

        if (problems.Count > 0)
        {
            throw new CymruMarkupAssertionException(problems);
        }
    }

    // ------------------------------------------------------------------ ids

    private static void CheckIds(IDocument document, List<string> problems)
    {
        var duplicates = document.QuerySelectorAll("[id]")
            .GroupBy(element => element.Id ?? string.Empty, StringComparer.Ordinal)
            .Where(group => group.Key.Length > 0 && group.Count() > 1);

        foreach (var duplicate in duplicates)
        {
            problems.Add(Message($"The id '{duplicate.Key}' is used by {duplicate.Count()} elements; ids must be unique."));
        }
    }

    // ------------------------------------------------------------------ references

    private static void CheckReferences(IDocument document, List<string> problems)
    {
        foreach (var attribute in ReferenceAttributes)
        {
            foreach (var element in document.QuerySelectorAll($"[{attribute}]"))
            {
                foreach (var id in Tokens(element.GetAttribute(attribute)))
                {
                    if (document.GetElementById(id) is null)
                    {
                        problems.Add(Message($"{Describe(element)} has {attribute}=\"{id}\" but no element has that id."));
                    }
                }
            }
        }

        foreach (var label in document.QuerySelectorAll("label[for]"))
        {
            var target = label.GetAttribute("for");

            if (!string.IsNullOrEmpty(target) && document.GetElementById(target) is null)
            {
                problems.Add(Message($"{Describe(label)} is for \"{target}\" but no element has that id."));
            }
        }
    }

    // ------------------------------------------------------------------ names

    private static void CheckNames(IDocument document, List<string> problems)
    {
        foreach (var control in document.QuerySelectorAll(ControlSelector))
        {
            if (IsHidden(control) || NeedsNoName(control))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(AccessibleName(control, document)))
            {
                problems.Add(Message($"{Describe(control)} has no accessible name. Give it a label, aria-label or aria-labelledby."));
            }
        }
    }

    private static bool NeedsNoName(IElement control)
    {
        var type = control.GetAttribute("type") ?? "text";

        return control.LocalName == "input" &&
            Array.Exists(UnnamedInputTypes, unnamed => string.Equals(unnamed, type, StringComparison.OrdinalIgnoreCase));
    }

    private static string AccessibleName(IElement control, IDocument document)
    {
        var labelledBy = string.Concat(
            Tokens(control.GetAttribute("aria-labelledby"))
                .Select(id => document.GetElementById(id)?.TextContent ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(labelledBy))
        {
            return labelledBy;
        }

        var ariaLabel = control.GetAttribute("aria-label");

        if (!string.IsNullOrWhiteSpace(ariaLabel))
        {
            return ariaLabel;
        }

        var id = control.Id;

        if (!string.IsNullOrEmpty(id))
        {
            var labelText = string.Concat(
                document.QuerySelectorAll("label[for]")
                    .Where(label => string.Equals(label.GetAttribute("for"), id, StringComparison.Ordinal))
                    .Select(label => label.TextContent));

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                return labelText;
            }
        }

        var wrapping = control.Closest("label")?.TextContent;

        if (!string.IsNullOrWhiteSpace(wrapping))
        {
            return wrapping;
        }

        // Buttons and role-based controls are named by their content. A plain input or select is not.
        if (control.LocalName is "button" || control.HasAttribute("role"))
        {
            if (!string.IsNullOrWhiteSpace(control.TextContent))
            {
                return control.TextContent;
            }
        }

        return control.GetAttribute("title") ?? string.Empty;
    }

    // ------------------------------------------------------------------ headings

    private static void CheckHeadings(IDocument document, List<string> problems)
    {
        var previous = 0;

        foreach (var heading in document.QuerySelectorAll("h1, h2, h3, h4, h5, h6, [role='heading']"))
        {
            if (IsHidden(heading))
            {
                continue;
            }

            var level = HeadingLevel(heading);

            if (previous > 0 && level > previous + 1)
            {
                var text = heading.TextContent.Trim();
                problems.Add(Message($"Heading level skips from {previous} to {level} at \"{text}\"."));
            }

            previous = level;
        }
    }

    private static int HeadingLevel(IElement heading)
    {
        if (heading.LocalName.Length == 2 && heading.LocalName[0] == 'h' && char.IsDigit(heading.LocalName[1]))
        {
            return heading.LocalName[1] - '0';
        }

        return int.TryParse(heading.GetAttribute("aria-level"), NumberStyles.None, CultureInfo.InvariantCulture, out var level)
            ? level
            : 2;
    }

    // ------------------------------------------------------------------ helpers

    private static bool IsHidden(IElement element)
    {
        for (var current = element; current is not null; current = current.ParentElement)
        {
            if (current.HasAttribute("hidden") || string.Equals(current.GetAttribute("aria-hidden"), "true", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string[] Tokens(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static string Describe(IElement element)
    {
        var id = string.IsNullOrEmpty(element.Id) ? string.Empty : $"#{element.Id}";
        var cssClass = element.ClassList.Length > 0 ? element.ClassList[0] : null;
        var suffix = cssClass is null ? string.Empty : $".{cssClass}";

        return $"<{element.LocalName}{id}{suffix}>";
    }

    private static string Message(FormattableString message) => FormattableString.Invariant(message);
}
