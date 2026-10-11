# CymruBlazor: Implementation Plan (1.6.0 → 1.12 → 2.0.0)

**Inputs reviewed:** `CymruBlazor-Improvement-Recommendations.md`, `MIGRATION-2.0.md`, the 1.6.0 source zip (library, tests, samples, CI, CSS, JS, ADRs, CHANGELOG).
**Suggested location:** `docs/IMPLEMENTATION-PLAN.md`

> **Caveat.** `dotnet` was not available where this plan was written, so nothing here has been built or run. Every claim marked *(verified)* was read directly in the source; anything else is flagged as an assumption or a spike. Each phase has a CI gate for exactly that reason.

---

## 0. Implementation status

| Phase | State | Evidence |
|---|---|---|
| 0 (pre-flight) | **Mostly done.** 0.1 (tools/ restored), 0.2 (full suite green: 623/623 unit tests + Playwright/axe smoke test), 0.3 (1.7.0 published 2026-10-02; baseline bumped to 1.7.0) and spike 0.4 done. 0.5 and 0.6 remain. | NuGet; local runs |
| A (1.7.0) | **Implemented**, A1–A9 | See below |
| B (1.8.0) | **Shipped (1.8.0, 2026-10-02).** B1–B5 and their demo pages exist. R1 (shared field-chrome refactor) not started; its gate (item 0.5 DOM snapshots) is TASK-107 in Phase G. | CHANGELOG `[1.8.0]` |
| C (1.9.0) | **Shipped.** C1–C7 built and verified (build, unit tests, axe 168/168). Released 2026-10-04. Open: three-theme visual pass. | CHANGELOG `[1.9.0]` |
| D (1.10.0) | **Shipped.** Six core components plus `CySegmentedControl`, `CyAvatar`/`CyAvatarGroup`, `CyNotificationBell`; build, unit tests and axe green. Released 2026-10-06. Open: three-theme visual pass and Welsh text review. | CHANGELOG `[1.10.0]` |
| E (1.11.0) | **Shipped: 1.11.0 is on nuget.org (2026-10-07).** `CyCombobox`/`CyMultiCombobox`, `CyFileUpload` and `CyDataTable<T>` with CSS, bUnit and axe tests, demo pages, English and Welsh overview cards and CHANGELOG. Build, unit tests and the containerised axe suite (179 tests, three themes) were taken through to green by the maintainer. Open: three-theme visual pass and Welsh text review (not confirmed done). `CyRuleBuilder` deferred. | See "Phase E" below, including "Phase E verification findings" |
| F (1.12.0) | **Shipped 2026-10-09 (F1 + F2 + F3).** `CyRuleBuilder` (model, edits, validator, summary, UI), page scaffolding (`CyPage`/`CyPageSection`, `CyPageHeader` and `CyCard` additions) and hygiene carry-forwards. Build, unit tests and axe all green in CI. Open: three-theme visual pass, Welsh text review, `dotnet pack` package-validation check (TASK-089). | See "Phase F" at the end |
| G (1.13.0) | **Scope confirmed 2026-10-09; hand-off 1 (G-D hygiene) written, not compiled.** Confirmed: G-A1, G-B2, G-C, G-D, G-F, plus a **spike** for G-A2 and the `CymruBlazor.Testing` package (G-B3). Out: G-A3, 2.0.0 removals, G-B1. Baseline is now **1.12.3** (GitHub Pages baseURL fix). Carry-forwards still yours: three-theme visual pass and Welsh text review for D, E and F, and TASK-089 (`dotnet pack` against 1.12.3). | See "Phase G" at the end |

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


---

## Phase D (1.10.0): "Flow and review"

**Status:** scope confirmed and code written; nothing has been compiled (no .NET SDK in the sandbox). Awaiting build, test and axe output.
**Baseline:** 1.9.0 (Phase C). Build, unit tests and the containerised axe suite (168/168) pass. Open for 1.9.0 and *not* Phase D work: the three-theme visual pass (TASK-022) and `dotnet pack` vs 1.8.0 (TASK-024); R1 stays deferred.
**Audit of the 1.9.0 zip:** a search for `CyStepper`, `CySummaryList`, `CyStatCard`, `CyConfirm*`, `CyUnsavedChanges`, `CySkeleton`, `CySegmentedControl`, `CyAvatar*` and `CyNotificationBell` (file names and contents, excluding docs) found **nothing**. Phase D starts from a clean slate. `CyDialog`, `CyLocalizedStrings`, `ILiveRegionRegistry`, `CyProgress`, `CyCard`, `CyBadge`, `CyButton` and `AddCymruBlazor()` are the existing pieces it builds on.
**Note on numbering:** the plan in the zip no longer contains the Phase C REQ/TASK tables (only the status line mentions TASK-022/024/026), so Phase D uses `REQ-D##` and continues tasks from `TASK-027`. Renumber if you have the original Phase C table.

### Global requirements (apply to every component)

| ID | Requirement |
|---|---|
| REQ-D00a | WCAG 2.2 AA: 24px minimum targets (44px where interactive controls are primary), visible focus, no colour-only meaning, `prefers-reduced-motion` and forced-colours respected. |
| REQ-D00b | Tokens only: no hex, no undefined `--cymru-*` (existing undefined-variable test must pass); new tokens, if any, declared on all theme scopes (`GenerateTokens.cs --check`). |
| REQ-D00c | Localisable strings: **no new required members on `CyLocalizedStrings`** (that would break 1.9.0 consumers). Each component with built-in phrases gets a `Cy<Name>Text` record with English defaults, as `CySortableListText` does, plus a `Text` parameter. Welsh defaults are shown in the demo pages. |
| REQ-D00d | Additive API only; no `[Obsolete]`; `docs/MIGRATION-2.0.md` untouched. Package validation vs 1.9.0 must pass. |
| REQ-D00e | No new JS file or npm dependency unless a requirement below says so; the plan currently needs **none** (only `NavigationManager` and the existing `cymru-overlay.js`). |
| REQ-D00f | Anything resolved from DI is registered in `AddCymruBlazor()`; a test asserts each service resolves from a provider built by `AddCymruBlazor()`. |
| REQ-D00g | User-supplied values written to a `style` attribute are validated (plain-length regex, as `CyWorkspacePane`); text content is always encoded. |
| REQ-D00h | Per component: razor + code-behind, CSS (token-only), bUnit tests, demo page, sidebar entry, `DemoNavigationIndex` entry, overview cards (English and Welsh), CHANGELOG `[1.10.0]` entry. |

### D1: `CyStepper` / `CyStep`

| ID | Requirement |
|---|---|
| REQ-D01 | Ordered progress indicator, `Orientation` Horizontal (default) or Vertical, `Steps` as child `CyStep` components or `Items`-free declarative children. |
| REQ-D02 | Parameters: `Current` (index, two-way via `CurrentChanged`), `Clickable` (default false), `OnStepSelected` (`EventCallback<int>`), `Label` (accessible name of the list, required), `HeadingLevel` not needed. Per step: `Title`, `Description`, `Status` (`Auto`/`Complete`/`Current`/`Upcoming`/`Error`), `Disabled`, `Href` (optional link mode). |
| REQ-D03 | Semantics: `<nav aria-label>` containing `<ol>`; current step has `aria-current="step"`; status is also conveyed as visually hidden text ("completed", "current", "not started", "has an error") and by icon/shape, not colour alone. |
| REQ-D04 | Clickable steps are real `<button>` (or `<a>` when `Href`), never `div`s; upcoming steps are not clickable unless `AllowForwardNavigation`; disabled steps use `aria-disabled`. |
| REQ-D05 | Horizontal collapses to a compact "Step 2 of 5: Title" summary below 40rem rather than overflowing. Connector lines decorative (`aria-hidden`). |
| REQ-D06 | `CyStepperText`: "Step {0} of {1}", status words. |

| Task | Description |
|---|---|
| TASK-027 | `CyStepper.razor(.cs)`, `CyStep.razor(.cs)` (registers with parent via cascading value), `CyStepperText.cs`. |
| TASK-028 | `stepper.css` (token-only; horizontal, vertical, compact, forced-colours). |
| TASK-029 | bUnit: ordering, `aria-current`, status text, click/keyboard, disabled, forward-navigation rule, vertical class, compact text, validation of empty `Label`. |
| TASK-030 | Demo page `/navigation/stepper`, sidebar, index, overview cards (EN/CY). |

### D2: `CySummaryList` / `CySummaryRow`

| ID | Requirement |
|---|---|
| REQ-D07 | "Check your answers" list following the NHS summary list pattern, rendered as a `<dl>` with `<dt>` key, `<dd>` value and an actions `<dd>`. |
| REQ-D08 | Rows by child `CySummaryRow` (`Key`, `Value` text or `ChildContent`, `ChangeHref`, `OnChange` callback, `ChangeLabel` override, `HiddenText` for the visually hidden key suffix). |
| REQ-D09 | The change link's accessible name is "Change {key}" via a visually hidden span (WCAG 2.4.4, 2.5.3), never a bare "Change". Rows with no change action keep column alignment. |
| REQ-D10 | `Borders` on/off, `Card` variant; stacks key above value below 40rem. Empty value renders a localisable "Not provided" placeholder. Value is encoded text unless `ChildContent` is used. |
| REQ-D11 | `CySummaryListText`: "Change", "Not provided". |

| Task | Description |
|---|---|
| TASK-031 | `CySummaryList.razor(.cs)`, `CySummaryRow.razor(.cs)`, `CySummaryListText.cs`. |
| TASK-032 | `summary-list.css`. |
| TASK-033 | bUnit: dl structure, hidden suffix text, `ChangeHref` vs `OnChange`, no-action alignment placeholder, empty value text, encoding. |
| TASK-034 | Demo page `/content/summary-list`, sidebar, index, overview cards (EN/CY). |

### D3: `CyStatCard`

| ID | Requirement |
|---|---|
| REQ-D12 | KPI card built on existing card tokens: `Label`, `Value` (string, encoded), `Unit`, `Description`, `Icon`, `Trend` (`Up`/`Down`/`Flat`/`None`) with `TrendText` (e.g. "+4% on last month"), `Status`/`Colour` for the accent, `Href` (whole card becomes a link), `Loading`. |
| REQ-D13 | Trend meaning is never conveyed by colour or arrow alone: arrow is `aria-hidden`, `TrendText` is always rendered, with a localisable "increase"/"decrease"/"no change" prefix for assistive technology. |
| REQ-D14 | Value/label order is label first in the DOM (reads naturally); `Loading` uses `CySkeleton` when D6 is in scope and falls back to `aria-busy` text otherwise. `HeadingLevel` optional so the card can sit under any outline. |
| REQ-D15 | `CyStatCardText`. |

| Task | Description |
|---|---|
| TASK-035 | `CyStatCard.razor(.cs)`, `CyStatCardText.cs`. |
| TASK-036 | `stat-card.css`. |
| TASK-037 | bUnit: label/value order, trend text, hidden prefix, link mode, loading state, encoding. |
| TASK-038 | Demo page `/content/stat-card`, sidebar, index, overview cards (EN/CY). |

### D4: `CyConfirmDialog` + `ICyConfirmService`

| ID | Requirement |
|---|---|
| REQ-D16 | `ICyConfirmService.ConfirmAsync(ConfirmOptions, CancellationToken)` returns `Task<bool>`; `ConfirmOptions` record: `Title`, `Message`, `ConfirmText`, `CancelText`, `Destructive`, `Size`. Convenience overload `ConfirmAsync(title, message, ...)`. |
| REQ-D17 | `CyConfirmDialog` component placed once (in the layout, like `CyToastContainer`) renders a `CyDialog` for the pending request. Without the host in the tree, `ConfirmAsync` fails fast with a clear `InvalidOperationException` (not a hung task). |
| REQ-D18 | Reuses `CyDialog` (ADR-0001: inert background, Tab containment, Escape, focus return to opener). Escape, close button and backdrop resolve `false`; no click path can leave the task unresolved; disposing the host resolves pending requests `false`. |
| REQ-D19 | Concurrent requests queue (FIFO). Default focus goes to Cancel for `Destructive`, otherwise to Confirm. Destructive confirm uses the danger button style plus explicit text (not colour alone). |
| REQ-D20 | Service registered scoped in `AddCymruBlazor()` (lesson from Phase C); resolves from DI test. `CyConfirmText` for default button text ("Confirm", "Cancel"). |

| Task | Description |
|---|---|
| TASK-039 | `ICyConfirmService`, `CyConfirmService`, `ConfirmOptions`, `CyConfirmDialog.razor(.cs)`, `CyConfirmText.cs`; registration in `AddCymruBlazor()`. |
| TASK-040 | CSS additions (small; likely reuses dialog classes). |
| TASK-041 | bUnit: true/false resolution paths, Escape/close/backdrop, queueing, dispose resolves false, no-host failure, destructive focus, DI resolution test. |
| TASK-042 | Demo page `/accessibility/confirm-dialog` (next to `CyDialog`), sidebar, index, overview cards (EN/CY); add the host to `MainLayout`. |

### D5: `CyUnsavedChanges`

| ID | Requirement |
|---|---|
| REQ-D21 | Parameters: `Dirty` (bool), `SaveState` (`Idle`/`Saving`/`Saved`/`Error`), `SavedAt` (optional), `Message` for the guard, `ShowIndicator` (default true), `ErrorText`. |
| REQ-D22 | In-app navigation guard via `NavigationManager.RegisterLocationChangingHandler` while `Dirty`: cancels navigation and asks to confirm through `ICyConfirmService` (D4) when available, otherwise the supplied `OnNavigationBlocked` callback. Handler is registered once and disposed with the component (no leak across re-renders). |
| REQ-D23 | Browser reload/close guard (`beforeunload`) **needs JS**; to honour "no new JS" it is **out of scope by default**. Option to add a few lines to the existing `cymru-overlay.js` is raised for confirmation (see question 3 below). |
| REQ-D24 | Indicator is a polite status region: "Saving...", "Saved at 14:32", "Unsaved changes", "Could not save". Announces on change only (not on every render), via the component's own `role="status"` element, with `ILiveRegionRegistry` not required. Respect reduced motion for the spinner. |
| REQ-D25 | Guard ignores same-page/hash navigation when `Dirty` is false; `forceLoad` navigations are noted in docs as not guardable by this API. |
| REQ-D26 | `CyUnsavedChangesText`. |

