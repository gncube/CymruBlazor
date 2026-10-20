using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Represents a single chronological milestone or event for data-bound <see cref="CyTimeline"/> usage.
/// </summary>
public record CyTimelineItemModel
{
    /// <summary>The title or milestone heading. Required.</summary>
    public required string Title { get; init; }

    /// <summary>Human-readable time or date string (e.g. "14:30, 20 Oct 2026").</summary>
    public string? Time { get; init; }

    /// <summary>Machine-readable timestamp used for the &lt;time&gt; element's datetime attribute.</summary>
    public DateTimeOffset? DateTime { get; init; }

    /// <summary>Optional plain-text description or notes for the event.</summary>
    public string? Description { get; init; }

    /// <summary>Optional actor or user attribution (e.g. "Dr. Jane Evans" or "Automation").</summary>
    public string? Actor { get; init; }

    /// <summary>Optional icon name from the built-in icon registry (e.g. "check", "clock", "alert-circle").</summary>
    public string? Icon { get; init; }

    /// <summary>Semantic colour for the marker node. Defaults to <see cref="ComponentColour.Neutral"/>.</summary>
    public ComponentColour Colour { get; init; } = ComponentColour.Neutral;

    /// <summary>Optional status badge text displayed alongside the heading.</summary>
    public string? BadgeText { get; init; }

    /// <summary>Semantic colour for the status badge pill. Defaults to <see cref="ComponentColour.Neutral"/>.</summary>
    public ComponentColour BadgeColour { get; init; } = ComponentColour.Neutral;
}
