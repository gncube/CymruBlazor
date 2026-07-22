# CymruBlazor - First NuGet Release: Implementation Plan

**Status:** Decisions confirmed (`0.1.0-preview.1`, MinVer). Phases A-D
implemented in this pass. Phases E-F (dry-run pack against a real SDK, and
tagging/publishing) require a real .NET environment and repo access - see
the end of this document for exactly what's left and who needs to do it.

---

## 1. Audit: What Actually Exists Today

Before planning the release, here's the current, verified state of the repo
(not the aspirational state from the PRD/Spec):

### 1.1 Components actually implemented

| Category | Implemented | Planned in PRD but **not yet built** |
|---|---|---|
| Layout | `CyContainer`, `CyStack`, `CySidebar`, `CyCluster`, `CyGrid`, `CyCenter` | `CymruNavigation`, `CymruHeroBanner`, `CymruFooter`, `CymruBreadcrumb`, `CymruPageHeader`, `CymruSkipLink` |
| Forms | `Button` | `TextBox`, `Select`, `Checkbox`, Validation summary |
| Content | *(none)* | `Card`, `Alert`, `Typography` |
| Accessibility | `FocusTrap`, `CyLiveRegion`, `CyScreenReaderOnly` | - |
| Infrastructure | `ThemeService` (C# service, no `CymruThemeProvider` component yet) | `CymruThemeProvider` component |

### 1.2 Test coverage

- `tests/CymruBlazor.Tests` has 11 test files covering layout primitives,
  accessibility components, and `ThemeService`.
- `tests/CymruBlazor.ApprovalTests` and `tests/CymruBlazor.AccessibilityTests`
  are **empty scaffolding** - `IsPackable=false` is correctly set, all the
  right packages (`ApprovalTests`, `Deque.AxeCore.Playwright`,
  `Microsoft.Playwright`) are referenced, but there isn't a single test in
  either project yet. The Spec's "80% coverage" / "accessibility tests
  required" bar is not met.

### 1.3 Packaging & versioning

- `src/CymruBlazor/CymruBlazor.csproj` already has solid NuGet metadata:
  `PackageId`, description, MIT licence expression, repo URL, tags,
  `PackageReadmeFile`, SourceLink, `snupkg` symbols, `GenerateDocumentationFile`.
- **`<PackageIcon>icon.png</PackageIcon>` is set, but no `icon.png` exists
  anywhere in the repo.** The `<None Include="icon.png" ... Condition="Exists('icon.png')" />`
  means the file is silently *not* packed when missing - but `PackageIcon`
  still points at it. `dotnet pack` will fail with **NU5046** ("icon file
  does not exist in the package"). **This blocks release today.**
- **Two versioning tools are configured, inconsistently.** `GitVersion.yml`
  exists at the repo root (Mainline mode, Conventional Commits), but nothing
  references `GitVersion.MsBuild`. Instead, `MinVer` is referenced in
  `CymruBlazor.csproj`. The csproj's own comment - *"Assembly identity
  (GitVersion patches these at build time)"* - describes tooling that isn't
  wired up. Only one of these should ship.
- `CymruBlazor.Theming` and `CymruBlazor.Icons` use `Sdk.Razor` (packable by
  default) but have **no NuGet metadata and no `IsPackable=false`**. Left
  as-is, `dotnet pack` on the solution produces two low-quality, undocumented
  packages nobody asked for.

### 1.4 CI/CD

- **`.github/workflows/` contains zero files.** There is no build, test,
  pack, or publish automation at all, despite the PRD/Spec requiring fully
  automated releases via GitHub Actions.
- `README.md` already links a `Build` badge to
  `actions/workflows/ci.yml` - a workflow that doesn't exist, so the badge
  is currently broken/red-linked.

### 1.5 Documentation & community health files

- `README.md`, `LICENSE` (MIT) exist and are reasonable.
- `README.md` references `CONTRIBUTING.md` - **this file doesn't exist.**
- No `CHANGELOG.md`.
- `docs/` has a single scaffold guide, not end-user component docs (the
  Demo app is the intended living documentation per the PRD, which is
  appropriate, but it isn't deployed anywhere yet - e.g. GitHub Pages).

---

## 2. The Central Decision: What Version Number Ships First?

Given section 1.1, shipping `1.0.0` would misrepresent the package - a
consumer installing "1.0.0" reasonably expects the PRD's advertised
component set (navigation, cards, alerts, forms). What's actually ready is a
solid **layout/accessibility/theming foundation** plus one form control.

**Recommendation:** ship the first release as a **pre-release, `0.1.0-preview.1`**,
built on tags via MinVer (see 3.1), with an explicit "What's in this
release" section in the README/release notes. Move to `1.0.0` once the PRD's
Layout + Content + Forms v1 scope (section 6 of the PRD) is actually
implemented and tested.

This is a recommendation, not a blocker - if you'd rather ship what exists
as `1.0.0` and treat future components as `1.x` minor additions, that's a
valid alternative; flag your preference and I'll adjust the plan
accordingly.

---

## 3. Implementation Plan

### Phase A - Packaging correctness (blocker fixes)

1. **Resolve the icon.** Either add a real `icon.png` (128×128 or 512×512
   PNG, <1MB, `Pack="true" PackagePath="\"`) to `src/CymruBlazor/`, or
   remove `<PackageIcon>` / the `<None Include="icon.png">` line entirely
   until artwork exists. Recommendation: remove for the first pre-release
   rather than block on artwork - add it back in a follow-up release.
2. **Pick one versioning tool.**
   - Recommended: **keep MinVer**, since it's already wired into
     `CymruBlazor.csproj` and needs zero extra configuration - it derives
     the version entirely from git tags (`v0.1.0-preview.1`) with no build
     server state to keep in sync.
   - Delete `GitVersion.yml` and correct the misleading comment in the
     csproj, **or** if Conventional-Commits-driven auto-bumping (as the
     PRD specifies) is a hard requirement, swap the other way: remove
     `MinVer`, add `GitVersion.MsBuild`, and drive the CI workflow (Phase B)
     from it instead. Needs a decision before Phase B is built, since the
     workflow's version-derivation step depends on it.
3. **Explicitly scope what gets packed.** Add `<IsPackable>false</IsPackable>`
   to `CymruBlazor.Demo`, `CymruBlazor.Theming`, and `CymruBlazor.Icons`
   (samples and tools already won't be packed, tests already correctly
   opt out). Only `CymruBlazor` ships to NuGet in this release. Theming/Icons
   remain project references consumed internally until they get their own
   release-readiness pass (matches the PRD's "core package stays
   lightweight" principle for future packages).
4. **Verify SourceLink / symbols end-to-end** by running a real
   `dotnet pack -c Release` once a .NET SDK is available (not possible in
   this sandbox) and inspecting the resulting `.nupkg`/`.snupkg` with
   `dotnet nuget verify` or NuGet Package Explorer.

### Phase B - CI/CD pipeline

Add `.github/workflows/ci.yml` (runs on every push/PR - build, test, pack
as a verification-only artifact, no publish) and
`.github/workflows/release.yml` (runs on tag push matching `v*` - build,
test, pack, publish to NuGet, create GitHub Release with generated notes).
This matches the pipeline both the PRD and Spec already describe:

```
Restore → Build → Test → Pack → Publish Demo → Publish NuGet → Create Release
```

Concretely:

1. `ci.yml`
   - Trigger: `push` to `main`, `pull_request`.
   - `actions/checkout` with `fetch-depth: 0` (required for MinVer/GitVersion
     to see tag history).
   - `dotnet restore`, `dotnet build -c Release --no-restore`.
   - `dotnet test -c Release --no-build` across all three test projects
     (fails the workflow if any test fails - this is the gate that makes
     Phase C's test-writing work matter).
   - `dotnet pack src/CymruBlazor/CymruBlazor.csproj -c Release -o ./artifacts`
     as a build-verification step (catches packaging regressions like the
     icon issue on every PR, not just at release time).
   - Upload the `./artifacts` folder as a workflow artifact for inspection.
2. `release.yml`
   - Trigger: `push` of tags matching `v[0-9]+.[0-9]+.[0-9]+*`.
   - Same restore/build/test/pack steps.
   - `dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate`.
   - Push the matching `.snupkg` symbol package alongside it.
   - Use `softprops/action-gh-release` (or `gh release create`) with
     `generate_release_notes: true` to produce the GitHub Release from
     merged PR titles/Conventional Commits.
   - Optional but recommended: a `publish-demo.yml` (or a job in the same
     workflow) that runs `dotnet publish src/CymruBlazor.Demo -c Release`
     and deploys the WASM output to GitHub Pages, so the Demo app becomes
     a real, linkable "living documentation" site per the PRD - right now
     nothing publishes it anywhere.
3. **Repository secrets needed before this can run:** `NUGET_API_KEY`
   (scoped to the `CymruBlazor` package on nuget.org, not a full-account
   key). This has to be created and added to GitHub repo secrets manually -
   I can write the workflow YAML, but generating/storing the actual key is
   outside what I can do from here.
4. Fix the already-broken `Build` badge in `README.md` once `ci.yml`
   exists at that exact path/name.

### Phase C - Quality gate before tagging a release

1. Run the full existing test suite locally/in CI and confirm all 11
   existing tests pass (untested in this sandbox - no .NET SDK available
   here; this must happen in a real environment before Phase E).
2. Either (a) add a minimal but real set of tests to
   `CymruBlazor.ApprovalTests` and `CymruBlazor.AccessibilityTests` so the
   Spec's testing requirement is genuinely met for the components shipping
   in this release (Layout primitives + `Button` + Accessibility utilities -
   a small, achievable scope), or (b) explicitly descope those two projects
   from the "definition of done" for this pre-release and track it as
   follow-up work. Recommendation: (a) for the accessibility tests
   specifically, since shipping a healthcare-sector accessibility library
   with zero automated a11y test coverage is the highest-risk gap here;
   defer full `ApprovalTests` snapshot coverage if time-constrained.
3. Confirm `TreatWarningsAsErrors` (already `Condition="'$(CI)' == 'true'"`
   in `Directory.Build.props`) doesn't fail the build once CI actually sets
   `CI=true` - this has never run in CI, so latent warnings may surface for
   the first time in Phase B.

### Phase D - Documentation & community health

1. Add `CONTRIBUTING.md` (README already links it - currently a 404).
2. Add `CHANGELOG.md`, seeded from the release notes convention Phase B's
   GitHub Release automation will use, so history isn't only on GitHub.
3. Update `README.md`:
   - Replace the "Getting Started" `dotnet add package` snippet's implied
     version with an explicit `--version 0.1.0-preview.1` note while
     pre-release, and/or a `--prerelease` flag callout, since `dotnet add
     package` won't resolve pre-release versions by default.
   - Add the "what's in this release / what's coming" scope note from
     section 2 above, so early adopters have accurate expectations.
4. Link the Demo site once Phase B's GitHub Pages deploy exists.

### Phase E - Dry run

1. Once Phases A-D land, cut a **local-only** pack (`dotnet pack -c
   Release`) and consume it from one of the existing `samples/` apps via a
   local NuGet feed (`dotnet nuget add source ./artifacts -n local`) to
   confirm the package actually works end-to-end for a consumer -
   including that `_content/CymruBlazor/css/cymrublazor.css` resolves and
   renders correctly (directly exercises the CSS bundling fixed in the
   prior phase).
2. Fix anything the dry run surfaces before tagging.

### Phase F - Tag & release

1. Merge everything to `main`.
2. Tag `v0.1.0-preview.1` (or the agreed version from section 2) and push
   the tag - this triggers `release.yml`.
3. Verify the package appears on nuget.org and the GitHub Release/notes
   look right.
4. Announce (repo README badge, any relevant NHS Wales/DHCW community
   channels) - outside the scope of this repo but worth planning for.

---

## 4. Risks

- **No .NET SDK in this sandbox** - none of Phases A-C can be *executed and
  verified* here, only authored. Everything needs a real build agent
  (exactly what Phase B's CI provides) before it can be trusted.
- **Two versioning tools** (MinVer vs GitVersion) is a decision that
  affects the CI workflow's shape - resolving it (Phase A.2) should happen
  before Phase B is written, not after, to avoid rework.
- **NUGET_API_KEY provisioning** is a manual, human step outside anything
  I can do from this environment.
- Shipping `0.1.0-preview` honestly with a small surface area is lower risk
  than shipping `1.0.0` against an incomplete component set and having to
  walk back compatibility expectations later.

## 5. Definition of Done for This Release

- [ ] `dotnet pack -c Release` succeeds with no NU5xxx warnings.
- [ ] Exactly one NuGet package (`CymruBlazor`) is produced from the solution pack step.
- [ ] All existing tests pass in CI.
- [ ] `ci.yml` runs green on `main` and on PRs.
- [ ] `release.yml` successfully publishes to nuget.org from a pushed tag.
- [ ] `README.md` badges are green, `CONTRIBUTING.md` link resolves.
- [ ] A sample app can `dotnet add package CymruBlazor --prerelease` and
      render `CyContainer`/`CyStack`/`CySidebar`/`Button` correctly with
      just the single CSS `<link>`.

---

## 7. Implementation Summary (this pass)

Confirmed decisions: **`0.1.0-preview.1`**, **MinVer** (GitVersion removed).

### Phase A - Packaging correctness

- Removed `<PackageIcon>icon.png</PackageIcon>` and the conditional
  `<None Include="icon.png">` from `CymruBlazor.csproj` (was a guaranteed
  `dotnet pack` failure - NU5046). Documented how to add real artwork back
  later.
- Removed the hardcoded `AssemblyVersion`/`FileVersion`/`InformationalVersion`
  `0.0.0` properties and their misleading "GitVersion" comment - MinVer now
  owns all version metadata exclusively.
- Deleted `GitVersion.yml`.
- Added `<MinVerTagPrefix>v</MinVerTagPrefix>` and
  `<MinVerDefaultPreReleaseIdentifiers>preview.0</MinVerDefaultPreReleaseIdentifiers>`
  to `Directory.Build.props`, so version tags look like `v0.1.0-preview.1`
  and local/CI builds without a tag get a sensible `-preview.0` default
  instead of MinVer's raw fallback.
- Added `<IsPackable>false</IsPackable>` to `CymruBlazor.Demo`,
  `CymruBlazor.Theming`, and `CymruBlazor.Icons`, so a solution-wide pack
  only ever produces the one intended `CymruBlazor` package.

### Phase B - CI/CD

- `.github/workflows/ci.yml` - restore/build/test/pack-verify on every push
  to `main` and every PR; uploads test results and the built package as
  workflow artifacts. This is also what makes the previously-broken
  `Build` badge in `README.md` valid.
- `.github/workflows/release.yml` - triggered by `v*` tags; builds, tests,
  packs, verifies exactly one `.nupkg` was produced (fails loudly if the
  `IsPackable` scoping above ever regresses), publishes to nuget.org, and
  creates a GitHub Release with auto-generated notes (marked pre-release
  automatically when the tag contains a hyphen, e.g. `-preview.1`).
- `.github/workflows/publish-demo.yml` - deploys the Demo WASM app to
  GitHub Pages on every push to `main`, rewriting `<base href>` for the
  project-site path and adding the standard SPA `404.html` fallback +
  `.nojekyll`. This finally gives the Demo app - the PRD's intended "living
  documentation" - a real, linkable URL.
- **Not done here (needs a human with repo access):** creating the
  `NUGET_API_KEY` on nuget.org and adding it as a GitHub Actions secret,
  and enabling GitHub Pages (Settings → Pages → Source: GitHub Actions) for
  `publish-demo.yml` to have somewhere to deploy to.

### Phase C - Quality gate

- Added bUnit tests for every previously-untested component:
  `CyContainerTests`, `CyStackTests`, `CyClusterTests`, `CyGridTests`
  (under `tests/CymruBlazor.Tests/Components/Layout/`), and `ButtonTests`.
  These closely follow the existing `CyCenterTests`/`CySidebarTests`
  pattern already in the repo.
- **Deliberately not done here:** populating
  `CymruBlazor.ApprovalTests`/`CymruBlazor.AccessibilityTests`. Writing
  Playwright + `Deque.AxeCore.Playwright` integration tests blind, without
  a .NET SDK available to compile and run them against the actual package
  API surface, risks shipping test code that doesn't build - worse than
  leaving them scaffolded. This is the top follow-up item for whoever picks
  this up next, in a real dev environment.

### Phase D - Documentation

- Added `CONTRIBUTING.md` (README's link was previously a 404), grounded in
  the existing `.github/skills/` coding/testing conventions and the
  CSS-bundle-drift lesson from the prior fix pass.
- Added `CHANGELOG.md`, seeded with the `0.1.0-preview.1` scope and known
  limitations.
- Updated `README.md`: added a pre-release status callout, and changed the
  install snippet to `dotnet add package CymruBlazor --prerelease` (a plain
  `dotnet add package` won't resolve a pre-release version).

## 8. What's Left (Phases E-F, need a real environment)

1. Run `dotnet build` / `dotnet test` / `dotnet pack -c Release` for real
   and fix anything that surfaces - none of Phase A-C's changes have been
   compiled in this sandbox (no .NET SDK available here).
2. Provision `NUGET_API_KEY` and add it as a GitHub Actions secret.
3. Enable GitHub Pages for the repo (Actions-based source).
4. Do the local dry-run pack-and-consume from a `samples/` app described in
   the original Phase E.
5. Push the `v0.1.0-preview.1` tag once 1-4 are green.

