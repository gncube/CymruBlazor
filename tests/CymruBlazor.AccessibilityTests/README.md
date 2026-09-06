# CymruBlazor.AccessibilityTests

Real, automated accessibility tests: each test renders a component via
bUnit, loads the resulting markup - together with the library's actual
CSS - into a real Chromium browser via Playwright, and runs a real
[axe-core](https://github.com/dequelabs/axe-core) scan against it.

This is deliberately not just more bUnit tests. bUnit can assert a CSS
class is present on an element, but it has no way to evaluate the CSS
cascade or compute contrast - so it will happily pass a component whose
colours resolve to invisible or illegible text. See `CHANGELOG.md`'s
`0.1.0-preview.7`/`0.1.0-preview.8` entries for several real examples of
exactly that bug shape shipping past the existing test suite. A real
browser running axe-core catches that class of bug automatically.

## One-time setup

Playwright needs real browser binaries, which aren't part of the NuGet
package and won't be installed by a normal `dotnet build`/`dotnet test`:

```powershell
dotnet build tests/CymruBlazor.AccessibilityTests
pwsh tests/CymruBlazor.AccessibilityTests/bin/Debug/net10.0/playwright.ps1 install chromium
```

(On macOS/Linux, use `pwsh` the same way if you have PowerShell installed,
or `./bin/Debug/net10.0/playwright.sh install chromium` if a shell script
was generated instead - whichever exists in your build output.)

After that one-time step, `dotnet test` runs these like any other test
project.

## Status

**This test project was written without being able to run it.** The
sandbox this was authored in has no Playwright browser binaries and
restricted network access, so none of these tests have actually been
executed - they're written carefully against the documented
`Deque.AxeCore.Playwright`/`Microsoft.Playwright` APIs (verified against
the versions pinned in `Directory.Packages.props`) and cross-checked
against this repo's own already-working bUnit test patterns (e.g. the
`EditContext`/`ValueExpression` wiring in `AxeTestBase`/
`FormFieldAxeTestBase` mirrors `CymruBlazor.Tests`' own
`FormFieldTestContext` exactly), but "carefully written" isn't the same
as "verified." Please run `dotnet test` locally after the one-time
Playwright install above and report back anything that fails to build or
run - the two most likely failure points, if any, are:

1. **`AxeTestBase.FindScopedCssPath()`** - this searches the test
   project's own build output for the generated `CymruBlazor.styles.css`
   (the per-component CSS-isolation bundle). Its exact location is a
   build-output detail that can vary by SDK version; if it isn't found,
   structural accessibility checks (labels, roles, landmarks, ARIA
   attributes) are unaffected, but component-specific visual contrast
   checks may be less accurate than intended. `AxeTestBase.
   FindBundledCssPath()` (the main design-token stylesheet) doesn't have
   this problem - it's a checked-in source path
   (`src/CymruBlazor/wwwroot/css/cymrublazor.css`), not a build artifact.
2. **`Page.RunAxe()`'s exact extension method signature** - written
   against `Deque.AxeCore.Playwright` 4.13.0 per its own documentation
   examples; if that package's API has since changed, the compiler error
   will point directly at the one call site in `AxeTestBase.
   ScanMarkupAsync`.

## What's covered so far

`CyButton`, `CyAlert`, `CyCard`, `CyTextBox`, `CyCheckbox`, `CySelect`,
`CyIcon` - a representative first batch (multiple states/variants each),
not exhaustive coverage of every component. `CyButtonAccessibilityTests`
also includes a dark-mode/high-contrast theme scan, demonstrating the
`theme` parameter on `ScanMarkupAsync` - worth extending to other
components, especially anything that renders on a coloured background
(`CyFooter`, `CyHeader`, `CyAlert`).
