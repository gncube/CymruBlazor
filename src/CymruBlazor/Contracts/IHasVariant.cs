using CymruBlazor.Enums;

namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that exposes a visual variant.
/// </summary>
[Obsolete("No component implements this contract and none is planned; it will be removed in 2.0.0.")]
public interface IHasVariant
{
    ComponentVariant Variant { get; }
}
