using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyStatCardTests : TestContextBase
{
    private IRenderedComponent<CyStatCard> RenderCard(Action<ComponentParameterCollectionBuilder<CyStatCard>>? configure = null) =>
        Render<CyStatCard>(p =>
        {
            p.Add(c => c.Label, "Median wait");
            p.Add(c => c.Value, "14");
            configure?.Invoke(p);
        });

    [Fact]
    public void Should_Render_Label_Before_Value()
    {
        var cut = RenderCard(p => p.Add(c => c.Unit, "days"));

        cut.Markup.IndexOf("Median wait", StringComparison.Ordinal)
            .ShouldBeLessThan(cut.Markup.IndexOf("cy-stat-card__value", StringComparison.Ordinal));
        cut.Find(".cy-stat-card__number").TextContent.ShouldBe("14");
        cut.Find(".cy-stat-card__unit").TextContent.ShouldBe("days");
    }

    [Fact]
    public void Should_Render_The_Label_As_A_Paragraph_By_Default()
    {
        var cut = RenderCard();

        cut.Find("p.cy-stat-card__label").TextContent.ShouldBe("Median wait");
        cut.FindAll("h1,h2,h3,h4,h5,h6").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_The_Label_As_A_Heading_When_HeadingLevel_Is_Set()
    {
        var cut = RenderCard(p => p.Add(c => c.HeadingLevel, 3));

        cut.Find("h3.cy-stat-card__label").TextContent.Trim().ShouldBe("Median wait");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Should_Reject_An_Invalid_HeadingLevel(int level)
    {
        Should.Throw<InvalidOperationException>(() => RenderCard(p => p.Add(c => c.HeadingLevel, level)));
    }

    [Fact]
    public void Should_Reject_An_Empty_Label()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyStatCard>(p => p.Add(c => c.Label, " ")));
    }

    [Theory]
    [InlineData(CyTrend.Up, "Increase:")]
    [InlineData(CyTrend.Down, "Decrease:")]
    [InlineData(CyTrend.Flat, "No change:")]
    public void Should_Prefix_The_Trend_Text_For_Assistive_Technology(CyTrend trend, string prefix)
    {
        var cut = RenderCard(p => p
            .Add(c => c.Trend, trend)
            .Add(c => c.TrendText, "4% on last month"));

        var element = cut.Find(".cy-stat-card__trend");
        element.QuerySelector(".u-sr-only")!.TextContent.Trim().ShouldBe(prefix);
        element.TextContent.ShouldContain("4% on last month");
    }

    [Fact]
    public void Should_Hide_The_Trend_Arrow_From_Assistive_Technology()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Trend, CyTrend.Up)
            .Add(c => c.TrendText, "More"));

        cut.Find(".cy-stat-card__trend-icon").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Should_Not_Render_An_Arrow_Without_Words()
    {
        var cut = RenderCard(p => p.Add(c => c.Trend, CyTrend.Up));

        cut.FindAll(".cy-stat-card__trend").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(ComponentColour.Success, "cy-stat-card__trend--good")]
    [InlineData(ComponentColour.Danger, "cy-stat-card__trend--bad")]
    [InlineData(ComponentColour.Neutral, "cy-stat-card__trend--neutral")]
    public void Should_Tone_The_Trend_Without_Changing_Its_Words(ComponentColour tone, string expected)
    {
        var cut = RenderCard(p => p
            .Add(c => c.Trend, CyTrend.Down)
            .Add(c => c.TrendText, "Fewer")
            .Add(c => c.TrendTone, tone));

        cut.Find(".cy-stat-card__trend").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Should_Use_Localised_Trend_Prefixes()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Trend, CyTrend.Up)
            .Add(c => c.TrendText, "4% yn fwy")
            .Add(c => c.Text, new CyStatCardText { Increase = "Cynnydd:" }));

        cut.Find(".cy-stat-card__trend .u-sr-only").TextContent.Trim().ShouldBe("Cynnydd:");
    }

    [Fact]
    public void Should_Render_A_Link_When_Href_Is_Set()
    {
        var cut = RenderCard(p => p.Add(c => c.Href, "/reports/waits"));

        var link = cut.Find("a.cy-stat-card");
        link.GetAttribute("href").ShouldBe("/reports/waits");
        link.ClassList.ShouldContain("cy-stat-card--link");
    }

    [Fact]
    public void Should_Render_A_Div_Without_Href()
    {
        var cut = RenderCard();

        cut.FindAll("a").Count.ShouldBe(0);
        cut.Find("div.cy-stat-card").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Show_A_Skeleton_And_Mark_The_Card_Busy_While_Loading()
    {
        var cut = RenderCard(p => p.Add(c => c.Loading, true));

        cut.Find(".cy-stat-card").GetAttribute("aria-busy").ShouldBe("true");
        cut.FindAll(".cy-skeleton").Count.ShouldBe(1);
        cut.FindAll(".cy-stat-card__number").Count.ShouldBe(0);
        cut.Find(".cy-stat-card > .u-sr-only").TextContent.Trim().ShouldBe("Loading");
    }

    [Fact]
    public void Should_Not_Be_Busy_When_Not_Loading()
    {
        RenderCard().Find(".cy-stat-card").HasAttribute("aria-busy").ShouldBeFalse();
    }

    [Fact]
    public void Should_Encode_Label_Value_And_Description()
    {
        var cut = Render<CyStatCard>(p => p
            .Add(c => c.Label, "<b>L</b>")
            .Add(c => c.Value, "<i>1</i>")
            .Add(c => c.Description, "<script>x</script>"));

        cut.FindAll("b,i,script").Count.ShouldBe(0);
        cut.Markup.ShouldContain("&lt;b&gt;L&lt;/b&gt;");
    }

    [Fact]
    public void Should_Render_A_Decorative_Icon()
    {
        var cut = RenderCard(p => p.Add(c => c.Icon, "chart"));

        cut.Find(".cy-stat-card__icon").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Should_Add_An_Accent_Class_For_A_Colour()
    {
        RenderCard(p => p.Add(c => c.Colour, ComponentColour.Danger))
            .Find(".cy-stat-card").ClassList.ShouldContain("cy-stat-card--danger");
    }
}
