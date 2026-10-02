# CymruBlazor: Implementation Plan (1.6.0 → 1.11 → 2.0.0)

**Inputs reviewed:** `CymruBlazor-Improvement-Recommendations.md`, `MIGRATION-2.0.md`, the 1.6.0 source zip (library, tests, samples, CI, CSS, JS, ADRs, CHANGELOG).
**Suggested location:** `docs/IMPLEMENTATION-PLAN.md`

> **Caveat.** `dotnet` was not available where this plan was written, so nothing here has been built or run. Every claim marked *(verified)* was read directly in the source; anything else is flagged as an assumption or a spike. Each phase has a CI gate for exactly that reason.

---

## 0. Implementation status

| Phase | State | Evidence |
|---|---|---|
| 0 (pre-flight) | **Partly done.** Spike 0.4 done. 0.1–0.3, 0.5, 0.6 need a person/CI (see below). | Findings below |
| A (1.7.0) | **Implemented**, A1–A9 | See below |
| B–F | Not started | |

**How A was verified (and what wasn't).** .NET 10 was installed in the sandbox, but nuget.org is blocked, so the repo's own test projects (xunit, bUnit, Shouldly, Playwright) could **not** be restored or run. Instead the library was compiled from source with a stub for `Mediator`, with the repo's `.editorconfig`, and exercised through a purpose-built harness (`HtmlRenderer` plus a small event-dispatching renderer): 69 checks, all passing. The library emits only the 14 `ASP0006` warnings already present in the untouched 1.6.0 build. A reflection diff of the public API against the original 1.6.0 build shows **0 removed or changed members** (44 additions). The new bUnit/xunit tests in `tests/` were written but **not run**; CI is their first real execution.

**Spike 0.4 result.** In .NET 10, `InputBase` itself does *not* require an `EditContext`. CymruBlazor's `HasValidationError` dereferences it unguarded and throws `NullReferenceException`. So standalone mode (B3) = make the base class null-safe; no synthetic `EditContext` needed.

**Corrections to the recommendations, found during implementation**

1. **The `@onclick` double-publish bug (§5.1) does not reproduce.** `@onclick` on `<CyButton>` binds to the `OnClick` parameter (Blazor binds parameters case-insensitively) and is guarded; verified against the unmodified 1.6.0 code (0 calls while `Loading`). A splat-capture fix was written, proven unnecessary, and removed. The genuine gap, re-entrancy of an in-flight async handler, is fixed by the opt-in `AutoLoading`. A7 was reworded accordingly.
2. `save` and `undo` already existed in the registry, so 17 icons were missing (plus `unknown`), not 19. `--cymru-shadow-xl` already existed; only `2xl` was added.
3. Theme scoping is the real dark-mode hazard: themes apply to `.cy-theme-provider` and `body:has()`, not `:root`, so a `var()`-derived token declared on `:root` silently keeps its light value. New tokens are declared on all theme scopes, and `scripts/GenerateTokens.cs --check` enforces it (negative-tested: it reports 18 violations when the aliases are `:root`-only). The 1.6.0 token CSS has no violations.
4. Several rows in §1.2 above stay valid; the `CyButton` `@onclick` row there is superseded by item 1.

**Still needs a human (Phase 0)**
- **0.1** ~~`tools/CymruBlazor.CssBundler` absent~~ Resolved: it was a packing omission; the second zip includes it. The real bundler was run against the patched CSS (37 stylesheets bundled, new tokens present).
- **0.2/0.3** Run the full suite on a machine with NuGet access, tag 1.6.0, then bump `PackageValidationBaselineVersion`.
- **0.5** DOM snapshot tests for the existing fields (do before Phase B's R1 refactor). Not done: they need to be generated from a real run of the existing component tests.
- **0.6** Consumer release note.
- Run `dotnet run scripts/GenerateTokens.cs` once in the repo and commit its outputs. They were generated in the sandbox and are included in the patch.

---

## 1. What the review found (read this first)

The two documents fit together well: the recommendations add API surface (all additive, so 1.x minors), and the migration doc removes dead API (2.0.0). The plan below keeps them on separate tracks. But reading the code turned up issues that change *how* several recommendations should be built, plus three things that need fixing before any feature work.

### 1.1 Pre-flight problems

| # | Finding | Why it matters |
|---|---|---|
| P1 | **`tools/CymruBlazor.CssBundler` is missing from the zip**, yet `CymruBlazor.slnx` lists it and `build/BundleCss.targets` runs `dotnet run --project ../../tools/CymruBlazor.CssBundler/...` *(verified)*. | Release builds, `dotnet pack`, and the exact command `MIGRATION-2.0.md` tells consumers to run (`dotnet build CymruBlazor.slnx`) cannot succeed from this zip. Probably a packing artefact (check `scripts/Pack-Solution.ps1` exclusions and the GitHub repo), but confirm. |
| P2 | **1.6.0 is still `[Unreleased]` in CHANGELOG**, and `PackageValidationBaselineVersion` is **1.2.0** *(verified)*. | The CHANGELOG itself says 1.6.0 was written without running the test projects. Package validation is comparing against a baseline four minors old, so an accidental break of anything added in 1.3–1.6 would not be caught. |
| P3 | The Recommendations doc already lives in `docs/` and is identical to the uploaded copy *(verified by diff)*. `docs/MIGRATION-2.0.md` likewise. | No duplication to clean up; this plan sits alongside them. |

### 1.2 Corrections and refinements to the recommendations

| Recommendation | What the code says | Adjustment |
|---|---|---|
| §5.1 `CyButton`: add `IconPosition` | `CymruBlazor.Enums.IconPosition` already exists and is `[Obsolete]`, scheduled for deletion; the migration doc says a future component should get *"a fresh, purpose-built type"*. | Use a new enum (proposed `ButtonIconPlacement { Start, End }`, logical not left/right). Same rule for `CyCard.Variant`: do **not** reuse obsolete `ComponentVariant` / `IHasVariant` / `IHasIcon`. |
| §5.1 `@onclick` hazard | Confirmed: `@onclick="HandleClickAsync"` precedes `@attributes="AdditionalAttributes"`, so a splatted `onclick` wins and bypasses the `Disabled`/`Loading` guard *(verified)*. | Better than "warn or strip": **capture** a splatted `onclick` and route it through the guarded handler, so existing consumer code is fixed rather than broken. Warn on top. |
| §5.2 "throw in Development only" and §9.3 "DEBUG-only diagnostics" | The package ships compiled **Release**, so `#if DEBUG` is never true in a consumer's app. The library also has no reference to a host-environment abstraction. | Dev-vs-prod behaviour needs a **runtime switch**: a `CymruBlazorOptions.Diagnostics` setting (`Strict`/`Lenient`) configured by the consumer from their own `IsDevelopment()`. See refactor R2. |
| §5.2 `IconRegistry.Register` | Registry is a `static readonly Dictionary` (not thread-safe) and icons render through `(MarkupString)`. | `Register` needs a concurrent store **and** input validation (allow-list of SVG shape elements; reject `<script>`, `on*=`, `javascript:`), otherwise it is an XSS hole in Blazor Server. |
| §5.2 missing-icon list | Registry uses a **semantic** naming scheme (`add`, `delete`, `back`) while the list uses Lucide names. CHANGELOG notes an unstarted "icon semantic-naming pass". | Decide the naming rule once (see §9, D3) before adding 19 icons. |
| §2.2 "Standalone mode: create a private `EditContext`" | `CyFormFieldComponentBase.HasValidationError` dereferences `EditContext` unguarded *(verified)*. I believe `InputBase` also rejects a later parameter set when `CascadedEditContext != EditContext`, which would make a private context fragile. | Prefer **null-safe base class** over a synthetic `EditContext`. Confirm with the Phase 0 spike against .NET 10. |
| §2.2 item 5 `CySelect` `Items` + selectors | `CySelect<TValue>` has one type parameter. Adding `TItem` would be breaking. | Additive API: `Options` of a small `CyOption<TValue>` record (Value, Text, Disabled, Group) plus a helper to project from any list. Selector-based `TItem` API is fine on **new** components (`CyCheckboxGroup`). |
| §2.2 item 8 `.cy-input`, `.cy-label`, `.cy-hint` | Library already uses `.cy-textbox`, `.cy-select`, `.cy-field__label`, `.cy-field__hint`, `.cy-field__error` *(verified)*. | Document and freeze **existing** names; add only what is genuinely missing. Introducing a second naming set recreates the collision problem §2.1 complains about. |
| §8 status tokens `bg/border/text` | Library suffixes are `-background` and `-text`; coverage is uneven (no `warning-text`, `info-text`, or any `-border`) *(verified)*. | Fill gaps using the **existing** suffix convention. |
| §4 `CyDataTable<T>` | `CyTable`'s own XML doc says sorting/filtering is a separate product and points consumers at `QuickGrid` (roadmap decision D5). | Not a blocker, but it reverses a recorded decision, so it needs **ADR-0003** before any code (Phase E). |
| §3 `CyCard.Variant (Flat/Outlined/Raised)` | `CyCard` already has `Elevation` (`None/Small/Medium/Large`). | Don't add a parallel concept: add an `Outlined`/border option and map "Flat" to `Elevation=None`. |
| Any new built-in text (Move up, Saving…, Loading…) | `CyLocalizedStrings` is a record of `required` init properties *(verified)*, and `HardcodedAriaLabelTests` fails on literal English `aria-label`s. | Adding a `required` member is a **source-breaking change** for anyone constructing the record. New strings must be non-required with English defaults (or live in a second record). |
| Migration doc: `CySidebar` | The new `States` API is **not yet independent** of the legacy one: `CySidebar` still reads `CollapseMode` internally for `NonCollapsible`, `CanNarrow`, and the `data-collapse-mode` attribute *(verified)*. `StarterApp`'s `AppSidebar` still uses the obsolete trio. | Removal in 2.0 is a refactor, not a delete. See Phase F. |

---

## 2. Release map

All feature work is **additive** and ships in 1.x minors, protected by package validation. Breaking changes are batched into 2.0.0.

| Release | Theme | Recommendation phase | Gate to start |
|---|---|---|---|
| **1.6.0** | Ship what exists | n/a | Phase 0 complete |
| **1.7.0** | Quick wins and de-risking | A | 1.6.0 tagged; baseline bumped |
| **1.8.0** | Form adoption and page scaffolding | B (+ §3) | 1.7.0 tagged |
| **1.9.0** | Editing experience | C | 1.8.0 tagged |
| **1.10.0** | Flow, review, feedback | D | 1.9.0 tagged |
| **1.11.0** | Advanced components | E | ADR-0003 accepted |
| **2.0.0** | Remove `[Obsolete]`, flip defaults | F (migration doc) | All of F's entry criteria |

Rule: **tag and publish each minor before starting the next**, then bump `PackageValidationBaselineVersion` to it (per `CONTRIBUTING.md`). Phases A→B→C→D can overlap in development, but not in release.

---

## 3. Working rules for every phase

1. **Non-breaking by construction.** New parameters have defaults that preserve current rendering. Where a default *should* change, add an opt-in now and record the flip as a 2.0 candidate (§8).
2. **Definition of done for a component** (all required, mirrors how existing components are built):
   - Razor + code-behind + scoped/partial CSS using **semantic tokens only**; imported in `cymrublazor.css`; verified present in dark and high-contrast themes.
   - XML docs on every public member; `[EditorRequired]` where applicable; trim-safe generics (follow the `DynamicallyAccessedMembers` pattern on `CySelect`).
   - bUnit tests; axe test in `CymruBlazor.AccessibilityTests`; keyboard-flow test where interactive.
   - Demo page (`Pages/Components/<Group>/CyXxxPage.razor`) with API table; Welsh via `ICyLocalizer` (see rule 4).
   - CHANGELOG entry; recipe/doc update where noted.
3. **Accessibility is the gate, not the polish.** WCAG 2.2 AA; 2.5.7 (dragging), 2.5.8 (24px targets), reduced motion, visible focus, and live-region announcements via existing `ILiveRegionRegistry`.
4. **Localisation.** No literal English in `aria-label`/visible built-in text. Add non-`required` members to `CyLocalizedStrings` (or a sibling record) with English + illustrative Welsh; append to the translator's second review pack.
5. **No new `[Obsolete]` surprises.** If a 1.x change deprecates anything, add it to `MIGRATION-2.0.md` in the same PR.
6. **Don't revive obsolete API.** Roving-tabindex/menu/combobox keyboard handling is designed against the real components (R3), not by resurrecting `IKeyboardNavigationService`.

---

## 4. Cross-cutting refactors

These are the "refactor the solution" part; each unblocks several features, so they go first inside their phase.

| ID | Refactor | Why | Lands in |
|---|---|---|---|
| **R1** | **Extract a shared field shell** (label + required marker + hint + error + `aria-describedby`) from the duplicated markup in `CySelect`, `CyTextBox`, `CyTextArea`, `CyCheckbox`, `CyDateInput`, `CyRadioGroup`. Public `CyField` is built on it. | Gives `CyField` (§2.2.1) for free, makes standalone mode a one-place fix, and stops six copies drifting. | 1.8.0 |
| **R2** | **`CymruBlazorOptions` + diagnostics service.** New `AddCymruBlazor(Action<CymruBlazorOptions>)` overload (existing signature untouched). `Diagnostics = Strict \| Lenient`; reports via `ILogger`/console, throws only in `Strict`. | One place for icon fallback, `onclick` warnings, missing `AriaLabel`, missing field `Label`, heading skips. Replaces the unworkable `#if DEBUG` idea. | 1.7.0 |
| **R3** | **One on-demand interaction JS module** (`cymru-interaction.js`, same lazy-import pattern as `cymru-overlay.js`): roving focus, `aria-activedescendant` helper, pointer-drag. | `CyMenu`, `CyToolbar`, `CyCombobox`, `CySortableList`, `CyStepper` all need it; avoids five bespoke scripts. | 1.9.0 |
| **R4** | **Theme parity guard:** a test asserting every semantic `--cymru-color-*` declared in `default.css`/`colours.css` is also resolved in `dark.css` and `high-contrast.css`. Extend `UndefinedCssVariableTests`' approach. | The recommendation doc's core complaint is dark mode failing silently. Today the guard only checks that a token is *declared somewhere*. | 1.7.0 |
| **R5** | **Token source of truth → generated artefacts:** a small script (or extension of the CssBundler tool, once P1 is resolved) emitting `tokens.json`, `tokens.d.ts`, and the Demo's token reference table from the CSS. | §8.2; one source, no hand-maintained table to rot. | 1.7.0 |
| **R6** | **`CyOption<TValue>` shared item model** for `CySelect`, `CyRadioGroup`, `CyCheckboxGroup`, `CyCombobox`. | Consistent `Items` story across all choice controls. | 1.8.0 |

---

## 5. Phase 0: pre-flight (before any 1.7 work)

| Task | Detail | Done when |
|---|---|---|
| 0.1 Resolve missing `tools/` | Compare zip to GitHub; restore `tools/CymruBlazor.CssBundler` or fix `Pack-Solution.ps1` so it stops excluding it. | `dotnet build CymruBlazor.slnx -c Release` succeeds on a clean clone. |
| 0.2 Prove 1.6.0 | Run the full suite on a machine with .NET 10, including `CymruBlazor.AccessibilityTests` in the Docker harness. The CHANGELOG states 1.6.0's fallback chain was traced by hand only. | Green CI on `main`. |
| 0.3 Tag and publish 1.6.0 | Move `[Unreleased]` → `[1.6.0]`; tag `v1.6.0`; confirm nuget.org. Then set `PackageValidationBaselineVersion` to `1.6.0`. | Package on nuget.org; baseline PR merged. |
| 0.4 Spike: standalone fields | bUnit: render `CyTextBox` / `CySelect` with `@bind-Value` and **no** `EditForm`. Record what throws and where. Also test: re-render with changed parameters. | One-paragraph finding appended to this plan; decides R1/B2 approach. |
| 0.5 Characterisation tests for fields | Add `MarkupMatches` DOM snapshots for every existing form component (valid/invalid/required/hint/disabled) **before** R1. | Snapshots pass on unmodified code. |
| 0.6 Release note for GenomicsHandbook | One-page "what changed 1.5.0 → 1.6.0 that affects consumers" (§9.1). | Posted in `docs/`. |

---

## 6. Phase A: 1.7.0 "Quick wins" (de-risk)

| # | Item | Files / approach | Tests |
|---|---|---|---|
| A1 | **R2 options + diagnostics** | `Extensions/ServiceCollectionExtensions.cs` (new overload), new `CymruBlazorOptions`, `CyDiagnostics` service. Default `Strict` = today's behaviour (no change for existing apps). Samples/Demo set it from `IsDevelopment()`. | Options default; strict throws; lenient logs. |
| A2 | **`CyIcon` unknown-name fallback** | `CyIcon.razor.cs.ValidateParameters` currently throws `ArgumentException`. In `Lenient`: render a neutral placeholder (new `unknown` icon) and log. | Update `CyIconTests` (the existing `Should.Throw<ArgumentException>` stays valid for `Strict`; add the lenient case). |
| A3 | **`IconRegistry.Register(name, markup)`** (+ optional `IIconProvider`) | Swap to `ConcurrentDictionary`; validate markup against an SVG-shape allow-list; throw on duplicate unless `overwrite: true`; `AllNames` returns a snapshot. Document as global, register once at startup. | Thread-safety; rejects `<script>`, `onload=`, `javascript:`; duplicate behaviour. |
| A4 | **Add missing icons** | Source from `lucide-static` (ISC; keep attribution). 19 icons from §5.2 plus `unknown`. Naming per decision D3. | `SampleIconNameTests` + a registry test that every icon has a domain and valid markup. |
| A5 | **Tokens: gaps and aliases** | Add `surface-hover`, `surface-subtle`, `text-secondary` (alias of `text-muted`), `radius-full` (alias of `radius-pill`), `shadow-xl/2xl`, `font-family-mono` alias, `page-max-width-*`, `focus-ring-*`, status `-border` / `warning-text` / `info-text`. UK-spelling `--cymru-colour-*` aliases **generated**, not hand-typed. Define each in dark and high-contrast. | R4 parity test; existing `UndefinedCssVariableTests`. |
| A6 | **R5 generated token reference** | `tokens.json`, `tokens.d.ts`, Demo `DesignTokens` page driven from them. Document a consumer lint recipe (stylelint `declaration-property-value-no-unknown` or regex over `var(--cymru-*)` vs `tokens.json`). | Generator runs in CI; output is diffed. |
| A7 | **`CyButton`: guarded splatted `onclick`** | `CyButton.razor`/`.cs`: detect `onclick` in `AdditionalAttributes`, remove it, and invoke it from `HandleClickAsync` after the `Disabled`/`Loading` check (cover `EventCallback`, `Func<MouseEventArgs,Task>`, `Action<MouseEventArgs>`). Diagnostics warning. | Double-click while `Loading` fires once via both `OnClick` and `@onclick`. |
| A8 | **`CyButton`: `Icon`, `ButtonIconPlacement`, `IconOnly`** | Reuse `CyIcon`. `IconOnly` requires `AriaLabel` → diagnostic in `Strict`. Add `CyButtonType` string constants (`Button`/`Submit`/`Reset`); keep `string Type` (changing it to an enum is breaking, see §8). | Rendered DOM, a11y name, axe for icon-only. |
| A9 | **`CyTable`: wrapping** | Add `Wrap` param (default `false` in 1.x) and a `.cy-table__cell--wrap`/`--truncate` cell utility. The `white-space: nowrap` default stays until 2.0 (§8). | Class output; no change to default markup. |

**Gate:** CI green incl. package validation vs 1.6.0; axe suite green; Demo bilingual for new strings. **Ship 1.7.0.**

---

## 7. Phase B: 1.8.0 "Form adoption and page scaffolding"

| # | Item | Approach | Notes |
|---|---|---|---|
| B1 | **R1 field shell** | Extract, then re-point the six existing fields at it. | DOM snapshots from 0.5 must pass **unchanged**. |
| B2 | **`CyField`** | `RenderFragment<CyFieldContext>` child gets `Id`, `DescribedBy`, `Invalid`; optional `EditContext`/`For` for error lookup. Works with raw `<input>`. | Delivers the accessibility wiring without adopting every input. |
| B3 | **Standalone mode** | Null-safe base class (per spike 0.4). `HasValidationError` etc. guard `EditContext`. Standalone fields take an optional `Error` string parameter. | Add tests: no `EditForm`, with and without `ValueExpression`. |
| B4 | **`CyNumberInput<TValue>`** | Follow the NHS/GOV.UK pattern: `type="text"` + `inputmode`, **not** `type="number"` (the library deliberately avoids it). `Min`/`Max`/`Step`/`Unit` suffix, parse-error message (localised), `ParsingErrorMessage`. | Supports `int`, `decimal`, `double`, nullable. |
| B5 | **`CyCheckboxGroup<TValue>`** | `fieldset`/`legend`, `Values` + `ValuesChanged` (`IEnumerable<TValue>`), `Items` with `ValueSelector`/`TextSelector` or child `CyCheckbox`es. | Pairs with `CyRadioGroup` structure. |
| B6 | **R6 `Options` API** on `CySelect` and `CyRadioGroup` | `CyOption<TValue>`; `ChildContent` still works; if both given, `ChildContent` wins and a diagnostic notes it. | Non-breaking. |
| B7 | **Publish CSS primitives** | Audit `forms.css`; document existing `.cy-textbox`, `.cy-select`, `.cy-field__*` as the supported contract; add only missing ones (e.g. a textarea primitive if it doesn't share `.cy-textbox`). | Demo "Form primitives" page showing raw markup themed in light/dark/HC. |
| B8 | **`CyPageHeader`** | Add `Eyebrow`, `Status`/`Badges` slot, `TitleLevel` (title is currently hard-wired H1 via `CyTypography`). The `Breadcrumb` slot **already exists**, so no new breadcrumbs slot. | Extend `HeadingLevelTests`. |
| B9 | **`CyPage` / `CyPageSection`** | `Width = Narrow \| Default \| Wide \| Full` → `--cymru-page-max-width-*` tokens from A5. | Fixes the 900px dead-space problem. |
| B10 | **`CyCard`** | `HeadingLevel` (resolves the open CHANGELOG item), `Padding` (`Compact/Default`), outlined option (see §1.2), `Collapsible`. | Keep `Header` as `RenderFragment`; add optional `Title` that renders the heading. |
| B11 | **Docs** | "Layout cookbook" mapping toolbar/form-row/card-grid to `CyStack`/`CyCluster`/`CyContainer`; first "page recipe" (list page) in `StarterApp`; Razor `v@q` parsing note. | |

**Gate:** zero DOM diffs in B1 snapshots; standalone-mode tests green; axe green on all new fields. **Ship 1.8.0.**

---

## 8. Phase C: 1.9.0 "Editing experience"

Build order inside the phase (small and low-risk first, so the hard one has its dependencies ready):

| Order | Component | Key decisions |
|---|---|---|
| C0 | **R3 interaction module** | Roving focus + `aria-activedescendant` + pointer-drag primitives. Pointer events only, no third-party dependency. |
| C1 | `CySwitch` | `role="switch"`, `Checked`/`CheckedChanged`, form-field base (uses B3), visible on/off text for non-colour cue. |
| C2 | `CyEmptyState` | `Icon`, `Title`, `Description`, `Actions`; title heading level configurable. |
| C3 | `CyMenu` | Menu-button pattern (`aria-haspopup`, roving tabindex, Esc returns focus to trigger). Designed to the menu's own needs, not the obsolete service. |
| C4 | `CyToolbar` | `role="toolbar"`, roving tabindex, orientation, overflow behaviour. |
| C5 | `CyDrawer` | Build on the native `<dialog>` approach from **ADR-0001** and the existing `showDialog` interop: `showModal()` for modal (free focus trap/inert), `show()` for non-modal. Inspector use is usually non-modal. |
| C6 | `CySortableList<TItem>` + `CyDragHandle` | See below. Largest item in the plan. |
| C7 | `CyWorkspace` / `CyWorkspacePane` | Collapsible first; **resizable second** (needs the splitter pattern: `role="separator"`, keyboard resize). Persisted sizes via a consumer-supplied callback, not `localStorage` by default. |

**`CySortableList` requirements** (from Recommendations §7.1, restated as acceptance criteria):

- Move up / Move down buttons and a "Move to section…" menu are **always available** (WCAG 2.5.7); drag is an enhancement.
- Keyboard: Space pick up, ↑/↓ move, ←/→ or menu to change list, Space/Enter drop, Esc cancel. Blazor can toggle `@onkeydown:preventDefault` per render, so keep the picked-up flag in C# state.
- Announcements via `ILiveRegionRegistry` using localised templates ("Picked up {item}, position {n} of {total}").
- Handle ≥ 24×24 px; `prefers-reduced-motion` honoured.
- State in C#; JS only reports pointer geometry; `OnReorder(ReorderEventArgs)` carries old/new index and source/target group.
- **Testing:** bUnit cannot simulate pointer drag. Add Playwright tests (drag with real pointer; full keyboard flow; announcement text) in the existing accessibility test project, plus axe in all three states (idle, picked up, dropped).

**Gate:** keyboard-only and screen-reader smoke test of the sortable list (needs a human with NVDA/VoiceOver, same as the outstanding tabs check in the CHANGELOG); axe green. **Ship 1.9.0.**

---

## 9. Phase D: 1.10.0 "Flow and review"

| # | Component | Notes |
|---|---|---|
| D1 | `CyStepper` | Horizontal/vertical, `aria-current="step"`, optional clickable steps, `OnStepSelected`. Replaces the hand-rolled wizard. |
| D2 | `CySummaryList` | NHS "check your answers" pattern: key, value, change link; `dl`/`dt`/`dd` semantics; change link accessible names include the key (e.g. "Change *Date of birth*"), localised. |
| D3 | `CyStatCard` | Value, label, trend; trend direction must not rely on colour alone. |
| D4 | `CyConfirmDialog` + `IConfirmService` | Model on the existing `IToastService` + `CyToastContainer` pair: register a service, place a host component in the layout. Built on `CyDialog`; focus returns to trigger (already supported by `IFocusManager`). |
| D5 | `CyUnsavedChanges` | `NavigationManager.RegisterLocationChangingHandler` for in-app navigation + a `beforeunload` JS hook for tab close. `SaveState` (`Idle/Saving/Saved/Error`) rendered through a polite live region. |
| D6 | `CySkeleton` | `aria-busy` on the container being loaded (not noisy per-skeleton announcements); animation off under reduced motion. |
| D7 | Docs | Remaining page recipes: editor page, wizard, tool workspace. |

**Gate:** same as Phase C. **Ship 1.10.0.**

---

## 10. Phase E: 1.11.0 "Advanced"

| # | Item | Pre-work |
|---|---|---|
| E0 | **ADR-0003: data table strategy** | Options: (a) thin generic `CyDataTable<T>` over `CyTable` (columns, sort, empty/loading, stacked mode, `PageState` with `CyPagination`); (b) CSS + guidance for `QuickGrid` only. Must explicitly supersede the D5 note in `CyTable`'s docs. Recommend (a), scoped *without* virtualisation. |
| E1 | `CyCombobox<TItem>` | ARIA 1.2 combobox pattern; async search `Func<string, CancellationToken, Task<IEnumerable<TItem>>>` with debounce and cancellation; loading and no-results states announced politely. Hardest a11y component after the sortable list. |
| E2 | `CyFileUpload` | Wraps `InputFile`; **does not upload** (raises events). Size/type validation, file list with remove buttons, localised errors. Mind the `OpenReadStream` max-size default. |
| E3 | `CyDataTable<T>` | Per ADR-0003. `aria-sort`, keyboard-operable sort headers, sticky header, row selection, density. |
| E4 | `CymruBlazor.Testing` package | New project exposing bUnit/axe helpers extracted from the existing tests. Keep Playwright as an optional second package so bUnit-only consumers aren't forced to take it. |
| E5 | `CyRuleBuilder` | **Defer.** Write its own ADR first; consider shipping as a separate package, since it is domain-heavy and not a core design-system component. |

**Gate:** ADR-0003 accepted; combobox passes a manual screen-reader check. **Ship 1.11.0.**

---

## 11. Phase F: 2.0.0 (the migration doc, executed)

### Entry criteria

- At least one minor release (1.11.0) has shipped with **no new `[Obsolete]` members**, so consumers have had time to see the warnings.
- `MIGRATION-2.0.md` is current, and every in-repo usage below has been migrated.
- Package-validation baseline is the last 1.x release; breaks are recorded in `CompatibilitySuppressions.xml` (the csproj comment refers to this file, but it does **not exist yet**; generate it with `-p:GenerateCompatibilitySuppressionFile=true` during the 2.0 pack).

### Work, in order

| Step | Work | Notes |
|---|---|---|
| F1 | **Decouple `CySidebar` internals from legacy API** *(do this in 1.x, before 2.0)* | Make `States`/`State` fully authoritative: `NonCollapsible` becomes `States.Count <= 1`; `CanNarrow`, `ShowCycleOrToggle`, and `CollapseModeAttribute` derive from `States`/`_state` only. Characterise current behaviour with tests *first* (there are ~8 `#pragma warning disable CS0618` sites). |
| F2 | **Migrate in-repo consumers** | `samples/StarterApp/Layout/AppSidebar.razor(.cs)` and its `MainLayout`; `Demo/Pages/Components/Layout/CySidebarPage.razor`; legacy-API assertions in `CySidebarTests`, `LayoutComponentTests`, `ComponentContractTests`. Validate with `dotnet build CymruBlazor.slnx -warnaserror:CS0618` (the command already in the migration doc). |
| F3 | **Decide `SidebarCollapseMode` enum's fate** | Only `CySidebar.CollapseMode` consumes it. If the parameter goes, the enum has no purpose, yet the migration doc only obsoletes `.Disabled`. Recommend: mark the **type** `[Obsolete]` in 1.x and add it to the doc; delete in 2.0. (Decision D5.) |
| F4 | **Remove** | `CySidebar.Collapsed`/`CollapsedChanged`/`CollapseMode`; `SidebarCollapseMode` (per F3); `Accessibility/Focus/{IKeyboardNavigationService, KeyboardNavigationService, KeyboardNavigationResult, KeyboardNavigationOptions, FocusNavigationMode}`; `Contracts/{IHasIcon, IHasVariant}`; `Enums/{ComponentVariant, IconPosition}`. None of the keyboard-navigation types are registered in `AddCymruBlazor()` *(verified)*, so no DI change. |
| F5 | **Delete their tests** | `KeyboardNavigationServiceTests`; the `Unused_Contracts_Are_Marked_Obsolete_For_Removal_In_2_0` theory in `ComponentContractTests`. |
| F6 | **Fix stale doc references** | `CyLanguageToggle` XML doc mentions `Collapsed`/`CollapsedChanged`; `Current-Solution-Structure.md` ("some members obsolete", `IHasIcon*`); README version notes. |
| F7 | **Flip the 2.0 candidate defaults** (below) | One PR each, each with its own CHANGELOG "Breaking" line. |
| F8 | **Release mechanics** | `-preview` tags via MinVer first; run consumer sanity build against GenomicsHandbook; delete migrated entries from `MIGRATION-2.0.md` as the doc itself prescribes, and publish an "upgrading" section in the CHANGELOG. |

### 2.0 candidates parking lot (do **not** do in 1.x)

| Candidate | Why it's breaking |
|---|---|
| `CyTable` `white-space: nowrap` default → wrap (flip `Wrap` default) | Changes layout for every existing table. |
| `CyButton.Type`: `string` → enum | Public parameter type change. |
| `CyIcon` default diagnostics mode `Strict` → `Lenient` | Behaviour change; arguably safe, but better in a major. |
| Make new `CyLocalizedStrings` members `required` | Source-breaking for custom localiser authors. |

---

## 12. Decisions needed from you

| ID | Question | My recommendation |
|---|---|---|
| D1 | Dev-vs-prod switch: explicit `CymruBlazorOptions.Diagnostics`, with default `Strict` (current behaviour) in 1.x? | Yes. Consumers opt in to `Lenient` for Production. Flip the default in 2.0. |
| D2 | `CyIcon` fallback: also silently placeholder in `Strict`? | No. `Strict` must keep throwing so typos surface in development. |
| D3 | Icon naming rule for new icons. | Lucide-literal for generic UI glyphs (`arrow-up`, `undo`, `save`), semantic names for domain concepts (existing pattern). Settle before A4 so the future naming pass doesn't rename them again. |
| D4 | `CyDataTable<T>` vs `QuickGrid` guidance. | ADR-0003; lean towards a thin `CyDataTable<T>`. |
| D5 | Obsolete the `SidebarCollapseMode` **type** in 1.x? | Yes (F3). |
| D6 | Ship `CyRuleBuilder` in core or a separate package? | Separate package, later. |
| D7 | Is the CssBundler absence (P1) a zip artefact or a genuine repo gap? | Check first; it blocks everything Release-related. |

---

## 13. Suggested first five PRs

1. **Fix P1** (restore `tools/`, or fix packing) and confirm Release build.
2. **Prove and tag 1.6.0**, then bump `PackageValidationBaselineVersion` to 1.6.0.
3. **Spike 0.4 + snapshot tests 0.5** (no behaviour change; unblocks R1).
4. **R2 options/diagnostics + A2 + A3** (icon fallback and `Register`): the two crash/silent-failure fixes from the app review, smallest blast radius.
5. **A7 `CyButton` `onclick` guard**: fixes a real double-submit bug without breaking existing call sites.

---

## 14. Risks

| Risk | Mitigation |
|---|---|
| Nothing here has been compiled; 1.6.0 itself was authored without running tests. | Phase 0.2 and a CI gate on every phase. |
| `CySortableList` and `CyCombobox` are the highest accessibility risk; automated axe cannot judge drag or combobox usability. | Playwright interaction tests plus a scheduled manual screen-reader pass; ship them last in their phases so they don't block the rest. |
| Scope: ~35 new components/changes across five minors. | Phases are independently shippable; A and B alone address most of the GenomicsHandbook friction (crashes, silent token failures, form adoption). |
| R1 field-shell refactor could subtly change rendered DOM and break consumer CSS. | Snapshot tests written before the refactor must pass unchanged. |
| Public `IconRegistry.Register` and `MarkupString` rendering. | Allow-list validation and tests (A3). |
