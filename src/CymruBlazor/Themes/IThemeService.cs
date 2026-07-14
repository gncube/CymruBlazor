namespace CymruBlazor.Themes;

public interface IThemeService
{
    ThemeDefinition CurrentTheme { get; }

    IReadOnlyCollection<ThemeDefinition> AvailableThemes { get; }

    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    ValueTask SetThemeAsync(ThemeMode mode);

    ValueTask SetThemeAsync(string cssTheme);

    ValueTask ToggleDarkModeAsync();

    ValueTask InitializeAsync();
}
