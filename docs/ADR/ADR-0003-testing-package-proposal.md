---
status: Accepted
---

# ADR-0003: Testing packages (`CymruBlazor.Testing` and `CymruBlazor.Testing.Axe`)

## Status

Accepted (Phase G, G-B3). Option A was chosen by the maintainer and built in Phase G (TASK-118 to TASK-120). The
original proposal and its recommendation are kept below as the record of the decision; the **Decision** section at the
end says what shipped and how the open questions were settled.

## Context

Recommendation 9.6 asks for the library's bUnit and axe helpers to be reusable by consumers. Today they live in two
test projects and are not packable:

- `CymruBlazor.Tests/TestContextBase.cs`: a `BunitContext` that registers `IComponentIdGenerator`, without which no component renders.
- `CymruBlazor.AccessibilityTests/AxeTestBase.cs`: renders with bUnit, then loads the markup plus the library's real CSS into Chromium and runs axe-core. It finds the CSS by searching this repository's build output, which a consumer does not have.

Two facts shape the design:

1. The axe path pulls in `Microsoft.Playwright` (the package alone is about 184 MB, seen in the CI restore) and a browser download. Forcing that on every consumer that only wants bUnit helpers is wrong.
2. Anything public in a package is a SemVer commitment, checked by `EnablePackageValidation` from the first release.

## Options

| Option | What ships | Cost | Commitment |
|---|---|---|---|
| **A. Two packages** | `CymruBlazor.Testing` (bUnit only) and `CymruBlazor.Testing.Axe` (Playwright and axe) | Two new csproj files, pack and release workflow for three packages, version lock-step through MinVer, CSS resolution for consumers | Two public API surfaces |
| **B. One package** | Everything in `CymruBlazor.Testing` | Smaller than A, but every consumer takes Playwright | One surface, heavy dependency |
| **C. Documentation only** | A `docs/Testing.md` recipe and a copy-paste base class in the Demo | No new project, no workflow change | None |
| **D. Nothing** | | | |

## Sketch of A (what the API would be)

`CymruBlazor.Testing` (depends on `CymruBlazor`, `bunit`):

- `CymruTestContext : BunitContext`: registers `IComponentIdGenerator`, loose JS interop for the library's modules, optional `AddCymruBlazor()`.
- Markup assertions on a rendered fragment, no browser needed: every `aria-labelledby` / `aria-describedby` target exists, ids are unique, every form control has an accessible name, heading levels do not skip. These are the checks the library's own structural tests already make.

`CymruBlazor.Testing.Axe` (depends on the above and `Deque.AxeCore.Playwright`):

- `CymruAxeTestBase`: today's `AxeTestBase`, with the CSS taken from the `CymruBlazor` package's static web assets (`_content/CymruBlazor/...`, resolved from the referencing project's output) rather than searched for, and `ScanMarkupAsync(markup, theme)`.

Open design questions for A: how a consumer's own app CSS is included in a scan; whether `Testing.Axe` pins the Playwright version or leaves it to the consumer (the repo's accessibility workflow reads it from one place today).

## Recommendation

**C now, A when someone outside this repository asks for it.** The helpers are about 150 lines; a documented, copyable base class gives a consumer most of the value with no API to maintain, and the CSS-resolution problem is better solved once there is a real consumer project to design against. If you want the package anyway, choose **A**, not B.

## Work if A is chosen (for the plan)

New projects and `slnx` entries; `Directory.Packages.props` entries; CI and release workflows pack all three packages and verify contents; no `PackageValidationBaselineVersion` for the new packages until their first release; README and docs page; tests for the helpers themselves; CHANGELOG. Roughly three hand-offs.

## Decision

Option A, two packages, released with `CymruBlazor` at one version:

- **`CymruBlazor.Testing`** (bUnit only; depends on `CymruBlazor` and `bunit`): `CymruTestContext` and
  `CymruMarkupChecks` (with `CymruMarkupAssertionException`).
- **`CymruBlazor.Testing.Axe`** (depends on `CymruBlazor.Testing`, `Deque.AxeCore.Playwright` and `Microsoft.Playwright`):
  `CymruAxeTestBase`, `CymruAxeAssertions` and `CymruAxeViolationException`.

The open questions:

- **Consumer app CSS** is added by overriding `CymruAxeTestBase.AppCss`, which returns the text of the stylesheet; it is
  loaded after the library CSS so overrides are scanned.
- **CSS resolution** reads the `CymruBlazor` package's static web assets: first an explicit `CssDirectory` override or the
  `CYMRUBLAZOR_CSS_DIR` variable, then the `staticwebassets` folder beside the restored `CymruBlazor.dll`, then the content
  roots in a web project's `*.staticwebassets.runtime.json`. The CSS is inlined, which is why the packaged (bundled)
  stylesheet works and the repository's `@import` development entry does not.
- **Playwright version** is not pinned by the package beyond what `Deque.AxeCore.Playwright` and the central version
  require. A consumer that references `Microsoft.Playwright` directly should use the same version, and installs the
  browser with the `playwright.ps1` script its own build generates. The repository's accessibility workflow keeps
  reading the version from one place.
- **No base class takes a test-framework dependency** (neither package references xunit), so they work with xunit,
  NUnit and MSTest. `CymruAxeTestBase` starts its browser on first use and releases it through `IAsyncDisposable`.

Release: CI and the release workflow pack all three packages and run `scripts/verify-packages.sh`, which checks that
each exists as a `.nupkg` and `.snupkg` at one version (the tag's, in a release), that each testing package depends on
exactly that version of the package below it, and that the files they rely on are inside them (notably
`staticwebassets/css/cymrublazor.css` in `CymruBlazor`). The two new packages have no
`PackageValidationBaselineVersion` until their first release; after publishing, add one to each project as
`CONTRIBUTING.md` describes. Each public type is a SemVer commitment from that first release.
