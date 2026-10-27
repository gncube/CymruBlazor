using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Localisable phrases for <see cref="CyUserNav"/>.
/// </summary>
public sealed record CyUserNavText
{
    /// <summary>Default accessible label for the user navigation trigger when no DisplayName is provided.</summary>
    public string UserMenu { get; init; } = "User menu";

    /// <summary>Default text for the unauthenticated login trigger.</summary>
    public string Login { get; init; } = "Log in";

    /// <summary>Default text for the logout button.</summary>
    public string Logout { get; init; } = "Log out";

    /// <summary>Status label for <see cref="AvatarStatus.Online"/>.</summary>
    public string StatusOnline { get; init; } = "Online";

    /// <summary>Status label for <see cref="AvatarStatus.Busy"/>.</summary>
    public string StatusBusy { get; init; } = "Busy";

    /// <summary>Status label for <see cref="AvatarStatus.Away"/>.</summary>
    public string StatusAway { get; init; } = "Away";

    /// <summary>Status label for <see cref="AvatarStatus.Offline"/>.</summary>
    public string StatusOffline { get; init; } = "Offline";
}
