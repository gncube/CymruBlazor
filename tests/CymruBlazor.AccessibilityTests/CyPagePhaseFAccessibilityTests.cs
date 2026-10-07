using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Phase F page scaffolding: card title, variants, padding and the collapsible card; page header eyebrow and
/// badges; page sections. Every state is rendered into one markup per theme to keep CI time down. The page
/// has one h1, and everything below it uses h2 so axe's heading-order rule stays meaningful.
/// </summary>
public sealed class CyPagePhaseFAccessibilityTests : AxeTestBase
{
    private string BuildMarkup()
    {
        var parts = new List<string>
        {
            "<h1>Phase F page scaffolding</h1>",
            Render<CyCard>(p => p
                .Add(c => c.Title, "Clinic details")
                .Add(c => c.HeadingLevel, 2)
                .Add(c => c.ChildContent, "Next available appointment: Thursday, 9:15am.")).Markup,
            Render<CyCard>(p => p
                .Add(c => c.Title, "Outlined card")
                .Add(c => c.HeadingLevel, 2)
                .Add(c => c.Variant, CyCardVariant.Outlined)
                .Add(c => c.Padding, CyCardPadding.Compact)
                .Add(c => c.ChildContent, "Outlined, compact padding.")).Markup,
            Render<CyCard>(p => p
                .Add(c => c.Title, "Flat card")
                .Add(c => c.HeadingLevel, 2)
                .Add(c => c.Variant, CyCardVariant.Flat)
                .Add(c => c.ChildContent, "Flat variant.")).Markup,
            Render<CyCard>(p => p
                .Add(c => c.Title, "Open collapsible card")
                .Add(c => c.HeadingLevel, 2)
                .Add(c => c.Collapsible, true)
                .Add(c => c.ChildContent, "Visible body.")).Markup,
            Render<CyCard>(p => p
                .Add(c => c.Title, "Closed collapsible card")
                .Add(c => c.HeadingLevel, 2)
                .Add(c => c.Collapsible, true)
                .Add(c => c.Expanded, false)
                .Add(c => c.ChildContent, "Hidden body.")).Markup,
            Render<CyPageHeader>(p => p
                .Add(c => c.Title, "Referral 42")
                .Add(c => c.Eyebrow, "Genomics")
                .Add(c => c.Badges, "<strong>Status: Draft</strong>")).Markup,
            Render<CyPage>(p => p
                .Add(c => c.Width, CyPageWidth.Narrow)
                .Add(c => c.ChildContent, "<p>Narrow page content.</p>")).Markup,
            Render<CyPageSection>(p => p
                .Add(c => c.Heading, "Results")
                .Add(c => c.ChildContent, "<p>Section content.</p>")).Markup,
            Render<CyPageSection>(p => p
                .Add(c => c.Heading, "Notes")
                .Add(c => c.ChildContent, "<p>Another section.</p>")).Markup
        };

        return string.Join(Environment.NewLine, parts);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Phase_F_Shapes(string theme)
    {
        var result = await ScanMarkupAsync(BuildMarkup(), theme);

        result.Violations.ShouldBeEmpty();
    }
}
