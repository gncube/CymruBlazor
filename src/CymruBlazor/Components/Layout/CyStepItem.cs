namespace CymruBlazor.Components.Layout;

/// <summary>Describes one step of a <see cref="CyStepper"/>.</summary>
public sealed record CyStepItem
{
    /// <summary>Creates a step with the given title.</summary>
    public CyStepItem(string title)
    {
        Title = title;
    }

    /// <summary>The step's name. Required.</summary>
    public string Title { get; init; }

    /// <summary>Optional short description shown under the title (hidden in the compact horizontal layout).</summary>
    public string? Description { get; init; }

    /// <summary>
    /// The step's state. <see cref="CyStepStatus.Auto"/> (the default) is worked out from the stepper's current index;
    /// set <see cref="CyStepStatus.Error"/> to flag a step that needs attention.
    /// </summary>
    public CyStepStatus Status { get; init; }

    /// <summary>A disabled step is plain text and cannot be selected.</summary>
    public bool Disabled { get; init; }

    /// <summary>When set (and the step can be selected) the step is a link to this URL instead of a button.</summary>
    public string? Href { get; init; }
}
