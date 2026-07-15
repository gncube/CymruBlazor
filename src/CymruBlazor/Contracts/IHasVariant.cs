namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that exposes a visual variant.
/// </summary>
public interface IHasVariant
{
    ComponentVariant Variant { get; }
}
