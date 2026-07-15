namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that exposes a semantic colour.
/// </summary>
public interface IHasColour
{
    ComponentColour Colour { get; }
}
