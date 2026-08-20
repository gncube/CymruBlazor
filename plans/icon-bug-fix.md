# Implementation Plan - Invalid `CyIcon` Name Bug in samples/Dashboard

**Status:** ✅ Implemented (Option A) - see "Implementation notes" at the bottom
**Owner:** TBC
**Affected project:** `samples/Dashboard`
**Related component (unaffected, used only as evidence):** `CymruBlazor.Icons.IconRegistry`, `CymruBlazor.Components.Content.CyIcon`

---

## 1. Summary

`samples/Dashboard/Layout/MainLayout.razor` currently renders two `CyIcon`
elements with names that are **not** registered icons:

```razor
new("Medical",     "clinical"),
new("Surgical",    "clinical-actions"),
```

(inside the `_wardFilters` list, rendered via
`<CyIcon Name="@filter.Icon" ... />` in the ward-filter navigation rail).

`"clinical"` and `"clinical-actions"` are real strings inside
`IconRegistry`, but they are **domain/category labels** used by the
private `_domains` lookup (`IconRegistry.GetDomain(name)` - which icon
*category* a given icon belongs to, e.g. `"critical"` belongs to domain
`"status"`), not entries in the `Icons` dictionary that actually holds
SVG path markup. `IconRegistry.Exists(name)` checks membership of the
`Icons` dictionary only, so it correctly reports both strings as
**not** valid icon names.

## 2. Impact

`CyIcon.ValidateParameters()` throws an `ArgumentException` for any
unregistered name:

```csharp
protected override void ValidateParameters()
{
    base.ValidateParameters();

    if (!IconRegistry.Exists(Name))
    {
        throw new ArgumentException(
            $"Unknown icon name '{Name}'. See {nameof(IconRegistry)}.{nameof(IconRegistry.AllNames)} " +
            "for the full list of available icons.",
            nameof(Name));
    }
}
```

This is **not** a cosmetic issue - it is a hard runtime crash. Blazor
throws this exception during the component's parameter-set lifecycle
step, before any markup is produced, so the entire Dashboard app fails
to render as soon as the primary navigation sidebar (which lists all
five ward filters unconditionally) is reached. There is no fallback
icon or silent no-op.

## 3. Root cause

