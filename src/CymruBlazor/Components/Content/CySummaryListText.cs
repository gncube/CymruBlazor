namespace CymruBlazor.Components.Content;

/// <summary>
/// The phrases <see cref="CySummaryList"/> shows, so a Welsh (or any other) service can replace them.
/// Pass an instance to <see cref="CySummaryList.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CySummaryListText
{
    /// <summary>The visible text of each change link. The row's key follows it as visually hidden text.</summary>
    public string Change { get; init; } = "Change";

    /// <summary>Shown when a row has no value.</summary>
    public string NotProvided { get; init; } = "Not provided";
}
