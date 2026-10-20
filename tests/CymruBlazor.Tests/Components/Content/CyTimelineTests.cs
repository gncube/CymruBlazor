using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyTimelineTests : TestContextBase
{
    [Fact]
    public void Should_Render_Ordered_List_With_Default_Aria_Label()
    {
        var cut = Render<CyTimeline>(p => p
            .AddChildContent(b =>
            {
                b.OpenComponent<CyTimelineItem>(0);
                b.AddComponentParameter(1, nameof(CyTimelineItem.Title), "First Step");
                b.CloseComponent();
            }));

        var ol = cut.Find("ol.cy-timeline");
        ol.ShouldNotBeNull();
        ol.GetAttribute("aria-label").ShouldBe("Activity timeline");
        cut.Find("li.cy-timeline__item").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Use_Custom_Label_When_Supplied()
    {
        var cut = Render<CyTimeline>(p => p
            .Add(t => t.Label, "Specimen Tracking")
            .AddChildContent(b =>
            {
                b.OpenComponent<CyTimelineItem>(0);
                b.AddComponentParameter(1, nameof(CyTimelineItem.Title), "Sample Logged");
                b.CloseComponent();
            }));

        var ol = cut.Find("ol.cy-timeline");
        ol.GetAttribute("aria-label").ShouldBe("Specimen Tracking");
    }

    [Fact]
    public void Should_Respect_Explicit_Aria_LabelledBy_In_AdditionalAttributes()
    {
        var cut = Render<CyTimeline>(p => p
            .AddUnmatched("aria-labelledby", "timeline-heading"));

        var ol = cut.Find("ol.cy-timeline");
        ol.GetAttribute("aria-labelledby").ShouldBe("timeline-heading");
        ol.HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void Should_Apply_Density_And_LineStyle_Classes()
    {
        var cut = Render<CyTimeline>(p => p
            .Add(t => t.Density, ComponentSize.Small)
            .Add(t => t.LineStyle, CyTimelineLineStyle.Dashed));

        var ol = cut.Find("ol.cy-timeline");
        ol.ClassList.ShouldContain("cy-timeline--compact");
        ol.ClassList.ShouldContain("cy-timeline--dashed");
    }

    [Fact]
    public void Should_Apply_Dotted_LineStyle_Class()
    {
        var cut = Render<CyTimeline>(p => p
            .Add(t => t.LineStyle, CyTimelineLineStyle.Dotted));

        var ol = cut.Find("ol.cy-timeline");
        ol.ClassList.ShouldContain("cy-timeline--dotted");
    }

    [Fact]
    public void Should_Render_Heading_Levels_Correctly()
    {
        var cutDefault = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Default Heading"));
        cutDefault.Find("h3.cy-timeline__title").TextContent.ShouldBe("Default Heading");

        var cutH2 = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Level 2 Heading")
            .Add(i => i.HeadingLevel, 2));
        cutH2.Find("h2.cy-timeline__title").TextContent.ShouldBe("Level 2 Heading");

        var cutParagraph = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Paragraph Title")
            .Add(i => i.HeadingLevel, null));
        cutParagraph.Find("p.cy-timeline__title").TextContent.ShouldBe("Paragraph Title");
    }

    [Fact]
    public void Should_Render_Time_Element_With_DateTime_Attribute()
    {
        var timestamp = new DateTimeOffset(2026, 10, 20, 14, 30, 0, TimeSpan.Zero);
        var cut = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Sample Processed")
            .Add(i => i.Time, "14:30 UTC")
            .Add(i => i.DateTime, timestamp));

        var timeEl = cut.Find("time.cy-timeline__time");
        timeEl.TextContent.Trim().ShouldBe("14:30 UTC");
        timeEl.GetAttribute("datetime").ShouldBe(timestamp.ToString("o", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Should_Render_Markers_With_Semantic_Colour_And_Icons()
    {
        var cutIcon = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Completed")
            .Add(i => i.Colour, ComponentColour.Success)
            .Add(i => i.Icon, "check"));

        var marker = cutIcon.Find(".cy-timeline__marker");
        marker.ClassList.ShouldContain("cy-timeline__marker--success");
        cutIcon.Find("svg.cy-icon").ShouldNotBeNull();

        var cutDot = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Dot Node")
            .Add(i => i.Colour, ComponentColour.Primary));

        var dotMarker = cutDot.Find(".cy-timeline__marker");
        dotMarker.ClassList.ShouldContain("cy-timeline__marker--primary");
        cutDot.Find(".cy-timeline__dot").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Render_Badge_And_Actor()
    {
        var cut = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Reviewed")
            .Add(i => i.BadgeText, "Verified")
            .Add(i => i.BadgeColour, ComponentColour.Success)
            .Add(i => i.Actor, "Dr. Jane Evans"));

        cut.Find(".cy-badge").TextContent.ShouldContain("Verified");
        cut.Find(".cy-timeline__actor").TextContent.ShouldBe("Dr. Jane Evans");
    }

    [Fact]
    public void Should_Render_Description_And_ChildContent()
    {
        var cutDesc = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Plain Description")
            .Add(i => i.Description, "Sample extracted successfully."));

        cutDesc.Find(".cy-timeline__body p").TextContent.ShouldBe("Sample extracted successfully.");

        var cutChild = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Rich Content")
            .AddChildContent("<span class=\"custom-body\">Rich markup inside body</span>"));

        cutChild.Find(".cy-timeline__body .custom-body").TextContent.ShouldBe("Rich markup inside body");
    }

    [Fact]
    public void Should_Render_Actions_When_Provided()
    {
        var cut = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Action Milestone")
            .Add(i => i.Actions, b => b.AddMarkupContent(0, "<button type=\"button\" class=\"action-btn\">View Details</button>")));

        cut.Find(".cy-timeline__actions button.action-btn").TextContent.ShouldBe("View Details");
    }

    [Fact]
    public void Should_Hide_Stem_When_IsLast_Is_True()
    {
        var cutNotLast = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Middle Item")
            .Add(i => i.IsLast, false));

        cutNotLast.Find(".cy-timeline__stem").ShouldNotBeNull();

        var cutLast = Render<CyTimelineItem>(p => p
            .Add(i => i.Title, "Final Item")
            .Add(i => i.IsLast, true));

        cutLast.FindAll(".cy-timeline__stem").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Render_Data_Bound_Items_Collection()
    {
        var items = new List<CyTimelineItemModel>
        {
            new() { Title = "Event 1", Time = "10:00", Colour = ComponentColour.Success, Icon = "check" },
            new() { Title = "Event 2", Time = "11:00", Colour = ComponentColour.Primary, Description = "Processing" },
            new() { Title = "Event 3", Time = "12:00", Colour = ComponentColour.Neutral }
        };

        var cut = Render<CyTimeline>(p => p
            .Add(t => t.Items, items));

        var renderedItems = cut.FindAll("li.cy-timeline__item");
        renderedItems.Count.ShouldBe(3);

        renderedItems[0].QuerySelector(".cy-timeline__title")!.TextContent.ShouldBe("Event 1");
        renderedItems[0].QuerySelector(".cy-timeline__stem").ShouldNotBeNull();

        renderedItems[1].QuerySelector(".cy-timeline__title")!.TextContent.ShouldBe("Event 2");
        renderedItems[1].QuerySelector(".cy-timeline__body p")!.TextContent.ShouldBe("Processing");
        renderedItems[1].QuerySelector(".cy-timeline__stem").ShouldNotBeNull();

        renderedItems[2].QuerySelector(".cy-timeline__title")!.TextContent.ShouldBe("Event 3");
        // Final item has IsLast = true, so stem is omitted
        renderedItems[2].QuerySelectorAll(".cy-timeline__stem").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Render_Custom_ItemTemplate_When_Supplied()
    {
        var items = new List<CyTimelineItemModel>
        {
            new() { Title = "Custom 1" },
            new() { Title = "Custom 2" }
        };

        var cut = Render<CyTimeline>(p => p
            .Add(t => t.Items, items)
            .Add(t => t.ItemTemplate, item => b =>
            {
                b.OpenElement(0, "li");
                b.AddAttribute(1, "class", "custom-item");
                b.AddContent(2, $"Template: {item.Title}");
                b.CloseElement();
            }));

        var customItems = cut.FindAll("li.custom-item");
        customItems.Count.ShouldBe(2);
        customItems[0].TextContent.ShouldBe("Template: Custom 1");
        customItems[1].TextContent.ShouldBe("Template: Custom 2");
    }
}
