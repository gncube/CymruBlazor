namespace CymruBlazor.Components.Layout;

/// <summary>
/// The phrases <see cref="CyNotificationBell"/> uses for its accessible name, so a Welsh (or any other) service can
/// replace them. Pass an instance to <see cref="CyNotificationBell.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyNotificationBellText
{
    /// <summary>The accessible name when <see cref="CyNotificationBell.Label"/> is not set.</summary>
    public string Label { get; init; } = "Notifications";

    /// <summary>The accessible name when there are unread items. {0} the label, {1} the count.</summary>
    public string UnreadFormat { get; init; } = "{0}, {1} unread";
}
