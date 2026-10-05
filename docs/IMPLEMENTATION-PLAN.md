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
| C (1.9.0) | **Implemented and verified:** build, unit tests and axe suite (168/168) pass. C1–C7 components, CSS, bUnit tests, demo pages and CHANGELOG written. Remaining: visual pass in 3 themes and `dotnet pack` vs 1.8.0 (TASK-022, TASK-024), then merge and tag (TASK-026). | CHANGELOG `[1.9.0]` |
| D (1.10.0) | **Written, not yet built or run** (see "Phase D" below). Scope confirmed: six core components plus `CySegmentedControl`, `CyAvatar`/`CyAvatarGroup`, `CyNotificationBell`. Awaiting your `dotnet build`, `dotnet test` and axe run. | `docs/IMPLEMENTATION-PLAN.md` |
| E–F | Not started | |

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
