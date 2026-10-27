namespace CymruBlazor.Components.Content;

/// <summary>
/// Localized text strings for <see cref="CyDetails"/>, supporting English defaults and Welsh translations.
/// Pass an instance to <see cref="CyDetails.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyDetailsText
{
    /// <summary>
    /// Default summary text rendered when neither <see cref="CyDetails.Summary"/> nor <see cref="CyDetails.SummaryTemplate"/> is supplied.
    /// Default is "Help".
    /// </summary>
    public string DefaultSummary { get; init; } = "Help";
}
