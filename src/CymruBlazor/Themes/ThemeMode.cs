namespace CymruBlazor.Themes;

/// <summary>
/// Represents the supported CymruBlazor theme modes.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Follow the operating system preference.
    /// </summary>
    System = 0,

    /// <summary>
    /// NHS Wales light theme.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Dark theme.
    /// </summary>
    Dark = 2,

    /// <summary>
    /// High contrast accessibility theme.
    /// </summary>
    HighContrast = 3
}
