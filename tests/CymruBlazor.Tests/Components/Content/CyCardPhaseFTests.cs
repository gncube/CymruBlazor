using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyCardPhaseFTests : TestContextBase
{
    private IRenderedComponent<CyCard> RenderCard(Action<ComponentParameterCollectionBuilder<CyCard>>? configure = null) =>
        Render<CyCard>(p =>
        {
            p.AddChildContent("<p>Body</p>");
            configure?.Invoke(p);
        });

    [Fact]
    public void Title_Renders_As_An_H3_In_The_Header_By_Default()
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "Clinic details"));

        var heading = cut.Find(".cy-card__header h3");
        heading.TextContent.Trim().ShouldBe("Clinic details");
        heading.ClassList.ShouldContain("cy-card__title");
    }

    [Theory]
    [InlineData(1, "H1")]
    [InlineData(2, "H2")]
    [InlineData(4, "H4")]
    public void HeadingLevel_Chooses_The_Heading_Element(int level, string expected)
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "Clinic").Add(c => c.HeadingLevel, level));

        var heading = cut.Find(".cy-card__title");
        heading.TagName.ShouldBe(expected);
        heading.ClassList.ShouldContain("cy-typography--h5"); // visual size does not change
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void HeadingLevel_Outside_1_To_6_Throws(int level)
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderCard(p => p.Add(c => c.Title, "Clinic").Add(c => c.HeadingLevel, level)));
    }

    [Fact]
    public void Without_A_Title_No_Heading_Is_Rendered()
    {
        var cut = RenderCard(p => p.Add(c => c.Header, "<strong>Head</strong>"));

        cut.FindAll("h1,h2,h3,h4,h5,h6").Count.ShouldBe(0);
    }

    [Fact]
    public void Header_Content_Follows_The_Title()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Title, "Clinic")
            .Add(c => c.Header, "<span class=\"extra\">Open</span>"));

        var header = cut.Find(".cy-card__header");
        header.Children[0].ClassList.ShouldContain("cy-card__title");
        header.Children[1].ClassList.ShouldContain("extra");
    }

    [Fact]
    public void Title_Is_Encoded()
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "<img src=x onerror=alert(1)>"));

        cut.FindAll("img").Count.ShouldBe(0);
        cut.Find(".cy-card__title").TextContent.ShouldContain("<img src=x onerror=alert(1)>");
    }

    [Fact]
    public void Defaults_Add_None_Of_The_New_Classes()
    {
        var cut = RenderCard();

        var classes = cut.Find(".cy-card").ClassList;
        classes.ShouldNotContain("cy-card--outlined");
        classes.ShouldNotContain("cy-card--flat");
        classes.ShouldNotContain("cy-card--padding-compact");
        classes.ShouldNotContain("cy-card--padding-none");
        classes.ShouldNotContain("cy-card--collapsible");
    }

    [Theory]
    [InlineData(CyCardPadding.Compact, "cy-card--padding-compact")]
    [InlineData(CyCardPadding.None, "cy-card--padding-none")]
    public void Padding_Adds_Its_Class(CyCardPadding padding, string expected)
    {
        var cut = RenderCard(p => p.Add(c => c.Padding, padding));

        cut.Find(".cy-card").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData(CyCardVariant.Outlined, "cy-card--outlined")]
    [InlineData(CyCardVariant.Flat, "cy-card--flat")]
    public void Variant_Adds_Its_Class(CyCardVariant variant, string expected)
    {
        var cut = RenderCard(p => p.Add(c => c.Variant, variant));

        cut.Find(".cy-card").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Collapsible_Renders_A_Button_Inside_The_Heading_That_Controls_The_Body()
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "Details").Add(c => c.Collapsible, true));

        var button = cut.Find(".cy-card__title > button.cy-card__toggle");
        button.GetAttribute("type").ShouldBe("button");
        button.GetAttribute("aria-expanded").ShouldBe("true");
        button.TextContent.ShouldContain("Details");

        var body = cut.Find(".cy-card__body");
        body.Id.ShouldNotBeNullOrWhiteSpace();
        button.GetAttribute("aria-controls").ShouldBe(body.Id);
        body.HasAttribute("hidden").ShouldBeFalse();
        cut.Find(".cy-card").ClassList.ShouldContain("cy-card--collapsible");
    }

    [Fact]
    public void Collapsible_Icon_Is_Hidden_From_Assistive_Technology()
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "Details").Add(c => c.Collapsible, true));

        cut.Find(".cy-card__toggle-icon").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Clicking_The_Toggle_Collapses_Then_Expands_And_Reports_Each_Change()
    {
        var changes = new List<bool>();
        var cut = RenderCard(p => p
            .Add(c => c.Title, "Details")
            .Add(c => c.Collapsible, true)
            .Add(c => c.ExpandedChanged, (bool value) => changes.Add(value)));

        cut.Find("button.cy-card__toggle").Click();

        cut.Find("button.cy-card__toggle").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find(".cy-card__body").HasAttribute("hidden").ShouldBeTrue();

        cut.Find("button.cy-card__toggle").Click();

        cut.Find("button.cy-card__toggle").GetAttribute("aria-expanded").ShouldBe("true");
        cut.Find(".cy-card__body").HasAttribute("hidden").ShouldBeFalse();
        string.Join(",", changes).ShouldBe("False,True");
    }

    [Fact]
    public void Expanded_False_Starts_Collapsed()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Title, "Details")
            .Add(c => c.Collapsible, true)
            .Add(c => c.Expanded, false));

        cut.Find("button.cy-card__toggle").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find(".cy-card__body").HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void Collapsible_Without_A_Title_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderCard(p => p.Add(c => c.Collapsible, true)));
    }

    [Fact]
    public void Collapsible_With_Href_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderCard(p => p
                .Add(c => c.Title, "Details")
                .Add(c => c.Collapsible, true)
                .Add(c => c.Href, "/x")));
    }

    [Fact]
    public void A_Non_Collapsible_Title_Has_No_Button()
    {
        var cut = RenderCard(p => p.Add(c => c.Title, "Details"));

        cut.FindAll("button").Count.ShouldBe(0);
        cut.Find(".cy-card__body").HasAttribute("id").ShouldBeFalse();
    }
}
