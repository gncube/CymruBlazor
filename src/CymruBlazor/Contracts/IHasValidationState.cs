namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that exposes a validation state.
/// </summary>
public interface IHasValidationState
{
    ValidationState ValidationState { get; }
}
