using System.Text.RegularExpressions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Smoke run over the <em>published</em> demo (roadmap 1.3.0-D): every <c>@page</c> route, in light, dark
/// and high-contrast, must render a heading, log no console errors or unhandled exceptions, and pass axe.
/// It is the regression harness for everything the component-level suites cannot see: real routing,
/// the real WebAssembly runtime, the real static web asset URLs and the components composed together.
/// </summary>
/// <remarks>
/// <para>
/// Point <c>CYMRU_DEMO_DIR</c> at the <c>wwwroot</c> folder of a published demo:
/// <code>dotnet publish src/CymruBlazor.Demo -c Release -o demo-publish</code>
/// <code>$env:CYMRU_DEMO_DIR = "$PWD\demo-publish\wwwroot"; dotnet test tests/CymruBlazor.AccessibilityTests --filter DemoSmokeTests</code>
/// The test serves those files itself (with SPA fallback) from a fake origin, so no web server is needed.
/// When the variable is not set the test is a no-op locally, but fails when <c>CI=true</c> so the
/// pipeline cannot silently skip it.
/// </para>
/// <para>
/// A violation that is understood and tracked can be listed in <see cref="KnownIssues"/> as
/// <c>"route|rule-id"</c> so the run stays green while the fix is scheduled; keep that list empty otherwise.
/// </para>
/// </remarks>
public sealed partial class DemoSmokeTests(ITestOutputHelper output) : IAsyncLifetime
{
    private const string Origin = "https://demo.test";
    private static readonly string[] Themes = ["light", "dark", "high-contrast"];

    /// <summary>Tracked, understood violations as "route|axe-rule-id". Keep empty.</summary>
    private static readonly HashSet<string> KnownIssues = [];

    /// <summary>
    /// Themes in which <c>color-contrast</c> is reported but not yet enforced. The light theme is fully enforced.
    /// The demo shell's own dark/high-contrast colours (nav links, header links, tab buttons, API-table names) and a
    /// few library components in high contrast still fail; triaging them is tracked in
    /// <c>plan/known-issues-and-backlog.md</c>. Every other axe rule is enforced in every theme. Remove a theme
    /// from this list as soon as its contrast findings are fixed.
    /// </summary>
    private static readonly HashSet<string> ThemesWithUnenforcedContrast = ["dark", "high-contrast"];

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    [GeneratedRegex("^@page\\s+\"(?<route>[^\"]+)\"", RegexOptions.Multiline)]
    private static partial Regex PageDirective();

