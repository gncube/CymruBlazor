using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyWarningCalloutTests : TestContextBase
{
    [Fact]
    public void Should_Render_Semantic_Callout_With_Alert_Role_And_Default_Heading()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.Heading, "School, nursery or work")
            .AddChildContent("<p>Stay away from school until 48 hours after symptoms stop.</p>"));

        var callout = cut.Find("div.cy-warning-callout");
        callout.ShouldNotBeNull();
        callout.GetAttribute("role").ShouldBe("alert");

        var label = cut.Find(".cy-warning-callout__label");
        label.TagName.ToLowerInvariant().ShouldBe("h3");

        var hidden = cut.Find(".cy-warning-callout__label .cy-visually-hidden");
        hidden.TextContent.Trim().ShouldBe("Important:");

        var roleText = cut.Find(".cy-warning-callout__label span[role='text']");
        roleText.ShouldNotBeNull();
        roleText.TextContent.ShouldContain("School, nursery or work");

        var body = cut.Find(".cy-warning-callout__body");
        body.InnerHtml.ShouldContain("<p>Stay away from school until 48 hours after symptoms stop.</p>");
    }

    [Fact]
    public void Should_Fallback_To_Default_Heading_When_Heading_Is_Omitted()
    {
        var cut = Render<CyWarningCallout>(p => p
            .AddChildContent("Critical instruction"));

        var label = cut.Find(".cy-warning-callout__label");
        label.TextContent.ShouldContain("Important");
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    public void Should_Render_Appropriate_Heading_Level(int level, string expectedTag)
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.HeadingLevel, level)
            .Add(c => c.Heading, "Custom Level")
            .AddChildContent("Content"));

        var label = cut.Find(".cy-warning-callout__label");
        label.TagName.ToLowerInvariant().ShouldBe(expectedTag);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(-1)]
    public void Should_Throw_When_HeadingLevel_Is_Out_Of_Range(int invalidLevel)
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            Render<CyWarningCallout>(p => p
                .Add(c => c.HeadingLevel, invalidLevel)
                .AddChildContent("Content"));
        });
    }

    [Fact]
    public void Should_Support_Custom_HeadingTemplate()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.HeadingTemplate, (RenderFragment)(b =>
            {
                b.OpenElement(0, "em");
                b.AddContent(1, "Urgent Notice");
                b.CloseElement();
            }))
            .AddChildContent("Content"));

        var em = cut.Find(".cy-warning-callout__label em");
        em.TextContent.ShouldBe("Urgent Notice");
    }

    [Fact]
    public void Should_Suppress_Heading_When_ShowHeading_Is_False()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.ShowHeading, false)
            .Add(c => c.Heading, "Ignored Heading")
            .AddChildContent("Content without heading"));

        cut.FindAll(".cy-warning-callout__label").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Suppress_VisuallyHidden_Prefix_When_Disabled()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.ShowVisuallyHiddenPrefix, false)
            .Add(c => c.Heading, "Direct Notice")
            .AddChildContent("Content"));

        cut.FindAll(".cy-visually-hidden").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Support_Custom_VisuallyHidden_Prefix()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.VisuallyHiddenPrefix, "Warning: ")
            .Add(c => c.Heading, "Allergy Alert")
            .AddChildContent("Content"));

        var hidden = cut.Find(".cy-visually-hidden");
        hidden.TextContent.Trim().ShouldBe("Warning:");
    }

    [Fact]
    public void Should_Support_Welsh_Localization_Via_Text_Record()
    {
        var welshText = new CyWarningCalloutText
        {
            VisuallyHiddenPrefix = "Pwysig: ",
            DefaultHeading = "Pwysig"
        };

        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.Text, welshText)
            .AddChildContent("Cynnwys Cymraeg"));

        var hidden = cut.Find(".cy-visually-hidden");
        hidden.TextContent.Trim().ShouldBe("Pwysig:");
        cut.Find(".cy-warning-callout__label").TextContent.ShouldContain("Pwysig");
    }

    [Fact]
    public void Should_Support_Custom_Role()
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.Role, "note")
            .Add(c => c.Heading, "Note Callout")
            .AddChildContent("Content"));

        cut.Find("div.cy-warning-callout").GetAttribute("role").ShouldBe("note");
    }

    [Theory]
    [InlineData(ComponentColour.Warning, "cy-warning-callout--warning")]
    [InlineData(ComponentColour.Danger, "cy-warning-callout--danger")]
    [InlineData(ComponentColour.Info, "cy-warning-callout--info")]
    [InlineData(ComponentColour.Primary, "cy-warning-callout--primary")]
    [InlineData(ComponentColour.Success, "cy-warning-callout--success")]
    public void Should_Apply_Severity_Modifier_Classes(ComponentColour severity, string expectedClass)
    {
        var cut = Render<CyWarningCallout>(p => p
            .Add(c => c.Severity, severity)
            .Add(c => c.Heading, "Severity Heading")
            .AddChildContent("Content"));

        var div = cut.Find("div.cy-warning-callout");
        div.ClassList.ShouldContain(expectedClass);
    }
}
