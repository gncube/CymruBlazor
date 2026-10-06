namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// The default button text of <see cref="CyConfirmDialog"/>, so a Welsh (or any other) service can replace it.
/// Per-request text in <see cref="CyConfirmOptions"/> wins over these.
/// </summary>
public sealed record CyConfirmText
{
    /// <summary>Default text of the confirm button.</summary>
    public string Confirm { get; init; } = "Confirm";

    /// <summary>Default text of the cancel button.</summary>
    public string Cancel { get; init; } = "Cancel";
}
