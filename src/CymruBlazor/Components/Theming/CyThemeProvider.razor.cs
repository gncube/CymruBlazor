using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Services;
using CymruBlazor.Themes;

namespace CymruBlazor.Components.Theming;

/// <summary>
/// Applies the active <see cref="IThemeService"/> theme to a root
/// <c>data-theme</c> attribute (matching the selector convention already
/// used by <c>wwwroot/css/themes/*.css</c>, e.g. <c>[data-theme="dark"]</c>)
/// and re-renders its subtree whenever the theme changes.
///
/// Wrap your application's root markup in this component to enable
/// runtime theme switching:
///
/// <code>
/// &lt;CyThemeProvider&gt;
///     &lt;Router ...&gt;...&lt;/Router&gt;
/// &lt;/CyThemeProvider&gt;
/// </code>
///
/// Persistence and OS colour-scheme detection require
/// <c>_content/CymruBlazor/js/theme.js</c> to be referenced by the host
/// page - without it, <see cref="IThemeService"/> still works for the
/// current session, it just won't remember the choice across page loads.
/// </summary>
public partial class CyThemeProvider : CyLayoutComponentBase, IDisposable
{
    private bool _hasInitialized;

    [Inject]
    private IThemeService ThemeService { get; set; } = default!;

    /// <summary>
    /// Optionally forces the theme to a specific mode on first render,
    /// taking priority over any persisted or OS-detected preference.
    /// </summary>
    [Parameter]
    public ThemeMode? InitialTheme { get; set; }

    protected override string BaseCssClass => "cy-theme-provider";

    protected override void OnInitialized()
    {
        ThemeService.ThemeChanged += OnThemeChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _hasInitialized)
        {
            return;
        }

        _hasInitialized = true;

        if (InitialTheme is { } mode)
        {
            await ThemeService.SetThemeAsync(mode);
        }

        // Must run after the first render, not in OnInitializedAsync -
        // this performs JS interop, which isn't available during
        // prerendering.
        await ThemeService.InitializeAsync();
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Unsubscribes from <see cref="IThemeService.ThemeChanged"/>.
    /// Does not dispose <see cref="ThemeService"/> itself - it's a
    /// DI-owned scoped service, not owned by this component.
    /// </summary>
    public void Dispose()
    {
        ThemeService.ThemeChanged -= OnThemeChanged;
        GC.SuppressFinalize(this);
    }
}
