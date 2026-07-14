namespace CymruBlazor.Themes;

/// <summary>
/// Raised when the active theme changes.
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    public ThemeChangedEventArgs(ThemeDefinition theme)
    {
        Theme = theme;
    }

    public ThemeDefinition Theme { get; }
}
