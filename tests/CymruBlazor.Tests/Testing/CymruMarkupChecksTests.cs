using CymruBlazor.Testing;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Testing;

public sealed class CymruMarkupChecksTests
{
    private static void ShouldHaveOneProblem(string markup, string containing)
    {
        var problems = CymruMarkupChecks.FindProblems(markup);

        problems.Count.ShouldBe(1, string.Join(" | ", problems));
        problems[0].ShouldContain(containing);
    }

    private static void ShouldHaveNoProblems(string markup) =>
        CymruMarkupChecks.FindProblems(markup).ShouldBeEmpty();

    // ------------------------------------------------------------------ ids and references

    [Fact]
    public void Well_Formed_Markup_Has_No_Problems()
    {
        ShouldHaveNoProblems(
            """
            <h1>Referral</h1>
            <h2>Patient</h2>
            <label for="name">Name</label><input id="name" aria-describedby="hint" />
            <p id="hint">As on the NHS number letter.</p>
            <h3>Address</h3>
            <button type="submit">Save</button>
            """);
    }

    [Fact]
    public void Duplicate_Ids_Are_Reported_Once_With_The_Count()
    {
        ShouldHaveOneProblem("""<p id="a">1</p><p id="a">2</p><p id="a">3</p><p id="b">4</p>""", "'a' is used by 3 elements");
    }

    [Fact]
    public void A_Describedby_Target_That_Does_Not_Exist_Is_Reported()
    {
        ShouldHaveOneProblem("""<input aria-label="Name" aria-describedby="nope" />""", "aria-describedby=\"nope\"");
    }

    [Fact]
    public void Only_The_Missing_Ids_In_A_List_Are_Reported()
    {
        ShouldHaveOneProblem(
            """<p id="a">x</p><input aria-label="Name" aria-describedby="a b" />""",
            "aria-describedby=\"b\"");
    }

    [Fact]
    public void A_Labelledby_Target_That_Does_Not_Exist_Is_Reported_Along_With_The_Missing_Name()
    {
        var problems = CymruMarkupChecks.FindProblems("""<input aria-labelledby="nope" />""");

        problems.Count.ShouldBe(2);
        problems.ShouldContain(p => p.Contains("aria-labelledby=\"nope\"", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("no accessible name", StringComparison.Ordinal));
    }

    [Fact]
    public void A_Label_For_An_Id_That_Does_Not_Exist_Is_Reported()
    {
        ShouldHaveOneProblem("""<label for="ghost">Name</label>""", "is for \"ghost\"");
    }

    // ------------------------------------------------------------------ accessible names

    [Theory]
    [InlineData("""<input id="a" />""")]
    [InlineData("""<select id="a"><option>1</option></select>""")]
    [InlineData("""<textarea id="a"></textarea>""")]
    [InlineData("""<input type="checkbox" id="a" />""")]
    [InlineData("""<div role="switch" aria-checked="false"></div>""")]
    [InlineData("""<div role="combobox"></div>""")]
    [InlineData("""<button type="button"><svg aria-hidden="true"></svg></button>""")]
    public void A_Control_With_No_Name_Is_Reported(string markup)
    {
        ShouldHaveOneProblem(markup, "no accessible name");
    }

    [Theory]
    [InlineData("""<input aria-label="Name" />""")]
    [InlineData("""<span id="l">Name</span><input aria-labelledby="l" />""")]
    [InlineData("""<label for="a">Name</label><input id="a" />""")]
    [InlineData("""<label>Name <input /></label>""")]
    [InlineData("""<input title="Name" />""")]
    [InlineData("""<label for="a">Name</label><select id="a"></select>""")]
    [InlineData("""<button type="button" aria-label="Close"><svg aria-hidden="true"></svg></button>""")]
    [InlineData("""<button type="button">Save</button>""")]
    [InlineData("""<div role="switch" aria-checked="true">Dark mode</div>""")]
    public void A_Control_With_A_Name_Is_Not_Reported(string markup)
    {
        ShouldHaveNoProblems(markup);
    }

    [Fact]
    public void A_Labelledby_Pointing_At_An_Empty_Element_Gives_No_Name()
    {
        ShouldHaveOneProblem("""<span id="l"></span><input aria-labelledby="l" />""", "no accessible name");
    }

    [Theory]
    [InlineData("""<input type="hidden" name="token" />""")]
    [InlineData("""<input type="submit" value="Go" />""")]
    [InlineData("""<input type="button" value="Go" />""")]
    [InlineData("""<input hidden />""")]
    [InlineData("""<div aria-hidden="true"><input /></div>""")]
    [InlineData("""<div hidden><button></button></div>""")]
    public void Controls_That_Need_No_Name_Or_Are_Hidden_Are_Ignored(string markup)
    {
        ShouldHaveNoProblems(markup);
    }

    // ------------------------------------------------------------------ headings

    [Fact]
    public void A_Skipped_Heading_Level_Is_Reported_With_Its_Text()
    {
        ShouldHaveOneProblem("<h1>Page</h1><h3>Detail</h3>", "skips from 1 to 3 at \"Detail\"");
    }

    [Fact]
    public void Going_Back_Up_The_Heading_Levels_Is_Allowed()
    {
        ShouldHaveNoProblems("<h1>A</h1><h2>B</h2><h3>C</h3><h2>D</h2><h3>E</h3><h1>F</h1>");
    }

    [Fact]
    public void The_First_Heading_May_Be_Any_Level_Because_A_Component_Has_No_Page_Around_It()
    {
        ShouldHaveNoProblems("<h3>Only a card title</h3><h4>Sub</h4>");
    }

    [Fact]
    public void An_Aria_Level_Heading_Counts_And_Defaults_To_Two()
    {
        ShouldHaveOneProblem("<h1>A</h1><div role=\"heading\" aria-level=\"4\">Deep</div>", "skips from 1 to 4");
        ShouldHaveOneProblem("<h4>A</h4><div role=\"heading\">Default two</div><h4>Back</h4>", "skips from 2 to 4");
    }

    [Fact]
    public void A_Hidden_Heading_Does_Not_Count()
    {
        ShouldHaveNoProblems("<h1>A</h1><h3 hidden>Hidden</h3><h2>B</h2>");
    }

    // ------------------------------------------------------------------ assertion

    [Fact]
    public void ShouldHaveNoProblems_Throws_With_Every_Problem_Listed()
    {
        var error = Should.Throw<CymruMarkupAssertionException>(
            () => CymruMarkupChecks.ShouldHaveNoProblems("""<p id="a"></p><p id="a"></p><input />"""));

        error.Problems.Count.ShouldBe(2);
        error.Message.ShouldContain("2 accessibility problem(s)");
        error.Message.ShouldContain("'a' is used by 2 elements");
        error.Message.ShouldContain("no accessible name");
    }

    [Fact]
    public void ShouldHaveNoProblems_Passes_Quietly_On_Good_Markup()
    {
        CymruMarkupChecks.ShouldHaveNoProblems("""<label for="a">Name</label><input id="a" />""");
    }

    [Fact]
    public void Null_Markup_Is_An_Argument_Error()
    {
        Should.Throw<ArgumentNullException>(() => CymruMarkupChecks.FindProblems(null!));
    }
}
