using CymruBlazor.Components.Status;

namespace CymruBlazor.Samples.Dashboard.Models;

/// <summary>A single specialty row in the waiting-list widget.</summary>
public sealed record WaitingListSpecialty(
    string Name,
    int PatientsWaiting,
    int BreachRiskCount,
    CymruSeverity Severity);
