using CymruBlazor.Themes;

namespace CymruBlazor.Services;

public sealed class ThemeService : IThemeService
{
    private readonly Dictionary<ThemeMode, ThemeDefinition> _themes;

    public ThemeService()
    {
        _themes = new()
        {
            [ThemeMode.Light] =
                new(ThemeMode.Light, "Light", "light"),

            [ThemeMode.Dark] =
                new(ThemeMode.Dark, "Dark", "dark"),

            [ThemeMode.HighContrast] =
                new(ThemeMode.HighContrast, "High Contrast", "high-contrast"),

            [ThemeMode.System] =
                new(ThemeMode.System, "System", "light")
        };

        CurrentTheme = _themes[ThemeMode.Light];
    }

    public ThemeDefinition CurrentTheme { get; private set; }

    public IReadOnlyCollection<ThemeDefinition> AvailableThemes =>
        _themes.Values;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SetThemeAsync(ThemeMode mode)
    {
        if (!_themes.TryGetValue(mode, out var theme))
        {
            return ValueTask.CompletedTask;
        }

        if (ReferenceEquals(CurrentTheme, theme))
        {
            return ValueTask.CompletedTask;
        }

        CurrentTheme = theme;

        ThemeChanged?.Invoke(
            this,
            new ThemeChangedEventArgs(theme));

        return ValueTask.CompletedTask;
    }

    public ValueTask SetThemeAsync(string cssTheme)
    {
        var theme = _themes.Values.FirstOrDefault(
            t => t.CssTheme.Equals(
                cssTheme,
                StringComparison.OrdinalIgnoreCase));

        return theme is null
            ? ValueTask.CompletedTask
            : SetThemeAsync(theme.Mode);
    }

    public ValueTask ToggleDarkModeAsync()
    {
        return CurrentTheme.Mode == ThemeMode.Dark
            ? SetThemeAsync(ThemeMode.Light)
            : SetThemeAsync(ThemeMode.Dark);
    }
}
