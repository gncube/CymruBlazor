using CymruBlazor.Enums;

namespace CymruBlazor.Components.Accessibility;

/// <summary>What <see cref="ICyConfirmService.ConfirmAsync(CyConfirmOptions, CancellationToken)"/> asks the user.</summary>
public sealed record CyConfirmOptions
{
    /// <summary>The dialog heading and accessible name, for example "Publish questionnaire?". Required.</summary>
    public required string Title { get; init; }

    /// <summary>Plain text explaining the consequence. Always encoded; also the dialog's accessible description.</summary>
    public string? Message { get; init; }

    /// <summary>Text of the confirm button. Defaults to the host's localised "Confirm".</summary>
    public string? ConfirmText { get; init; }

    /// <summary>Text of the cancel button. Defaults to the host's localised "Cancel".</summary>
    public string? CancelText { get; init; }

    /// <summary>
    /// Marks an action that cannot be undone: the confirm button uses the danger style and initial focus goes to
    /// the cancel button, so a stray Enter key does not destroy work. Make <see cref="ConfirmText"/> say what happens
    /// ("Discard draft"), because colour alone is not enough.
    /// </summary>
    public bool Destructive { get; init; }

    /// <summary>The dialog width. Defaults to <see cref="ComponentSize.Small"/>.</summary>
    public ComponentSize Size { get; init; } = ComponentSize.Small;
}
