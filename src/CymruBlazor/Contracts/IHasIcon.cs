using CymruBlazor.Enums;

namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that can display an icon.
/// </summary>
[Obsolete("No component implements this contract and none is planned; it will be removed in 2.0.0.")]
public interface IHasIcon
{
    string? Icon { get; }

    IconPosition IconPosition { get; }
}
