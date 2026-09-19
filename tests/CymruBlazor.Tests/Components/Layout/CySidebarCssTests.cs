using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Layout;

/// <summary>
/// bUnit cannot evaluate the CSS cascade, so the sidebar's rail layout is
/// guarded with a static scan of <c>grid.css</c>.
/// </summary>
public sealed class CySidebarCssTests
{
    [Fact]
    public void Item_Should_Be_A_Flex_Container_In_Every_State()
    {
        // The Compact and IconOnly rules set flex-direction / align-items /
        // justify-content on .cy-sidebar__item. Without display: flex on the
        // base rule those are inert, and the icon and label flow inline
        // (short labels sit beside the icon, long ones wrap beneath it).
        var block = ReadRule(@"\.cy-sidebar__item");

        block.ShouldContain("display: flex");
    }

    [Fact]
    public void Compact_Rail_Should_Centre_Icon_And_Label_In_A_Column()
    {
        var block = ReadRule(@"\.cy-sidebar--compact \.cy-sidebar__item");

        block.ShouldContain("flex-direction: column");
        block.ShouldContain("align-items: center");
        block.ShouldContain("justify-content: center");
        block.ShouldContain("text-align: center");
    }

    [Fact]
    public void IconOnly_Icons_Should_Be_1_5rem_And_Centred()
    {
        var item = ReadRule(@"\.cy-sidebar--icon-only \.cy-sidebar__item");
        item.ShouldContain("flex-direction: column");
        item.ShouldContain("align-items: center");
        item.ShouldContain("justify-content: center");

        var icon = ReadRule(@"\.cy-sidebar--icon-only \.cy-sidebar__item \.cy-icon");
        icon.ShouldContain("width: 1.5rem");
        icon.ShouldContain("height: 1.5rem");
    }

    private static string ReadRule(string escapedSelector)
    {
        var path = Path.Combine(
            FindRepositoryRoot(), "src", "CymruBlazor", "wwwroot", "css", "layout", "grid.css");

        var match = Regex.Match(
            File.ReadAllText(path),
            @"(?m)^" + escapedSelector + @"\s*\{([^}]*)\}");

        match.Success.ShouldBeTrue($"Rule '{escapedSelector}' not found in grid.css");

        return match.Groups[1].Value;
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
