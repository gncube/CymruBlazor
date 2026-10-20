namespace CymruBlazor.Components.Content;

/// <summary>
/// Localised phrases and accessible labels for <see cref="CyTimeline"/>.
/// </summary>
public record CyTimelineText
{
    /// <summary>Default accessible label when no specific label is provided.</summary>
    public string DefaultLabel { get; init; } = "Activity timeline";
}
