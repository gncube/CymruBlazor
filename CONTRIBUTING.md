# Contributing to CymruBlazor

Thanks for your interest in contributing. CymruBlazor is an open-source
component library implementing the NHS Wales Design System.

## Before you start

- For anything beyond a small fix, please open an issue first to discuss
  the change - especially for new components, since they need to align
  with the NHS Wales Design System rather than introduce a new visual
  language (see `PRD.md`, section 3, "Non Goals").
- This repo's coding standards, testing standards, and modern .NET
  conventions are documented as Copilot/agent skills under
  `.github/skills/`. They apply to human contributors just as much as to
  AI-assisted changes - skim them before your first PR.

## Getting set up

Requires the .NET 10 SDK.

```bash
git clone [https://github.com/gncube/CymruBlazor.git](https://github.com/gncube/CymruBlazor.git)
cd CymruBlazor
dotnet restore CymruBlazor.slnx
dotnet build CymruBlazor.slnx
dotnet run --project src/CymruBlazor.Demo

## Releasing and package validation

`CymruBlazor.csproj` sets `EnablePackageValidation` with a
`PackageValidationBaselineVersion`. `dotnet pack` compares the new package
with that published baseline and fails with a `CP****` error on an accidental
breaking API change, so 1.x patch and minor releases stay semver-compatible.

- The baseline is the **last published version**. After publishing `vX.Y.Z`,
  bump `PackageValidationBaselineVersion` to `X.Y.Z` in the next PR.
- CI restores the baseline package from nuget.org (`NuGet.CI.Config`), so
  validation needs no workflow changes.
- Working offline? Pass `-p:EnablePackageValidation=false` to `dotnet pack`.
  CI never does.
- An intentional break (only for a new major version, e.g. 2.0.0) is recorded
  with `dotnet pack -p:GenerateCompatibilitySuppressionFile=true`, which writes
  `CompatibilitySuppressions.xml`; commit it with the change and explain it in
  the PR.
- Release flow: update `CHANGELOG.md`, open a PR, merge, then
  `git tag -a vX.Y.Z -m "vX.Y.Z"` and push the tag (MinVer derives the version
  from the tag).

## Tests worth knowing about

- `UndefinedCssVariableTests` fails when library CSS uses a `var(--x)` that no
  stylesheet or C# code defines. Fix the reference or define the token; do not
  add to its allowlist lightly.
- `CymruBlazor.AccessibilityTests` runs real axe-core scans in Chromium.
  Components with themed styling are scanned in the light, dark and
  high-contrast themes.
