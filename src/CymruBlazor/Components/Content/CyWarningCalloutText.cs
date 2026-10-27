namespace CymruBlazor.Components.Content;

/// <summary>
/// Localized text strings for <see cref="CyWarningCallout"/>, supporting English defaults and Welsh translations.
/// Pass an instance to <see cref="CyWarningCallout.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyWarningCalloutText
{
    /// <summary>
    /// Visually hidden prefix read by screen readers immediately preceding the heading.
    /// Default is "Important: ".
    /// </summary>
    public string VisuallyHiddenPrefix { get; init; } = "Important: ";

    /// <summary>
    /// Default heading text rendered when neither <see cref="CyWarningCallout.Heading"/> nor
    /// <see cref="CyWarningCallout.HeadingTemplate"/> is supplied.
    /// Default is "Important".
    /// </summary>
    public string DefaultHeading { get; init; } = "Important";
}
