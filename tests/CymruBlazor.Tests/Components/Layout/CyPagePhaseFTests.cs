using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyPagePhaseFTests : TestContextBase
{
    // ---- CyPageHeader additions

    [Fact]
    public void Eyebrow_Renders_Above_The_Title()
    {
        var cut = Render<CyPageHeader>(p => p
            .Add(c => c.Title, "Referral 42")
            .Add(c => c.Eyebrow, "Genomics"));

        var heading = cut.Find(".cy-page-header__heading");
        heading.Children[0].ClassList.ShouldContain("cy-page-header__eyebrow");
        heading.Children[0].TextContent.ShouldBe("Genomics");
        heading.Children[1].TagName.ShouldBe("H1");
    }

    [Fact]
    public void Badges_Share_A_Row_With_The_Title()
    {
        var cut = Render<CyPageHeader>(p => p
            .Add(c => c.Title, "Referral 42")
            .Add(c => c.Badges, "<span class=\"status\">Draft</span>"));

        var row = cut.Find(".cy-page-header__title-row");
        row.Children[0].TagName.ShouldBe("H1");
        row.Children[1].ClassList.ShouldContain("cy-page-header__badges");
        row.QuerySelector(".cy-page-header__badges .status")!.TextContent.ShouldBe("Draft");
    }

    [Theory]
    [InlineData(2, "H2")]
    [InlineData(3, "H3")]
    public void TitleLevel_Chooses_The_Heading_Element_Without_Changing_Its_Look(int level, string expected)
    {
        var cut = Render<CyPageHeader>(p => p
            .Add(c => c.Title, "Section")
            .Add(c => c.TitleLevel, level));

        var title = cut.Find(".cy-page-header__heading > *");
        title.TagName.ShouldBe(expected);
        title.ClassList.ShouldContain("cy-typography--h1");
        cut.FindAll("h1").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void TitleLevel_Outside_1_To_6_Throws(int level)
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyPageHeader>(p => p.Add(c => c.Title, "x").Add(c => c.TitleLevel, level)));
    }

    [Fact]
    public void Eyebrow_Is_Encoded()
    {
        var cut = Render<CyPageHeader>(p => p
            .Add(c => c.Title, "x")
            .Add(c => c.Eyebrow, "<b>bold</b>"));

        cut.FindAll(".cy-page-header__eyebrow b").Count.ShouldBe(0);
    }

    // ---- CyPage

    [Fact]
    public void Page_Renders_A_Div_With_The_Default_Width_Class()
    {
        var cut = Render<CyPage>(p => p.AddChildContent("<p>Hi</p>"));

        var page = cut.Find(".cy-page");
        page.TagName.ShouldBe("DIV");
        page.ClassList.ShouldContain("cy-page--default");
        cut.FindAll("main").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(CyPageWidth.Narrow, "cy-page--narrow")]
    [InlineData(CyPageWidth.Default, "cy-page--default")]
    [InlineData(CyPageWidth.Wide, "cy-page--wide")]
    [InlineData(CyPageWidth.Full, "cy-page--full")]
    public void Page_Width_Maps_To_A_Class(CyPageWidth width, string expected)
    {
        var cut = Render<CyPage>(p => p.Add(c => c.Width, width));

        cut.Find(".cy-page").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Page_RemovePadding_Adds_Its_Class()
    {
        var cut = Render<CyPage>(p => p.Add(c => c.RemovePadding, true));

        cut.Find(".cy-page").ClassList.ShouldContain("cy-page--no-padding");
    }

    [Fact]
    public void Page_Passes_Through_Class_And_Attributes()
    {
        var cut = Render<CyPage>(p => p
            .Add(c => c.Class, "mine")
            .AddUnmatched("data-test", "x"));

        var page = cut.Find(".cy-page");
        page.ClassList.ShouldContain("mine");
        page.GetAttribute("data-test").ShouldBe("x");
    }

    // ---- CyPageSection

    [Fact]
    public void Section_With_A_Heading_Is_A_Labelled_Section()
    {
        var cut = Render<CyPageSection>(p => p
            .Add(c => c.Heading, "Results")
            .AddChildContent("<p>Body</p>"));

        var section = cut.Find("section.cy-page-section");
        var heading = cut.Find("h2.cy-page-section__heading");
        heading.TextContent.Trim().ShouldBe("Results");
        heading.Id.ShouldNotBeNullOrWhiteSpace();
        section.GetAttribute("aria-labelledby").ShouldBe(heading.Id);
    }

    [Fact]
    public void Section_Without_A_Heading_Is_A_Plain_Div()
    {
        var cut = Render<CyPageSection>(p => p.AddChildContent("<p>Body</p>"));

        var root = cut.Find(".cy-page-section");
        root.TagName.ShouldBe("DIV");
        root.HasAttribute("aria-labelledby").ShouldBeFalse();
        cut.FindAll("section,h1,h2,h3,h4,h5,h6").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(3, "H3")]
    [InlineData(1, "H1")]
    public void Section_HeadingLevel_Chooses_The_Element(int level, string expected)
    {
        var cut = Render<CyPageSection>(p => p
            .Add(c => c.Heading, "Results")
            .Add(c => c.HeadingLevel, level));

        cut.Find(".cy-page-section__heading").TagName.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Section_HeadingLevel_Outside_1_To_6_Throws(int level)
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyPageSection>(p => p.Add(c => c.Heading, "x").Add(c => c.HeadingLevel, level)));
    }

    [Fact]
    public void Section_Heading_Is_Encoded()
    {
        var cut = Render<CyPageSection>(p => p.Add(c => c.Heading, "<img src=x onerror=alert(1)>"));

        cut.FindAll("img").Count.ShouldBe(0);
    }
}
