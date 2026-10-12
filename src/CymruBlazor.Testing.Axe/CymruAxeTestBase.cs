using System.Globalization;
using System.Text;
using Bunit;
using CymruBlazor.Diagnostics;
using CymruBlazor.Extensions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;

namespace CymruBlazor.Testing.Axe;

/// <summary>
/// A <see cref="CymruTestContext"/> that can also scan what it renders with axe-core in a real browser.
/// </summary>
/// <remarks>
/// <para>
/// A scan renders the component with bUnit, puts the markup and CymruBlazor's real CSS in Chromium and runs axe-core,
/// so it finds what depends on the CSS cascade (contrast, focus visibility) in each theme. Behaviour is for bUnit.
/// </para>
/// <para>
/// Install the browser once per machine, after building the test project once so the script exists:
/// <c>pwsh bin/Debug/net10.0/playwright.ps1 install chromium</c>. The browser starts on the first scan, so a class
/// whose tests do not scan does not need it.
/// </para>
/// <para>
/// The stylesheet is read from the CymruBlazor package's static web assets. Override <see cref="CssDirectory"/>, or set
/// the <c>CYMRUBLAZOR_CSS_DIR</c> environment variable, to point at a different <c>staticwebassets</c> folder.
/// </para>
/// </remarks>
public abstract class CymruAxeTestBase : CymruTestContext, IAsyncDisposable
{
    private static readonly Lock CssLock = new();
    private static string? s_libraryCss;
    private static string? s_libraryCssDirectory;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    /// <summary>Creates the context with CymruBlazor registered with its default options.</summary>
    protected CymruAxeTestBase()
    {
    }

    /// <summary>Creates the context with CymruBlazor registered with the options you configure.</summary>
    /// <param name="configure">Configures the options, or null for the defaults.</param>
    protected CymruAxeTestBase(Action<CymruBlazorOptions>? configure)
        : base(configure)
    {
    }

    /// <summary>
    /// Extra CSS to load after the library's, for example your app's own stylesheet. Return the text of the file, or
    /// null for none. Overrides here are scanned too.
    /// </summary>
    protected virtual string? AppCss => null;

    /// <summary>
    /// The <c>staticwebassets</c> folder of the CymruBlazor package (the one holding <c>css/cymrublazor.css</c>), or
    /// null to find it from the package layout, a web project's asset manifest, or the
    /// <c>CYMRUBLAZOR_CSS_DIR</c> environment variable.
    /// </summary>
    protected virtual string? CssDirectory => null;

    /// <summary>Renders a component with bUnit, then scans it with axe-core.</summary>
    /// <typeparam name="TComponent">The component to render.</typeparam>
    /// <param name="parameters">Sets the component's parameters, or null for none.</param>
    /// <param name="theme">"light", "dark" or "high-contrast".</param>
    /// <returns>The axe-core result. Call <see cref="CymruAxeAssertions.ShouldHaveNoViolations"/> on it.</returns>
    protected Task<AxeResult> ScanComponentAsync<TComponent>(
        Action<ComponentParameterCollectionBuilder<TComponent>>? parameters = null,
        string theme = "light")
        where TComponent : IComponent
    {
        var cut = parameters is null ? Render<TComponent>() : Render(parameters);

        return ScanMarkupAsync(cut.Markup, theme);
    }

    /// <summary>Scans markup with axe-core.</summary>
    /// <param name="bodyMarkup">The markup to put in the page body.</param>
    /// <param name="theme">"light", "dark" or "high-contrast".</param>
    /// <returns>The axe-core result. Call <see cref="CymruAxeAssertions.ShouldHaveNoViolations"/> on it.</returns>
    protected async Task<AxeResult> ScanMarkupAsync(string bodyMarkup, string theme = "light")
    {
        ArgumentNullException.ThrowIfNull(bodyMarkup);
        ArgumentException.ThrowIfNullOrWhiteSpace(theme);

        var page = await GetPageAsync();

        await page.SetContentAsync(BuildHtml(bodyMarkup, theme));

        return await page.RunAxe(new AxeRunOptions
        {
            Rules = new Dictionary<string, RuleOptions>
            {
                // A component is not a whole page, so it has no h1 of its own.
                ["page-has-heading-one"] = new() { Enabled = false }
            }
        });
    }

    /// <summary>The browser page the scans use, for tests that need to drive it (focus, keyboard, layout).</summary>
    /// <returns>The page, created on first use.</returns>
    protected async Task<IPage> GetPageAsync()
    {
        if (_page is not null)
        {
            return _page;
        }

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _page = await _browser.NewPageAsync();

        return _page;
    }

    /// <summary>Closes the browser and releases the bUnit context.</summary>
    /// <returns>A task that completes when everything is released.</returns>
    public new async ValueTask DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    private string BuildHtml(string bodyMarkup, string theme) =>
        new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\" />")
            .AppendLine("<title>Accessibility test host</title>")
            .AppendLine(CultureInfo.InvariantCulture, $"<style>{GetLibraryCss()}</style>")
            .AppendLine(CultureInfo.InvariantCulture, $"<style>{AppCss}</style>")
            .AppendLine("</head><body><main>")
            // data-theme goes on this wrapper, as the real CyThemeProvider does. The dark and high-contrast
            // selectors target it; on <html> a "dark" scan would silently test light.
            .AppendLine(CultureInfo.InvariantCulture, $"<div class=\"cy-theme-provider\" data-theme=\"{theme}\">")
            .AppendLine(bodyMarkup)
            .AppendLine("</div></main></body></html>")
            .ToString();

    private string GetLibraryCss()
    {
        var directory = CssDirectory;

        lock (CssLock)
        {
            // Cached per process, but not across a different CssDirectory.
            if (s_libraryCss is null || !string.Equals(s_libraryCssDirectory, directory, StringComparison.Ordinal))
            {
                s_libraryCss = CymruLibraryCss.Load(
                    directory,
                    typeof(ServiceCollectionExtensions).Assembly.Location,
                    AppContext.BaseDirectory);
                s_libraryCssDirectory = directory;
            }

            return s_libraryCss;
        }
    }
}
