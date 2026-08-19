using CymruBlazor.Enums;
using CymruBlazor.Icons;
using CymruBlazor.Samples.Dashboard.Models;

namespace CymruBlazor.Samples.Dashboard.Services;

/// <summary>
/// Provides deterministic, read-only demonstration data for the dashboard sample.
/// A real application would replace this with an injected HTTP/API-backed service;
/// intentionally kept this simple per PROMPT.md §11/§12 (no speculative infrastructure).
/// </summary>
public sealed class DashboardSampleDataService
{
    public IReadOnlyList<KpiMetric> GetKpiMetrics() =>
    [
        new("Today's Appointments", "128", CyIconName.UserPlus, ComponentColour.Info, Trend.Up, "+8 vs yesterday"),
        new("Referrals Received", "34", CyIconName.FileText, ComponentColour.Success, Trend.Up, "+12% vs last week"),
        new("Watchlist Patients", "6", CyIconName.Watchlist, ComponentColour.Warning, Trend.Flat, "No change"),
        new("Pending DALs", "11", CyIconName.ArrowUpDown, ComponentColour.Danger, Trend.Down, "-3 vs yesterday")
    ];

    public IReadOnlyList<AppointmentSummary> GetTodaysAppointments() =>
    [
        new("J.E.", "09:00", "Dr. A. Rhys", "Follow-up", "Checked in", ComponentColour.Success),
        new("M.K.", "09:20", "Dr. S. Owen", "New patient", "Waiting", ComponentColour.Info),
        new("R.T.", "09:40", "Dr. A. Rhys", "Follow-up", "Delayed", ComponentColour.Warning),
        new("L.P.", "10:00", "Dr. C. Bevan", "Review", "Waiting", ComponentColour.Info),
        new("D.H.", "10:20", "Dr. S. Owen", "New patient", "Cancelled", ComponentColour.Danger),
        new("N.W.", "10:40", "Dr. C. Bevan", "Follow-up", "Checked in", ComponentColour.Success)
    ];

    public IReadOnlyList<OccupancyArea> GetWardOccupancy() =>
    [
        new("Medical Ward A", 42, 48, ComponentColour.Warning),
        new("Surgical Ward B", 30, 40, ComponentColour.Info),
        new("Paediatric Ward", 12, 24, ComponentColour.Success),
        new("Critical Care", 18, 20, ComponentColour.Danger)
    ];

    public IReadOnlyList<WaitingListSpecialty> GetWaitingList() =>
    [
        new("Orthopaedics", 214, 18, ComponentColour.Danger),
        new("Cardiology", 132, 4, ComponentColour.Warning),
        new("Dermatology", 87, 0, ComponentColour.Success),
        new("Ophthalmology", 156, 9, ComponentColour.Warning),
        new("ENT", 64, 1, ComponentColour.Success)
    ];
}
