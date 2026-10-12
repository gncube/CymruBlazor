# CymruBlazor.Testing.Axe

Browser accessibility tests for apps built with [CymruBlazor](https://github.com/gncube/CymruBlazor). It renders a
component with bUnit, loads the markup with the library's own CSS into Chromium through Playwright, and scans it with
[axe-core](https://github.com/dequelabs/axe-core), so it finds what depends on the CSS: contrast, focus visibility, hidden
text, in the light, dark and high-contrast themes.

```csharp
using CymruBlazor.Testing.Axe;

public sealed class ReferralFormAccessibilityTests : CymruAxeTestBase
{
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Referral_form_has_no_violations(string theme)
    {
        var result = await ScanComponentAsync<ReferralForm>(theme: theme);

        result.ShouldHaveNoViolations();   // names each rule, how to fix it and the elements affected
    }
}
```

Install the browser once per machine, after building the test project once so the script exists:

```powershell
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

- **`CymruAxeTestBase`** derives from `CymruTestContext` (from `CymruBlazor.Testing`, which this package brings), so you
  can also use bUnit assertions and `ShouldHaveNoMarkupProblems()` in the same class. `ScanComponentAsync<T>()` and
  `ScanMarkupAsync()` scan; `GetPageAsync()` gives you the Playwright page for tests that drive focus or the keyboard.
  The browser starts on the first scan.
- **`AppCss`**: return the text of your own stylesheet to scan your overrides too.
- **`CssDirectory`**: the library's stylesheet is read from the CymruBlazor package's static web assets, found from where
  `CymruBlazor.dll` was restored or from a web project's asset manifest. Override `CssDirectory` or set the
  `CYMRUBLAZOR_CSS_DIR` environment variable to the package's `staticwebassets` folder if neither works.
- **`ShouldHaveNoViolations()`** throws `CymruAxeViolationException` (with `Violations`).

A scan covers what is rendered, so render the open state of a dialog or menu to scan it. Playwright is large (about
184 MB of packages) and needs a browser: keep these tests in a separate project from your bUnit tests. The Playwright
and axe versions come with this package; if you reference `Microsoft.Playwright` yourself, use the same version.

The package version always matches the CymruBlazor version it was built against. See the
[testing guide](https://github.com/gncube/CymruBlazor/blob/main/docs/Testing.md).
