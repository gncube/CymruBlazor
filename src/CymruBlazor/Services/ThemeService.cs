using CymruBlazor.Themes;
using Microsoft.JSInterop;

namespace CymruBlazor.Services;

/// <summary>
/// Default <see cref="IThemeService"/> implementation.
///
/// JS interop (theme persistence + OS preference detection) is optional:
/// when constructed without an <see cref="IJSRuntime"/> (e.g. in unit
/// tests, or during static/prerendering scenarios), <see cref="InitializeAsync"/>
/// falls back to the previous no-op behaviour and the service still works
/// correctly for the current session - it simply won't remember the
/// choice across page loads.
/// </summary>
public sealed class ThemeService : IThemeService, IAsyncDisposable
{
    private const string InteropModule = "cymruBlazorTheme";

    private readonly Dictionary<ThemeMode, ThemeDefinition> _themes;
    private readonly IJSRuntime? _jsRuntime;
    private DotNetObjectReference<ThemeService>? _dotNetRef;

    public ThemeService(IJSRuntime? jsRuntime = null)
    {
        _jsRuntime = jsRuntime;

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

    public async ValueTask InitializeAsync()
    {
        if (_jsRuntime is null)
        {
            return;
        }

        try
        {
            var storedTheme = await _jsRuntime.InvokeAsync<string?>(
                $"{InteropModule}.getStoredTheme");

            if (!string.IsNullOrWhiteSpace(storedTheme))
            {
                await SetThemeAsync(storedTheme);
            }
            else
            {
                var preferredScheme = await _jsRuntime.InvokeAsync<string?>(
                    $"{InteropModule}.getPreferredScheme");

                if (!string.IsNullOrWhiteSpace(preferredScheme))
                {
                    await SetThemeAsync(preferredScheme);
                }
            }

            _dotNetRef = DotNetObjectReference.Create(this);

            await _jsRuntime.InvokeVoidAsync(
                $"{InteropModule}.watchSystemPreference",
                _dotNetRef);
        }
        catch (JSException)
        {
            // The interop module isn't available (e.g. server-side
            // prerendering, or a consuming app that hasn't referenced
            // the script). Fall back silently to the Light default -
            // this must never prevent the app from rendering.
        }
        catch (InvalidOperationException)
        {
            // JS interop isn't available yet during static SSR.
        }
    }

    /// <summary>
    /// Invoked from JS (see wwwroot/js/theme.js, watchSystemPreference)
    /// when the OS colour-scheme preference changes live and the user
    /// hasn't set an explicit in-app preference of their own.
    /// </summary>
    [JSInvokable]
    public async Task OnSystemPreferenceChanged(string cssTheme)
    {
        await SetThemeAsync(cssTheme);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsRuntime is not null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync($"{InteropModule}.disposeWatch");
            }
            catch (JSException)
            {
            }
            catch (InvalidOperationException)
            {
                // The JS runtime may already be torn down (e.g. circuit
                // disconnect) by the time a scoped service is disposed -
                // this is expected and safe to ignore.
            }
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    public async ValueTask SetThemeAsync(ThemeMode mode)
    {
        if (!_themes.TryGetValue(mode, out var theme))
        {
            return;
        }

        if (ReferenceEquals(CurrentTheme, theme))
        {
            return;
        }

        CurrentTheme = theme;

        await PersistThemeAsync(theme.CssTheme);

        ThemeChanged?.Invoke(
            this,
            new ThemeChangedEventArgs(theme));
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

    private async ValueTask PersistThemeAsync(string cssTheme)
    {
        if (_jsRuntime is null)
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                $"{InteropModule}.setStoredTheme",
                cssTheme);
        }
        catch (JSException)
        {
            // Persistence is best-effort - the theme still applies for
            // the current session even if localStorage is unavailable
            // (e.g. private browsing restrictions in some browsers).
        }
        catch (InvalidOperationException)
        {
        }
    }
}
