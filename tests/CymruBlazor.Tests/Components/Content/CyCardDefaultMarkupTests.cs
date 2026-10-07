using AngleSharp.Dom;
using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Content;

/// <summary>
/// Phase F (1.12.0) added parameters to <see cref="CyCard"/> and <see cref="CyPageHeader"/>. With default
/// parameters their markup must be exactly what 1.11.0 produced. These tests pin the element structure and
/// class lists (id and style are generated and not part of the contract).
/// </summary>
public sealed class CyCardDefaultMarkupTests : TestContextBase
{
    private static string Outline(IElement element)
    {
        var classes = string.Join('.', element.ClassList);
        var children = element.Children.Select(Outline).ToList();

        return $"{element.TagName.ToLowerInvariant()}.{classes}" +
               (children.Count > 0 ? $"[{string.Join(",", children)}]" : string.Empty);
    }

    [Fact]
    public void Default_Card_Is_A_Div_With_Only_A_Body()
    {
        var cut = Render<CyCard>(p => p.AddChildContent("<p>Body</p>"));

        Outline(cut.Find(".cy-card"))
            .ShouldBe("div.cy-card.cy-card--elevation-small[div.cy-card__body[p.]]");
    }

    [Fact]
    public void Default_Card_Has_No_Body_Id_Or_Hidden_Attribute()
    {
        var cut = Render<CyCard>(p => p.AddChildContent("<p>Body</p>"));

        var body = cut.Find(".cy-card__body");
        body.HasAttribute("id").ShouldBeFalse();
        body.HasAttribute("hidden").ShouldBeFalse();
    }

    [Fact]
    public void Card_With_Header_And_Footer_Keeps_Its_Three_Regions()
    {
        var cut = Render<CyCard>(p => p
            .Add(c => c.Header, "<strong>Head</strong>")
            .Add(c => c.Footer, "<span>Foot</span>")
            .AddChildContent("<p>Body</p>"));

        Outline(cut.Find(".cy-card"))
            .ShouldBe("div.cy-card.cy-card--elevation-small[div.cy-card__header[strong.],div.cy-card__body[p.],div.cy-card__footer[span.]]");
    }

    [Fact]
    public void Whole_Card_Link_Keeps_Its_Shape()
    {
        var cut = Render<CyCard>(p => p
            .Add(c => c.Href, "/somewhere")
            .AddChildContent("<p>Body</p>"));

        Outline(cut.Find(".cy-card"))
            .ShouldBe("a.cy-card.cy-card--elevation-small.cy-card--interactive[div.cy-card__body[p.]]");
    }

    [Fact]
    public void Default_Page_Header_Has_Only_The_Title_In_Its_Heading_Region()
    {
        var cut = Render<CyPageHeader>(p => p.Add(c => c.Title, "Manage patients"));

        var root = cut.Find("header");
        string.Join(" ", root.ClassList).ShouldBe("cy-page-header");

        var heading = cut.Find(".cy-page-header__heading");
        heading.Children.Length.ShouldBe(1);
        heading.Children[0].TagName.ShouldBe("H1");
        heading.Children[0].ClassList.ShouldContain("cy-typography--h1");

        cut.FindAll(".cy-page-header__eyebrow").Count.ShouldBe(0);
        cut.FindAll(".cy-page-header__title-row").Count.ShouldBe(0);
        cut.FindAll(".cy-page-header__badges").Count.ShouldBe(0);
    }

    [Fact]
    public void Page_Header_With_Subtitle_Puts_Title_Then_Subtitle_Directly_In_The_Heading_Region()
    {
        var cut = Render<CyPageHeader>(p => p
            .Add(c => c.Title, "Manage patients")
            .Add(c => c.Subtitle, "View records"));

        var heading = cut.Find(".cy-page-header__heading");
        string.Join(",", heading.Children.Select(c => c.TagName)).ShouldBe("H1,P");
    }
}
