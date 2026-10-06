namespace CymruBlazor.Components.Content;

/// <summary>One person in a <see cref="CyAvatarGroup"/>.</summary>
/// <param name="Name">The person's name. Required.</param>
public sealed record CyAvatarItem(string Name)
{
    /// <summary>Optional photo. Without it the initials are shown.</summary>
    public string? ImageUrl { get; init; }
}
