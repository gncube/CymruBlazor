using Xunit;
using Shouldly;
using CymruBlazor.Icons;

namespace CymruBlazor.Tests.Icons;

public sealed class IconRegistryRegisterTests
{
    private const string Good = "<path d=\"M5 12h14\" /> <circle cx=\"12\" cy=\"12\" r=\"3\" />";

    // The registry is process-wide, so every test uses its own unique name.
    private static string Unique() => "t-" + Guid.NewGuid().ToString("N")[..12];

    [Fact]
    public void Register_Makes_Icon_Available()
    {
        var name = Unique();

        IconRegistry.Register(name, Good, domain: "ui");

        IconRegistry.Exists(name).ShouldBeTrue();
        IconRegistry.GetMarkup(name).ShouldBe(Good);
        IconRegistry.GetDomain(name).ShouldBe("ui");
    }

    [Fact]
    public void Register_Rejects_Duplicates_Unless_Overwrite()
    {
        var name = Unique();
        IconRegistry.Register(name, Good);

        Should.Throw<ArgumentException>(() => IconRegistry.Register(name, Good));

        IconRegistry.Register(name, "<path d=\"M0 0\" />", overwrite: true);
        IconRegistry.GetMarkup(name).ShouldBe("<path d=\"M0 0\" />");
    }

    [Fact]
    public void Register_Cannot_Replace_A_BuiltIn()
    {
        Should.Throw<ArgumentException>(() => IconRegistry.Register("search", Good, overwrite: true));
    }

    [Theory]
    [InlineData("Bad Name")]
    [InlineData("UPPER_case")]
    [InlineData("-lead")]
    [InlineData("trail-")]
    [InlineData("a--b")]
    [InlineData("")]
    [InlineData("../x")]
    public void Register_Rejects_Invalid_Names(string name)
    {
        Should.Throw<ArgumentException>(() => IconRegistry.Register(name, Good));
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<svg><path d=\"M0 0\"/></svg>")]
    [InlineData("<path d=\"M0 0\" onload=\"alert(1)\" />")]
    [InlineData("<circle cx=\"1\" cy=\"1\" r=\"1\" onclick=\"x()\" />")]
    [InlineData("<foreignObject><div>hi</div></foreignObject>")]
    [InlineData("<path d=\"M0 0\" style=\"x:y\" />")]
    [InlineData("<style>*{}</style>")]
    [InlineData("<use href=\"#x\" />")]
    [InlineData("<a href=\"javascript:alert(1)\"><path d=\"M0 0\"/></a>")]
    [InlineData("<path xmlns=\"http://www.w3.org/2000/svg\" d=\"M0 0\" />")]
    [InlineData("hello <path d=\"M0 0\" />")]
    [InlineData("<path d=\"M0 0\" /><![CDATA[<script>]]>")]
    [InlineData("<!DOCTYPE x [<!ENTITY e SYSTEM \"file:///etc/passwd\">]><path d=\"&e;\" />")]
    [InlineData("<path d=\"M0 0\" ")]
    [InlineData("   ")]
    [InlineData("<path d=\"M0 0\" fill=\"data:text/html,x\" />")]
    public void Register_Rejects_Unsafe_Or_Malformed_Markup(string markup)
    {
        Should.Throw<ArgumentException>(() => IconRegistry.Register(Unique(), markup));
    }

    [Fact]
    public void Every_BuiltIn_Icon_Passes_The_Validator()
    {
        // Guards against the allow-list silently rejecting a legitimate icon.
        foreach (var name in IconRegistry.AllNames.Where(n => !n.StartsWith("t-", StringComparison.Ordinal)).ToList())
        {
            Should.NotThrow(
                () => IconRegistry.Register(Unique(), IconRegistry.GetMarkup(name)),
                $"built-in icon '{name}'");
        }
    }

    [Fact]
    public void Register_Is_Thread_Safe()
    {
        var names = Enumerable.Range(0, 100).Select(_ => Unique()).ToList();

        Should.NotThrow(() => Parallel.ForEach(names, n =>
        {
            IconRegistry.Register(n, Good);
            _ = IconRegistry.AllNames.Count;
        }));

        names.ShouldAllBe(n => IconRegistry.Exists(n));
    }

    [Theory]
    [InlineData("grip-vertical")]
    [InlineData("arrow-up")]
    [InlineData("arrow-down")]
    [InlineData("git-branch")]
    [InlineData("layers")]
    [InlineData("panel-right")]
    [InlineData("monitor")]
    [InlineData("tablet")]
    [InlineData("smartphone")]
    [InlineData("list-checks")]
    [InlineData("text-cursor-input")]
    [InlineData("hash")]
    [InlineData("calendar-days")]
    [InlineData("circle-dot")]
    [InlineData("check-square")]
    [InlineData("redo")]
    [InlineData("search-check")]
    [InlineData("unknown")]
    public void New_1_7_Icons_Exist_With_A_Domain(string name)
    {
        IconRegistry.Exists(name).ShouldBeTrue();
        IconRegistry.GetDomain(name).ShouldNotBeNullOrWhiteSpace();
    }
}
