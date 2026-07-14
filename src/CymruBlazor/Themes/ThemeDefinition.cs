namespace CymruBlazor.Themes;

/// <summary>
/// Represents a theme definition.
/// </summary>
public sealed record ThemeDefinition(
    ThemeMode Mode,
    string Name,
    string CssTheme);
