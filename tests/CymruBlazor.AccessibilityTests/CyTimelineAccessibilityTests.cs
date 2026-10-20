using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyTimelineAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_In_Light_Theme()
    {
        var result = await ScanComponentAsync<CyTimeline>(parameters => parameters
            .Add(t => t.Label, "Specimen processing journey")
            .AddChildContent(b =>
            {
                b.OpenComponent<CyTimelineItem>(0);
                b.AddComponentParameter(1, nameof(CyTimelineItem.Title), "Specimen Collected");
                b.AddComponentParameter(2, nameof(CyTimelineItem.Time), "08:45, 20 Oct 2026");
                b.AddComponentParameter(3, nameof(CyTimelineItem.DateTime), new DateTimeOffset(2026, 10, 20, 8, 45, 0, TimeSpan.Zero));
                b.AddComponentParameter(4, nameof(CyTimelineItem.Colour), ComponentColour.Success);
                b.AddComponentParameter(5, nameof(CyTimelineItem.Icon), "check");
                b.AddComponentParameter(6, nameof(CyTimelineItem.BadgeText), "Collected");
                b.AddComponentParameter(7, nameof(CyTimelineItem.BadgeColour), ComponentColour.Success);
                b.AddComponentParameter(8, nameof(CyTimelineItem.Actor), "Phlebotomy Clinic");
                b.AddComponentParameter(9, nameof(CyTimelineItem.Description), "Venous blood sample collected.");
                b.CloseComponent();

                b.OpenComponent<CyTimelineItem>(10);
                b.AddComponentParameter(11, nameof(CyTimelineItem.Title), "Lab Processing");
                b.AddComponentParameter(12, nameof(CyTimelineItem.Time), "10:15, 20 Oct 2026");
                b.AddComponentParameter(13, nameof(CyTimelineItem.Colour), ComponentColour.Primary);
                b.AddComponentParameter(14, nameof(CyTimelineItem.Icon), "history");
                b.AddComponentParameter(15, nameof(CyTimelineItem.BadgeText), "In Progress");
                b.AddComponentParameter(16, nameof(CyTimelineItem.Description), "Sequencing in progress.");
                b.CloseComponent();

                b.OpenComponent<CyTimelineItem>(20);
                b.AddComponentParameter(21, nameof(CyTimelineItem.Title), "Quality Review");
                b.AddComponentParameter(22, nameof(CyTimelineItem.Time), "Estimated 21 Oct");
                b.AddComponentParameter(23, nameof(CyTimelineItem.Colour), ComponentColour.Neutral);
                b.AddComponentParameter(24, nameof(CyTimelineItem.IsLast), true);
                b.CloseComponent();
            }));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_Across_Themes(string theme)
    {
        var items = new List<CyTimelineItemModel>
        {
            new()
            {
                Title = "Order Received",
                Time = "09:00, 20 Oct 2026",
                Colour = ComponentColour.Success,
                Icon = "check",
                BadgeText = "Completed",
                BadgeColour = ComponentColour.Success,
                Description = "Order logged into the system."
            },
            new()
            {
                Title = "Analysing Samples",
                Time = "11:30, 20 Oct 2026",
                Colour = ComponentColour.Info,
                Icon = "history",
                BadgeText = "Active",
                BadgeColour = ComponentColour.Info,
                Actor = "LIMS Automation",
                Description = "Bioinformatic pipeline running."
            },
            new()
            {
                Title = "Clinical Sign-off",
                Time = "Pending",
                Colour = ComponentColour.Neutral,
                BadgeText = "Queued",
                Description = "Awaiting pathologist review."
            }
        };

        var markup = Render<CyTimeline>(parameters => parameters
            .Add(t => t.Label, "Diagnostic order tracking")
            .Add(t => t.Items, items)).Markup;

        var result = await ScanMarkupAsync(markup, theme);
        result.Violations.ShouldBeEmpty();
    }
}
