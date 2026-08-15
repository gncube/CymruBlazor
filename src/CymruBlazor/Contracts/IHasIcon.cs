using CymruBlazor.Enums;

namespace CymruBlazor.Contracts;

/// <summary>
/// Represents a component that can display an icon.
/// </summary>
public interface IHasIcon
{
    string? Icon { get; }

    IconPosition IconPosition { get; }
}
