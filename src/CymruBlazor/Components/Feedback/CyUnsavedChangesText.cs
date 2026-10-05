namespace CymruBlazor.Components.Feedback;

/// <summary>
/// The phrases <see cref="CyUnsavedChanges"/> shows, so a Welsh (or any other) service can replace them.
/// Pass an instance to <see cref="CyUnsavedChanges.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyUnsavedChangesText
{
    /// <summary>Indicator while a save is in progress.</summary>
    public string Saving { get; init; } = "Saving...";

    /// <summary>Indicator after a save, when no time is known.</summary>
    public string Saved { get; init; } = "Saved";

    /// <summary>Indicator after a save. {0} is the time.</summary>
    public string SavedAt { get; init; } = "Saved at {0}";

    /// <summary>Indicator while there are unsaved changes.</summary>
    public string Unsaved { get; init; } = "Unsaved changes";

    /// <summary>Indicator after a failed save.</summary>
    public string Error { get; init; } = "Could not save";

    /// <summary>Title of the leave-the-page confirmation.</summary>
    public string ConfirmTitle { get; init; } = "Leave this page?";

    /// <summary>Message of the leave-the-page confirmation, when <see cref="CyUnsavedChanges.Message"/> is not set.</summary>
    public string ConfirmMessage { get; init; } = "You have unsaved changes. If you leave now, they will be lost.";

    /// <summary>Confirm button of the leave-the-page confirmation.</summary>
    public string Leave { get; init; } = "Leave page";

    /// <summary>Cancel button of the leave-the-page confirmation.</summary>
    public string Stay { get; init; } = "Stay on page";
}
