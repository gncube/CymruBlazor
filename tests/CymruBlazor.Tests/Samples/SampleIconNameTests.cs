using System.Text.RegularExpressions;
using Xunit;
using Shouldly;
using CymruBlazor.Icons;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// Defence-in-depth companion to <see cref="DashboardMainLayoutTests"/>:
/// statically scans every *.razor file under samples/ for a literal
/// (non-bound) <c>&lt;CyIcon Name="..."&gt;</c> value and asserts it is
/// a registered <see cref="IconRegistry"/> icon.
///
/// This would NOT, on its own, have caught the original
/// "clinical"/"clinical-actions" bug (see plans/icon-bug-fix.md) - that
/// value only ever flowed through a bound C# field
/// (<c>Name="@filter.Icon"</c>), which this scan deliberately skips
/// since it can't be resolved statically. It still guards against the
/// more common mistake of a typo'd/wrong literal string directly on a
/// &lt;CyIcon&gt; tag, across every sample, without needing to render
/// each one.
/// </summary>
public sealed class SampleIconNameTests
{
    private static readonly Regex CyIconTagPattern = new(
        @"<CyIcon\b(.*?)(?:/>|>)",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex NameAttributePattern = new(
        @"Name\s*=\s*""([^""]*)""",
        RegexOptions.Compiled);

    [Fact]
    public void Every_Literal_CyIcon_Name_In_Samples_Should_Be_Registered()
    {
        var samplesDirectory = Path.Combine(FindRepositoryRoot(), "samples");

        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(samplesDirectory, "*.razor", SearchOption.AllDirectories))
        {
            var content = File.ReadAllText(file);

            foreach (Match tagMatch in CyIconTagPattern.Matches(content))
            {
                var nameMatch = NameAttributePattern.Match(tagMatch.Groups[1].Value);

                if (!nameMatch.Success)
                {
                    continue;
                }

                var name = nameMatch.Groups[1].Value;

                // Skip bound expressions (e.g. Name="@item.Icon") - not
                // statically resolvable; covered instead by
                // DashboardMainLayoutTests actually rendering the
                // component.
                if (name.StartsWith('@'))
                {
                    continue;
                }

                if (!IconRegistry.Exists(name))
                {
                    failures.Add($"{Path.GetRelativePath(samplesDirectory, file)}: Name=\"{name}\"");
                }
            }
        }

        failures.ShouldBeEmpty(
            $"Found {failures.Count} unregistered CyIcon name(s) - see {nameof(IconRegistry)}.{nameof(IconRegistry.AllNames)} " +
            $"for the full list of valid names:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
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
