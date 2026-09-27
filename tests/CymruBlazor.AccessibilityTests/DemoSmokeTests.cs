using System.Text.RegularExpressions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CymruBlazor.AccessibilityTests;

///
/// Smoke run over the *published* demo (roadmap 1.3.0-D): every @page route, in light, dark
/// and high-contrast, must render a heading, log no console errors or unhandled exceptions, and pass axe.
/// It is the regression harness for everything the component-level suites cannot see: real routing,
/// the real WebAssembly runtime, the real static web asset URLs and the components composed together.
///
///
///
/// Point CYMRU_DEMO_DIR at the wwwroot folder of a published demo:
/// `dotnet publish src/CymruBlazor.Demo -c Release -o demo-publish`
/// `$env:CYMRU_DEMO_DIR = "$PWD\demo-publish\wwwroot"; dotnet test tests/CymruBlazor.AccessibilityTests --filter DemoSmokeTests`
/// The test serves those files itself (with SPA fallback) from a fake origin, so no web server is needed.
/// When the variable is not set the test is a no-op locally, but fails when CI=true so the
/// pipeline cannot silently skip it.
///
///
/// A violation that is understood and tracked can be listed in  as
/// "route|rule-id" so the run stays green while the fix is scheduled; keep that list empty otherwise.
///
///
public sealed partial class DemoSmokeTests(ITestOutputHelper output) : IAsyncLifetime
{
    private const string Origin = "https://demo.test";
    private static readonly string[] Themes = ["light", "dark", "high-contrast"];

    /// Tracked, understood violations as "route|axe-rule-id". Keep empty.
    private static readonly HashSet<string> KnownIssues = [];

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
    public async Task EveryDemoRouteRendersInEveryThemeWithoutConsoleErrorsOrAxeViolations()
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

        foreach (var theme in Themes)
        {
            await SmokeThemeAsync(demoDirectory, routes, theme, failures);
        }

        WriteReport(routes.Count, failures);

        failures.ShouldBeEmpty(string.Join(Environment.NewLine, failures));
    }

    ///
    /// Writes the full findings to demo-smoke-report.md (in CYMRU_SMOKE_REPORT_DIR, else next to the test
    /// binaries) and, on GitHub Actions, to the job summary, so a CI run can be read without downloading logs or
    /// running Playwright locally.
    ///
    private void WriteReport(int routeCount, List<string> failures)
    {
        var lines = new List<string>
        {
            "# Demo smoke run",
            string.Empty,
            $"{routeCount} routes x {Themes.Length} themes: **{failures.Count} failure(s)**.",
            string.Empty,
            "## Failures",
            string.Empty
        };

        lines.AddRange(failures.Count == 0 ? ["None."] : failures.Select(f => $"- {f.ReplaceLineEndings(" ")}"));

        var report = string.Join(Environment.NewLine, lines);

        try
        {
            var directory = Environment.GetEnvironmentVariable("CYMRU_SMOKE_REPORT_DIR") ?? AppContext.BaseDirectory;
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "demo-smoke-report.md"), report);

            var summary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
            if (!string.IsNullOrWhiteSpace(summary))
            {
                File.AppendAllText(summary, string.Join(Environment.NewLine, lines.Take(400)) + Environment.NewLine);
            }
        }
        catch (IOException ex)
        {
            output.WriteLine($"Could not write the smoke report: {ex.Message}");
        }
    }

    private async Task SmokeThemeAsync(string demoDirectory, List<string> routes, string theme, List<string> failures)
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
