using System;
using System.Threading.Tasks;
using CymruBlazor.Themes;

namespace CymruBlazor.Services;

/// <summary>
/// Manages the current theme mode and notifies subscribers when it changes.
/// </summary>
public sealed class ThemeService
{
    private ThemeMode _current = ThemeMode.Default;

    /// <summary>
    /// Gets or sets the current theme mode.
    /// </summary>
    public ThemeMode Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            NotifyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raised when the theme changes.
    /// </summary>
    public event EventHandler? NotifyChanged;

    /// <summary>
    /// Apply the theme to the document by returning the data-theme attribute value.
    /// </summary>
    public Task<string> ApplyAsync(ThemeDefinition? def = null)
    {
        return Task.FromResult(_current switch
        {
            ThemeMode.Default => "",
            ThemeMode.Dark => "dark",
            ThemeMode.HighContrast => "high-contrast",
            _ => ""
        });
    }
}