| Task | Description |
|---|---|
| TASK-043 | `CyUnsavedChanges.razor(.cs)`, `CySaveState` enum, `CyUnsavedChangesText.cs`. |
| TASK-044 | `unsaved-changes.css` (indicator only). |
| TASK-045 | bUnit with `FakeNavigationManager`: blocks when dirty, allows when clean, confirm yes/no, single registration, disposal, state text, announcement on change only. |
| TASK-046 | Demo page `/feedback/unsaved-changes`, sidebar, index, overview cards (EN/CY). |

### D6: `CySkeleton`

| ID | Requirement |
|---|---|
| REQ-D27 | Placeholder shapes: `Shape` (`Text`/`Rectangle`/`Circle`), `Lines`, `Width`/`Height` (validated plain lengths), `Animated` (default true, off under reduced motion). |
| REQ-D28 | Skeleton shapes are `aria-hidden`; the wrapper conveys loading with a single localisable visually hidden "Loading" text and `aria-busy="true"` on the region it stands in for (documented usage: put skeletons in a container with `role="status"`). Never more than one announcement per group. |
| REQ-D29 | Colours from tokens; visible in forced-colours (border fallback). |

| Task | Description |
|---|---|
| TASK-047 | `CySkeleton.razor(.cs)`, `CySkeletonText.cs`, `skeleton.css`. |
| TASK-048 | bUnit: shapes, lines, length validation (rejects `calc(`, `;`, `url(`), aria-hidden, single status text. |
| TASK-049 | Demo page `/feedback/skeleton`, sidebar, index, overview cards (EN/CY). |

### D-X: cross-cutting

| Task | Description |
|---|---|
| TASK-050 | Register new CSS in `cymrublazor.css` bundle order and confirm the CssBundler picks it up; run the undefined-variable test. |
| TASK-051 | `ComponentContractTests`/public-API check: confirm the Phase D types are additions only (list new public types in the CHANGELOG). |
| TASK-052 | CHANGELOG: **new `## [1.10.0] - Unreleased` heading above `[1.9.0]`** (the top heading today is `[1.9.0] - Unreleased`; do not append to it). |
| TASK-053 | Heading-order check on every new demo page (`<h2>` under page `<h1>`); add routes to the axe suite (picked up automatically by `DemoSmokeTests`). |
| TASK-054 | Hand-off to you: `dotnet build`, `dotnet test`, `.\scripts\Run-AccessibilityTests.ps1`; fix round from pasted output. |
| TASK-055 | Visual pass in light, dark and high-contrast for all six components (you, in the demo). |

### Suggested build order

D6 `CySkeleton` -> D3 `CyStatCard` -> D2 `CySummaryList` -> D1 `CyStepper` -> D4 `CyConfirmDialog` -> D5 `CyUnsavedChanges` (needs D4). Each lands as its own reviewable chunk so a build failure is easy to localise.

### Not in scope unless confirmed

`CySegmentedControl`, `CyAvatar` / `CyAvatarGroup`, `CyNotificationBell` (Recommendations section 6).

### Phase D: as built (deviations from the plan above)

Scope confirmed: all six core components, the three optional ones (`CySegmentedControl`, `CyAvatar`/`CyAvatarGroup`, `CyNotificationBell`), and `CyUnsavedChanges` limited to in-app navigation (no `beforeunload`, so no JS change at all).

| Plan said | Built | Why |
|---|---|---|
| `CyStepper` with child `CyStep` components | `CyStepper` takes `Steps` (`IReadOnlyList<CyStepItem>`) | Child-registration needs a second render to know the total and breaks when steps are added or removed; a model list is deterministic and easier to test. |
| `CySkeletonText` record | `Label` parameter, defaulting to the existing localised spinner label | One phrase only; no new type needed. |
| One stylesheet per component | `flow.css` (stepper, summary list, stat card, skeleton, unsaved changes) and `controls.css` (segmented, avatar, bell); confirm dialog reuses `dialog.css` | Matches the Phase C `editing.css` grouping. |
| Confirm demo under `/accessibility` | `/feedback/confirm-dialog` | `CyDialog`'s page lives under Feedback. |
| `CyStat...` only | Also `CyTrend` enum | Direction of change. |

New demo routes: `/navigation/stepper`, `/navigation/notification-bell`, `/forms/segmented-control`, `/content/summary-list`, `/content/stat-card`, `/content/avatar`, `/feedback/skeleton`, `/feedback/confirm-dialog`, `/feedback/unsaved-changes`.

Task status (all code written, none verified): TASK-027 to TASK-053 written; TASK-054 (your build/test/axe run) and TASK-055 (three-theme visual pass) are yours. Optional components D7 to D9 have no separate task numbers; they follow the same five steps (component, CSS, tests, demo, overview/changelog).

Items to check first when you run it (areas I could not verify without a compiler):

1. Razor: `@inherits` and `@namespace` pairs, the inline `RenderFragment` templates in `CyStatCard`/`CyNotificationBell`, and `CyStepper`'s `@code` fragment.
2. bUnit navigation-guard tests (`CyUnsavedChangesTests`) rely on `NavigationManager.NavigateTo` running location-changing handlers; if bUnit's behaviour differs, those four tests need adjusting, not the component.
3. `CyConfirmTests` rely on the dialog's JS module mock returning a token (`showDialog` -> 7), as `CyDialogTests` does.
4. Axe: avatar tone colours and `cy-stat-card__trend--good/bad` use the semantic text tokens; confirm contrast in the three themes.


---

## Phase E (1.11.0): "Advanced inputs and data"

**Status:** **shipped as 1.11.0.** The authoring sandbox has no .NET SDK, so the code was written blind and taken to green through the maintainer's build, test and axe runs (see "Phase E verification findings"). See "Phase E as built" at the end of this section for what changed from the plan below; where the two disagree, "as built" and the repo win.
**Baseline:** 1.10.0 (Phase D). Build, unit tests (948) and axe/Playwright suite (168/168) pass. Open for 1.10.0 and *not* Phase E work: the three-theme visual pass, Welsh text review, and `dotnet pack` vs 1.8.0.
**Audit of the 1.10.0 zip:** a search for `Combobox`, `FileUpload`, `DataTable` and `RuleBuilder` (whole tree, excluding `.artifacts`) found only the mention in `docs/CymruBlazor-Improvement-Recommendations.md`. Phase E starts from a clean slate. Existing pieces it builds on:

| Piece | Where | Used by |
|---|---|---|
| `CyFormFieldComponentBase<T>` (standalone-capable `InputBase<T>`, `LocalError`, validation classes) | `Components/Forms` | Combobox |
| `CyOption<TValue>` / `ToCyOptions(...)` (one shared item shape, no second type parameter) | `Components/Forms/CyOption.cs` | Combobox |
| `CyField` / `CyFieldContext.Attributes` (id, `aria-describedby`, `aria-invalid` splat) | `Components/Forms` | Combobox, FileUpload |
| `CyTable` (caption, scroll region, `Wrap`, `.cy-table__cell--*`) | `Components/Data/CyTable.razor` | DataTable |
| `CyPagination`, `CyEmptyState`, `CySkeleton`, `CyProgress`, `CyCheckbox`, `CyButton`, `CyIcon` (`sort`, `chevron-*`, `arrow-up/down`, `upload`, `file`, `close`, `check`, `search` exist) | various | all |
| `ILiveRegionRegistry` / `CyLiveRegion` | `Accessibility` | optional |
| `CySortableListText` pattern, `OverlayInterop` / `EditingInterop` / `SortableInterop` module pattern | `Components/Content`, `Accessibility/Focus` | text records, JS |
| `CyCheckbox` has no indeterminate state; no `InputFile` is used anywhere in the library | grep | DataTable, FileUpload |

**Numbering:** `REQ-E##`; tasks continue from `TASK-056` (Phase D ended at `TASK-055`).

### Decisions to confirm before any code is written

**1. JavaScript: one small on-demand module, for the combobox only.**

| Component | Needs script? | Reason |
|---|---|---|
| `CyCombobox` / `CyMultiCombobox` | **Yes, proposed: `wwwroot/js/cymru-inputs.js` (~50 lines)** | Blazor can only `preventDefault` on *all* keys of an element or none. With the list open, ArrowUp/ArrowDown/Home/End must not scroll the page and Enter on the active option must not submit the surrounding `EditForm`; Escape must not also close an enclosing `CyDialog`/`CyDrawer`; the active option must be scrolled into view. The module reads `aria-expanded` / `aria-activedescendant` straight from the DOM, so there are no round trips and no state sync. Same shape as `attachMenu` in `cymru-editing.js`. |
| `CyFileUpload` | **No** | A native `<input type="file">` (Blazor `InputFile`) already accepts drops when the visual drop zone is the input itself, stretched over it with CSS. Pick, drop, `accept`, `multiple` and keyboard activation are native. Paste, folder drops and drag-over highlighting beyond `:has(:focus-visible)` / `:hover` are out of scope. |
| `CyDataTable<T>` | **No** | Sort buttons, checkboxes, paging and live announcements are all C# + native elements. **Column resizing is proposed out of scope** (it is the only table feature that needs pointer script). The header "select all" checkbox shows no indeterminate state (that needs a DOM property set from script); the selected count is shown and announced instead. |

Rules if approved: new file `cymru-inputs.js` (so `cymru-overlay.js`, `cymru-editing.js` and `sortable-list.js` stay byte-identical and separately cached), loaded on first render via `ComboboxInterop.ModulePath`, never a `<script>` tag, no npm or third-party code. If the module cannot load (prerender, disconnected circuit) the component still works, minus the four behaviours above; the failure is swallowed the way `CyMenu` does.
*Alternative if you prefer zero JS:* ship the combobox with those four gaps and document them. I do not recommend it: Enter submitting a form from an open list is a real defect.

**2. `CyFileUpload` and security.** See REQ-E20 to REQ-E31. Headline points:
- Everything the browser reports about a file (name, size, content type) is **untrusted input**. Client-side checks are a usability aid, not enforcement; the component says so in its XML docs, demo page and CHANGELOG, and the demo shows a server-side validation sketch.
- The library ships **no endpoint, no storage and no `HttpClient`**. The consumer passes an `Upload` callback; the component never POSTs anywhere itself.
- `MaxFileSize` has a safe default (5 MiB) and is the limit passed to `IBrowserFile.OpenReadStream`, so the *framework* throws if the real size exceeds it, not just our pre-check.
- Names are always HTML-encoded text, never used as a path or in a `style`/`href`.

**3. `CyDataTable<T>` builds on `CyTable`.** It *composes* `CyTable` (renders it and writes the `<thead>`/`<tbody>` for you); `CyTable` itself is not modified, so its output is byte-identical. Sort, selection and paging are all optional parameters, off by default. Stated scope cuts are listed below rather than half-built.

**4. `CyCombobox` follows the WAI-ARIA 1.2 combobox pattern (list autocomplete, no inline completion).** Two public types, because the bound value differs (the same reason `CyCheckboxGroup` binds `IEnumerable<T>` while `CySelect` binds `T`): `CyCombobox<TValue>` (single) and `CyMultiCombobox<TValue>` (multiple), over one internal engine. Items are `CyOption<TValue>`, the Phase B decision, so no second type parameter.

**5. `CyRuleBuilder` (Recommendations section 7, item 14, "optional, later"): not planned here. I will ask you separately before building it** (see the questions at the end). If you say yes it becomes E4 with its own REQ/TASK table.

### Global requirements (apply to every component)

| ID | Requirement |
|---|---|
| REQ-E00a | WCAG 2.2 AA: 24px minimum targets (44px where primary), visible focus, no colour-only meaning (selection, sort direction, error, progress), `prefers-reduced-motion` and forced-colours respected, 2.5.7 (no drag-only action: file picking is by button/keyboard), 3.3.1/3.3.3 for upload errors, 4.1.3 status messages. |
| REQ-E00b | Tokens only: no hex, no undefined `--cymru-*` (existing undefined-variable test must pass); any new token declared on all theme scopes (`GenerateTokens.cs --check`). Plan: no new tokens. |
| REQ-E00c | Localisable strings: **no new required members on `CyLocalizedStrings`**. One `Cy<Name>Text` record per component with English defaults and a `Text` parameter, as `CySortableListText`. Welsh defaults shown on the demo pages. |
| REQ-E00d | Additive API only; no `[Obsolete]`; `docs/MIGRATION-2.0.md` untouched; package validation vs 1.10.0 passes. `CyTable`, `CyPagination`, `CyField`, `CyOption<T>` and every existing JS file unchanged. |
| REQ-E00e | JS: only `cymru-inputs.js`, only if confirmed (decision 1); no npm package. |
| REQ-E00f | Nothing new in DI is planned. If that changes, register in `AddCymruBlazor()` and add a registration test (extend `PhaseDRegistrationTests` pattern). |
| REQ-E00g | User-supplied values written to `style` are validated with the plain-length regex (`CssLength`); all text content (item text, file names, error text) is encoded. |
| REQ-E00h | Per component: razor + code-behind, CSS, bUnit tests, demo page, sidebar entry, `DemoNavigationIndex` entry, overview cards (English and Welsh), CHANGELOG `[1.11.0]` entry. |
| REQ-E00i | Async callbacks supplied by the consumer (items provider, upload) may throw or be cancelled: the component catches, shows an accessible error and never takes down the circuit. Stale responses are discarded (last request wins). |

### E1: `CyCombobox<TValue>` / `CyMultiCombobox<TValue>`

