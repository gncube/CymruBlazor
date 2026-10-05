namespace CymruBlazor.Components.Content;

/// <summary>
/// The phrases <see cref="CyStatCard"/> adds for assistive technology, so a Welsh (or any other) service can
/// replace them. Pass an instance to <see cref="CyStatCard.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyStatCardText
{
    /// <summary>Visually hidden prefix before the trend text when the value went up.</summary>
    public string Increase { get; init; } = "Increase:";

    /// <summary>Visually hidden prefix before the trend text when the value went down.</summary>
    public string Decrease { get; init; } = "Decrease:";

    /// <summary>Visually hidden prefix before the trend text when the value did not change.</summary>
    public string NoChange { get; init; } = "No change:";

    /// <summary>Visually hidden text while <see cref="CyStatCard.Loading"/> is true.</summary>
    public string Loading { get; init; } = "Loading";
}
