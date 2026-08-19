using CymruBlazor.Enums;

namespace CymruBlazor.Samples.Dashboard.Models;

/// <summary>A single row in the "Today's Appointments" widget.</summary>
public sealed record AppointmentSummary(
    string PatientInitials,
    string Time,
    string Clinician,
    string Type,
    string StatusText,
    ComponentColour StatusSeverity);
