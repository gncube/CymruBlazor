using Bunit;
using CymruBlazor.Diagnostics;
using CymruBlazor.Extensions;
using CymruBlazor.Services;
using CymruBlazor.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Testing;

/// <summary>
/// A bUnit context with CymruBlazor registered the way an app registers it. Derive your test classes from it.
/// </summary>
/// <remarks>
/// <para>
/// Without <c>AddCymruBlazor()</c> no CymruBlazor component renders: they all need <c>IComponentIdGenerator</c>,
/// and most need the localiser and diagnostics services too. This context calls it for you.
/// </para>
/// <para>
/// JavaScript interop is set to <see cref="JSRuntimeMode.Loose"/>, and each of the library's script modules is set
/// up (also loose), so components that load a script on first render, such as the combobox, the row menus and the
/// sortable list, render without a browser. Set up your own calls and modules on <c>JSInterop</c> as usual.
/// </para>
/// <para>
/// <c>IThemeService</c> is a ready-made instance without JavaScript (the theme still changes in memory, it just is not
/// remembered). The real one can only be disposed asynchronously, and a test context is disposed synchronously, so a
/// container-owned one would fail every test that renders <c>CyThemeProvider</c> at teardown.
/// </para>
/// </remarks>
public abstract class CymruTestContext : BunitContext
{
    /// <summary>The library's script modules, as their file names under <c>_content/CymruBlazor/js/</c>.</summary>
    private static readonly string[] ModuleFiles =
    [
        "cymru-editing.js",
        "cymru-inputs.js",
        "cymru-overlay.js",
        "cymrublazor.js",
        "sortable-list.js"
    ];

    /// <summary>Creates the context with the library's default options.</summary>
    protected CymruTestContext()
        : this(null)
    {
    }

    /// <summary>Creates the context with options, as in <c>services.AddCymruBlazor(options => ...)</c>.</summary>
    /// <param name="configure">Configures <see cref="CymruBlazorOptions"/>, or null for the defaults.</param>
    protected CymruTestContext(Action<CymruBlazorOptions>? configure)
    {
        if (configure is null)
        {
            Services.AddCymruBlazor();
        }
        else
        {
            Services.AddCymruBlazor(configure);
        }

        Services.AddSingleton<IThemeService>(new ThemeService());

        JSInterop.Mode = JSRuntimeMode.Loose;

        foreach (var file in ModuleFiles)
        {
            JSInterop.SetupModule($"./_content/CymruBlazor/js/{file}").Mode = JSRuntimeMode.Loose;
        }
    }
}
