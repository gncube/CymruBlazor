# CymruBlazor: Implementation Plan (1.6.0 → 1.11 → 2.0.0)

**Inputs reviewed:** `CymruBlazor-Improvement-Recommendations.md`, `MIGRATION-2.0.md`, the 1.6.0 source zip (library, tests, samples, CI, CSS, JS, ADRs, CHANGELOG).
**Suggested location:** `docs/IMPLEMENTATION-PLAN.md`

> **Caveat.** `dotnet` was not available where this plan was written, so nothing here has been built or run. Every claim marked *(verified)* was read directly in the source; anything else is flagged as an assumption or a spike. Each phase has a CI gate for exactly that reason.

---

## 0. Implementation status

| Phase | State | Evidence |
|---|---|---|
| 0 (pre-flight) | **Mostly done.** 0.1 (tools/ restored), 0.2 (full suite green: 623/623 unit tests + Playwright/axe smoke test), 0.3 (1.7.0 published 2026-10-02; baseline bumped to 1.7.0) and spike 0.4 done. 0.5 and 0.6 remain. | NuGet; local runs |
| A (1.7.0) | **Implemented**, A1–A9 | See below |
| B (1.8.0) | **Implemented**, B1–B5 (see below). Demo pages and R1 not done. | CHANGELOG `[Unreleased]` |
| C (1.9.0) | **Implemented, not yet verified by a build.** C1–C7 components, CSS, bUnit tests, demo pages and CHANGELOG written. Needs the release gate (build, tests, visual pass in 3 themes, axe on new routes, `dotnet pack` vs 1.8.0). | CHANGELOG `[1.9.0]` |
| D–F | Not started | |

