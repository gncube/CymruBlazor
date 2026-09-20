using System.Globalization;
using System.Text;
using Bunit;
using CymruBlazor.Components.Core;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Base class for real, automated accessibility tests: renders a component
/// via bUnit, loads the resulting markup - together with the library's
/// actual CSS - into a real Chromium browser via Playwright, and runs a
/// real axe-core scan against it.
///
/// This deliberately does NOT use bUnit alone. bUnit can assert a CSS class
/// is present on an element, but it cannot evaluate the CSS cascade, so it
/// would happily pass a component whose colours resolve to invisible or
/// low-contrast text - exactly the class of bug (missing focus rings,
/// contrast failures, cascade-order bugs) that a real browser running
/// axe-core catches automatically instead of requiring a human to notice.
///
/// Requires Playwright's browser binaries to be installed once per machine:
///   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
/// (from this test project's output directory, after building it at least
/// once so the script exists).
/// </summary>
public abstract class AxeTestBase : BunitContext, IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly List<string> _tempFiles = [];

    protected IPage Page { get; private set; } = null!;

    protected AxeTestBase()
    {
        // Every CymruBlazor component derives from CyComponentBase, which
        // injects IComponentIdGenerator and calls it from OnParametersSet.
        // CymruBlazor.Tests gets this for free from TestContextBase; this
        // project doesn't share that base (it needs Playwright's
        // IAsyncLifetime instead), so it has to register the same default
        // implementation itself or every component fails to render.
        Services.AddSingleton<IComponentIdGenerator, ComponentIdGenerator>();
    }

    public async Task InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        Page = await _browser.NewPageAsync();
    }

    public new async Task DisposeAsync()
    {
        if (Page is not null)
        {
            await Page.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();

        foreach (var file in _tempFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Best-effort temp file cleanup - never fail a test over this.
            }
        }

        // BunitContext.DisposeAsync() releases the bUnit render tree, fake
        // JSInterop, and DI container it set up in the base class. Our
        // DisposeAsync() above has the same name/signature, so it hides
        // (rather than overrides) the base member - `new` makes that
        // explicit, and this call makes sure the base's own cleanup still
        // actually runs instead of silently being skipped.
        await base.DisposeAsync();
    }

    /// <summary>
    /// Renders <typeparamref name="TComponent"/> via bUnit with the given
    /// parameters, then runs a real axe-core scan against the result.
    /// </summary>
    protected Task<AxeResult> ScanComponentAsync<TComponent>(
        Action<ComponentParameterCollectionBuilder<TComponent>>? parameters = null)
        where TComponent : IComponent
    {
        var cut = parameters is null
            ? Render<TComponent>()
            : Render(parameters);

        return ScanMarkupAsync(cut.Markup);
    }

    /// <summary>
    /// Runs a real axe-core scan against arbitrary pre-rendered markup, for
    /// scenarios spanning more than one component (e.g. a labelled form
    /// field alongside its own label, or a component inside a themed
    /// wrapper).
    /// </summary>
    protected async Task<AxeResult> ScanMarkupAsync(string bodyMarkup, string theme = "light")
    {
        var cssPath = FindBundledCssPath();
        var scopedCssPath = FindScopedCssPath();

        var html = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\">")
            .AppendLine("<head>")
            .AppendLine("<meta charset=\"utf-8\" />")
            .AppendLine("<title>CymruBlazor accessibility test host</title>")
            .AppendLine(cssPath is not null
                ? $"<link rel=\"stylesheet\" href=\"file:///{ToFileUrl(cssPath)}\" />"
                : "<!-- cymrublazor.css not found - see AxeTestBase.FindBundledCssPath -->")
            .AppendLine(scopedCssPath is not null
                ? $"<link rel=\"stylesheet\" href=\"file:///{ToFileUrl(scopedCssPath)}\" />"
                : "<!-- CymruBlazor.styles.css (scoped component CSS) not found - see "
                  + "AxeTestBase.FindScopedCssPath. Structural checks (labels, roles, "
                  + "landmarks) are unaffected; component-specific visual contrast may "
                  + "be inaccurate without it. -->")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .AppendLine("<main>")
            // data-theme belongs on THIS element specifically - it's what
            // the real CyThemeProvider sets it on, and what dark.css/
            // high-contrast.css's [data-theme="..."] and
            // body:has(.cy-theme-provider[data-theme="..."]) selectors
            // actually target. Putting it on <html> instead (an earlier
            // draft of this method did) would mean neither selector
            // matches anything, and a "dark theme" scan would silently
            // test plain light-mode styling instead - passing for the
            // wrong reason rather than actually exercising dark mode.
            .AppendLine(CultureInfo.InvariantCulture, $"<div class=\"cy-theme-provider\" data-theme=\"{theme}\">")
            .AppendLine(bodyMarkup)
            .AppendLine("</div>")
            .AppendLine("</main>")
            .AppendLine("</body>")
            .AppendLine("</html>")
            .ToString();

        var tempFile = Path.Combine(Path.GetTempPath(), $"cymru-a11y-{Guid.NewGuid():N}.html");
        await File.WriteAllTextAsync(tempFile, html);
        _tempFiles.Add(tempFile);

        await Page.GotoAsync($"file:///{ToFileUrl(tempFile)}");

        var options = new AxeRunOptions
        {
            Rules = new Dictionary<string, RuleOptions>
            {
                ["page-has-heading-one"] = new() { Enabled = false }
            }
        };

        return await Page.RunAxe(options);
    }

    /// <summary>
    /// Loads markup into a page served from a fake origin (<c>https://cymru.test/</c>) that maps
    /// <c>/_content/CymruBlazor/*</c> onto the library's real <c>wwwroot</c>, exactly like the
    /// static web asset URLs in a running app. Unlike <see cref="ScanMarkupAsync"/> (which uses
    /// <c>file://</c>) this lets the page import the library's ES module, so tests can exercise the
    /// real JavaScript - focus containment, <c>&lt;dialog&gt;</c> wiring, tooltip dismissal - in a
    /// real browser. The module is imported and exposed as <c>window.overlay</c>.
    /// </summary>
    protected async Task LoadHostedAsync(
        string bodyMarkup,
        string theme = "light",
        int width = 1280,
        int height = 800)
    {
        var wwwroot = FindLibraryWwwroot();

        await Page.SetViewportSizeAsync(width, height);

        await Page.RouteAsync($"{HostedOrigin}/**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;

            if (path == "/")
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    ContentType = "text/html; charset=utf-8",
                    Body = BuildHostedHtml(bodyMarkup, theme)
                });

                return;
            }

            const string prefix = "/_content/CymruBlazor/";
            var relative = path.StartsWith(prefix, StringComparison.Ordinal) ? path[prefix.Length..] : null;
            var file = relative is null ? null : Path.GetFullPath(Path.Combine(wwwroot, relative));

            if (file is null || !file.StartsWith(wwwroot, StringComparison.Ordinal) || !File.Exists(file))
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 404 });
                return;
            }

            await route.FulfillAsync(new RouteFulfillOptions
            {
                ContentType = Path.GetExtension(file) switch
                {
                    ".js" => "text/javascript",
                    ".css" => "text/css",
                    ".svg" => "image/svg+xml",
                    _ => "application/octet-stream"
                },
                Body = await File.ReadAllTextAsync(file)
            });
        });

        await Page.GotoAsync($"{HostedOrigin}/");
        await Page.WaitForFunctionAsync("() => window.overlay !== undefined");
    }

    /// <summary>
    /// Measures an element with real layout: visibility, whether it sits inside the viewport, its
    /// resolved size, and whether the page scrolls horizontally.
    /// </summary>
    protected async Task<ElementMetrics> MeasureAsync(string selector)
    {
        ElementMetrics? metrics = await Page.EvaluateAsync<ElementMetrics?>(
            """
            (selector) => {
              const el = document.querySelector(selector);
              if (!el) { return null; }
              const r = el.getBoundingClientRect();
              const cs = getComputedStyle(el);
              const root = document.documentElement;
              return {
                found: true,
                visible: cs.visibility !== 'hidden' && cs.display !== 'none' && r.width > 0 && r.height > 0,
                left: r.left, top: r.top, right: r.right, bottom: r.bottom,
                width: r.width, height: r.height,
                viewportWidth: window.innerWidth, viewportHeight: window.innerHeight,
                pageScrollWidth: root.scrollWidth,
                zIndex: cs.zIndex, opacity: parseFloat(cs.opacity)
              };
            }
            """,
            selector);

        return metrics ?? new ElementMetrics { Found = false };
    }

    /// <summary>
    /// Runs axe-core against the page as it currently is (after a test has opened a dialog, focused a
    /// trigger, and so on), with the same rule configuration as <see cref="ScanMarkupAsync"/>.
    /// </summary>
    protected Task<AxeResult> RunAxeOnPageAsync()
    {
        var options = new AxeRunOptions
        {
            Rules = new Dictionary<string, RuleOptions>
            {
                ["page-has-heading-one"] = new() { Enabled = false }
            }
        };

        return Page.RunAxe(options);
    }

    /// <summary>The id of the element that currently has focus ("" when it has none).</summary>
    protected Task<string> ActiveElementIdAsync() =>
        Page.EvaluateAsync<string>("() => document.activeElement?.id ?? ''");

    private const string HostedOrigin = "https://cymru.test";

    private static string BuildHostedHtml(string bodyMarkup, string theme) =>
        $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>CymruBlazor browser test host</title>
        <link rel="stylesheet" href="/_content/CymruBlazor/css/cymrublazor.css" />
        </head>
        <body>
        <main>
        <div class="cy-theme-provider" data-theme="{{theme}}">
        {{bodyMarkup}}
        </div>
        </main>
        <script type="module">
          import * as overlay from '/_content/CymruBlazor/js/cymru-overlay.js';
          window.overlay = overlay;
        </script>
        </body>
        </html>
        """;

    private static string FindLibraryWwwroot()
    {
        var repoRoot = FindRepoRoot()
            ?? throw new InvalidOperationException("Could not locate the repository root (.git).");

        return Path.GetFullPath(Path.Combine(repoRoot, "src", "CymruBlazor", "wwwroot"));
    }

    private static string ToFileUrl(string path) => path.Replace('\\', '/');

    /// <summary>
    /// Locates the library's real bundled design-token/component stylesheet.
    /// Generated at build time by tools/CymruBlazor.CssBundler into
    /// src/CymruBlazor/wwwroot/css/cymrublazor.css - a checked-in path
    /// under source control (see src/CymruBlazor/build/BundleCss.targets),
    /// not a build-output artifact, so this location is stable regardless
    /// of build configuration. Walks up from the test assembly's own
    /// output directory looking for the repo root (marked by .git) rather
    /// than assuming a fixed number of ".." segments, so this keeps working
    /// regardless of Debug/Release or exact bin/ nesting.
    /// </summary>
    private static string? FindBundledCssPath()
    {
        var repoRoot = FindRepoRoot();

        if (repoRoot is null)
        {
            return null;
        }

        var path = Path.Combine(repoRoot, "src", "CymruBlazor", "wwwroot", "css", "cymrublazor.css");

        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Locates the generated scoped/isolated CSS bundle for CymruBlazor's
    /// own *.razor.css files (e.g. CyButton.razor.css). Unlike
    /// cymrublazor.css, this one IS a build output artifact - the standard
    /// Blazor CSS isolation convention names it "{AssemblyName}.styles.css"
    /// and its exact folder can vary by SDK version - so this searches the
    /// test project's own output tree for it rather than assuming one fixed
    /// path. If a future SDK version moves it somewhere this search doesn't
    /// reach, scoped-CSS-dependent visual checks may be inaccurate; this is
    /// called out explicitly in ScanMarkupAsync's generated HTML comment so
    /// a failing/skipped assertion is traceable back to this note rather
    /// than looking like a mysterious axe-core false result.
    /// </summary>
    private static string? FindScopedCssPath()
    {
        try
        {
            return Directory
                .EnumerateFiles(AppContext.BaseDirectory, "CymruBlazor.styles.css", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static string? FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
