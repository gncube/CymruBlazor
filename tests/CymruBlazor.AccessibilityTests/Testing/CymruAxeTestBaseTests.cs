using CymruBlazor.Components.Content;
using CymruBlazor.Testing.Axe;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests.Testing;

/// <summary>
/// Tests the published axe base class. The library CSS is a small stand-in so the contrast results are known:
/// light-grey text fails on white in the light theme and is black in the dark theme.
/// </summary>
public sealed class CymruAxeTestBaseTests : CymruAxeTestBase
{
    private const string Faint = "<p class=\"faint\">Waiting list</p>";

    private static readonly string AssetsDirectory = WriteStandInAssets();

    protected override string? CssDirectory => AssetsDirectory;

    protected override string? AppCss => ".app-faint{color:#cccccc;background:#ffffff}";

    [Fact]
    public async Task Should_Report_A_Contrast_Violation_From_The_Css()
    {
        // Act
        var result = await ScanMarkupAsync(Faint);

        // Assert
        result.Violations.Select(v => v.Id).ShouldContain("color-contrast");
    }

    [Fact]
    public async Task Should_Scan_The_Requested_Theme()
    {
        // Act
        var result = await ScanMarkupAsync(Faint, "dark");

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Scan_The_App_Css_After_The_Library_Css()
    {
        // Act
        var result = await ScanMarkupAsync("<p class=\"app-faint\">Faint</p>");

        // Assert
        result.Violations.Select(v => v.Id).ShouldContain("color-contrast");
    }

    [Fact]
    public async Task ShouldHaveNoViolations_Should_Throw_And_Name_The_Rule_And_The_Element()
    {
        // Arrange
        var result = await ScanMarkupAsync(Faint);

        // Act
        var exception = Should.Throw<CymruAxeViolationException>(() => result.ShouldHaveNoViolations());

        // Assert
        exception.Violations.ShouldNotBeEmpty();
        exception.Message.ShouldContain("color-contrast");
        exception.Message.ShouldContain("Waiting list");
    }

    [Fact]
    public async Task ShouldHaveNoViolations_Should_Pass_For_Clean_Markup()
    {
        // Act
        var result = await ScanMarkupAsync("<p>Waiting list</p>");

        // Assert
        result.ShouldHaveNoViolations();
    }

    [Fact]
    public async Task Should_Render_A_Library_Component_With_Services_Registered_And_Scan_It()
    {
        // Act
        var result = await ScanComponentAsync<CyBadge>(parameters => parameters.AddChildContent("Cardiology"));

        // Assert
        result.ShouldHaveNoViolations();
    }

    [Fact]
    public async Task Should_Expose_The_Page_For_Tests_That_Drive_The_Browser()
    {
        // Arrange
        await ScanMarkupAsync("<button id=\"go\">Go</button>");

        // Act
        var page = await GetPageAsync();

        // Assert
        (await page.Locator("#go").CountAsync()).ShouldBe(1);
    }

    private static string WriteStandInAssets()
    {
        var directory = Path.Combine(Path.GetTempPath(), "cymrublazor-axe-base-tests");
        Directory.CreateDirectory(Path.Combine(directory, "css"));
        File.WriteAllText(
            Path.Combine(directory, "css", "cymrublazor.css"),
            ".faint{color:#cccccc;background:#ffffff}"
            + "[data-theme=\"dark\"] .faint{color:#000000;background:#ffffff}");

        return directory;
    }
}
