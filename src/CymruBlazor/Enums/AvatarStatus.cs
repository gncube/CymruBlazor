namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the user activity or presence status for an avatar.
/// </summary>
public enum AvatarStatus
{
    /// <summary>
    /// No status indicator is displayed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user is online and available.
    /// </summary>
    Online = 1,

    /// <summary>
    /// The user is busy or occupied (e.g. in a meeting or consultation).
    /// </summary>
    Busy = 2,

    /// <summary>
    /// The user is temporarily away or inactive.
    /// </summary>
    Away = 3,

    /// <summary>
    /// The user is offline.
    /// </summary>
    Offline = 4
}
