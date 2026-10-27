namespace CymruBlazor.Components.Content;

/// <summary>
/// Localized text strings for <see cref="CyInsetText"/>, supporting English defaults and Welsh translations.
/// Pass an instance to <see cref="CyInsetText.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyInsetTextText
{
    /// <summary>
    /// Visually hidden prefix rendered before the inset text content for assistive technology.
    /// Default is "Information: ".
    /// </summary>
    public string VisuallyHiddenText { get; init; } = "Information: ";
}
