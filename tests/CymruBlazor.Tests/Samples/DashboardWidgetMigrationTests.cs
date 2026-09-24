using Bunit;
using Shouldly;
using Xunit;
using CymruBlazor.Enums;
using CymruBlazor.Samples.Dashboard.Components;
using CymruBlazor.Samples.Dashboard.Models;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// Regression coverage for the roadmap v1.5.0 migration of
/// <c>samples/Dashboard</c> off its hand-rolled table, progress bars and
/// status pills, onto <c>CyTable</c>, <c>CyProgress</c> and
/// <c>CyBadge</c> respectively (see <c>plan/plan-post-1.2-roadmap.md</c>,
/// finding F7 and the v1.5.0 acceptance criterion). This is the
/// regression harness the roadmap promised: rendering the real widgets
/// with real sample data, rather than only unit-testing the library
/// components in isolation.
/// </summary>
public sealed class DashboardWidgetMigrationTests : TestContextBase
{
    public DashboardWidgetMigrationTests()
    {
        // Every widget under test renders a CyTooltip in its header, which
        // performs JS interop (positioning) on render - see CyTooltipTests
        // for the same, established pattern.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static readonly IReadOnlyList<WaitingListSpecialty> Specialties =
    [
        new("Orthopaedics", 214, 18, ComponentColour.Danger),
        new("Dermatology", 87, 0, ComponentColour.Success)
    ];

    private static readonly IReadOnlyList<OccupancyArea> Areas =
    [
        new("Medical Ward A", 42, 48, ComponentColour.Warning),
        new("Critical Care", 18, 20, ComponentColour.Danger)
    ];

    private static readonly IReadOnlyList<AppointmentSummary> Appointments =
    [
        new("J.E.", "09:00", "Dr. A. Rhys", "Follow-up", "Checked in", ComponentColour.Success)
    ];

    [Fact]
    public void WaitingListWidget_Should_Render_A_Real_Table_With_No_HandRolled_Markup()
    {
        // Act
        var cut = Render<WaitingListWidget>(parameters => parameters
            .Add(p => p.Specialties, Specialties));

        // Assert - a real CyTable, not the old hand-rolled <table class="dashboard-waiting-table">.
        cut.FindAll("table.dashboard-waiting-table").Count.ShouldBe(0);
        cut.Find("table.cy-table").ShouldNotBeNull();
        cut.Find("div.cy-table__scroll").ShouldNotBeNull();

        // The "at risk" / "None" pills are CyBadge, not hand-rolled dashboard-status spans.
        cut.FindAll("span.dashboard-status").Count.ShouldBe(0);
        cut.FindAll(".cy-badge").Count.ShouldBe(Specialties.Count);
        cut.Markup.ShouldContain("18 at risk");
        cut.Markup.ShouldContain("None");
    }

    [Fact]
    public void OccupancyWidget_Should_Render_CyProgress_With_No_HandRolled_Bars()
    {
        // Act
        var cut = Render<OccupancyWidget>(parameters => parameters
            .Add(p => p.Areas, Areas));

        // Assert - real <progress> elements, not the old hand-rolled role="group" bars.
        cut.FindAll("div[role='group']").Count.ShouldBe(0);
        cut.FindAll("progress.cy-progress__bar").Count.ShouldBe(Areas.Count);
        cut.Markup.ShouldContain("42 / 48 beds");
        cut.Markup.ShouldContain("18 / 20 beds");
    }

    [Fact]
    public void OccupancyWidget_Should_Not_Throw_When_A_Ward_Has_Zero_Total_Beds()
    {
        // Arrange - the old width calculation guarded division by zero;
        // CyProgress.Max must not be given zero either (see CyProgress.ValidateParameters).
        IReadOnlyList<OccupancyArea> areasWithEmptyWard =
        [
            new("Closed Ward", 0, 0, ComponentColour.Success)
        ];

        // Act & Assert
        Should.NotThrow(() =>
            Render<OccupancyWidget>(parameters => parameters
                .Add(p => p.Areas, areasWithEmptyWard)));
    }

    [Fact]
    public void AppointmentsWidget_Should_Render_CyBadge_For_Status_Instead_Of_A_HandRolled_Pill()
    {
        // Act
        var cut = Render<AppointmentsWidget>(parameters => parameters
            .Add(p => p.Appointments, Appointments));

        // Assert
        cut.FindAll("span.dashboard-status").Count.ShouldBe(0);
        cut.Find(".cy-badge").ShouldNotBeNull();
        cut.Markup.ShouldContain("Checked in");
    }
}
