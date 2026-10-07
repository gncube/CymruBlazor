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
| B (1.8.0) | **Implemented**, B1–B5 (see below). Demo pages and R1 not done. | CHANGELOG `[Unreleased]` |
| C (1.9.0) | **Implemented and verified:** build, unit tests and axe suite (168/168) pass. C1–C7 components, CSS, bUnit tests, demo pages and CHANGELOG written. Remaining: visual pass in 3 themes and `dotnet pack` vs 1.8.0 (TASK-022, TASK-024), then merge and tag (TASK-026). | CHANGELOG `[1.9.0]` |
| D (1.10.0) | **Implemented and verified:** build, unit tests (948) and axe/Playwright suite (168/168) pass. Six core components plus `CySegmentedControl`, `CyAvatar`/`CyAvatarGroup`, `CyNotificationBell`, demo pages, CSS, tests and CHANGELOG written. Remaining: visual pass in 3 themes, Welsh text review, `dotnet pack` vs 1.8.0, then merge and tag. | CHANGELOG `[1.10.0]` |
| E (1.11.0) | **Shipped: 1.11.0 is on nuget.org (2026-10-07).** `CyCombobox`/`CyMultiCombobox`, `CyFileUpload` and `CyDataTable<T>` with CSS, bUnit and axe tests, demo pages, English and Welsh overview cards and CHANGELOG. Build, unit tests and the containerised axe suite (179 tests, three themes) were taken through to green by the maintainer. Open: three-theme visual pass and Welsh text review (not confirmed done). `CyRuleBuilder` deferred. | See "Phase E" below, including "Phase E verification findings" |
| F (1.12.0) | **Scope confirmed (F1 + F2 + F3, rule builder defaults accepted).** In scope: `CyRuleBuilder` (deferred from E), page scaffolding (`CyPage`/`CyPageSection`, `CyPageHeader` and `CyCard` additions) and hygiene carry-forwards. **F2 and F3 built and green** (hand-off 1, one test fix pending merge); F1 not started. Package validation baseline is already 1.11.0. | See "Phase F" at the end |

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

**Status:** **scope confirmed by the maintainer on 2026-10-07: F1 + F2 + F3, with the F1 defaults in decisions 2 to 4 accepted.** Build in progress: F2 and F3 passed build, unit and axe runs (hand-off 1, see "Hand-off 1 result"); `CyRuleBuilder` (F1) follows as hand-off 2. The sandbox has no .NET SDK, so all code will again be written blind and taken to green through the maintainer's build, test and axe runs.
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
