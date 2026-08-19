using CymruBlazor.Components.Content;
using CymruBlazor.Components.Status;
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
        new("Today's Appointments", "128", CymruIconName.UserPlus, CymruSeverity.Info, CymruTrend.Up, "+8 vs yesterday"),
        new("Referrals Received", "34", CymruIconName.FileText, CymruSeverity.Success, CymruTrend.Up, "+12% vs last week"),
        new("Watchlist Patients", "6", CymruIconName.Watchlist, CymruSeverity.Warning, CymruTrend.Flat, "No change"),
        new("Pending DALs", "11", CymruIconName.ArrowUpDown, CymruSeverity.Critical, CymruTrend.Down, "-3 vs yesterday")
    ];

    public IReadOnlyList<AppointmentSummary> GetTodaysAppointments() =>
    [
        new("J.E.", "09:00", "Dr. A. Rhys", "Follow-up", "Checked in", CymruSeverity.Success),
        new("M.K.", "09:20", "Dr. S. Owen", "New patient", "Waiting", CymruSeverity.Info),
        new("R.T.", "09:40", "Dr. A. Rhys", "Follow-up", "Delayed", CymruSeverity.Warning),
        new("L.P.", "10:00", "Dr. C. Bevan", "Review", "Waiting", CymruSeverity.Info),
        new("D.H.", "10:20", "Dr. S. Owen", "New patient", "Cancelled", CymruSeverity.Critical),
        new("N.W.", "10:40", "Dr. C. Bevan", "Follow-up", "Checked in", CymruSeverity.Success)
    ];

    public IReadOnlyList<OccupancyArea> GetWardOccupancy() =>
    [
        new("Medical Ward A", 42, 48, CymruSeverity.Warning),
        new("Surgical Ward B", 30, 40, CymruSeverity.Info),
        new("Paediatric Ward", 12, 24, CymruSeverity.Success),
        new("Critical Care", 18, 20, CymruSeverity.Critical)
    ];

    public IReadOnlyList<WaitingListSpecialty> GetWaitingList() =>
    [
        new("Orthopaedics", 214, 18, CySeverity.Critical),
        new("Cardiology", 132, 4, CymruSeverity.Warning),
        new("Dermatology", 87, 0, CymruSeverity.Success),
        new("Ophthalmology", 156, 9, CymruSeverity.Warning),
        new("ENT", 64, 1, CymruSeverity.Success)
    ];
}
