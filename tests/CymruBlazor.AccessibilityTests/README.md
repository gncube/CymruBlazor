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

## Running the tests

The published-demo smoke suite runs in the repository's Docker Playwright
image, which supplies the browser, PowerShell, and Linux dependencies:

```powershell
.\scripts\Run-AccessibilityTests.ps1
```

The wrapper builds `docker/playwright/Dockerfile`, mounts the repository,
restores and publishes the demo inside the container, then runs
`DemoSmokeTests`. Reports are written to `.artifacts/accessibility`.

The component-level accessibility tests can still be run directly with
`dotnet test` when a local Playwright browser installation is available.

## Status

The test project uses the `Deque.AxeCore.Playwright` and
`Microsoft.Playwright` versions pinned in `Directory.Packages.props`. The
Docker runner is the reproducible path for the published-demo smoke suite;
the two main implementation details to verify when changing the test
project are:

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

## Browser suites added in 1.3.0

- `AxeTestBase.LoadHostedAsync(markup, theme, width, height)` serves the page
  from `https://cymru.test/` and maps `/_content/CymruBlazor/*` onto the
  library `wwwroot`, so the page can import the real `cymru-overlay.js` (an ES
  module cannot load from `file://`). Use it when a test needs JavaScript,
  focus, hover or keyboard behaviour; `ScanMarkupAsync` remains the simplest
  axe-only path.
- `MeasureAsync(selector)` returns `ElementMetrics` (visible, in viewport,
  size, page overflow). Prefer it to eyeballing screenshots.
- `OverlayModuleBrowserTests`, `CyDialogAccessibilityTests`,
  `CyTooltipAccessibilityTests`, `ComputedStyleTests`.
- `DemoSmokeTests` drives every demo route in three themes against a
  *published* demo. The Docker runner sets `CYMRU_DEMO_DIR` automatically.
  For a direct local run, publish the demo and set it manually:

  ```powershell
  dotnet publish src/CymruBlazor.Demo -c Release -o demo-smoke
  $env:CYMRU_DEMO_DIR = "$PWD\demo-smoke\wwwroot"
  dotnet test tests/CymruBlazor.AccessibilityTests --filter DemoSmokeTests
  ```

  Without the variable it is a no-op locally and a failure when `CI=true`.

  The Docker accessibility workflow publishes the demo in the container and
  posts the findings to the job summary and an accessibility report artifact.
  Understood, tracked violations can be listed in `KnownIssues` as
  `"route|rule-id"`; keep that empty otherwise.
- Tooltips fade in over ~120ms; wait for it to settle before scanning, or axe
  reports false contrast failures from half-transparent colours.