| ID | Requirement |
|---|---|
| REQ-E01 | Single: derives from `CyFormFieldComponentBase<TValue>` (works standalone, in `EditForm`, and inside `CyField`; `Value`/`ValueChanged`/`ValueExpression`). Multiple: same engine, binds `IEnumerable<TValue>` (assigns a new collection on each change, as `CyCheckboxGroup`). |
| REQ-E02 | Items: `Items` (`IReadOnlyList<CyOption<TValue>>`, filtered in memory) **or** `ItemsProvider` (`Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<TValue>>>>`, async search; request carries the typed text and a cancellation token). Setting both throws a clear `InvalidOperationException`. `Filter` optional (default: culture-aware, case-insensitive contains). |
| REQ-E03 | Async: `DebounceMilliseconds` (default 250; C# `Task.Delay` + cancellation, no JS), `MinSearchLength` (default 0 for `Items`, 1 for `ItemsProvider`), stale responses discarded, provider exceptions caught and shown as "Could not load results" in the popup and status. `MaxResults` (default 50) caps rendered options; when exceeded the status says "Showing the first 50 of N results. Keep typing to narrow." (no virtualisation). |
| REQ-E04 | ARIA 1.2 list-autocomplete: the `input` has `role="combobox"`, `aria-autocomplete="list"`, `aria-expanded`, `aria-controls` (listbox id), `aria-activedescendant` (only when an option is active). The popup is `role="listbox"` with `role="option"` children carrying `aria-selected`. DOM focus never leaves the input. Multiple: listbox has `aria-multiselectable="true"`. |
| REQ-E05 | Keyboard: Down opens / moves (wraps off by default); Up; Alt+Down opens without moving; Enter selects the active option; Escape closes, and a second Escape on a closed list clears the typed text; Home/End move the caret in the input (they move the active option only when the user is in the list, per APG, so they are not intercepted); Tab leaves without selecting. Typing opens the list. |
| REQ-E06 | Single, on blur: typed text that is not a selection reverts to the selected option's text (or empties if none). **No free-text values** (`AllowCustomValue` is out of scope: coded terms must come from the list). Optional clear button (`AllowClear`, default true) with an accessible name. |
| REQ-E07 | Multiple: selected values render as a list of chips before the input, each with a remove button named "Remove {text}"; removing moves focus to the input (or the next chip's button); already-selected options stay in the list marked selected, not hidden; selecting keeps the list open (`CloseOnSelect` default false). Backspace-removes-last is **not** implemented (an unannounced destructive key). |
| REQ-E08 | Live region (the component's own visually hidden `role="status"` `aria-live="polite"`, so it works without a `CyLiveRegion` on the page): "{n} results available", "1 result available", **"No results found"**, "Loading results", "Type {0} or more characters", the capped-results message, "{text} selected / removed". Updated after the debounce, not on every keystroke, and never twice with the same text in a row. |
| REQ-E09 | Pointer: option `mousedown` is default-prevented (`@onmousedown:preventDefault` on options only, so the input keeps focus and the blur-closes-list logic never fires before the click). Opening below the input only; the popup is positioned with CSS (`position: absolute`, `MaxListHeight` validated as a `CssLength`). Known limitation, documented: a clipping ancestor (`overflow: hidden`) clips the list; there is no flip-above logic (it would need script). |
| REQ-E10 | `Disabled`, `ReadOnly` (list never opens), `Placeholder`, `Label`/`Hint` as `CyTextBox`; `Id`, `Required` and described-by supplied by `CyField` are honoured via `AdditionalAttributes`. |
| REQ-E11 | `CyComboboxText`: results available / one result / no results / loading / min length / capped / load error / selected / removed / remove chip / clear / "Selected: " list label. No new member on `CyLocalizedStrings`. |
| REQ-E12 | JS (if confirmed): `attachCombobox(input)` / `detachCombobox(token)` in `cymru-inputs.js`; prevents default for ArrowUp/Down/Home/End-in-list, Enter with an active option, and stops Escape propagating while expanded; scrolls the active option into view. Detached on dispose, including via the `RemovableHost` path. |

| Task | Description |
|---|---|
| TASK-056 | (if approved) `wwwroot/js/cymru-inputs.js`, `Accessibility/Focus/ComboboxInterop.cs`. |
| TASK-057 | Internal engine (`CyComboboxEngine`: filtering, debounce, active index, status text), `CyCombobox.razor(.cs)`, `CyComboboxRequest`, `CyComboboxText.cs`. |
| TASK-058 | `CyMultiCombobox.razor(.cs)`, chips, focus handling after removal. |
| TASK-059 | `combobox.css` (token-only; chip, popup, forced-colours, active/selected not colour-only, reduced motion). |
| TASK-060 | bUnit: ARIA attributes in every state; Down/Up/Enter/Escape/Tab paths (`TriggerEvent("onkeydown", ...)`); filter; debounce and stale-response discard (poll, do not wait for render); provider exception; capped results; "No results" status text; single revert-on-blur; multi add/remove and focus; `CyField` and `EditForm` integration; `Items`+`ItemsProvider` throws; disposal via `RemovableHost`; `JSInterop.Mode = Loose` with `SetupModule`. |
| TASK-061 | Demo page `/forms/combobox` (static, async "coded terms" example with a fake provider, multiple, in `CyField`, Welsh text), sidebar, index, overview cards (EN/CY). |

### E2: `CyFileUpload`

| ID | Requirement |
|---|---|
| REQ-E20 | Wraps `InputFile`. The input is the drop zone (no script). A visible, 44px "Choose file(s)" affordance is the input's own label/button styling; keyboard and screen-reader activation are native. |
| REQ-E21 | Parameters: `Accept` (extensions such as `.pdf` and/or MIME types such as `image/png`; format-validated, invalid value throws), `Multiple`, `MaxFileSize` (bytes, default 5 MiB, must be > 0), `MaxFiles` (default 1, or 10 when `Multiple`), `MaxTotalSize` (optional), `Disabled`, `Required` (via `CyField`), `Files` (read-only snapshot, `CyFileItem`), `OnFilesChanged`, `OnRejected`, `Text`. `AdditionalAttributes` are splatted onto the `<input>` so `CyField`'s id/`aria-describedby`/`aria-invalid` land on it. |
| REQ-E22 | **Validation is advisory.** Checked in C# on selection: size (reported size > `MaxFileSize`), empty file (size 0), type (extension in `Accept`, and, when `Accept` lists MIME types, the browser-reported content type), count, total size, duplicates (same name and size). Each failure is a `CyFileRejection` (`Reason` enum + file name) and a visible, specific message ("report.docx is 12 MB. The limit is 5 MB."). `accept` is also written to the input for the browser picker, which is a hint only. |
| REQ-E23 | **No enforcement claims.** XML docs, demo page and CHANGELOG state that name, size and content type come from the client and can be forged; the consumer's server must re-validate size, extension *and content* (magic bytes), scan for malware, generate its own storage name and never trust the original name. The demo page includes that checklist. |
| REQ-E24 | **Upload via callback, no endpoint in the library.** `Upload`: `Func<CyFileUploadContext, Task<CyUploadResult>>`. The context exposes `File` (`IBrowserFile`), `OpenReadStream()` (always passes `MaxFileSize`, so the framework enforces the real limit and throws `IOException` past it, shown as a "too large" failure), `Progress` (`IProgress<long>` of bytes) and a `CancellationToken`. `CyUploadResult.Success()` / `.Failure(message)`; an exception is treated as failure with the generic text (the exception message is **not** shown to the user, to avoid leaking server detail; it can be logged by the consumer). With no `Upload`, the component only validates and lists files and the consumer reads `Files` on submit. |
| REQ-E25 | Blazor Server note, documented: `OpenReadStream` streams over the SignalR connection and the hub's `MaximumReceiveSize` (default 32 KB message, with the stream chunked) and the consumer's own limits apply; large files should be uploaded from the browser to an API, which is exactly why the library does not do transport. |
| REQ-E26 | Per-file list (`<ul>`): name (encoded), human size, status text ("Ready", "Uploading", "Uploaded", "Failed: ...", "Not accepted: ..."), a `CyProgress` (determinate when the size is known) labelled with the file name, **Remove** (name "Remove {file}") and, while uploading, **Cancel** (name "Cancel upload of {file}"), and on failure **Retry**. Uploads run sequentially by default; `MaxParallel` (default 1) bounds concurrency. Removing mid-upload cancels first. |
| REQ-E27 | Announcements (own `role="status"` region, polite; failures `role="alert"` on the per-file error, once): "{n} files added", "{file} rejected: {reason}" (every rejection, concatenated into one message when several), "Uploading {file}", "{file} uploaded", "{file} failed", "{file} removed". **Percent progress is not announced** (it would flood the reader); the visual bar has `aria-valuenow` and is not live. |
| REQ-E28 | Errors are linked: each file error is its own element, the input's `aria-describedby` (merged with the one from `CyField`) lists the summary id; moving focus after removal goes to the input. Error text is plain text, never colour only (icon + words). |
| REQ-E29 | Dispose cancels in-flight uploads; no callback runs after disposal; `OnFilesChanged` is not called for rejected files. |
| REQ-E30 | No `FileReader`, no `accept`-based "security", no MIME sniffing in the library, no thumbnail preview (an image preview would need `URL.createObjectURL`, i.e. JS, and is out of scope). |
| REQ-E31 | `CyFileUploadText`: choose (single/multiple), drop hint, each status word, each rejection message (format strings: `{0}` file, `{1}` size, `{2}` limit), remove / cancel / retry names, announcements. |

| Task | Description |
|---|---|
| TASK-062 | `CyFileUpload.razor(.cs)`, `CyFileItem`, `CyFileStatus`, `CyFileRejection`, `CyFileRejectionReason`, `CyFileUploadContext`, `CyUploadResult`, `CyFileUploadText.cs`, size formatter, `Accept` parser/validator. |
| TASK-063 | `file-upload.css` (drop-zone via stretched input, focus ring on the visible affordance via `:has(input:focus-visible)`, list, progress, forced-colours). |
| TASK-064 | bUnit with `InputFileContent`: every rejection reason, accept parsing (valid and invalid), sequential upload and concurrency bound, progress, success/failure/exception paths (generic message), cancel, retry, remove mid-upload, dispose cancels, announcements (poll), `CyField` described-by merge, encoding of a name such as `<img onerror=x>.pdf`, size formatting, `MaxFileSize` passed to `OpenReadStream`. |
| TASK-065 | Demo page `/forms/file-upload` (validate-only, fake upload with progress and failure, `CyField`, Welsh text, server-side checklist), sidebar, index, overview cards (EN/CY). |

### E3: `CyDataTable<TItem>`

| ID | Requirement |
|---|---|
| REQ-E40 | Composes `CyTable` (`Caption` required, `CaptionVisuallyHidden`, `ScrollContainer`, `Wrap`, `Class` pass through). Real `<table>`, `<thead>`, `<tbody>`, `scope="col"` on headers, optional `scope="row"` on a designated row-header column (`RowHeader` on one column). Not an ARIA grid. |
| REQ-E41 | Columns: `Columns` (`IReadOnlyList<CyDataColumn<TItem>>`, model list as in D1 `CyStepper`, avoiding child-registration re-render problems). `CyDataColumn<TItem>`: `Header` (required text), `Cell` (`RenderFragment<TItem>`) or `Value` (`Func<TItem, object?>`, rendered as encoded text), `Sortable`, `SortKey`/`Comparer`, `Align` (Start/End/Center; numbers should be End), `Wrap`/`Truncate` (reuse `cy-table__cell--*`), `Width` (validated `CssLength`), `RowHeader`, `HeaderVisuallyHidden`. |
| REQ-E42 | Data: `Items` (in-memory, client sort and page) **or** `ItemsProvider` (`Func<CyDataTableRequest, CancellationToken, Task<CyDataTableResult<TItem>>>`; the request carries `Page`, `PageSize` and `SortKey`/`Descending`; the result carries `Items` and `TotalCount`). Both set throws. Stale responses are discarded. Provider exceptions are caught and shown as an error row (`role="alert"` once) with a Retry button. |
| REQ-E43 | Sorting: sortable header is a `<button>` inside the `<th>` (name = header text; icon `aria-hidden`); the `<th>` carries `aria-sort="ascending"` or `"descending"` **only on the sorted column** (none elsewhere). Click/Enter/Space cycles ascending then descending (`AllowUnsorted` adds a third "none" step). `SortKey`/`SortDescending` two-way bindable; `OnSortChanged`. A change is announced: "Sorted by Name, ascending". Stable sort. Direction is shown by icon **and** `aria-sort`, never colour. |
| REQ-E44 | Selection: `SelectionMode` (`None` default / `Single` / `Multiple`), `SelectedItems` two-way, `KeySelector` (default: reference equality; **required for selection to survive paging and server reloads**, validated when selection is on and the provider is used). `RowLabel` (`Func<TItem,string>`) names each row's control ("Select {label}"). Multiple: header "select all on this page" checkbox ("Select all 10 rows on this page" / "Clear selection on this page"); Single: radio inputs in one group. A visible and announced "{n} selected" status. `aria-selected` is **not** used (not valid on rows of a plain table); the selected row is styled and its checkbox is checked. No indeterminate state (see decision 1). |
| REQ-E45 | Paging (additive): `PageSize` (0 = off), `Page` (two-way), `TotalCount` (provider mode returns it; `Items` mode derives it), `PageSizeOptions` (renders a `CySelect`-style "Rows per page" if set). Renders `CyPagination` below, plus "Showing {0} to {1} of {2}" status. A page change announces "Page 3 of 12" and leaves focus on the pagination control (not the table top) so keyboard users keep their place; it does not scroll. Sorting resets to page 1. |
| REQ-E46 | States: `Loading` (rows replaced by `CySkeleton` rows, `aria-busy="true"` on the table, one "Loading" status), empty (`EmptyContent`, default a `CyEmptyState` with "No results"; the empty message is a table row spanning all columns so the table keeps its structure and is announced), error (REQ-E42). |
| REQ-E47 | Keyboard model, stated explicitly: the table is **Tab-navigable**, not arrow-navigable. Tab order is: scroll region (already focusable in `CyTable`), sort buttons left to right, then per row the select control and any focusable content in the cells, then pagination. No roving tabindex, no grid navigation (that would need script, change the role and the screen-reader reading mode). Row actions are ordinary buttons/links in a cell (`RowActions` fragment renders a last column with a visually hidden "Actions" header). |
| REQ-E48 | Sticky header: `MaxHeight` (validated `CssLength`) makes the scroll region vertical-scroll with `position: sticky` headers; documented that the region is keyboard-focusable (existing `CyTable` behaviour). |
| REQ-E49 | Toolbar slot: optional `Toolbar` fragment above the table (consumer filters, bulk actions shown when something is selected); the component does not implement filtering. |
| REQ-E50 | `CyDataTableText`: sort status, select/select all/clear, selected count, showing range, rows per page, page changed, no results, loading, load error, retry, actions header. |

**Out of scope (said once, here, rather than half-built):** virtualisation (use `QuickGrid` for very large in-memory sets; paging covers the rest); inline cell editing; column resizing, reordering and show/hide; **responsive stacked rows and `Priority`-based column hiding** (both rely on `display: block` on table parts or removing cells, which strips table semantics in several browser/screen-reader pairings; `CyTable`'s keyboard-focusable scroll region remains the narrow-screen answer); row grouping, expandable and tree rows; built-in filtering UI; export; ARIA grid navigation; persisted user preferences. Each can be a later additive release.

| Task | Description |
|---|---|
| TASK-066 | `CyDataTable.razor(.cs)`, `CyDataColumn<TItem>`, `CyDataTableRequest`, `CyDataTableResult<TItem>`, `CyDataSelectionMode`, `CyColumnAlign`, `CyDataTableText.cs`; sort/page helpers kept internal and unit-testable. |
| TASK-067 | `data-table.css` (alignment, sort button, sticky header, selected row not colour-only, skeleton rows, forced-colours). |
| TASK-068 | bUnit: markup structure and `scope`; `aria-sort` only on the sorted column; sort cycle and stability; announcements; selection modes, select-all, key-based selection across pages; paging maths, range text, reset on sort; provider paths (loading, stale response, exception, retry); empty row; `Items`+`ItemsProvider` throws; `MaxHeight` rejection of `calc(`, `;`, `url(`; encoding of `Value` text; `CyTable` output unchanged (snapshot of the existing `CyTable` tests still green). |
| TASK-069 | Demo page `/data/data-table` (client sort/page, server-style provider with delay, selection with a bulk action, empty, loading, Welsh text), sidebar, index, overview cards (EN/CY). |

### E4: `CyRuleBuilder` (optional)

Not planned. **Will not be built or specified until you say so.** Rough size if you do: bigger than the other three combined (field/operator/value editors, nested groups, and a keyboard model for adding/removing/reordering conditions that would reuse `CySortableList` and `CyMenu`), so I would propose it as its own phase (1.12.0).

### E-X: cross-cutting

| Task | Description |
|---|---|
| TASK-070 | Register new CSS in the `cymrublazor.css` bundle order; confirm the CssBundler picks it up; run the undefined-variable test. |
| TASK-071 | Public-API check: Phase E types are additions only; list them in the CHANGELOG. `CyTable`, `CyPagination`, `CyField`, `CyOption<T>` and the three existing JS files unchanged (compare hashes). |
| TASK-072 | CHANGELOG: **new `## [1.11.0] - Unreleased` heading above `[1.10.0]`** (top heading today is `[1.10.0] - Unreleased`; do not append to it). Include the security note for `CyFileUpload` and the out-of-scope list for `CyDataTable`. |
| TASK-073 | Heading-order check on every new demo page (`<h2>` under the page `<h1>`); routes picked up by `DemoSmokeTests` automatically; add `CyComboboxAccessibilityTests`, `CyFileUploadAccessibilityTests`, `CyDataTableAccessibilityTests` in the axe project (open list, error, selected, sorted states). |
| TASK-074 | Registration test confirming `AddCymruBlazor()` still resolves everything and that no new service is *required* (a guard, since none is planned). |
| TASK-075 | Hand-off to you: `dotnet build`, `dotnet test`, `.\scripts\Run-AccessibilityTests.ps1` (all three themes); fix rounds from pasted output. |
| TASK-076 | Visual pass in light, dark and high-contrast for all three components; Welsh text review (you). |

**Task status (as shipped):** TASK-056 to TASK-073 done. TASK-075 (build, test, axe) done through several fix rounds. **Not done:** TASK-074 (the `AddCymruBlazor()` guard test: no service was added, so nothing could regress, but the test was not written), the hash comparison in TASK-071 (public API and untouched files were checked by reading, not by hash or reflection diff), and TASK-076 (visual pass and Welsh review are the maintainer's; completion not confirmed). `PageSizeOptions` (REQ-E45), `OnSortChanged` (REQ-E43, replaced by the two-way `SortKeyChanged`/`SortDescendingChanged`) and `CyField` wrapping (E1, E2) were dropped; see "as built".

### Suggested build order

(`cymru-inputs.js`, if approved) -> `CyCombobox` -> `CyMultiCombobox` -> `CyFileUpload` -> `CyDataTable<T>`. Each lands as its own reviewable chunk, and you build and paste output after each, so a compile error is easy to localise (no .NET SDK in my sandbox).

### Risks and assumptions (unverified until you build)

1. `CyFieldContext.Attributes` is a splat for a control; I assume `CyField` passes it via a `RenderFragment<CyFieldContext>`. I will read `CyField.razor` before writing the combobox or upload wiring, not rely on this note.
2. bUnit's `InputFile` support (`InputFileContent`) is assumed adequate for the upload tests; if not, the validation logic lives in an internal class tested directly.
3. Debounce and stale-response tests are timing-sensitive; they will poll with `WaitForAssertion` and use a `TaskCompletionSource`-controlled fake provider rather than real delays.
4. Axe on the open combobox listbox and on a selected/sorted table must be checked in all three themes.
5. `aria-activedescendant` support in the axe/Playwright version in the repo is assumed; the ARIA rules are standard.

### Phase E as built (read this before the plan text above)

Confirmed scope: one on-demand script `wwwroot/js/cymru-inputs.js` (combobox only); `CyCombobox<TValue>` and `CyMultiCombobox<TValue>`; `CyDataTable<TItem>` with both `Items` and `ItemsProvider`; `CyRuleBuilder` deferred to 1.12.0.

Where the build differs from the wording of the plan:

- The combobox does not wrap `CyField`; it renders its own label, hint and error like `CyTextBox`.
- Result counts and selections are announced through the component's own `role="status"` region; load failures and file rejections use `role="alert"`.
- `CyDataTable` has no `PageSizeOptions`. After a page change focus moves to the "Showing x to y of z" line, because `CyPagination` replaces the pressed button.
- `CyFileUpload` renders its own label and error rather than using `CyField`.
- The script only calls `preventDefault` (never `stopPropagation`), so Blazor's delegated events keep working.

Written: components, CSS (`combobox.css`, `file-upload.css`, `data-table.css`), unit tests, demo pages, sidebar and search-index entries, English and Welsh overview cards, axe tests and the CHANGELOG `[1.11.0]` entry.

Also changed during verification (see below): selected rows and options use `--cymru-color-surface-alt` with `--cymru-color-text` and an edge bar, not `primary-subtle`; the busy table is no longer dimmed; the disabled file-upload prompt keeps normal text colour.

Still open after the 1.11.0 release:

1. A visual pass in light, dark and high contrast, and a Welsh text review (TASK-076).
2. The combobox's disabled option (`.cy-combobox__option--disabled`) still uses `--cymru-color-disabled-background` / `-disabled-text`. It is not axe-scanned (the closed list is `hidden`), but it has the same dark-theme flaw that failed `CyFileUpload`; switch it to `surface-alt` and `text`.
3. TASK-074 and the hash/reflection comparison in TASK-071.

### Phase E verification findings

What the first real build, test and axe runs found. These are the faults a blind-written component is likely to have, and they became the checklist for Phase F.

| Stage | Finding | Fix |
|---|---|---|
| Build | `InputFile` not recognised in `CyFileUpload.razor` (RZ10012), then `ElementReference` to `InputFile` (CS0029). | `@using Microsoft.AspNetCore.Components.Forms`. |
| Build (analyzers are errors in CI) | CA1822 on `OptionClass`, `HeaderClass`, `CellClass`, `ItemId`; CA1859 on a private `IReadOnlyList` parameter; CA1720 on the enum member `Single`. | Make helpers `static`; take `List<T>`; pragma-suppress CA1720 with a reason. |
| Test compile | CS8604 (`Id` is `string?` in `ShouldContain`); `JSException` unresolved. | `.Id!`; `using Microsoft.JSInterop;`. |
| Axe | Combobox tests failed: bUnit had no handler for the `import` of `cymru-inputs.js`. | Axe test classes that render a component importing a module need `JSInterop.Mode = Loose` plus `SetupModule(path)`. |
| Axe | `landmark-unique`: six `CyDataTable` instances in one document shared a caption, so their scroll regions shared a name. | Unique caption per rendered instance in multi-state scans. |
| Axe | Light theme contrast: the busy table used `opacity: 0.6`. | Removed; loading is shown by skeleton rows and `aria-busy`. |
| Axe | Dark and high-contrast contrast: the selected row used `--cymru-color-primary-subtle` (light, undefined in dark) with light text, ratio 1.35. | `surface-alt` + `text` + edge bar. Same change on the combobox selected option. |
| Axe | `CyFileUpload` disabled state failed in all three themes: the prompt is plain `aria-hidden` text (not a disabled control, so not exempt) using `disabled-text` on `disabled-background`, which is undefined in dark and high-contrast. | Normal text colour on the disabled fill; state shown by fill and cursor. |

Lessons recorded for later phases: dark and high-contrast themes do not define every light token (`primary-subtle`, `disabled-background`, `disabled-text`), so use tokens that every theme defines; never dim with `opacity`; non-control text is contrast-checked even when it looks disabled and even when `aria-hidden`; one axe markup render needs unique landmark names; stub every JS module in axe tests.


---

## Phase F (1.12.0): "Rules and page scaffolding"

**Status:** **scope confirmed by the maintainer on 2026-10-07: F1 + F2 + F3, with the F1 defaults in decisions 2 to 4 accepted.** **Shipped as 1.12.0 on 2026-10-09** (hand-offs 1, 2a and 2b all green in CI). The sandbox had no .NET SDK, so the code was written blind and taken to green through the maintainer's build, test and axe runs.
**Baseline:** 1.11.0 (Phase E, on nuget.org). `PackageValidationBaselineVersion` is already `1.11.0` in `CymruBlazor.csproj`; run `dotnet pack` once to confirm validation passes.
**Numbering:** `REQ-F##`; tasks continue from `TASK-077`.

**Audit of the 1.11.0 zip** (file names and contents, excluding docs and build output):

| Searched for | Found | Consequence |
|---|---|---|
| `RuleBuilder` | nothing | Starts from a clean slate. |
| Recommendations sections 6 and 7, items 1 to 13 (`CyStepper`, `CyStatCard`, `CySwitch`, `CySegmentedControl`, `CyAvatar*`, `CyNotificationBell`, `CySortableList`, `CyWorkspace`, `CyDrawer`, `CyEmptyState`, `CyMenu`, `CyToolbar`, `CySummaryList`, `CyConfirmDialog`, `CySkeleton`, `CyUnsavedChanges`, `CyCheckboxGroup`) | all present | No missing component remains in those tables except `CyRuleBuilder` (item 14). |
| `CyPage` / `CyPageSection` (Recommendations section 3) | no component; tokens `--cymru-page-max-width-narrow/default/wide/full` exist in `tokens/aliases.css` | The tokens are shipped, nothing consumes them. |
| `CyPageHeader` parameters | `Title`, `Subtitle`, `Breadcrumb`, `Actions` only | No `Eyebrow`, `Badges`/`Status` or `TitleLevel`. |
| `CyCard` parameters | `Header`, `Footer`, `Href`, `Elevation` only | No `HeadingLevel`, `Padding`, `Variant`, `Collapsible`. |
| `.cy-combobox__option--disabled` in `combobox.css` | still `disabled-background` / `disabled-text` | The Phase E dark-theme flaw is still there (carry-forward). |
| Top CHANGELOG heading | `## [1.11.0] - Unreleased` | 1.11.0 is published; the heading needs its date. |

Not proposed (Recommendations section 9): the `CymruBlazor.Testing` package and extra dev-mode diagnostics. Both are separate-package or cross-cutting work and would dilute a phase already dominated by `CyRuleBuilder`.

### Decisions to confirm before any code is written

1. **Scope.** Recommended: F1 `CyRuleBuilder`, F2 page scaffolding (`CyPage`/`CyPageSection`, `CyPageHeader` and `CyCard` additions), F3 hygiene. Alternative: F1 alone.
2. **No new JavaScript.** The builder reuses `CySortableList` (its existing module), `CyMenu`, `CyCombobox`, `CySegmentedControl`, `CyField` and the other form fields. Page scaffolding is CSS and markup only.
3. **The builder edits a model; it does not evaluate it.** No SQL, LINQ or expression generation and no JSON serialiser ships in the library; consumers translate the model on their own server. The component exposes an immutable model plus a plain-language summary.
4. **Depth and size caps** (`MaxDepth` default 3, `MaxConditions` default 50) so a nested editor stays usable with a keyboard and a screen reader.

### Global requirements (apply to every component)

| ID | Requirement |
|---|---|
| REQ-F00a | WCAG 2.2 AA: 24px minimum targets (44px where primary), visible focus, no colour-only meaning, `prefers-reduced-motion` and forced-colours respected, 2.5.7 (every reorder has a button path), 4.1.3 status messages. |
| REQ-F00b | Tokens only, and only tokens that light, dark and high-contrast all define. Selected or disabled fills use `--cymru-color-surface-alt` with `--cymru-color-text` plus an edge bar, tick or weight. No `opacity` dimming. A variant's text colour lives in the same `.razor.css` rule as its background. |
| REQ-F00c | `Cy<Name>Text` record per component with English defaults and a `Text` parameter; **no new required members on `CyLocalizedStrings`**. Welsh defaults shown on the demo pages. |
| REQ-F00d | Additive API only; no `[Obsolete]`; `docs/MIGRATION-2.0.md` untouched; package validation vs 1.11.0 passes. Existing parameter defaults of `CyCard` and `CyPageHeader` produce **byte-identical markup** (proved by a snapshot test written before the change). |
| REQ-F00e | No npm or third-party code; no new JS file. |
| REQ-F00f | Nothing new in DI is planned; TASK-074 adds the guard test that `AddCymruBlazor()` resolves everything. |
| REQ-F00g | User-supplied style values validated with `CssLength`; all text (field labels, operator labels, values, summaries) HTML-encoded. |
| REQ-F00h | Per component: razor + code-behind, CSS (`@layer components`, forced-colors, imported in `cymrublazor.css`), bUnit tests, axe tests (JS modules stubbed with `JSInterop.Mode = Loose` + `SetupModule`), demo page (`<h2>` under the page `<h1>`), sidebar and `DemoNavigationIndex` entries, English and Welsh overview cards, CHANGELOG `[1.12.0]` entry. |
| REQ-F00i | Consumer callbacks (`ValueEditor`, option providers) may throw: the component catches, shows an accessible error and never takes down the circuit. Exception text is never shown. |

### F1: `CyRuleBuilder`

| ID | Requirement |
|---|---|
| REQ-F01 | Model: `CyRuleGroup` (`Combinator` `And`/`Or`, `Not` flag, `Children`), `CyRuleCondition` (`FieldKey`, `OperatorKey`, `Value`), `CyRuleNode` base, all immutable records with `with`-style edits and a stable `Id` per node. Bound with `Value`/`ValueChanged` (a new root on every change). |
| REQ-F02 | Fields: `Fields` (`IReadOnlyList<CyRuleField>`): `Key`, `Label`, `Type` (`Text`/`Number`/`Date`/`Boolean`/`Choice`/`Coded`), `Operators` (optional override), `Options` (`CyOption<object>` list for `Choice`), `ItemsProvider` (for `Coded`, backed by `CyCombobox`), `Min`/`Max`/`Step`/`Unit` for `Number`. Operator sets default per type (`equals`, `not equals`, `contains`, `starts with`, `greater than`, `between`, `is empty`, ...), each with a localisable label and a value arity (0, 1 or 2). |
| REQ-F03 | Value editors chosen per field type from the existing controls (`CyTextBox`, `CyNumberInput`, `CyDateInput`, `CySwitch`, `CySelect`, `CyCombobox`); two inputs for `between`; none for arity 0. Custom editor via a `ValueEditor` `RenderFragment<CyRuleEditorContext>` per field. Changing the field resets an incompatible operator and clears the value, with an announcement. |
| REQ-F04 | Structure and semantics: each group is a `<fieldset>` whose `<legend>` reads "All of the following" / "Any of the following" (plus "Not" when set); conditions are `<li>` in an `<ul>` inside it. Nested groups nest the same way. Combinator uses `CySegmentedControl`. Every control has a programmatic name that includes its position ("Field, condition 2 of 3"). |
| REQ-F05 | Add condition, add group, remove condition, remove group: real `<button>`s, named "Remove condition {n}", etc. Removing a node moves focus to the next sibling's first control, else the previous sibling's, else the group's Add button. Removing a non-empty group goes through `ICyConfirmService` when available. |
| REQ-F06 | Reorder and move reuse `CySortableList` (one list per group, a shared `Group` name so conditions can move between groups), including its Move up/down buttons, keyboard model and announcements. A per-row `CyMenu` adds "Duplicate", "Move to group...", "Wrap in group" and "Ungroup". No new drag script. |
| REQ-F07 | Limits: `MaxDepth` (default 3) and `MaxConditions` (default 50) disable the matching Add buttons with an explanatory `aria-describedby` hint (not a bare disabled button). `MinConditions` (default 0). |
| REQ-F08 | Validation: an incomplete condition (no field, no operator, missing or invalid value) is flagged with text and icon, linked through `aria-describedby`, and reported by `IsValid` / `OnValidationChanged`. Works inside `CyField`/`EditForm` via a `ValidationMessage`-compatible summary; no custom validation attribute is shipped. |
| REQ-F09 | Summary: a read-only plain-language sentence ("Age is greater than 18 and (Status is Active or Status is Pending)") rendered under the builder, encoded text, `aria-live` off (it is not announced on every keystroke). `ShowSummary` default true; also available as `CyRuleSummary.Describe(group, fields, text)` for consumers. |
| REQ-F10 | `ReadOnly` renders the summary and the structure with no editing controls; `Disabled` disables everything. |
| REQ-F11 | Announcements through the component's own `role="status"` region (condition added/removed/moved/duplicated, group added, limit reached), once per action, polite. |
| REQ-F12 | `CyRuleBuilderText`: all operator labels, combinator phrases, button names, announcements, validation messages and the summary connectors (format strings; word order must be overridable for Welsh). |

| Task | Description |
|---|---|
| TASK-077 | Model records, `CyRuleField`, `CyRuleOperator`, operator catalogue, `CyRuleBuilderText.cs`, `CyRuleSummary` (pure, unit-tested). |
| TASK-078 | `CyRuleBuilder.razor(.cs)`, `CyRuleGroupView`/`CyRuleConditionView` internal components, value-editor switch. |
| TASK-079 | Structure edits (add, remove, duplicate, wrap, ungroup, move between groups) as pure functions on the model, then wired to the UI; focus handling after each edit. |
| TASK-080 | `rule-builder.css` (nested indent with edge rule, no colour-only meaning, narrow-screen stacking of field/operator/value, forced-colours). |
| TASK-081 | bUnit: model edits (every operation, depth and count caps), operator reset on field change, value editors per type, `between`, focus after removal (`RemovableHost` for disposal), confirm path, validation states, summary text, read-only and disabled, announcements, encoding of `<img onerror=x>` in labels and values, `ValueEditor` exception path. |
| TASK-082 | Axe tests (empty, populated, nested, invalid, read-only; three themes, unique group names) and demo page `/forms/rule-builder` (clinical-criteria example, `CyCombobox` coded field, summary, Welsh text, server-side translation checklist), sidebar, index, overview cards (EN/CY). |

### F2: Page scaffolding

| ID | Requirement |
|---|---|
| REQ-F20 | `CyPage`: a `<main>`-agnostic wrapper (renders a `<div>`; the layout owns the `<main>` landmark) with `Width` `Narrow`/`Default`/`Wide`/`Full` mapped to the existing `--cymru-page-max-width-*` tokens, centred, with the standard inline padding. `CyPageSection` adds vertical rhythm and an optional `Heading` + `HeadingLevel`. |
| REQ-F21 | `CyPageHeader` additions, all optional and default-off: `Eyebrow` (text above the title), `Badges` (`RenderFragment`, rendered after the title), `TitleLevel` (1 to 3; default unchanged). Default output is byte-identical to 1.11.0. |
| REQ-F22 | `CyCard` additions, all optional and default-off: `Title` (new parameter; `CyCard` had none) with `HeadingLevel` (default 3; no heading when `Title` is not set), `Padding` (`Default`/`Compact`/`None`), `Variant` (`Raised` = current behaviour, `Outlined`, `Flat`), `Collapsible` + `Expanded` (two-way) implemented with a `<button aria-expanded aria-controls>` on the heading and a `hidden` panel (no JS). Default output is byte-identical to 1.11.0. |
| REQ-F23 | `CyPageText` / `CyCardText` only if built-in phrases appear (expected: collapse/expand name). |

| Task | Description |
|---|---|
| TASK-083 | **First**, snapshot tests of current `CyPageHeader` and `CyCard` markup (this doubles as the Phase B item 0.5 pattern for these two components). |
| TASK-084 | `CyPage`, `CyPageSection`, parameter additions, CSS (`page.css`, additions to the card/header stylesheets). |
| TASK-085 | bUnit and axe tests (collapsed and expanded card, heading order, widths, three themes); demo pages `/layouts/page`, additions to the card and page-header demos; sidebar, index, overview cards (EN/CY). |

### F3: Hygiene and carry-forward

| Task | Description |
|---|---|
| TASK-086 | Combobox disabled option: switch `.cy-combobox__option--disabled` to `surface-alt` + `text` (plus a non-colour cue); add an axe state that renders a disabled option in an open list. |
| TASK-087 | TASK-074: `AddCymruBlazor()` guard test (everything resolves; nothing new required). |
| TASK-088 | CHANGELOG: date the `[1.11.0]` heading; add `## [1.12.0] - Unreleased` above it. |
| TASK-089 | Public-API reflection diff vs 1.11.0 (additions only) and a hash comparison of the five existing JS files; list new public types in the CHANGELOG. |

### F-X: cross-cutting

| Task | Description |
|---|---|
| TASK-090 | Register new CSS in `cymrublazor.css`; run the undefined-variable test and `GenerateTokens.cs --check`. |
| TASK-091 | Hand-off to you: `dotnet build`, `dotnet test`, `.\scripts\Run-AccessibilityTests.ps1` (all three themes, after `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`); fix rounds from pasted output. |
| TASK-092 | Visual pass in light, dark and high-contrast and Welsh text review for D, E and F (you). |

### Out of scope (said once)

Rule evaluation or code generation (SQL, LINQ, JSON Logic), a bundled serialiser, saved-rule libraries, drag-only interactions, a free-text expression mode, `CymruBlazor.Testing`, further dev-mode diagnostics, and R1 (the shared field-chrome refactor, still gated on Phase B item 0.5).

### Suggested build order

TASK-083 snapshots, then F2 (small, low risk, lands first so CI proves the byte-identical rule) -> F3 hygiene -> F1 in the order model and summary (pure code), structure edits (pure code), UI, CSS, tests, demo. Each lands as its own reviewable chunk so a build failure is easy to localise.

### Risks and assumptions (unverified until you build)

1. `CySortableList`'s `Group` mechanism is assumed to work across lists nested inside other list items; I will read `SortableGroupRegistry` and `sortable-list.js` before relying on it. If nesting is a problem, cross-group moves fall back to the "Move to group..." menu, which REQ-F06 already requires.
2. Nested `CyCombobox` instances in one document need unique ids and the stubbed `cymru-inputs.js` module in every bUnit and axe test.
3. Focus restoration after removal is timing-sensitive in bUnit; tests poll with `WaitForAssertion`.
4. The collapsible card relies on the `hidden` attribute and `aria-controls`; axe behaviour for a collapsed panel is assumed fine and checked in the three themes.

### Conventions (unchanged, plus Phase E lessons)

WCAG 2.2 AA; tokens only, using only tokens that light, dark and high-contrast all define; `Cy<Name>Text` records, nothing new required on `CyLocalizedStrings`; additive API only; per component razor + code-behind, CSS imported in `cymrublazor.css`, bUnit tests, axe tests (with JS modules stubbed), demo page with `<h2>` under the page `<h1>`, sidebar and `DemoNavigationIndex` entries, English and Welsh overview cards, and a CHANGELOG `[1.12.0]` heading above `[1.11.0]`. See "Phase E verification findings" for the analyzer, bUnit and axe checklist.

### Phase F as built, hand-off 1 (F2 and F3)

Written, not compiled. Deviations from the plan text above:

- Demo route is `/layouts/page` (the Layout section uses `/layouts/*`), one page for `CyPage`, `CyPageSection` and the `CyPageHeader` additions; the `CyCard` additions extend the existing `/content/card` page.
- `CyCard` needed a new `Title` parameter (it had none), so `HeadingLevel` applies to that. Because Blazor binds parameters case-insensitively, a `title="..."` attribute that used to land on the root element now binds to `Title`; noted in the CHANGELOG.
- No `CyPageText` / `CyCardText`: the collapsible button's name is the title and its state is `aria-expanded`, so no built-in phrase exists.
- The byte-identical rule is enforced by `CyCardDefaultMarkupTests` (element structure and class lists; id and style are generated). The snapshot is structural, not a golden HTML file, because no 1.11.0 output could be captured without a .NET SDK.
- TASK-074 is implemented as a reflection guard (`AddCymruBlazorGuardTests`): every `CymruBlazor.*` service injected by any component must resolve from `AddCymruBlazor()`. If it fails it names the component and the missing service; that is a real finding, not a test fault.
- TASK-086: the combobox disabled option now uses `surface-alt` + `text` + italics. No new axe state was added for it: the closed list is `hidden`, and `CyComboboxAccessibilityTests` renders the list through its own helpers that I did not change; add one if you want it scanned.
- TASK-089 (reflection diff vs 1.11.0, JS hashes) is not done: it needs the 1.11.0 package. Run `dotnet pack` and package validation instead; no JS file was touched.

Things to check first when you build: Razor inline template in `CyCard.razor` (the `@<text>` block with component tags), `CyPageHeader.razor` (`@<CyTypography ...>` template and `As="@TitleTag"` with a null value), `CyPageSection` passing `Id` to `CyTypography`, and the axe run for the three themes on `CyPagePhaseFAccessibilityTests`.

**Hand-off 1 result (2026-10-07):** build clean; 1115 of 1116 unit tests passed; axe suite 182/182 (including all three themes for `CyPagePhaseFAccessibilityTests` and the demo smoke run over every route, which covers `/layouts/page`). The one failure was in the new `AddCymruBlazorGuardTests`: the guard's assertion passed (every injected library service resolves), but the test disposed its scope synchronously and `ThemeService` only implements `IAsyncDisposable`. Fixed by using `CreateAsyncScope` and `await using`. Lesson for later tests: build providers with `await using` when `AddCymruBlazor()` registers an async-only disposable.

**Second guard fix:** after the async-dispose fix the guard reached resolution and failed because `JsFocusManager` is registered but needs the host's `IJSRuntime`. The guard now treats "registered, but needs only host-provided services" as registered (it still fails for an unregistered library type or a missing library dependency). Everything else was green: 1115 of 1116 unit tests, build clean.

### Phase F as built, hand-off 2a (F1 logic, no UI)

Hand-off 1 closed green: 1116 of 1116 unit tests, axe 182 of 182. Hand-off 2 is split in two so a failure is easy to localise.

**2a (this zip, not compiled)** is TASK-077 and TASK-079 plus their unit tests: `CyRuleModel.cs`, `CyRuleField.cs`, `CyRuleOperators.cs`, `CyRuleBuilderText.cs`, `CyRuleValues.cs` (internal), `CyRuleValidator.cs`, `CyRuleSummary.cs`, `CyRuleEdits.cs`; tests `CyRuleEditsTests`, `CyRuleOperatorsTests`, `CyRuleValidatorTests`, `CyRuleSummaryTests`. All in `Components/Forms` with namespace `CymruBlazor.Components.Forms`; none needs bUnit.

Deviations from the REQ text: `CyRuleCondition` also has `Value2` (for "between") and `ValueLabel` / `Value2Label` (display text of a coded value, so the summary can name it); `MaxDepth` counts the root group as 1 (default 3 = root plus two nested levels); edit methods return the same root instance when an edit is refused (limit, root, unknown id), which the UI uses to announce "not allowed"; `CyRuleLimits` carries `MaxDepth`, `MaxConditions` and `MinConditions`; Boolean fields use the operators `true` and `false` (no value editor) instead of a switch.

**2a result (2026-10-08):** build clean; 1201 of 1201 unit tests; axe 182/182.

**2b** is the UI: `CyRuleBuilder` (razor + code-behind), group and condition views, value editors, `rule-builder.css`, bUnit and axe tests, demo page, sidebar, search index, English and Welsh overview entries.

Things to check first when you build 2a: collection expressions as lambda return values in `CyRuleEdits` (`node => [change(node)]`, `_ => []`), `is var d and > 0` in `DepthCore`, and the `en-GB` date test in `CyRuleSummaryTests` (needs ICU culture data).

### Phase F as built, hand-off 2b (F1 UI)

**2b (this zip, not compiled)** is TASK-078, TASK-080, TASK-081 and TASK-082: `CyRuleBuilder.razor` and `.razor.cs` (all views are inline templates in the one component, not separate internal components), `wwwroot/css/components/rule-builder.css` (imported in `cymrublazor.css`), `CyRuleBuilderTests` (bUnit), `CyRuleBuilderAccessibilityTests` (axe, three themes), demo page `/forms/rule-builder`, sidebar entry, `DemoNavigationIndex` entry, English and Welsh overview cards, CHANGELOG.

Deviations from the plan text:

- **No drag-and-drop and no `CySortableList`.** Reordering uses Move up / Move down buttons, moving between groups uses a per-row menu ("Move to Group 2: ..."), which satisfies WCAG 2.5.7 and avoids nested sortable lists. Drag can be added later without changing the model.
- **No `ValueEditor` fragment.** The value editor is chosen by field type; a per-field custom editor is deferred (it needs a change to `CyRuleField`).
- `CyRuleBuilderText` gained `MoveUp`, `MoveDown`, `MoveGroupUp`, `MoveGroupDown`, `MoreGroupActions`, `GroupPosition`, `TopLevel` and `GroupName`, and `MoveTo` now takes `{0}` group name and `{1}` legend. All have English defaults; the record is unreleased.
- Issue messages are shown when `ShowIssues` is true or after `ValidateAsync()`, not as the user types. `IsValid` and `OnValidationChanged` always track the current state.
- The builder cascades a null `EditContext` to its inner editors so they never join a surrounding `EditForm`.

Things to check first when you build 2b: Razor inline templates with nested `@if` / `@switch` and component tags in `CyRuleBuilder.razor`; lambdas without parameter types converted to `EventCallback<T>` on the inner editors; the `default(EditContext)!` cascading value; the bUnit menu tests (open the `CyMenu`, click an item).


---

## Phase G (next release after 1.12.0): scoping

**Status:** **scope confirmed by the maintainer on 2026-10-09:** G-A1, G-B2, G-C, G-D and G-F, plus G-A2 as a spike first and G-B3 (`CymruBlazor.Testing`, to be scoped in detail before any code). Not in scope: G-A3, G-B1, `[Obsolete]` removals (2.0.0). Code is written without a .NET SDK and taken to green through your build, test and axe runs.
**Baseline:** 1.12.3 (1.12.1 to 1.12.3 are patch releases, the last fixing the GitHub Pages baseURL; their entries are **not in the CHANGELOG**, see TASK-105). `PackageValidationBaselineVersion` was `1.11.0` and is bumped to `1.12.3` in hand-off 1.
**Numbering:** `REQ-G##`; tasks continue from `TASK-093` (Phase F ended at `TASK-092`).
**Carry-forwards (yours, not blocking):** three-theme visual pass and Welsh text review for D, E and F; TASK-089 (`dotnet pack` validation against 1.12.0).

### Audit of the 1.12.0 zip

| Area | Finding (verified in the tree) | Consequence for Phase G |
|---|---|---|
| Recommendations section 6 and section 7 | Every listed component now exists (`CyStepper`, `CyStatCard`, `CySwitch`, `CySegmentedControl`, `CyAvatar*`, `CyNotificationBell`, `CySortableList`, `CyWorkspace`, `CyDrawer`, `CyEmptyState`, `CyMenu`, `CyToolbar`, `CySummaryList`, `CyConfirmDialog`, `CySkeleton`, `CyUnsavedChanges`, `CyCheckboxGroup`, `CyRuleBuilder`). | **G-E (missing design-system components) is empty.** Nothing to build there. |
| Rule builder | `CyRuleBuilder` has **no** per-field `ValueEditor` (`CyRuleField` has no such member) and **no** drag reorder (no `CySortableList` in `CyRuleBuilder.razor`); both were deferred in 2b. Move up/down buttons and the per-row `CyMenu` exist. | G-A is real work, additive to `CyRuleField` and `CyRuleBuilder`. |
| Tokens (Recommendations 8.2) | `tokens.json` and `tokens.d.ts` **already ship** in `wwwroot/tokens` (generated by `scripts/GenerateTokens.cs`) and `docs/design-tokens.md` exists. What is missing is the **consumer-side** undefined-variable check: `UndefinedCssVariableTests` only scans this repo. | G-B shrinks: no token reference to write; only the consumer check remains. |
| Diagnostics (Recommendations 9.3) | `ICyDiagnostics` / `CyDiagnosticsMode` / `CymruBlazorOptions` exist; codes `CY0001` (unknown icon), `CY0003` and `CY0004` (`CyButton` misuse) are in use. Not covered: missing `Label` on fields, heading-level skips, unknown tokens in `style`. | G-B diagnostics is extending an existing mechanism, not building one. Heading skips and token checks need the rendered DOM or a build step, so are not proposed as component diagnostics. |
| Testing package (9.6) | No `CymruBlazor.Testing` project in `CymruBlazor.slnx`. | New package: new csproj, pack, MinVer and release-workflow changes. Highest cost in G-B. |
| Documentation (9.2, 9.5, section 3) | No page recipes, no layout cookbook; `CyStack`, `CyCluster`, `CyContainer` have API pages only; no worked localisation example for consumer strings beside library strings. `samples/StarterApp` has a Dashboard and Settings feature but no editor, wizard or workspace recipe. | G-C is documentation and demo pages only; lowest risk. |
| Snapshot tests (item 0.5) | Only `CyCardDefaultMarkupTests` exists. No DOM snapshots for `CyTextBox`, `CySelect`, `CyTextArea`, `CyCheckbox`, `CyRadioGroup`, `CyDateInput`. R1 (shared field-chrome refactor) is still gated on this. | Item 0.5 can be done in G-D without doing R1. |
| `CyDataTable` | `PageSizeOptions` and `OnSortChanged` were dropped in Phase E (see "Phase E as built"); responsive stacked rows and `Priority` hiding are explicit non-goals. | Small optional follow-up (G-F). |
| CHANGELOG | Headings `[1.10.0] - Unreleased` and `[1.9.0] - Unreleased` are stale (both are published); date formats differ (`02/10/2026`, `2026/10/02`); the `[1.12.0]` intro ends in `..`. | Hygiene (G-D). Needs your confirmation of the real dates of 1.9.0 and 1.10.0. |
| Plan document | Status table rows for B, C and D are stale (they say demo pages "not done", "merge and tag" pending); the Phase B demo pages exist. | Hygiene (G-D). |
| Disabled combobox option | Colours fixed in F3; `CyComboboxAccessibilityTests` renders a disabled *item* but through a closed list, so the disabled option is still not axe-scanned. | G-D. |
| `[Obsolete]` members | `IHasIcon`, `IHasVariant`, `ComponentVariant`, `IconPosition`, the `KeyboardNavigation*` types and the legacy `CySidebar` members are marked for removal in 2.0.0 (`docs/MIGRATION-2.0.md`). | **Breaking: not in Phase G.** A major version is a separate decision (see below). |

### Options

| Option | Scope | Effort | Risk | Notes |
|---|---|---|---|---|
| **G-A1** | `CyRuleField.ValueEditor` (`RenderFragment<CyRuleEditorContext>`), exception-safe, never shows exception text | Small to medium | Low | Additive property on a sealed record. Needs a context type (`Value`, `Value2`, `Label`, `Disabled`, `Invalid`, `ValueChanged` callbacks) and a fallback error line. |
| **G-A2** | Drag reorder in `CyRuleBuilder` via `CySortableList` (buttons and menu stay) | Medium to large | **Medium to high** | Deferred in 2b because nested sortable lists across groups are unproven; spike `SortableGroupRegistry` and `sortable-list.js` first. Falls back to "within a group only". |
| **G-A3** | In-memory evaluator or JSON round-trip helpers | Medium | Policy | **Recommend no.** Phase F decision 3 kept evaluation and serialisation out of the library; owning a JSON shape creates a compatibility contract. Offer a documented sample in the demo instead. |
| **G-B1** | Consumer-side undefined-variable check: an MSBuild target or `dotnet` script in the NuGet `build/` folder (opt-in property) that scans the consumer's CSS against `tokens.json` | Medium | Medium | Runs on consumer builds, so false positives hurt. Must be opt-in and warning-by-default. |
| **G-B2** | Extra diagnostics `CY0005` (form field with no `Label` and no `aria-label`), `CY0006` (a component that requires `Label`, such as `CySortableList` or `CyStepper`, rendered empty, if not already thrown) | Small | Low | Uses the existing `ICyDiagnostics`; Strict throws, Lenient warns once. DEBUG-only wording is unnecessary because Lenient already exists. |
| **G-B3** | `CymruBlazor.Testing` package (bUnit + axe helpers) | Large | **Medium to high** | New package: csproj, `Directory.Packages.props`, pack scripts, release workflow, package-validation baseline, docs. axe needs Playwright in the consumer's pipeline, so it is a heavy dependency. Recommend its own release. |
| **G-C** | Docs: page recipes (list, editor, wizard, workspace) as demo pages under Getting Started plus a markdown copy in `docs/`; layout cookbook for `CyStack` / `CyCluster` / `CyContainer` / `CyPage`; localisation worked example (consumer `*Strings` beside `CyLocalizedStrings`) | Medium | Low | No library code. Recipes compile in the Demo project, so they are axe-scanned by `DemoSmokeTests` for free. |
| **G-D** | Hygiene: bump `PackageValidationBaselineVersion` to 1.12.0; CHANGELOG headings and dates; stale plan status rows; item 0.5 DOM snapshots for the six existing form fields; axe state with an open list containing a disabled option | Small | Low | Needs you to confirm the 1.9.0 and 1.10.0 release dates. |
| **G-F** | `CyDataTable` follow-ups: `PageSizeOptions` ("Rows per page") and an `OnSortChanged` callback | Small | Low | Both were dropped in E; both are additive. Optional. |
| **G-E** | Missing design-system components | none | none | Audit found nothing missing. |
| **2.0.0** | Remove the `[Obsolete]` members | Small code, large consumer impact | Breaking | Separate decision; needs a major version and a migration pass. Not proposed here. |

**Recommended scope:** G-A1 + G-B2 + G-C + G-D (+ G-F if you want a short release). All additive, no new JavaScript, no new package. Hold G-A2 for a spike, G-B1 for a design discussion, and G-B3 for its own release.

### Global requirements (apply to everything built in Phase G)

| ID | Requirement |
|---|---|
| REQ-G00a | WCAG 2.2 AA, forced-colours, reduced motion, no colour-only meaning; tokens only, using only tokens that light, dark and high-contrast define; no `opacity` dimming. |
| REQ-G00b | Additive API only versus 1.12.0; no `[Obsolete]`; `docs/MIGRATION-2.0.md` untouched; package validation passes. |
| REQ-G00c | No new required member on `CyLocalizedStrings`; new phrases go on existing `Cy<Name>Text` records with English defaults (new members must have defaults). |
| REQ-G00d | No new JavaScript and no npm or third-party code unless a confirmed item (G-A2) says otherwise. |
| REQ-G00e | Consumer callbacks may throw: catch, show an accessible message, never show exception text, never take down the circuit. |
| REQ-G00f | Per component or page: bUnit tests, axe tests (JS stubbed with `JSInterop.Mode = Loose` + `SetupModule`), demo `<h2>` under the page `<h1>`, sidebar and `DemoNavigationIndex` entries, English and Welsh overview cards, CHANGELOG `[1.13.0]` entry above `[1.12.0]`. |

### G-A1: `CyRuleField.ValueEditor`

| ID | Requirement |
|---|---|
| REQ-G01 | `CyRuleField.ValueEditor`: `RenderFragment<CyRuleEditorContext>?`, default `null` (existing behaviour unchanged). When set it replaces the built-in editor for that field's value(s) for every operator with arity above 0. |
| REQ-G02 | `CyRuleEditorContext` (sealed record): `Field`, `Operator`, `Value`, `Value2`, `Arity`, `Disabled`, `ReadOnly`, `Invalid`, `ElementId` (for the label's `for`), `DescribedBy`, and `SetValue(string?)` / `SetValue2(string?)` callbacks that route through the builder's normal edit path (new root, announcements, validation). Values stay `string` like the existing model. |
| REQ-G03 | The builder keeps the programmatic name of the editor region ("Value, condition 2 of 3") and the validation message wiring; a custom editor that renders nothing for a required value is reported as incomplete by the existing validator. |
| REQ-G04 | A throwing editor is caught per condition and replaced by a fixed, localisable "This value editor could not be shown" line (`CyRuleBuilderText.EditorFailed`), logged through `ILogger` without surfacing the message; the rest of the builder keeps working. |
| REQ-G05 | Encoding: the context exposes plain strings only; the library never renders them as markup. |

| Task | Description |
|---|---|
| TASK-093 | `CyRuleEditorContext`, `CyRuleField.ValueEditor`, `CyRuleBuilderText.EditorFailed` (default English). |
| TASK-094 | `CyRuleBuilder.razor(.cs)`: use the editor when present; per-condition error boundary; wire `SetValue*` through the existing edit path. |
| TASK-095 | bUnit: custom editor renders and edits; operator arity 0 hides it; `between` offers `Value2`; throwing editor shows the fixed line only; validation of an empty custom value; no change for fields without an editor. |
| TASK-096 | Demo: a "heart rate range" custom editor on `/forms/rule-builder`; axe state; CHANGELOG. |

### G-B2: diagnostics

| ID | Requirement |
|---|---|
| REQ-G10 | `CY0005`: a form field (`CyTextBox`, `CyTextArea`, `CySelect`, `CyNumberInput`, `CyDateInput`, `CyCombobox`, `CyMultiCombobox`, `CyFileUpload`, `CyCheckboxGroup`, `CyRadioGroup`) rendered with no `Label`, no `aria-label` / `aria-labelledby` in `AdditionalAttributes`, and not inside a `CyField`. Strict (default) behaviour is unchanged for existing code **unless the field previously rendered without error**: therefore `CY0005` **warns in both modes** and never throws, so no existing page starts failing. |
| REQ-G11 | Warnings use `ICyDiagnostics.Warn` (logged once per process per message); message names the component and the fix. |
| REQ-G12 | Documented in `docs/` and on the Getting Started page with the full code table (`CY0001` to `CY0005`). |

| Task | Description |
|---|---|
| TASK-097 | Shared helper on `CyFormFieldComponentBase` (and the non-derived fields) that checks the label once per first render. |
| TASK-098 | bUnit: warns once, silent with `Label`, with `aria-label`, inside `CyField`, and when diagnostics are not registered. |
| TASK-099 | Diagnostics table in docs and Getting Started page; CHANGELOG. |

### G-C: documentation

| ID | Requirement |
|---|---|
| REQ-G20 | Page recipes as Demo pages under Getting Started: list page (`CyPageHeader` + `CyDataTable`), editor page (`CyPage` + `CyToolbar` + `CyUnsavedChanges` + `CyField`s), wizard (`CyStepper` + `CySummaryList`), tool workspace (`CyWorkspace`). Each is a working page, not a snippet, and is axe-scanned by `DemoSmokeTests`. |
| REQ-G21 | Layout cookbook: toolbar, form row, card grid and page width policy mapped to `CyStack` / `CyCluster` / `CyContainer` / `CyGrid` / `CyPage`, with the markup beside the rendered result. |
| REQ-G22 | Localisation worked example: a consumer `AppStrings` record resolved beside `CyLocalizedStrings` through `ILocalisationService`, with a Welsh variant and a language switch. **Correction (hand-off 4):** there is no `ILocalisationService` in the library or the Demo; the real pattern is the Demo's scoped `AppStrings` service (`Library` strings plus app-owned records), so the example uses that and says so. |
| REQ-G23 | Razor trap note (Recommendations 9.4): `@(...)` for interpolated text beside static characters (`v@q` parsed as an email address). |

| Task | Description |
|---|---|
| TASK-100 | Four recipe pages, sidebar and index entries, English and Welsh overview cards. |
| TASK-101 | Layout cookbook page. |
| TASK-102 | Localisation example page and a copy of the recipes in `docs/` for readers who do not run the Demo. |
| TASK-103 | CHANGELOG. |

### G-D: hygiene

| Task | Description |
|---|---|
| TASK-104 | `PackageValidationBaselineVersion` to `1.12.3` (the latest published release). |
| TASK-105 | CHANGELOG: real dates for `[1.9.0]` and `[1.10.0]` (**you confirm**), entries for 1.12.1 to 1.12.3 (written from the GitHub commits; note `v1.12.1` has no tag), one date format throughout, remove the stray `..` in `[1.12.0]`. |
| TASK-106 | Plan document: refresh the status rows for B, C and D; keep Phase history. |
| TASK-107 | Item 0.5: structural DOM snapshot tests (as `CyCardDefaultMarkupTests`) for `CyTextBox`, `CyTextArea`, `CySelect`, `CyCheckbox`, `CyRadioGroup`, `CyDateInput`, valid and invalid. R1 itself stays out of scope. |
| TASK-108 | Axe state that renders an open combobox list containing a disabled option, in three themes. |

### G-F (optional): `CyDataTable` follow-ups

| ID | Requirement |
|---|---|
| REQ-G30 | `PageSizeOptions` (`IReadOnlyList<int>`): renders a labelled "Rows per page" `CySelect`; changing it resets to page 1 and announces the new range. Default `null` keeps current markup. |
| REQ-G31 | `OnSortChanged` (`EventCallback<(string? Key, bool Descending)>`-style record) raised in addition to the two-way parameters. |

| Task | Description |
|---|---|
| TASK-109 | Implementation, bUnit, axe state, demo additions, `CyDataTableText.RowsPerPage`, CHANGELOG. |

### Cross-cutting

| Task | Description |
|---|---|
| TASK-110 | Register any new CSS in `cymrublazor.css`; run the undefined-variable test and `GenerateTokens.cs --check`. |
| TASK-111 | CHANGELOG: new `## [1.13.0] - Unreleased` heading above `[1.12.0]`; list new public types. |
| TASK-112 | Hand-off to you: `dotnet build`, `dotnet test`, `.\scripts\Run-AccessibilityTests.ps1`; fix rounds from pasted output. |
| TASK-113 | Visual pass and Welsh review for the new pages and for D, E and F (you). |

### Suggested build order

G-D hygiene (TASK-104 to 108, small, no behaviour change) -> G-B2 diagnostics -> G-A1 `ValueEditor` (model, then UI) -> G-C documentation -> G-F if confirmed. Each lands as its own zip of new and modified files.

### Risks and assumptions (unverified until you build)

1. `CyRuleBuilder`'s inline templates are large; adding an error boundary around a user fragment may need a small internal component (`ErrorBoundary` cannot be used because it logs the exception text to the UI by default).
2. `CY0005` must not fire for fields wrapped by `CyField`, which supplies the label; the check relies on `CyFieldContext` being cascaded, which I will confirm in `CyField.razor` before writing it.
3. Recipe pages add several routes; `DemoSmokeTests` scans every route in three themes, so an axe failure on a recipe is likely and expected to need one fix round.
4. Dates for 1.9.0 and 1.10.0 are not recorded anywhere I can see; I will not invent them.

### Phase G as built, hand-off 1 (G-D hygiene)

Written, not compiled, against the 1.12.3 zip. Contents (TASK-104 to TASK-108):

- `PackageValidationBaselineVersion` is now `1.12.3` (TASK-104). If `dotnet pack` reports differences, they are real differences against the published package; read them rather than suppressing.
- CHANGELOG (TASK-105, done): new `[1.13.0] - Unreleased` heading; 1.9.0 dated 2026-10-04 and 1.10.0 dated 2026-10-06 (from you); 1.7.0 and 1.8.0 dates in ISO form; entries added for 1.12.1, 1.12.2 and 1.12.3 from the GitHub history (1.12.2 is the commit that introduced the stray `..`, which is now removed). Note for later: 1.12.3 changed the default of `CyBrandLogo.Href` from `"/"` to `""` in a patch release; it is recorded under Changed and is worth remembering when judging what counts as additive.
- Plan status rows for B, C and D refreshed (TASK-106).
- `CyFormFieldDefaultMarkupTests` (TASK-107): 12 structural snapshots (default, and hint / required / error variants) for `CyTextBox`, `CyTextArea`, `CySelect`, `CyCheckbox`, `CyRadioGroup`, `CyDateInput`. Expected outlines were derived by reading the current Razor, not by capturing output, so a first-run failure means a typo in an expectation or a real surprise in the markup; read the actual outline in the failure message before changing either side.
- `CyComboboxAccessibilityTests.Should_Have_No_Violations_With_The_List_Open_And_A_Disabled_Option` (TASK-108), light, dark and high-contrast: opens the list with ArrowDown and asserts the disabled option class is in the scanned markup.

Things to check first when you build: `ShouldBe(new[] { ... })` calls against your Shouldly version; `RenderOpen()` relies on `Render<>` and `CreateEditContext` from the axe base class.

Next hand-offs: **2** G-B2 (`CY0005`), **3** G-A1 (`ValueEditor`), **4** G-C (recipes, cookbook, localisation), **5** G-F (`CyDataTable`), then the G-A2 spike report and the G-B3 package design for your decision.

**Hand-off 1 result (2026-10-09):** build clean apart from four CA1861 warnings in `CyFormFieldDefaultMarkupTests` (fixed in hand-off 2: constant arrays moved to `static readonly` fields); 1254 of 1254 unit tests; axe 188 of 188, including the new open-list disabled-option scan in all three themes. All 12 snapshot expectations were right first time. TASK-104 to TASK-108 done; TASK-089 still yours.

### Phase G as built, hand-off 2 (G-B2, `CY0005`)

Written, not compiled. TASK-097 to TASK-099:

- `Diagnostics/CyAccessibleNameCheck.cs` (internal): the rule and wording in one place. Warns when `Label` is blank and `AdditionalAttributes` has no non-blank `aria-label` or `aria-labelledby`. Component name is the type name without the generic arity.
- `CyFormFieldComponentBase` now injects `IServiceProvider` and overrides `OnParametersSet` to run the check, so all eleven derived fields are covered with one change. Derived classes that override `OnParametersSet` already call `base` (`CyNumberInput`, `CyCombobox`); nothing else needed changing.
- Deviation from REQ-G10: **`CyFileUpload` is not included** because it already throws for an empty `Label`. **`CyField` is not a special case**: it does not cascade to its content, so a library field inside a `CyField` still needs its own `Label` or `aria-label` (raw inputs, the intended use, are not library fields and are unaffected). Risk 2 in the plan is closed by reading, not assumed.
- Tests: `AccessibleNameDiagnosticsTests` (8 facts/theories: warns once with code and component name, arity stripped, blank label, labelled, `aria-label`, blank `aria-label`, warns not throws in `Strict`, works with no diagnostics registered).
- `docs/Diagnostics.md`: modes, full code table, why `CY0005` never throws. The Getting Started demo page link is deferred to G-C.
- CHANGELOG `[1.13.0]` Added entry.

Things to check first when you build: `OnParametersSet` on `InputBase<T>` (should not be sealed; if it is, move the call to `OnInitialized` and `SetParametersAsync`); that no existing test fixture renders a field with an empty label and asserts "no warnings logged"; `AddUnmatched` with a blank value in `A_Blank_AriaLabel_Does_Not_Count`.

**Hand-off 2 result (2026-10-09):** build clean with no warnings; 1263 of 1263 unit tests; axe 188 of 188. TASK-097 to TASK-099 done.

### Phase G as built, hand-off 3 (G-A1, `ValueEditor`)

Written, not compiled. TASK-093 to TASK-096. Deviations from REQ-G01 to REQ-G05:

- **Values are `object?`, not `string`.** The rule model stores typed values (`decimal`, `DateOnly`, `string`), so `CyRuleEditorContext.Value`/`Value2` are `object?` and `SetValueAsync(object? value, string? label = null)` / `SetValue2Async` go through the builder's existing `SetValueAsync`. A blank string clears the value (existing behaviour).
- **One editor call per condition, not per value.** The editor draws both values when `Arity` is 2. It is not rendered for operators with arity 0 or when `ReadOnly`.
- **Context members:** `Field`, `OperatorKey`, `Arity`, `Value`, `Value2`, `ValueLabel`, `Value2Label`, `ValueId`, `Value2Id`, `Label`, `Label2`, `IssueId`, `Invalid`, `Disabled`. The builder supplies the wrapper (`role="group"`, `aria-label="Value"`, `aria-describedby` = the condition's message when showing), so the editor only has to label its own controls.
- **Failure handling:** `CyRuleEditorBoundary` (an `ErrorBoundaryBase`) around `CyRuleEditorHost`, which renders the consumer fragment from inside its own component so a throw in either the outer lambda or the render delegate is caught. Fixed message `CyRuleBuilderText.EditorFailed`; exception logged at Warning (event 2, `RuleValueEditorFailed`) through `ILoggerFactory` when one is registered; never shown. `@key` on field and operator gives a fresh boundary when either changes.
- New public types: `CyRuleEditorContext`; new members: `CyRuleField.ValueEditor`, `CyRuleBuilderText.EditorFailed`. Internal: `CyRuleEditorBoundary`, `CyRuleEditorHost`. Additive.
- Tests: `CyRuleBuilderValueEditorTests` (14 cases: replaces built-in, grouped and named, context contents, storing value and label, between, no editor for arity 0, built-in kept without an editor, invalid and complete states, outer and inner throw with no exception text, rest of builder keeps working, retry on operator change, Welsh fallback text, disabled, read-only); axe `Should_Have_No_Violations_With_Custom_Value_Editors_Including_A_Failed_One` in three themes.
- Demo: "Custom value editor" section on `/forms/rule-builder` (triage level as radio buttons), API row and accessibility note.

Things to check first when you build: Razor `@key` and `@<CyRadioGroup ...>` template inside a lambda in the demo `@code`; `ErrorBoundaryBase.CurrentException` and `OnErrorAsync` accessibility in your SDK (if `CurrentException` is not visible, track the exception in `OnErrorAsync` instead); that `Throwing(inOuterLambda: true)` really reaches the boundary (the host calls the outer lambda inside its own render, so it should); the retry test relies on the operator `<select>` id ending in `-operator`.

**Hand-off 3 first build (2026-10-09):** failed with two errors, both mine. (1) `CyRuleField.cs` lacked `using Microsoft.AspNetCore.Components;` for `RenderFragment<>`. (2) Razor does not discover **internal** components as tags (RZ10012), so `CyRuleEditorBoundary` and `CyRuleEditorHost` could not be used in `CyRuleBuilder.razor`; they are now opened from code (`CustomEditorBody` in `CyRuleBuilder.razor.cs`, via `OpenComponent<T>` and `SetKey`), which works for internal types. Lesson for later phases: internal components must be created with `RenderTreeBuilder.OpenComponent`, not as Razor tags. The axe run did not start (the container build failed first).

**Hand-off 3 result (2026-10-09):** green after two small fixes (a null-unsafe `id` lookup in one test, and the demo's `_unusedTriage` field made a property to clear CS0649). Build clean, 1279 of 1279 unit tests, axe 191 of 191 including `Should_Have_No_Violations_With_Custom_Value_Editors_Including_A_Failed_One` in three themes. The `No test matches the given testcase filter` line is the unit-test run skipping the axe project and is expected.

### Phase G as built, hand-off 4 (G-C documentation)

Written, not compiled. New files only apart from the sidebar, nav index and CHANGELOG:

- `Pages/GettingStarted/Recipes/`: `RecipesOverview` (`/recipes`, English and Welsh, with the Razor `v@q` trap note and a link to `docs/Diagnostics.md`), `RecipeListPage`, `RecipeEditorPage`, `RecipeWizardPage`, `RecipeWorkspacePage`, `RecipeLayoutCookbookPage`, `RecipeLocalisationPage` (TASK-100 to TASK-102). The six recipe pages are English only, like the component pages; each shows the working page, notes, and the copy-and-adapt code.
- Sidebar entries and `DemoNavigationIndex` entries under Getting Started. `DemoSmokeTests` discovers the new `@page` routes by itself, so they are axe-scanned in three themes with no test change.
- `docs/Recipes.md`: the same code for readers who do not run the Demo.
- CHANGELOG entry under `[1.13.0]` Added (TASK-103).

What to look at in the first run: a Razor compile error in one of the recipe pages is the likeliest failure (e.g. the workspace's `ValueChanged` lambda or `aria-pressed` on `CyButton`); the smoke test will report any axe finding with the route.

**Hand-off 4 result (2026-10-10):** green first time. Build clean, 1279 of 1279 unit tests, axe 191 of 191 (the smoke test now covers the seven `/recipes` routes in three themes).

### Phase G as built, hand-off 5 (G-F, `CyDataTable` follow-ups)

Written, not compiled (TASK-109):

- `PageSizeOptions` (`IReadOnlyList<int>?`) and `PageSizeChanged` (`EventCallback<int>`, so `@bind-PageSize` works). The table now keeps its own `_pageSize`, synced from the parameter exactly as `Page` and the sort are (an unchanged parameter never overwrites the user's choice). Every internal use of `PageSize` (skeleton rows, total pages, range text, in-memory paging, provider request and reload check) reads `_pageSize`. The select is a `CySelect<int>` labelled `CyDataTableText.RowsPerPage`; it lists the options plus the current size (distinct, ascending), so a `PageSize` outside the list never produces a select with nothing selected. Choosing a size returns to page 1, focuses the range line, announces `RowsPerPageChanged` ("{0} rows per page. Page 1 of {1}."), asks the provider for the new size and raises `PageSizeChanged` (and `PageChanged` only if the page actually moved). Options at or below zero throw. With no options the markup is unchanged; with `PageSize` 0 the options are ignored.
- `OnSortChanged` (`EventCallback<CyDataSortChange>`, new record `CyDataSortChange(Key, Descending)`), raised once at the end of `SortAsync`, after `SortKeyChanged`, `SortDescendingChanged` and `PageChanged`; `Key` is null when sorting is cleared.
- New `CyDataTableText.RowsPerPage` and `RowsPerPageChanged` (init properties with English defaults; additive). New CSS `.cy-data-table__page-size`.
- Tests: `CyDataTablePageSizeTests` (13 cases), one new axe state (table with a page-size select, three themes), demo section "Rows per page and sort changes", Welsh phrases and API rows.
- Watch on the first run: the `CySelect<int>` label/`for` wiring and `option[selected]` assertions in the new tests (bUnit renders the selected option from the bound value), and CA warnings on the new code.

**Hand-off 5 result (2026-10-10):** green first time. Build clean, 1293 of 1293 unit tests, axe 191 of 191.

### Phase G decision documents (hand-off 6)

- `docs/Spike-G-A2-rule-builder-drag-reorder.md`: **feasible, medium effort, moderate risk.** The model already supports it (`Reorder`, `MoveTo`). It needs two additive changes to `sortable-list.js` (skip descendants of the dragged item; pass the source list id), an opt-in `AllowDragReorder`, an always-present list for empty groups, a non-focusable grip (the keyboard path stays the existing buttons and menu), and Playwright browser tests. **Your decision: build in 1.13.0 or defer.**
- `docs/ADR/ADR-0003-testing-package-proposal.md` (Proposed): A two packages, B one package, C documentation only, D nothing. **Recommendation: C now, A when an outside consumer asks.** **Your decision.**

### Phase G decisions (2026-10-10) and remaining scope

| Decision | Outcome |
|---|---|
| G-A2 drag reorder | **Build in 1.13.0**, opt-in `AllowDragReorder`, per `docs/Spike-G-A2-rule-builder-drag-reorder.md`. |
| G-B3 testing package | **Both**: documentation first (`docs/Testing.md` recipe and copyable base class), then two packages (`CymruBlazor.Testing`, `CymruBlazor.Testing.Axe`) per ADR-0003 option A. ADR-0003 moves to Accepted when the packages land. |

| Task | Description |
|---|---|
| TASK-114 | `sortable-list.js`: skip lists inside the dragged item; pass the source list id as a fourth `OnPointerDrop` argument (additive; `CySortableList` ignores it). |
| TASK-115 | `CyRuleBuilder` `AllowDragReorder` (default false): non-focusable grip, `data-cy-*` attributes, always-present list for empty groups, `[JSInvokable] OnPointerDrop` calling `Reorder` / `MoveTo`, `CyRuleBuilderText.CannotMoveHere`, CSS (tokens only, three themes), demo and CHANGELOG. |
| TASK-116 | Playwright tests: reorder within a group, move into another group, drop into an empty group, refuse a drop on a descendant, refuse past `MaxDepth`; axe with grips in three themes. |
| TASK-117 | `docs/Testing.md` and a copyable base class (bUnit context and axe base) in the docs; CHANGELOG. |
| TASK-118 | `CymruBlazor.Testing` (bUnit helpers and markup assertions): csproj, slnx, central package versions, tests, README. |
| TASK-119 | `CymruBlazor.Testing.Axe` (Playwright and axe base with CSS resolved from static web assets): csproj, tests, README. |
| TASK-120 | CI and release workflows pack and verify all three packages; MinVer lock-step; ADR-0003 to Accepted. |

Order: TASK-117 (docs, no risk), then TASK-114 to 116 (one hand-off, one fix round expected), then TASK-118 to 120.
Still yours: three-theme visual pass and Welsh review for D, E, F and the new pages; TASK-089 `dotnet pack` against 1.12.3.

### Phase G as built, hand-off 7 (TASK-117)

Documentation only, nothing to compile: `docs/Testing.md` (bUnit base class, axe base class with the library CSS read from the NuGet package's `staticwebassets` folder, usage, notes) and a CHANGELOG entry. To check by hand: the packaged layout (`staticwebassets/css/cymrublazor.css`, `staticwebassets/CymruBlazor.bundle.scp.css`) in the `.nupkg` from TASK-089, since the axe base class depends on those paths.

### Phase G as built, hand-off 8 (TASK-114 to 116, drag reorder)

Written, not compiled:

- `sortable-list.js` (TASK-114): a list inside the dragged item is never a drop target; a wrapper marked `data-cy-drop-zone` lets an empty list be hit through its message; the ghost of an item with nested lists is its header only; new `syncTree(container, dotNetRef, options)` / `disposeTree(container)` register every list in a builder and forget removed ones. **Deviation from the task text:** the source list id is sent as a fourth argument only when a list registers with `passSource: true`, not always. `CySortableList.OnPointerDrop(string, int, int)` is public API, and `[JSInvokable]` does not accept a missing argument, so always sending four would have broken it (or changed its public signature).
- `CyRuleBuilder` (TASK-115): `AllowDragReorder` (default false), `IAsyncDisposable`, public `[JSInvokable] OnPointerDrop(target, oldIndex, newIndex, source)` mapping to `Reorder` / `MoveTo`, a `span` grip (`aria-hidden`, not focusable) on conditions and nested groups, `data-cy-*` hooks only when enabled, an empty group rendered as a drop zone (hidden empty list plus the message), `CyRuleBuilderText.CannotMoveHere`, CSS (existing tokens only, no new opacity; the shared `data-cy-drag-source` fade is overridden with a dashed outline), demo section, Welsh phrase, API and accessibility rows, CHANGELOG.
- Tests (TASK-116): `CyRuleBuilderDragTests` (bUnit, 17 cases: markup, drops, refusals, depth limit, disabled, dispose), `CyRuleBuilderDragBrowserTests` (Playwright, 12 cases) and one axe theory (three themes) in `CyRuleBuilderAccessibilityTests`. The browser tests have no Blazor circuit, so a recorder stands in for .NET; the depth-limit refusal is therefore a bUnit test (`Dropping_Past_The_Depth_Limit...`), not a browser one.

What to look at in the first run: Playwright pointer timing and the `li:has(> ...)` selectors in the browser tests; `BunitJSModuleInterop.Invocations.Count` in two unit tests; CA warnings on `CyRuleBuilder.razor.cs`; the demo smoke test over the new "Drag to reorder" section.

**Hand-off 8 result (2026-10-10):** green first time apart from one unrelated test. Build clean. Unit tests 1309 of 1310: the 17 new `CyRuleBuilderDragTests` pass; the failure is `CyComboboxTests.Should_Announce_Loading_Then_Discard_A_Stale_Response` (a bUnit `WaitForAssertion` timeout on an async provider result, a file this hand-off did not touch; it passed in hand-off 5, so it looks load-dependent, to be re-run on its own). Axe run 206 of 206: all 12 `CyRuleBuilderDragBrowserTests`, the three-theme drag axe theory, and the demo smoke test (including the new "Drag to reorder" section) pass.

### Phase G as built, hand-off 9 (TASK-118, `CymruBlazor.Testing`)

Written, not compiled. New project `src/CymruBlazor.Testing` (net10.0, packable, bUnit only; MinVer, SourceLink and the NuGet metadata as `CymruBlazor`, `EnablePackageValidation` on with no baseline until first release) and its entry in `CymruBlazor.slnx`:

- `CymruTestContext` (abstract `BunitContext`): `AddCymruBlazor()` (or `AddCymruBlazor(configure)` through the second constructor), `JSInterop.Mode = Loose`, and the five library script modules set up loose.
- `CymruMarkupChecks`: `FindProblems(string)` and `FindMarkupProblems(this IRenderedComponent<T>)`, `ShouldHaveNoProblems(string)` and `ShouldHaveNoMarkupProblems(...)`, throwing `CymruMarkupAssertionException` (with `Problems`). Checks: duplicate ids; `aria-labelledby`, `aria-describedby` and `label for` targets exist; accessible name on `input`, `select`, `textarea`, `button` and role-based controls (hidden and `type` hidden/submit/reset/button/image ignored); heading levels do not skip (the first heading may be any level). Parsed with AngleSharp, which arrives through bUnit. The public surface is these three types and nothing else.
- Tests in `CymruBlazor.Tests/Testing` (the test project now references the new one): `CymruMarkupChecksTests` (pure markup) and `CymruTestContextTests` (services registered, a script-loading component renders, options pass through, and `CyRuleBuilder` is checked with the new markup checks).
- `docs/Testing.md`, `docs/Current-Solution-Structure.md`, CHANGELOG. README in the package.

Not yet done for this package: the release workflow does not pack it (TASK-120), so it cannot be published yet. What to look at in the first run: the AngleSharp calls (`Closest`, `GetElementById`, `ParseDocument`) and CA warnings in `CymruMarkupChecks.cs`; the dogfood test on `CyRuleBuilder`, which would show a real problem or a false positive in the checks.

**Hand-off 9 result, round 1 (2026-10-10):** build clean apart from CA1826 (`ClassList.FirstOrDefault()` in `CymruMarkupChecks.Describe`); unit tests 1350 of 1351. The failure was real and a good catch by the new tests: resolving `IThemeService` from `AddCymruBlazor()` in a bUnit context makes teardown throw, because `ThemeService` implements only `IAsyncDisposable` and `BunitContext.Dispose()` is synchronous. Anyone rendering `CyThemeProvider` in a `CymruTestContext` would have hit it. Fix: `CymruTestContext` registers `new ThemeService()` (no JavaScript, container does not own it), as the library's own `CyThemeProviderTests` already do; `ClassList` indexed directly; a test that renders `CyThemeProvider` in the context.

