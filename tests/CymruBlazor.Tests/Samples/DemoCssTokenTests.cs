using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// Regression guard for the dark-mode icon-preview bug: the CyIcon
/// workbench's scoped stylesheet referenced <c>--cb-color-*</c> custom
/// properties that are not defined anywhere. Every usage therefore fell
/// back to its hardcoded light-mode default (<c>#0f172a</c> text), which is
/// near-invisible on the dark preview surface. The dark-mode override that
/// was supposed to define them lived in a scoped stylesheet, where the
/// selector is rewritten to require the Blazor scope attribute on the
/// <c>[data-theme]</c> element itself - which it never has - so it silently
/// never applied.
///
/// The demo's real, theme-reactive tokens are <c>--cb-bg-*</c>,
/// <c>--cb-text-*</c> and <c>--cb-border-*</c> (defined in
/// <c>wwwroot/css/demo.css</c> on <c>.cy-theme-provider[data-theme]</c>).
/// bUnit cannot evaluate the CSS cascade, so this is a static scan.
/// </summary>
public sealed class DemoCssTokenTests
{
    [Fact]
    public void Demo_Css_Should_Not_Reference_Undefined_CbColor_Tokens()
    {
        var demoDirectory = Path.Combine(FindRepositoryRoot(), "src", "CymruBlazor.Demo");

        var offenders = Directory
            .EnumerateFiles(demoDirectory, "*.css", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => File.ReadAllText(f).Contains("--cb-color-", StringComparison.Ordinal))
            .Select(f => Path.GetRelativePath(demoDirectory, f))
            .ToList();

        offenders.ShouldBeEmpty(
            "These demo stylesheets reference undefined --cb-color-* tokens; " +
            "use the theme-reactive --cb-bg-*/--cb-text-*/--cb-border-* tokens from demo.css instead.");
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
