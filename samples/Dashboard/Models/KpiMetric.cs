using CymruBlazor.Components.Content;
using CymruBlazor.Components.Status;
using CymruBlazor.Icons;

namespace CymruBlazor.Samples.Dashboard.Models;

/// <summary>A single summary metric shown at the top of the dashboard.</summary>
public sealed record KpiMetric(
    string Label,
    string Value,
    CymruIconName Icon,
    CymruSeverity Severity,
    CymruTrend Trend,
    string DeltaText);
