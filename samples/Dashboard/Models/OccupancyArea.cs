using CymruBlazor.Enums;

namespace CymruBlazor.Samples.Dashboard.Models;

/// <summary>A single ward's bed occupancy, shown as a progress bar in the occupancy widget.</summary>
public sealed record OccupancyArea(
    string WardName,
    int OccupiedBeds,
    int TotalBeds,
    ComponentColour Severity);