    public async Task InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }

    [Fact]
    public async Task Every_Demo_Route_Renders_In_Every_Theme_Without_Console_Errors_Or_Axe_Violations()
    {
        var demoDirectory = Environment.GetEnvironmentVariable("CYMRU_DEMO_DIR");

        if (string.IsNullOrWhiteSpace(demoDirectory))
        {
            if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail("CYMRU_DEMO_DIR must point at a published demo's wwwroot folder in CI.");
            }

            output.WriteLine("CYMRU_DEMO_DIR is not set; skipping the published-demo smoke run.");
            return;
        }

        Directory.Exists(demoDirectory).ShouldBeTrue($"CYMRU_DEMO_DIR '{demoDirectory}' does not exist.");

        var routes = DiscoverRoutes();
        routes.Count.ShouldBeGreaterThan(10, "Route discovery found suspiciously few @page routes.");
        output.WriteLine($"Smoke testing {routes.Count} routes x {Themes.Length} themes.");

        var failures = new List<string>();
        var unenforced = new List<string>();

        foreach (var theme in Themes)
        {
            await SmokeThemeAsync(demoDirectory, routes, theme, failures, unenforced);
        }

        foreach (var line in unenforced)
        {
            output.WriteLine($"not enforced: {line}");
        }

        failures.ShouldBeEmpty(string.Join(Environment.NewLine, failures));
    }

    private async Task SmokeThemeAsync(string demoDirectory, List<string> routes, string theme, List<string> failures, List<string> unenforced)
    {
        await using var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ServiceWorkers = ServiceWorkerPolicy.Block,
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });

        // The theme service reads this key before anything renders.
        await context.AddInitScriptAsync($"try {{ localStorage.setItem('cymru-blazor-theme', '{theme}'); }} catch (e) {{}}");
        await context.RouteAsync($"{Origin}/**", route => ServeAsync(route, demoDirectory));

        var page = await context.NewPageAsync();
        var currentRoute = "(startup)";
        var consoleErrors = new List<string>();

        page.Console += (_, message) =>
        {
            if (message.Type == "error" && !message.Text.Contains("favicon", StringComparison.OrdinalIgnoreCase))
            {
                consoleErrors.Add($"{currentRoute}: console error: {message.Text}");
            }
        };
        page.PageError += (_, error) => consoleErrors.Add($"{currentRoute}: unhandled exception: {error}");

        await page.GotoAsync($"{Origin}{routes[0]}");

        foreach (var route in routes)
        {
            currentRoute = route;

            try
            {
                if (!string.Equals(await page.EvaluateAsync<string>("() => location.pathname"), route, StringComparison.Ordinal))
                {
                    // Mark the outgoing page's heading so a stale one is never mistaken for the new page's, even
                    // when two pages share the same heading text.
                    await page.EvaluateAsync("() => document.querySelectorAll('h1').forEach(h => h.setAttribute('data-stale', ''))");
                    await page.EvaluateAsync("(path) => Blazor.navigateTo(path)", route);
                    await page.WaitForFunctionAsync(
                        "(path) => location.pathname === path && document.querySelector('h1:not([data-stale])') !== null",
                        route,
                        new PageWaitForFunctionOptions { Timeout = 15_000 });
                }
                else
                {
                    await page.WaitForSelectorAsync("h1", new PageWaitForSelectorOptions { Timeout = 60_000 });
                }

                await page.WaitForTimeoutAsync(150);

                var result = await page.RunAxe(new AxeRunOptions
                {
                    Rules = new Dictionary<string, RuleOptions> { ["page-has-heading-one"] = new() { Enabled = false } }
                });

                foreach (var violation in result.Violations.Where(v => !KnownIssues.Contains($"{route}|{v.Id}")))
                {
                    var first = violation.Nodes.FirstOrDefault()?.Html ?? string.Empty;
                    var summary = $"[{theme}] {route}: axe {violation.Id} ({violation.Nodes.Length} node(s)), e.g. {(first.Length > 120 ? first[..120] : first)}";

                    if (violation.Id == "color-contrast" && ThemesWithUnenforcedContrast.Contains(theme))
                    {
                        unenforced.Add(summary);
                        continue;
                    }

                    failures.Add(summary);
                }
            }
            catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
            {
                failures.Add($"[{theme}] {route}: did not render a heading: {ex.Message.Split('\n')[0]}");
            }
        }

        failures.AddRange(consoleErrors.Select(e => $"[{theme}] {e}"));
    }

    private static async Task ServeAsync(IRoute route, string demoDirectory)
    {
        var path = Uri.UnescapeDataString(new Uri(route.Request.Url).AbsolutePath).TrimStart('/');
        var root = Path.GetFullPath(demoDirectory);
        var file = Path.GetFullPath(Path.Combine(root, path));

        if (!file.StartsWith(root, StringComparison.Ordinal))
        {
            await route.FulfillAsync(new RouteFulfillOptions { Status = 403 });
            return;
        }

        // SPA fallback: an extension-less path is a client-side route.
        if (!File.Exists(file))
        {
            if (Path.HasExtension(path))
            {
                await route.FulfillAsync(new RouteFulfillOptions { Status = 404 });
                return;
            }

            file = Path.Combine(root, "index.html");
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            ContentType = ContentTypeFor(file),
            BodyBytes = await File.ReadAllBytesAsync(file)
        });
    }

    private static string ContentTypeFor(string file) => Path.GetExtension(file).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".js" or ".mjs" => "text/javascript",
        ".css" => "text/css",
        ".json" => "application/json",
        ".wasm" => "application/wasm",
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        ".ico" => "image/x-icon",
        ".woff2" => "font/woff2",
        ".webmanifest" => "application/manifest+json",
        _ => "application/octet-stream"
    };

    private static List<string> DiscoverRoutes()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CymruBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        var demo = Path.Combine(
            directory?.FullName ?? throw new InvalidOperationException("Could not locate CymruBlazor.slnx."),
            "src",
            "CymruBlazor.Demo");

        return Directory
            .EnumerateFiles(demo, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(f => PageDirective().Matches(File.ReadAllText(f)).Select(m => m.Groups["route"].Value))
            .Where(r => !r.Contains('{', StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }
}
