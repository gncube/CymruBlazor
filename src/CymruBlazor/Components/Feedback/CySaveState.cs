namespace CymruBlazor.Components.Feedback;

/// <summary>Where a save is, as shown by <see cref="CyUnsavedChanges"/>.</summary>
public enum CySaveState
{
    /// <summary>Nothing is being saved.</summary>
    Idle = 0,

    /// <summary>A save is in progress.</summary>
    Saving = 1,

    /// <summary>The last save succeeded.</summary>
    Saved = 2,

    /// <summary>The last save failed.</summary>
    Error = 3
}
