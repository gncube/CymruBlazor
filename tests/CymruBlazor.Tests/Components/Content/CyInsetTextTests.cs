using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyInsetTextTests : TestContextBase
{
    [Fact]
    public void Should_Render_Semantic_Div_With_Default_Class_And_Role()
    {
        var cut = Render<CyInsetText>(p => p
            .AddChildContent("<p>It can take up to 3 working days to process your request.</p>"));

        var div = cut.Find("div.cy-inset-text");
        div.ShouldNotBeNull();
        div.GetAttribute("role").ShouldBe("note");
        div.InnerHtml.ShouldContain("<p>It can take up to 3 working days to process your request.</p>");
    }

    [Fact]
    public void Should_Render_Default_VisuallyHidden_Prefix()
    {
        var cut = Render<CyInsetText>(p => p
            .AddChildContent("Important information"));

        var hidden = cut.Find(".cy-visually-hidden");
        hidden.ShouldNotBeNull();
        hidden.TextContent.Trim().ShouldBe("Information:");
    }

    [Fact]
    public void Should_Support_Custom_VisuallyHidden_Text()
    {
        var cut = Render<CyInsetText>(p => p
            .Add(c => c.VisuallyHiddenText, "Notice: ")
            .AddChildContent("Custom note"));

        var hidden = cut.Find(".cy-visually-hidden");
        hidden.TextContent.Trim().ShouldBe("Notice:");
    }

    [Fact]
    public void Should_Suppress_VisuallyHidden_Span_When_Disabled()
    {
        var cut = Render<CyInsetText>(p => p
            .Add(c => c.ShowVisuallyHiddenText, false)
            .AddChildContent("Text without prefix"));

        cut.FindAll(".cy-visually-hidden").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Support_Custom_Role_Or_Null()
    {
        var cutWithCustomRole = Render<CyInsetText>(p => p
            .Add(c => c.Role, "region")
            .AddChildContent("Content"));

        cutWithCustomRole.Find("div.cy-inset-text").GetAttribute("role").ShouldBe("region");

        var cutWithNullRole = Render<CyInsetText>(p => p
            .Add(c => c.Role, null)
            .AddChildContent("Content"));

        cutWithNullRole.Find("div.cy-inset-text").HasAttribute("role").ShouldBeFalse();
    }

    [Fact]
    public void Should_Support_Welsh_Localization_Via_Text_Record()
    {
        var welshText = new CyInsetTextText
        {
            VisuallyHiddenText = "Gwybodaeth: "
        };

        var cut = Render<CyInsetText>(p => p
            .Add(c => c.Text, welshText)
            .AddChildContent("Cynnwys Cymraeg"));

        var hidden = cut.Find(".cy-visually-hidden");
        hidden.TextContent.Trim().ShouldBe("Gwybodaeth:");
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-inset-text--primary")]
    [InlineData(ComponentColour.Info, "cy-inset-text--info")]
    [InlineData(ComponentColour.Warning, "cy-inset-text--warning")]
    [InlineData(ComponentColour.Danger, "cy-inset-text--danger")]
    [InlineData(ComponentColour.Success, "cy-inset-text--success")]
    public void Should_Apply_Colour_Modifier_Classes(ComponentColour colour, string expectedClass)
    {
        var cut = Render<CyInsetText>(p => p
            .Add(c => c.Colour, colour)
            .AddChildContent("Content"));

        var div = cut.Find("div.cy-inset-text");
        div.ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Merge_Custom_Class_And_Pass_Through_Attributes()
    {
        var cut = Render<CyInsetText>(p => p
            .Add(c => c.Class, "my-custom-callout")
            .Add(c => c.Style, "margin-top: 10px;")
            .Add(c => c.Id, "custom-id")
            .AddUnmatched("data-testid", "inset-test")
            .AddChildContent("Content"));

        var div = cut.Find("#custom-id");
        div.ClassList.ShouldContain("cy-inset-text");
        div.ClassList.ShouldContain("my-custom-callout");
        div.GetAttribute("style")!.ShouldContain("margin-top: 10px;");
        div.GetAttribute("data-testid").ShouldBe("inset-test");
    }
}
