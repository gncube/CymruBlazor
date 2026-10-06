namespace CymruBlazor.Components.Layout;

/// <summary>The state of one step in a <see cref="CyStepper"/>.</summary>
public enum CyStepStatus
{
    /// <summary>Worked out from <see cref="CyStepper.Current"/>: earlier steps are complete, later ones upcoming.</summary>
    Auto = 0,

    /// <summary>The step is finished.</summary>
    Complete = 1,

    /// <summary>The step the user is on.</summary>
    Current = 2,

    /// <summary>The step has not been reached.</summary>
    Upcoming = 3,

    /// <summary>The step has a problem to fix.</summary>
    Error = 4
}
