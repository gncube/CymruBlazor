# Contributing to CymruBlazor

Thanks for your interest in contributing. CymruBlazor is an open-source
component library implementing the NHS Wales Design System.

## Before you start

- For anything beyond a small fix, please open an issue first to discuss
  the change - especially for new components, since they need to align
  with the NHS Wales Design System rather than introduce a new visual
  language.
- See `docs/Current-Solution-Structure.md` for a map of the repository, and
  `docs/ADR/` for the design decisions behind existing components.

## Getting set up

Requires the .NET 10 SDK.

```bash
git clone https://github.com/gncube/CymruBlazor.git
cd CymruBlazor
dotnet restore CymruBlazor.slnx
dotnet build CymruBlazor.slnx
dotnet run --project src/CymruBlazor.Demo
```

`dotnet test CymruBlazor.slnx` runs both test projects, including the real
axe-core/Chromium scans in `CymruBlazor.AccessibilityTests`. To skip those
(faster, no browser install required) while iterating:

```bash
dotnet test CymruBlazor.slnx --filter "FullyQualifiedName!~CymruBlazor.AccessibilityTests"
```

`./test-local.ps1` runs exactly that restore/build/test sequence in one
step, mirroring what CI does before the pack/publish stages.

## Working on the samples (`samples/Dashboard`, `HealthcarePortal`, `StarterApp`)

The samples restore CymruBlazor from a local NuGet feed rather than a
published version, so they always build against your working copy of
`src/CymruBlazor`, not whatever was last published to nuget.org. Before
building or running a sample for the first time (and again after any change
to `src/CymruBlazor` you want the samples to pick up), run:

```powershell
./New-LocalPackageFeed.ps1
```

This packs `src/CymruBlazor` into `./artifacts` and writes
`local-package-versions.props`, which `Directory.Packages.props` imports to
pin the samples' `CymruBlazor` package reference to whatever was just
packed. See `docs/Current-Solution-Structure.md` ("NuGet configuration")
for how the three `NuGet.*Config` files relate to this.

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
- Marking something `[Obsolete]` ahead of a 2.0.0 removal? Add it to
  `docs/MIGRATION-2.0.md` in the same PR, alongside the replacement API.

## Tests worth knowing about

- `UndefinedCssVariableTests` fails when library CSS uses a `var(--x)` that no
  stylesheet or C# code defines. Fix the reference or define the token; do not
  add to its allowlist lightly.
- `CymruBlazor.AccessibilityTests` runs real axe-core scans in Chromium.
  Components with themed styling are scanned in the light, dark and
  high-contrast themes.