**How A was verified (and what wasn't).** .NET 10 was installed in the sandbox, but nuget.org is blocked, so the repo's own test projects (xunit, bUnit, Shouldly, Playwright) could **not** be restored or run. Instead the library was compiled from source with a stub for `Mediator`, with the repo's `.editorconfig`, and exercised through a purpose-built harness (`HtmlRenderer` plus a small event-dispatching renderer): 69 checks, all passing. The library emits only the 14 `ASP0006` warnings already present in the untouched 1.6.0 build. A reflection diff of the public API against the original 1.6.0 build shows **0 removed or changed members** (44 additions). The new bUnit/xunit and axe tests in `tests/` have now passed in CI.

**Spike 0.4 result.** In .NET 10, `InputBase` itself does *not* require an `EditContext`. CymruBlazor's `HasValidationError` dereferences it unguarded and throws `NullReferenceException`. So standalone mode (B3) = make the base class null-safe; no synthetic `EditContext` needed.

**Corrections to the recommendations, found during implementation**

1. **The `@onclick` double-publish bug (§5.1) does not reproduce.** `@onclick` on `<CyButton>` binds to the `OnClick` parameter (Blazor binds parameters case-insensitively) and is guarded; verified against the unmodified 1.6.0 code (0 calls while `Loading`). A splat-capture fix was written, proven unnecessary, and removed. The genuine gap, re-entrancy of an in-flight async handler, is fixed by the opt-in `AutoLoading`. A7 was reworded accordingly.
2. `save` and `undo` already existed in the registry, so 17 icons were missing (plus `unknown`), not 19. `--cymru-shadow-xl` already existed; only `2xl` was added.
3. Theme scoping is the real dark-mode hazard: themes apply to `.cy-theme-provider` and `body:has()`, not `:root`, so a `var()`-derived token declared on `:root` silently keeps its light value. New tokens are declared on all theme scopes, and `scripts/GenerateTokens.cs --check` enforces it (negative-tested: it reports 18 violations when the aliases are `:root`-only). The 1.6.0 token CSS has no violations.
4. Several rows in §1.2 above stay valid; the `CyButton` `@onclick` row there is superseded by item 1.

**Phase 0 status**
- **0.1** Resolved: `tools/CymruBlazor.CssBundler` was a packing omission.
- **0.2/0.3** Done: 1.7.0 is on nuget.org (2026-10-02); the CHANGELOG now records it; `PackageValidationBaselineVersion` is 1.7.0. The Playwright image/package mismatch that failed the accessibility gate is fixed (the image version now derives from `Directory.Packages.props`).
- **0.5** Still to do: DOM snapshot tests for the existing form fields. Do this before Phase B's R1 refactor.
- **0.6** Still to do: consumer release note.
- Run `dotnet pack` once to confirm package validation passes against 1.6.0 (it needs nuget.org access).

## Phase B status (1.8.0, "Make forms adoptable")

| Item | Deliverable | State |
|---|---|---|
| B1 | `CyField` + `CyFieldContext` (any control; `Error`, or `For` + `EditContext`; `Group` mode) | Done |
| B2 | `.cy-input` / `.cy-label` / `.cy-hint` primitives, `aria-invalid` styling | Done |
| B3 | Standalone mode: base class null-safe, no synthetic `EditContext` (per spike 0.4) | Done |
| B4 | `CyNumberInput<T>`, `CyCheckboxGroup<T>` | Done |
| B5 | `Items` on `CySelect`/`CyRadioGroup` via shared `CyOption<T>` (no new type parameter) | Done |
| Demo pages, `FormsOverview` entries, sidebar links | One page each for `CyField`, `CyNumberInput`, `CyCheckboxGroup`; Items example on the Select/Radio pages | **Not done** (see below) |
| R1 | Shared field-chrome refactor | **Not started**. Gate: item 0.5 first |

**How B was verified (and what wasn't).** Same method as A, extended:

- The library compiled from source against the .NET 10 SDK with the Mediator stub and the repo `.editorconfig`: **0 errors and no diagnostics beyond the 7 `ASP0006` warnings the untouched 1.6.0 source also produces** (7 here versus the 14 Phase A reported; not investigated, the point is that the diff against baseline is empty). The trim analyzer is on.
- **Existing fields are unchanged.** 14 scenarios (textbox basic/rich/invalid/disabled, textarea, select, checkbox, radio group, date input, valid and invalid) were rendered with `HtmlRenderer` against the pristine 1.6.0 source and against the modified source: **byte-identical output on all 14.** This is a stand-in for item 0.5, not a replacement for it.
- **New behaviour.** 97 checks through `HtmlRenderer` and a component-reference harness, all passing. Covered: standalone rendering of every field; parse/range/step errors with and without `EditContext` (message shown exactly once); culture parsing; `Items`/`Placeholder`/enum round-trip; checkbox-group ordering, unknown-value retention and null `Value`; `CyField` `Error` precedence, `For` reacting to validation changes, group mode, and unsubscribe on dispose.
- **Public API.** Reflection diff of public and protected members against the 1.7.0 build: **0 removed or changed**; 6 new types, 129 added members.
- **CI passed:** the new bUnit/xunit and axe tests in `tests/` passed in CI. The CSS was not rendered in a browser, so the new styles (`.cy-input-affix`, `.cy-checkbox-group__item-hint` offset, number-input width) still need a visual pass in light, dark and high-contrast.

**Decisions worth knowing**

1. **`Items` takes one shared `CyOption<T>` instead of per-component selector delegates.** Adding a second type parameter to `CySelect<TValue>`/`CyRadioGroup<TValue>` would have broken every existing usage; a record avoids it. `ToCyOptions(...)` covers the "my own type" case.
2. **`CyNumberInput` renders `type="text"` + `inputmode`**, matching `CyTextBox`'s reasoning, and does parsing in C#. Standalone, the framework discards parse messages (they only go to an `EditContext`), so the component keeps its own `LocalError`. Inside an `EditForm` that field stays `null`, which is why the message is not shown twice.
3. **`CyCheckboxGroup` binds `IEnumerable<T>`.** Each change assigns a *new* collection (Items order, then unknown values kept), so the bound property must accept `IEnumerable<T>`; for `List<T>`/`HashSet<T>` properties, bind `Value`/`ValueChanged` separately.
4. **Standalone still needs `@bind-Value`** (or `Value`+`ValueChanged`+`ValueExpression`): `InputBase` builds its `FieldIdentifier` from `ValueExpression` and throws without it.
5. `CyField(For=...)` uses `Expression<Func<object>>`, so value-type properties are boxed in the expression; the framework unwraps the conversion.

**Remaining for Phase B to be closed:** CI has passed for the new bUnit/xunit and axe coverage; do 0.5 (committed DOM snapshots) before R1; add the demo pages (the Demo project needs the WebAssembly packages, which were unavailable, so they were not written blind; `DemoSmokeTests` will axe-scan each new route automatically); `dotnet pack` to confirm package validation against 1.7.0.
