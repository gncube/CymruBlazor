using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Branding;

/// <summary>
/// bUnit cannot evaluate the CSS cascade, so the logo size scale is
/// guarded with a static scan of <c>branding.css</c>. Both the built-in SVG
/// mark and external image assets must follow the same documented scale
/// (ExtraSmall 18 | Small 32 | Medium 48 | Large 64 | ExtraLarge 72 px),
/// otherwise a logo would change size depending on which kind is rendered.
/// If you change the scale, update the docs on the CyBrandLogo demo page too.
/// </summary>
public sealed class BrandLogoSizeScaleTests
{
    private static readonly (string Size, int Pixels)[] ExpectedScale =
    [
        ("extrasmall", 18),
        ("small", 32),
        ("medium", 48),
        ("large", 64),
        ("extralarge", 72)
    ];

    [Fact]
    public void Image_Assets_Should_Follow_The_Documented_Scale()
    {
        var css = ReadBrandingCss();

        foreach (var (size, pixels) in ExpectedScale)
        {
            ReadPixels(css, $".cy-brand-logo--{size} .cy-brand-logo__asset", "height")
                .ShouldBe(pixels, $"{size} image asset height");
        }
    }

    [Fact]
    public void Built_In_Mark_Should_Follow_The_Same_Scale_As_Image_Assets()
    {
        var css = ReadBrandingCss();

        foreach (var (size, pixels) in ExpectedScale)
        {
            var selector = $".cy-brand-logo--{size} .cy-brand-logo__mark";

            ReadPixels(css, selector, "width").ShouldBe(pixels, $"{size} mark width");
            ReadPixels(css, selector, "height").ShouldBe(pixels, $"{size} mark height");
        }
    }

    [Fact]
    public void Scale_Should_Increase_Strictly_With_Each_Size()
    {
        ExpectedScale.Select(s => s.Pixels).ShouldBe(ExpectedScale.Select(s => s.Pixels).OrderBy(p => p).Distinct());
    }

    private static int ReadPixels(string css, string selector, string property)
    {
        var block = Regex.Match(css, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
        block.Success.ShouldBeTrue($"'{selector}' rule not found in branding.css");

        var value = Regex.Match(block.Groups[1].Value, property + @":\s*(\d+)px");
        value.Success.ShouldBeTrue($"'{selector}' has no pixel '{property}'");

        return int.Parse(value.Groups[1].Value);
    }

    private static string ReadBrandingCss()
    {
        var path = Path.Combine(
            FindRepositoryRoot(), "src", "CymruBlazor", "wwwroot", "css", "components", "branding.css");

        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CymruBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException(
                $"Could not locate the repository root (CymruBlazor.slnx) above '{AppContext.BaseDirectory}'.");
    }
}
