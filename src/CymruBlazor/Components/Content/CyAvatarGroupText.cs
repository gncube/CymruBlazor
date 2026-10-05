namespace CymruBlazor.Components.Content;

/// <summary>
/// The phrase <see cref="CyAvatarGroup"/> adds for assistive technology, so a Welsh (or any other) service can replace it.
/// </summary>
public sealed record CyAvatarGroupText
{
    /// <summary>Visually hidden text beside the "+N" overflow badge. {0} is the number of people not shown.</summary>
    public string MoreFormat { get; init; } = "and {0} more";
}
