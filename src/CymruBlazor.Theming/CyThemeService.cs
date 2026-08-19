namespace CymruBlazor.Theming;

/// <summary>
/// Holds the current <see cref="CyTheme"/> for a CymruBlazor application and notifies
/// subscribers when it changes. Register as a scoped (Blazor Web App) or singleton
/// (Blazor WebAssembly) service; <see cref="CyThemeProvider"/> renders the result.
/// </summary>
public sealed class CyThemeService
{
    /// <summary>The active theme. Defaults to <see cref="CyTheme.System"/>.</summary>
    public CyTheme Theme { get; private set; } = CyTheme.System;

    /// <summary>Raised whenever <see cref="Theme"/> changes.</summary>
    public event Action? Changed;

    /// <summary>Sets the active theme and notifies subscribers.</summary>
    public void SetTheme(CyTheme theme)
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
            CyTheme.System => CyTheme.Light,
            CyTheme.Light => CyTheme.Dark,
            _ => CyTheme.System
        });
    }
}
