namespace CymruBlazor.Demo.Shared;

/// <summary>
/// The single, ordered source of truth for every documentation page in the
/// demo, matching <c>DemoSidebar</c>'s order exactly. Drives prev/next page
/// navigation (<see cref="DemoPageNav"/>) so the two never drift apart.
/// "Focus Trap" appears in both the Foundations and Accessibility sidebar
/// sections but is listed once here, at its first (Foundations) position,
/// so prev/next doesn't loop back on itself.
/// </summary>
public static class DemoNavigationIndex
{
    public sealed record Entry(string Category, string Title, string Href, string Description = "");

    public static readonly IReadOnlyList<Entry> Pages =
    [
        new("Getting Started", "Overview", "/getting-started",
            "What CymruBlazor is, its design principles, and requirements."),
        new("Getting Started", "Installation", "/installation",
            "NuGet package, stylesheet, theme script, and service registration."),
        new("Getting Started", "Design Tokens", "/design-tokens",
            "Colour, spacing, and typography tokens sourced from the DHCW design system."),

        new("Foundations", "Theme Provider", "/foundations/theme-provider",
            "Applies the active theme app-wide and enables runtime theme switching."),
        new("Foundations", "Typography", "/foundations/typography",
            "The NHS Wales type scale, via CyTypography."),
        new("Foundations", "Focus Trap", "/accessibility/focus-trap",
            "Traps keyboard focus within a region, for modals and menus."),

        new("Layout", "Container", "/layouts/container",
            "Constrains content to a maximum readable width."),
        new("Layout", "Stack", "/layouts/stack",
            "Flexbox-based directional layout primitive."),
        new("Layout", "Grid", "/layouts/grid",
            "CSS grid layout with configurable columns and gap."),
        new("Layout", "Cluster", "/layouts/cluster",
            "Wraps inline items with consistent spacing and alignment."),
        new("Layout", "Sidebar", "/layouts/sidebar",
            "A two-column layout with a fixed-width side panel."),
        new("Layout", "Center", "/layouts/center",
            "Horizontally centers content with an optional max width."),

        new("Forms", "Button", "/forms/button",
            "Trigger actions and submit forms."),
        new("Forms", "TextBox", "/forms/textbox",
            "A labelled single-line text input with hint and validation support."),
        new("Forms", "Select", "/forms/select",
            "A labelled dropdown selection field."),
        new("Forms", "Checkbox", "/forms/checkbox",
            "A single labelled checkbox field."),
        new("Forms", "Validation Summary", "/forms/validation-summary",
            "A titled summary of an EditForm's current validation errors."),

        new("Content", "Alert", "/content/alert",
            "An inline status/alert banner."),
        new("Content", "Card", "/content/card",
            "A content container with optional header, footer, and whole-card link."),
        new("Content", "Icon", "/content/icons",
            "The built-in icon set and how to render them."),

        new("Branding", "Brand Logo", "/branding/brand-logo",
            "The CymruBlazor/product logo mark and wordmark."),
        new("Branding", "Language Toggle", "/branding/language-toggle",
            "Switches the active display language between Welsh and English."),

        new("Navigation", "Breadcrumb", "/navigation/breadcrumb",
            "A breadcrumb trail showing the current page's location in the site hierarchy."),
        new("Navigation", "Header", "/navigation/header",
            "The page-level header chrome bar: brand, primary content, and trailing actions."),
        new("Navigation", "Navigation", "/navigation/navigation",
            "Top-level site navigation with a responsive mobile toggle."),
        new("Navigation", "Page Header", "/navigation/page-header",
            "A page-level heading region: title, subtitle, breadcrumb, and actions."),
        new("Navigation", "Skip Link", "/navigation/skip-link",
            "A visually-hidden-until-focused link that jumps to the main content."),
        new("Navigation", "Footer", "/navigation/footer",
            "Site footer with optional link groups, copyright, and version display."),

        new("Accessibility", "Live Region", "/accessibility/live-region",
            "Announces dynamic content changes to screen readers."),
        new("Accessibility", "Screen Reader Only", "/accessibility/screen-reader-only",
            "Hides content visually while keeping it available to screen readers.")
    ];

    public static int IndexOf(string relativePath)
    {
        var normalized = "/" + relativePath.Trim('/');

        for (var i = 0; i < Pages.Count; i++)
        {
            if (string.Equals(Pages[i].Href, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Simple client-side text search: title/category matches rank above
    /// description-only matches, ties broken by the page's natural order.
    /// No fuzzy matching - deliberately simple for a ~30-page site where a
    /// dependency-free substring search is plenty.
    /// </summary>
    public static IReadOnlyList<Entry> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var trimmed = query.Trim();

        return Pages
            .Select((entry, position) => (entry, position, rank: Rank(entry, trimmed)))
            .Where(x => x.rank > 0)
            .OrderByDescending(x => x.rank)
            .ThenBy(x => x.position)
            .Select(x => x.entry)
            .ToList();
    }

    private static int Rank(Entry entry, string query)
    {
        if (entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (entry.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (entry.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 0;
    }
}
