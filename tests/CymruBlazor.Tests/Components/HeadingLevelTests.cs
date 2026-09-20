using Bunit;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Found by the published-demo smoke run (axe <c>heading-order</c>, <c>scrollable-region-focusable</c>):
/// components that emit a heading must let the page choose its level, and a scrollable code sample must be
/// reachable by keyboard.
/// </summary>
public sealed class HeadingLevelTests : TestContextBase
{
    public HeadingLevelTests()
    {
        Services.AddSingleton(new Mock<IMediator>().Object);
    }

    [Fact]
    public void CyAlert_Title_Defaults_To_An_H6()
    {
        var cut = Render<CyAlert>(p => p.Add(a => a.Title, "Heads up"));

        cut.Find(".cy-alert__title").TagName.ShouldBe("H6");
    }

    [Theory]
    [InlineData(2, "H2")]
    [InlineData(3, "H3")]
    [InlineData(1, "H1")]
    public void CyAlert_TitleLevel_Chooses_The_Heading_Element(int level, string expected)
    {
        var cut = Render<CyAlert>(p => p.Add(a => a.Title, "Heads up").Add(a => a.TitleLevel, level));

        var title = cut.Find(".cy-alert__title");
        title.TagName.ShouldBe(expected);
        title.ClassList.ShouldContain("cy-typography--h6"); // visual size is unchanged
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void CyAlert_TitleLevel_Outside_1_To_6_Throws(int level)
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyAlert>(p => p.Add(a => a.Title, "Heads up").Add(a => a.TitleLevel, level)));
    }

    private IRenderedComponent<CyAccordion> RenderAccordion(int? level)
        => Render<CyAccordion>(p => p.AddChildContent<CyAccordionItem>(i =>
        {
            i.Add(x => x.ItemId, "a").Add(x => x.Title, "Item");

            if (level is not null)
            {
                i.Add(x => x.HeadingLevel, level.Value);
            }
        }));

    [Fact]
    public void CyAccordionItem_Heading_Is_An_H3_With_No_AriaLevel_By_Default()
    {
        var heading = RenderAccordion(null).Find(".cy-accordion-item__heading");

        heading.TagName.ShouldBe("H3");
        heading.HasAttribute("aria-level").ShouldBeFalse();
    }

    [Fact]
    public void CyAccordionItem_HeadingLevel_Is_Announced_Through_AriaLevel()
    {
        RenderAccordion(2).Find(".cy-accordion-item__heading").GetAttribute("aria-level").ShouldBe("2");
    }

    [Fact]
    public void CyAccordionItem_HeadingLevel_Outside_1_To_6_Throws()
    {
        Should.Throw<InvalidOperationException>(() => RenderAccordion(7));
    }

    [Fact]
    public void CyCodeBlock_Scrollable_Pre_Is_Keyboard_Focusable()
    {
        var cut = Render<CyCodeBlock>(p => p.Add(c => c.Code, "var x = 1;"));

        cut.Find("pre.cy-code-block__pre").GetAttribute("tabindex").ShouldBe("0");
    }
}
