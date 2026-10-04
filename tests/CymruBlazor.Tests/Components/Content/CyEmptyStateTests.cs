using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Content;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyEmptyStateTests : TestContextBase
{
    [Fact]
    public void Should_Render_The_Title_As_A_Heading_Level_2_By_Default()
    {
        var cut = Render<CyEmptyState>(p => p.Add(c => c.Title, "No questions yet"));

        var heading = cut.Find("h2.cy-empty-state__title");
        heading.TextContent.Trim().ShouldBe("No questions yet");
    }

    [Fact]
    public void Should_Honour_HeadingLevel()
    {
        var cut = Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.HeadingLevel, 3));

        cut.Find("h3.cy-empty-state__title").ShouldNotBeNull();
        cut.FindAll("h2").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Should_Reject_An_Invalid_HeadingLevel(int level)
    {
        Should.Throw<InvalidOperationException>(() => Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.HeadingLevel, level)));
    }

    [Fact]
    public void Should_Reject_An_Empty_Title()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyEmptyState>(p => p.Add(c => c.Title, " ")));
    }

    [Fact]
    public void Should_Render_The_Description_As_Escaped_Text()
    {
        var cut = Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.Description, "<img src=x onerror=alert(1)> Add one"));

        var description = cut.Find(".cy-empty-state__description");
        description.QuerySelector("img").ShouldBeNull();
        description.TextContent.ShouldContain("<img src=x onerror=alert(1)>");
    }

    [Fact]
    public void Should_Render_A_Named_Icon_Hidden_From_Assistive_Technology()
    {
        var cut = Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.Icon, "layers"));

        var icon = cut.Find(".cy-empty-state__icon");
        icon.GetAttribute("aria-hidden").ShouldBe("true");
        icon.QuerySelector("svg").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Prefer_IconContent_Over_The_Icon_Name()
    {
        var cut = Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.Icon, "layers")
            .Add(c => c.IconContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"custom\">x</span>"))));

        cut.Find("#custom").ShouldNotBeNull();
        cut.FindAll(".cy-empty-state__icon svg").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Actions_In_Their_Own_Container_Only_When_Provided()
    {
        Render<CyEmptyState>(p => p.Add(c => c.Title, "Empty"))
            .FindAll(".cy-empty-state__actions").Count.ShouldBe(0);

        var cut = Render<CyEmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.Actions, (RenderFragment)(b =>
            {
                b.OpenElement(0, "button");
                b.AddContent(1, "Add question");
                b.CloseElement();
            })));

        cut.Find(".cy-empty-state__actions button").TextContent.ShouldBe("Add question");
    }
}