Both bad values were introduced by hand (not generated/copy-pasted from
a working reference) when the ward-filter list was authored, most
likely by mistakenly reusing the *domain* name shown in
`IconRegistry`'s own doc comment (`/// Gets the documentation domain an
icon belongs to (e.g. "clinical", ...`) as if it were an icon name.
There is no compile-time guard against this today - `Name` is a plain
`string` parameter (deliberately, per its own XML doc: *"there is no
fixed enum, since the registry is the source of truth and grows
independently of this component"*), so nothing catches a typo/wrong
value until the component actually renders.

## 4. Fix options

### Option A (recommended) - correct the two values, add a regression test

Replace the two bad values with real, semantically-close registered
icon names. Candidates already available in `IconRegistry.AllNames`
that fit "Medical" and "Surgical" ward-filter rows:

| Filter row | Current (broken) | Proposed replacement | Why |
|---|---|---|---|
| Medical | `"clinical"` | `"pill"` | Registered; reads clearly as "medical/medication" in a ward-filter context. |
| Surgical | `"clinical-actions"` | `"syringe"` | Registered; reads clearly as "surgical/procedural". |

(Both `"pill"` and `"syringe"` are already used correctly elsewhere in
the codebase's icon domain mapping as `clinical-actions`-domain icons,
so they're thematically consistent even though the *domain* string
itself must never be passed as the `Name`.)

This is a one-line-per-row change in
`samples/Dashboard/Layout/MainLayout.razor`'s `_wardFilters` array.

Additionally, add a lightweight **build-time or test-time guard** so a
mistake like this fails fast instead of shipping:

- A small xUnit test in `tests/CymruBlazor.Tests` (or a new
  `CymruBlazor.SamplesTests`/`ApprovalTests` project, if samples aren't
  currently covered by any test project) that bUnit-renders
  `samples/Dashboard/Layout/MainLayout.razor` and asserts it renders
  without throwing. This directly would have caught this bug.
- Optionally, a second, more general test that walks every hard-coded
  `CyIcon Name="..."` string literal across `samples/**/*.razor` (via a
  simple regex/Roslyn syntax-tree scan in a test) and asserts
  `IconRegistry.Exists(name)` for each - this catches the *class* of
  bug everywhere in samples, not just this one occurrence, and would
  also cover the `HealthcarePortal` and `StarterApp` samples.

### Option B - make `IconRegistry.Exists` alias-tolerant (not recommended)

Could redefine `Exists`/`GetMarkup` to fall back to *some* icon when
given a domain name (e.g. resolve `"clinical"` to the first icon in
that domain). Rejected: this silently changes what an ambiguous name
resolves to, is surprising, and papers over the actual authoring
mistake rather than surfacing it - the whole point of
`ValidateParameters` throwing is to catch exactly this class of typo
during development rather than at runtime.

### Option C - relax `CyIcon` to render nothing / a placeholder on an unknown name

Rejected: this would convert a loud, catchable failure into a silent
one (a missing icon that nobody notices), which is worse for both
developers and, more importantly, for accessibility - a blank/void
icon in a ward-filter row with no visible fallback is a regression an
end user could hit with no warning.

**Decision: proceed with Option A.**

## 5. Proposed change set

1. `samples/Dashboard/Layout/MainLayout.razor`
   - `_wardFilters`: change `"clinical"` -> `"pill"`, `"clinical-actions"` -> `"syringe"`.
2. `tests/CymruBlazor.Tests/Samples/DashboardMainLayoutTests.cs` (new)
   - bUnit test rendering `MainLayout` with a stub `IThemeService` and
     asserting no exception is thrown and all five ward-filter rows are
     present.
3. `tests/CymruBlazor.Tests/Samples/SampleIconNameTests.cs` (new, optional
   but recommended)
   - Scans `samples/**/*.razor` for `Name="..."`/`Name="@..."` literal
     values passed to `CyIcon` and asserts each literal (skipping
     `@`-bound expressions, which can't be statically checked this way)
     exists in `IconRegistry.AllNames`.
4. No changes to `CymruBlazor` (the library itself) are required -
   `IconRegistry`/`CyIcon` are both behaving correctly; the bug is
   entirely in sample content.

## 6. Out of scope

- Adding a compile-time-checked icon name type (e.g. a source
  generator producing a `partial` enum/const class from
  `IconRegistry`) would prevent this entire class of bug permanently,
  but is a larger design change to a public, documented API
  (`CyIcon.Name` is deliberately a plain `string`, see its XML doc) and
  should be proposed and reviewed separately, not bundled into this
  fix.
- No other samples (`HealthcarePortal`, `StarterApp`) currently
  reference `"clinical"`/`"clinical-actions"` as `CyIcon` names (only
  `samples/Dashboard` does), so no other files need the same literal
  correction - though they would benefit from item 3's regression test
  once it exists.

## 7. Verification

- `dotnet build` the solution - the two edits alone cannot be
  verified without a build (a valid icon name string still compiles
  either way; the failure is a runtime `ArgumentException`), so the new
  bUnit test in item 2 is the actual verification step, not the
  compiler.
- Run `dotnet test` and confirm `DashboardMainLayoutTests` (and
  `SampleIconNameTests`, if included) pass.
- Manually run `samples/Dashboard` and open the primary navigation
  sidebar in each `CollapseMode` (via the new "Sidebar" selector added
  in `MainLayout.razor`) to visually confirm all five ward-filter rows
  render an icon.

## 8. Implementation notes (post-fix)

Implemented as Option A, with one adjustment from the original plan:

- `samples/Dashboard/Layout/MainLayout.razor`: `_wardFilters` changed
  `"clinical"` -> `"pill"` and `"clinical-actions"` -> `"syringe"`,
  exactly as proposed.
- `tests/CymruBlazor.Tests/Samples/DashboardMainLayoutTests.cs` (new):
  bUnit-renders the real `MainLayout` (via a new `ProjectReference` from
  `CymruBlazor.Tests.csproj` to `samples/Dashboard/Dashboard.csproj`)
  and asserts it renders without throwing, that all 9 nav/filter rows
  produce a `<svg class="cy-icon">`, and that switching through all
  four `SidebarCollapseMode` values via the sample's own "Sidebar"
  `<select>` doesn't break anything. This is the test that actually
  would have caught the original bug - the bad values only ever flowed
  through a bound field (`Name="@filter.Icon"`), not a literal
  attribute, so only rendering the component (not scanning source text)
  can catch that class of mistake.
- `tests/CymruBlazor.Tests/Samples/SampleIconNameTests.cs` (new,
  implemented as planned): statically scans every literal (non-`@`-bound)
  `<CyIcon Name="...">` value across all of `samples/**/*.razor` against
  `IconRegistry.Exists`. Kept as a defence-in-depth companion, with its
  file-level doc comment explicitly noting it would *not* have caught
  this particular bug (bound value, not literal) - it guards against
  the more common literal-typo case instead, across every sample.
- No changes to `CymruBlazor` (the library) were needed, confirming the
  original root-cause analysis - `IconRegistry`/`CyIcon` were already
  correct.
- **Deviation from the plan / open risk:** I could not run `dotnet
  build`/`dotnet test` in the environment that produced this change (no
  .NET SDK, network locked to a small domain allowlist that excludes
  NuGet/dotnet feeds), so the new `ProjectReference` from the xunit test
  project to the `Microsoft.NET.Sdk.BlazorWebAssembly` Dashboard project
  is unverified. This is a well-established, commonly-used pattern for
  bUnit-testing Blazor WASM app components, but please run `dotnet
  build`/`dotnet test` locally before relying on it in CI.
