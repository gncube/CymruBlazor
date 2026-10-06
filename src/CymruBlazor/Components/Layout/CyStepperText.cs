namespace CymruBlazor.Components.Layout;

/// <summary>
/// The phrases <see cref="CyStepper"/> shows or adds for assistive technology, so a Welsh (or any other) service can
/// replace them. Pass an instance to <see cref="CyStepper.Text"/>; unset properties keep their English defaults.
/// </summary>
public sealed record CyStepperText
{
    /// <summary>The compact summary. {0} position, {1} number of steps, {2} title.</summary>
    public string StepOfFormat { get; init; } = "Step {0} of {1}: {2}";

    /// <summary>Visually hidden status of a finished step.</summary>
    public string Completed { get; init; } = "completed";

    /// <summary>Visually hidden status of an upcoming step.</summary>
    public string NotStarted { get; init; } = "not started";

    /// <summary>Visually hidden status of a step with an error.</summary>
    public string HasError { get; init; } = "has an error";
}
