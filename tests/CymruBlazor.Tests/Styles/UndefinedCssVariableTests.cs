using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Styles;

/// <summary>
/// Static guard for a bug class that has bitten the library repeatedly: a
/// <c>var(--name, fallback)</c> that points at a custom property nobody
/// defines. The fallback hides the mistake (nothing visibly breaks), but the
/// component then ignores the active theme. Examples fixed in 1.2.1:
/// <c>--cb-color-*</c> in the toast, <c>--cymru-z-index-sticky</c> in the
/// header, <c>--cymru-font-family-mono</c> in the code block.
///
/// Every <c>var(--name</c> in the shipped library CSS must resolve to a
/// custom property that is either declared in library CSS (<c>--name:</c>)
/// or set from C#/Razor (a <c>"--name</c> string literal, e.g.
/// <c>--cy-sidebar-mobile-breakpoint</c>). Samples and the Demo are out of
/// scope: they legitimately use Blazor-injected and aliased variables.
///
/// bUnit cannot evaluate the cascade, so this is a text scan.
/// </summary>
public sealed partial class UndefinedCssVariableTests
{
    /// <summary>
    /// Intentional exceptions, as <c>name</c>. Deliberately empty: fix the
    /// reference (or define the token) rather than adding to this list.
    /// </summary>
    private static readonly HashSet<string> Allowlist = new(StringComparer.Ordinal);

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"var\(\s*(--[A-Za-z0-9_-]+)")]
    private static partial Regex VarReference();

    [GeneratedRegex(@"(?<![A-Za-z0-9_-])(--[A-Za-z0-9_-]+)\s*:")]
    private static partial Regex CssDeclaration();

    [GeneratedRegex("\"(--[A-Za-z0-9_-]+)")]
    private static partial Regex CodeLiteral();

    [Fact]
    public void Library_Css_Should_Only_Reference_Defined_Custom_Properties()
    {
        var libraryRoot = Path.Combine(FindRepositoryRoot(), "src", "CymruBlazor");

        var cssFiles = EnumerateSourceFiles(libraryRoot, "*.css").ToList();
        cssFiles.ShouldNotBeEmpty("No library CSS was found - is the path wrong?");

        var references = new List<(string File, string Name)>();
        var defined = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in cssFiles)
        {
            var css = CssComment().Replace(File.ReadAllText(file), string.Empty);
            var relative = Path.GetRelativePath(libraryRoot, file);

            foreach (Match m in CssDeclaration().Matches(css))
            {
                defined.Add(m.Groups[1].Value);
            }

            foreach (Match m in VarReference().Matches(css))
            {
                references.Add((relative, m.Groups[1].Value));
            }
        }

        // Custom properties set from C#/Razor (inline style strings).
        foreach (var pattern in new[] { "*.cs", "*.razor" })
        {
            foreach (var file in EnumerateSourceFiles(libraryRoot, pattern))
            {
                foreach (Match m in CodeLiteral().Matches(File.ReadAllText(file)))
                {
                    defined.Add(m.Groups[1].Value);
                }
            }
        }

        references.ShouldNotBeEmpty("No var(--...) references were found - the scan is broken.");

        var offenders = references
            .Where(r => !defined.Contains(r.Name) && !Allowlist.Contains(r.Name))
            .GroupBy(r => r.File)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key} -> {string.Join(", ", g.Select(r => r.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal))}")
            .ToList();

        offenders.ShouldBeEmpty(
            "These library stylesheets reference custom properties that are defined nowhere " +
            "(the fallback masks it, but the theme is ignored). Use a real --cymru-* token:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void ToastContainer_Css_Should_Use_The_Toast_Layer_And_No_Hard_Coded_Colours()
    {
        var css = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "CymruBlazor", "Components", "Feedback", "CyToastContainer.razor.css"));
        css = CssComment().Replace(css, string.Empty);

        css.ShouldContain("var(--cymru-z-toast)");
        css.ShouldNotContain("--cb-color-");

        // Only rgba() shadow values may be literal; any hex colour would
        // defeat dark and high-contrast theming.
        Regex.Matches(css, "#[0-9A-Fa-f]{3,8}\\b")
            .Select(m => m.Value)
            .ShouldBeEmpty("CyToastContainer.razor.css must use --cymru-* colour tokens, not hex literals.");
    }

    private static IEnumerable<string> EnumerateSourceFiles(string root, string pattern)
    {
        var sep = Path.DirectorySeparatorChar;
        return Directory
            .EnumerateFiles(root, pattern, SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{sep}bin{sep}", StringComparison.Ordinal)
                     && !f.Contains($"{sep}obj{sep}", StringComparison.Ordinal));
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
