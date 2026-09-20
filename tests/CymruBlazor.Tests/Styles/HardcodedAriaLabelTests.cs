using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Styles;

/// <summary>
/// Static guard for localisation (roadmap D1): a library <c>.razor</c> file
/// must not contain a literal English <c>aria-label="Some text"</c>. Labels
/// must be bound to a parameter (<c>@(Label ?? "default")</c>) so a Welsh
/// service can override them.
/// </summary>
public sealed partial class HardcodedAriaLabelTests
{
    [GeneratedRegex("aria-label=\"[A-Za-z][^\"@]*\"")]
    private static partial Regex LiteralAriaLabel();

    [Fact]
    public void Library_Razor_Files_Should_Not_Contain_Literal_Aria_Labels()
    {
        var root = FindRepositoryRoot();
        var library = Path.Combine(root, "src", "CymruBlazor");

        var offenders = Directory
            .EnumerateFiles(library, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(f => LiteralAriaLabel().Matches(File.ReadAllText(f))
                .Select(m => $"{Path.GetRelativePath(library, f)}: {m.Value}"))
            .ToList();

        offenders.ShouldBeEmpty(
            "Bind aria-label to a string? parameter with an English default instead of a literal:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CymruBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate CymruBlazor.slnx.");
    }
}
