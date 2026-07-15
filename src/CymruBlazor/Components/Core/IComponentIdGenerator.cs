namespace CymruBlazor.Components.Core;

/// <summary>
/// Generates deterministic, unique HTML element identifiers.
/// </summary>
public interface IComponentIdGenerator
{
    /// <summary>
    /// Generates a unique identifier using the supplied prefix.
    /// </summary>
    /// <param name="prefix">
    /// The identifier prefix (for example "button" or "alert").
    /// </param>
    /// <returns>
    /// A unique identifier.
    /// </returns>
    string Create(string? prefix = null);
}
