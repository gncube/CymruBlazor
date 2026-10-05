using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Feedback;

namespace CymruBlazor.Tests.Feedback;

public sealed class CySkeletonTests : TestContextBase
{
    [Fact]
    public void Should_Render_One_Text_Bar_Hidden_From_Assistive_Technology_By_Default()
    {
        var cut = Render<CySkeleton>();

        cut.Find(".cy-skeleton").ClassList.ShouldContain("cy-skeleton--text");
        cut.FindAll(".cy-skeleton__line").Count.ShouldBe(1);
        cut.Find(".cy-skeleton__shapes").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Should_Announce_Loading_Once_As_Visually_Hidden_Text()
    {
        var cut = Render<CySkeleton>(p => p.Add(c => c.Lines, 3));

        var hidden = cut.FindAll(".u-sr-only");
        hidden.Count.ShouldBe(1);
        hidden[0].TextContent.Trim().ShouldBe("Loading");
    }

    [Fact]
    public void Should_Use_A_Custom_Label()
    {
        var cut = Render<CySkeleton>(p => p.Add(c => c.Label, "Loading questionnaires"));

        cut.Find(".u-sr-only").TextContent.Trim().ShouldBe("Loading questionnaires");
    }

    [Fact]
    public void Should_Not_Announce_When_Announce_Is_False()
    {
        var cut = Render<CySkeleton>(p => p.Add(c => c.Announce, false));

        cut.FindAll(".u-sr-only").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Several_Lines_With_A_Shorter_Last_Line()
    {
        var cut = Render<CySkeleton>(p => p.Add(c => c.Lines, 3));

        var lines = cut.FindAll(".cy-skeleton__line");
        lines.Count.ShouldBe(3);
        lines[0].ClassList.ShouldNotContain("cy-skeleton__line--last");
        lines[2].ClassList.ShouldContain("cy-skeleton__line--last");
    }

    [Theory]
    [InlineData(CySkeletonShape.Rectangle, "cy-skeleton--rectangle")]
    [InlineData(CySkeletonShape.Circle, "cy-skeleton--circle")]
    public void Should_Render_A_Single_Block_For_Non_Text_Shapes(CySkeletonShape shape, string expectedClass)
    {
        var cut = Render<CySkeleton>(p => p.Add(c => c.Shape, shape));

        cut.Find(".cy-skeleton").ClassList.ShouldContain(expectedClass);
        cut.FindAll(".cy-skeleton__block").Count.ShouldBe(1);
        cut.FindAll(".cy-skeleton__line").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Write_Valid_Lengths_As_Custom_Properties()
    {
        var cut = Render<CySkeleton>(p => p
            .Add(c => c.Shape, CySkeletonShape.Rectangle)
            .Add(c => c.Width, "12rem")
            .Add(c => c.Height, "40%"));

        var style = cut.Find(".cy-skeleton").GetAttribute("style")!;
        style.ShouldContain("--cy-skeleton-width");
        style.ShouldContain("12rem");
        style.ShouldContain("--cy-skeleton-height");
        style.ShouldContain("40%");
    }

    [Theory]
    [InlineData("calc(1px + 2px)")]
    [InlineData("10px; color: red")]
    [InlineData("url(javascript:alert(1))")]
    [InlineData("auto")]
    [InlineData("-5px")]
    public void Should_Reject_A_Width_That_Is_Not_A_Plain_Length(string width)
    {
        Should.Throw<InvalidOperationException>(() => Render<CySkeleton>(p => p.Add(c => c.Width, width)));
    }

    [Fact]
    public void Should_Reject_An_Invalid_Height()
    {
        Should.Throw<InvalidOperationException>(() => Render<CySkeleton>(p => p.Add(c => c.Height, "1;2")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Should_Reject_Lines_Out_Of_Range(int lines)
    {
        Should.Throw<InvalidOperationException>(() => Render<CySkeleton>(p => p.Add(c => c.Lines, lines)));
    }

    [Fact]
    public void Should_Only_Animate_When_Animated_Is_True()
    {
        Render<CySkeleton>().Find(".cy-skeleton").ClassList.ShouldContain("cy-skeleton--animated");

        Render<CySkeleton>(p => p.Add(c => c.Animated, false))
            .Find(".cy-skeleton").ClassList.ShouldNotContain("cy-skeleton--animated");
    }
}
