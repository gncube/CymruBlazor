namespace CymruBlazor.Theming;

/// <summary>
/// Holds the current <see cref="CymruTheme"/> for a CymruBlazor application and notifies
/// subscribers when it changes. Register as a scoped (Blazor Web App) or singleton
/// (Blazor WebAssembly) service; <see cref="CymruThemeProvider"/> renders the result.
/// </summary>
public sealed class ThemeService
{
    /// <summary>The active theme. Defaults to <see cref="CymruTheme.System"/>.</summary>
    public CymruTheme Theme { get; private set; } = CymruTheme.System;

    /// <summary>Raised whenever <see cref="Theme"/> changes.</summary>
    public event Action? Changed;

    /// <summary>Sets the active theme and notifies subscribers.</summary>
    public void SetTheme(CymruTheme theme)
    {
        if (Theme == theme)
        {
            return;
        }

        Theme = theme;
        Changed?.Invoke();
    }

    /// <summary>Cycles System &#8594; Light &#8594; Dark &#8594; System, for a single theme-toggle control.</summary>
    public void CycleTheme()
    {
        SetTheme(Theme switch
        {
            CymruTheme.System => CymruTheme.Light,
            CymruTheme.Light => CymruTheme.Dark,
            _ => CymruTheme.System
        });
    }
}
