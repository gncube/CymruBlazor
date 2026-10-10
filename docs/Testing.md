# Testing your CymruBlazor pages

Two kinds of test catch different problems, and CymruBlazor uses both on itself:

| Kind | Tool | Catches | Does not catch |
|---|---|---|---|
| Component test | [bUnit](https://bunit.dev) | Wrong markup, missing labels, wrong ARIA wiring, behaviour on click and input | Anything that depends on CSS: contrast, focus rings, hidden text |
| Browser accessibility scan | Playwright and [axe-core](https://github.com/dequelabs/axe-core) | Colour contrast, focus visibility, anything the CSS cascade decides, in each theme | Behaviour (use bUnit for that) |

This page gives you one base class for each to copy into your own test projects. They are plain code in your
repository, so there is no extra package to depend on.

## bUnit base class

Add `bunit`, `xunit` and `Microsoft.NET.Test.Sdk` to a test project that references your app (and so `CymruBlazor`).

```csharp
using Bunit;
using CymruBlazor.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Tests;

/// <summary>
/// A bUnit context that has CymruBlazor registered the way an app registers it.
/// Without AddCymruBlazor() no CymruBlazor component renders: they all need
/// IComponentIdGenerator, and most need the localiser and diagnostics services too.
/// </summary>
public abstract class CymruTestContext : BunitContext
{
    protected CymruTestContext()
    {
        Services.AddCymruBlazor();

        // The library's overlay, input and theme modules call into JavaScript.
        // Loose mode accepts those calls so render tests do not need a browser.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
```

Use it like this:

```csharp
public sealed class ReferralFormTests : CymruTestContext
{
    [Fact]
    public void Name_field_has_an_accessible_label()
    {
        var cut = Render<ReferralForm>();

        var input = cut.Find("input[name='patient-name']");
        var labelId = input.GetAttribute("id");

        Assert.NotNull(cut.Find($"label[for='{labelId}']"));
    }
}
```

Notes:

- Pass options with `Services.AddCymruBlazor(o => ...)` if you want to test with a particular diagnostics mode (see
  [Diagnostics](Diagnostics.md)). Call it once only.
- Replace a service after registering, for example your own `ICyLocalizer`, with `Services.AddScoped<ICyLocalizer, MyLocalizer>()`;
  the last registration wins.
- bUnit tests assert on markup. Do not assert on the exact CSS classes the library uses; they are not part of its
  public contract. Assert on roles, names, `aria-*` attributes and text.

## axe base class

This renders a component with bUnit, puts the markup and the library's real CSS in Chromium, and runs axe-core.
Add `Deque.AxeCore.Playwright` and `Microsoft.Playwright` (plus the bUnit packages above) to a **separate** test
project, because Playwright is large (about 184 MB of packages) and needs a browser.

Install the browser once per machine, after building the project once so the script exists:

```powershell
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

```csharp
using System.Globalization;
using System.Text;
using Bunit;
using CymruBlazor.Extensions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;

namespace MyApp.AccessibilityTests;

public abstract class CymruAxeTestBase : BunitContext, IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    protected CymruAxeTestBase()
    {
        Services.AddCymruBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Extra CSS to load after the library's, for example your app's own stylesheet.
    /// Return the file's text, or null for none.
    /// </summary>
    protected virtual string? AppCss => null;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _page = await _browser.NewPageAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_page is not null) { await _page.CloseAsync(); }
        if (_browser is not null) { await _browser.DisposeAsync(); }
        _playwright?.Dispose();

        await base.DisposeAsync();
    }

    /// <summary>Renders a component with bUnit, then scans it with axe-core.</summary>
    protected Task<AxeResult> ScanComponentAsync<TComponent>(
        Action<ComponentParameterCollectionBuilder<TComponent>>? parameters = null,
        string theme = "light")
        where TComponent : IComponent
    {
        var cut = parameters is null ? Render<TComponent>() : Render(parameters);

        return ScanMarkupAsync(cut.Markup, theme);
    }

    /// <summary>Scans markup. <paramref name="theme"/> is "light", "dark" or "high-contrast".</summary>
    protected async Task<AxeResult> ScanMarkupAsync(string bodyMarkup, string theme = "light")
    {
        var html = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\" />")
            .AppendLine("<title>Accessibility test host</title>")
            .AppendLine(CultureInfo.InvariantCulture, $"<style>{LibraryCss.Value}</style>")
            .AppendLine(CultureInfo.InvariantCulture, $"<style>{AppCss}</style>")
            .AppendLine("</head><body><main>")
            // data-theme goes on this wrapper, as the real CyThemeProvider does. The dark and
            // high-contrast selectors target it; on <html> a "dark" scan would silently test light.
            .AppendLine(CultureInfo.InvariantCulture, $"<div class=\"cy-theme-provider\" data-theme=\"{theme}\">")
            .AppendLine(bodyMarkup)
            .AppendLine("</div></main></body></html>")
            .ToString();

        await _page!.SetContentAsync(html);

        return await _page.RunAxe(new AxeRunOptions
        {
            Rules = new Dictionary<string, RuleOptions>
            {
                // A component is not a whole page, so it has no h1 of its own.
                ["page-has-heading-one"] = new() { Enabled = false }
            }
        });
    }

    private static readonly Lazy<string> LibraryCss = new(LoadLibraryCss);

    /// <summary>
    /// Reads the CSS that ships in the CymruBlazor NuGet package (it is a static web asset):
    /// cymrublazor.css for the design tokens and shared styles, and CymruBlazor.bundle.scp.css for the
    /// per-component styles. Set CYMRUBLAZOR_CSS_DIR to the package's staticwebassets folder to
    /// override the search, for example when the NuGet cache is somewhere unusual.
    /// </summary>
    private static string LoadLibraryCss()
    {
        var assembly = typeof(CymruBlazor.Extensions.ServiceCollectionExtensions).Assembly;
        var version = assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .Select(a => a.InformationalVersion.Split('+')[0])
            .FirstOrDefault() ?? assembly.GetName().Version!.ToString(3);

        var cache = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        var dir = Environment.GetEnvironmentVariable("CYMRUBLAZOR_CSS_DIR")
            ?? Path.Combine(cache, "cymrublazor", version.ToLowerInvariant(), "staticwebassets");

        var main = Path.Combine(dir, "css", "cymrublazor.css");

        if (!File.Exists(main))
        {
            throw new FileNotFoundException(
                $"Could not find the CymruBlazor stylesheet at '{main}'. Restore the CymruBlazor package, " +
                "or set CYMRUBLAZOR_CSS_DIR to its staticwebassets folder.");
        }

        var scoped = Path.Combine(dir, "CymruBlazor.bundle.scp.css");

        return File.ReadAllText(main) + (File.Exists(scoped) ? File.ReadAllText(scoped) : string.Empty);
    }
}
```

Use it like this, one test per theme:

```csharp
public sealed class ReferralFormAccessibilityTests : CymruAxeTestBase
{
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Referral_form_has_no_violations(string theme)
    {
        var result = await ScanComponentAsync<ReferralForm>(theme: theme);

        Assert.Empty(result.Violations);
    }
}
```

Notes:

- The scan covers what is rendered. Open a dialog, menu or combobox first (render it with its open state, or render
  the markup after an interaction) to scan the open state; closed overlays hide their content from axe.
- When a scan fails, print `result.Violations` (each has `Id`, `Help` and the `Nodes` with their HTML) so the
  failure message says what to fix.
- Scan each theme. Contrast problems usually appear in one theme only.
- Your own stylesheet: override `AppCss` to return its text so your overrides are scanned too.
- The CSS lookup reads the NuGet cache folder of the package version your project restored. If your build uses a
  project reference to CymruBlazor instead of the package, point `CYMRUBLAZOR_CSS_DIR` at
  `src/CymruBlazor/wwwroot` and copy the scoped bundle (`CymruBlazor.styles.css` in the build output) beside it as
  `CymruBlazor.bundle.scp.css`.

## What the library itself does

The library's own tests use the same two ideas, with a few extras you can copy as needed:

- `tests/CymruBlazor.Tests/TestContextBase.cs` is the bUnit base above, for the library's repository.
- `tests/CymruBlazor.AccessibilityTests/AxeTestBase.cs` is the axe base above, plus `LoadHostedAsync`, which
  serves the library's real JavaScript modules from a fake origin so tests can drive focus containment, dialogs and
  drag and drop in a real browser, and `MeasureAsync` for real layout checks.
- `scripts/Run-AccessibilityTests.ps1` runs the published demo through axe in a Docker Playwright image, which
  supplies the browser and Linux dependencies.

See also [Diagnostics](Diagnostics.md) for the runtime warnings (`CY0001`, `CY0003` to `CY0005`) that catch missing labels
and invalid parameters while you develop.
