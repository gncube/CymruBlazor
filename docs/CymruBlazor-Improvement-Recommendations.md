# CymruBlazor: Improvement Recommendations

**Source of findings:** review of `GenomicsHandbook.Web` (CymruBlazor 1.5.0 package reference) against the public CymruBlazor repository (`gncube/CymruBlazor`, currently 1.6.0 unreleased).
**Purpose:** identify where the library makes adoption in a new solution harder than it should be, and propose components and changes that close the gap.

> **Confidence key.** *Verified* = seen directly in the app code or library source. *Inferred* = a likely cause I could not confirm by running the app.

---

## 1. Headline finding

The app uses CymruBlazor for layout chrome, buttons, cards, badges, tables and icons, but **bypasses the library for almost everything else**:

| Measure (GenomicsHandbook.Web) | Result |
|---|---|
| Raw `<input>` / `<select>` / `<textarea>` elements | **92** |
| Uses of `CyTextBox`, `CySelect`, `CyTextArea`, `CyCheckbox`, `CyDateInput` | **0** (only `CyRadioGroup` is used) |
| Uses of `CyPageHeader`, `CyStack`, `CyCluster`, `CyTabs`, `CyDialog`, `CyProgress`, `CyAccordion`, `CyContainer` | **0** |
| Files that each redefine `.dashboard-hero__*` page-header CSS | **8+** |
| `!important` declarations in app CSS | **614** |
| `[data-theme="dark"]` override selectors in app CSS | **276** |
| Hard-coded hex colours in app CSS | **558** |
| Hand-rolled stepper, KPI/stat card, status dots | present (no library equivalent) |

Every one of these is a signal that adopting the library costs more than copying markup. The recommendations below are ordered by how much friction they remove.

---

## 2. Form fields (highest impact)

### 2.1 Why the form components go unused

| Barrier | Evidence | Confidence |
|---|---|---|
| `CyFormFieldComponentBase` derives from `InputBase<T>`, which **requires a cascading `EditContext`**. Pages that are not wrapped in `<EditForm>` cannot use `CyTextBox`/`CySelect`. The app has only 2 `EditForm`s across ~40 pages. | `CyFormFieldComponentBase.cs` | Verified (source); cause of non-adoption inferred |
| No numeric field. `CyTextBox` deliberately avoids `type="number"`; there is no `CyNumberInput` with min/max/step/unit. | Domain has `QuestionType.Integer` | Verified |
| No checkbox **group**. `CyCheckbox` is a single bool. | Domain has `QuestionType.MultiSelect` | Verified |
| No combobox / typeahead. | Domain has `QuestionType.CodedTerm` | Verified |
| No file upload. | Domain has `QuestionType.FileAttachment` | Verified |
| `CySelect` needs hand-written `<option>` children; no `Items` + selectors. | `CySelect.razor` | Verified |
| No CSS-only primitives documented for raw elements, so the app **invented `.cy-text-input` / `.cy-select-input`**, which do not exist in the library (namespace collision risk). | `RecordOfDiscussionCard.razor.css` | Verified |

### 2.2 Recommendations

1. **`CyField` wrapper** (label, hint, required marker, error, correct `aria-describedby`) around *any* child control. Works without `EditContext`; optionally reads one if present. This alone would let teams adopt the accessibility wiring without adopting every input.
2. **Standalone mode** for `CyTextBox`/`CySelect`/`CyTextArea`: when no `EditContext` is cascaded, create a private one rather than throwing.
3. **`CyNumberInput<TValue>`**: `Min`, `Max`, `Step`, `Unit` suffix, parse-error message, `inputmode` handled.
4. **`CyCheckboxGroup<TValue>`**: fieldset/legend, `IEnumerable<TValue> Values`, `Items` + `ValueSelector`/`TextSelector`.
5. **`CySelect` / `CyRadioGroup` `Items` API** in addition to `ChildContent`.
6. **`CyCombobox<TItem>`**: async search, `aria-activedescendant`, loading and no-results states (for coded terms such as HPO/ICD).
7. **`CyFileUpload`**: wraps `InputFile`, shows file list, size/type validation, remove button.
8. **Publish the CSS primitives** (`.cy-input`, `.cy-select`, `.cy-textarea`, `.cy-label`, `.cy-hint`) as a documented, supported contract so raw markup themes correctly (including dark mode).

---

## 3. Page scaffolding and layout

| Issue | Evidence | Recommendation |
|---|---|---|
| `CyPageHeader` exists but is unused; each page hand-rolls a hero block. | 0 uses vs 8+ CSS copies | Make it the obvious default: add `Badges`/`Status` slot, `Eyebrow`, `TitleLevel`, and a `Breadcrumbs` slot; add a one-page "page anatomy" recipe. |
| No reusable **page width policy**. `QuestionnaireEditor` hard-caps at `max-width: 900px`, leaving the right half of a wide screen empty (your second screenshot). | `QuestionnaireEditor.razor.css` | Add `CyPage` / `CyPageSection` with `Width="Narrow|Default|Wide|Full"` mapped to tokens (`--cymru-page-max-width-*`), and a `CyWorkspace` for tool-style pages (see §6). |
| `CyStack`, `CyCluster`, `CyContainer` unused; pages use bespoke flex CSS. | 0 uses | Add a short "layout cookbook" mapping common patterns (toolbar, form row, card grid) to these primitives. |
| `CyCard` has Header/Footer slots but no heading semantics or density options. | `CyCard.razor.cs` | Add `HeadingLevel`, `Padding` (`Compact/Default`), `Variant` (`Flat/Outlined/Raised`), `Collapsible`. |

---

## 4. Tables

Screenshot 1 shows a **permanent horizontal scrollbar** on the questionnaire list even though the content is only marginally too wide. Cause (verified): `table.css` applies `white-space: nowrap` to cells, so long descriptions force overflow.

Recommendations:

1. `Wrap="true"` / `Truncate` (with tooltip) per column; do not default everything to `nowrap`.
2. **`CyDataTable<TItem>`**: `Items`, `Columns` (header, template, `Sortable`, `Width`, `Align`, `Priority` for responsive hiding), built-in empty state, loading skeleton, sticky header, density, optional row selection and row-action menu.
3. Sortable header convention with `aria-sort` and keyboard support.
4. A responsive **stacked-row mode** for narrow viewports instead of horizontal scroll.
5. Integrate with `CyPagination` (page size, total count) via one `PageState` object.

---

## 5. Buttons and icons

### 5.1 `CyButton`
- **No `Icon` / icon-only support.** Every call site nests `<CyIcon/>` and `<span>` manually, and icon-only buttons have no enforced accessible name.
  Add `Icon`, `IconPosition`, `IconOnly` (requires `AriaLabel`; log a dev warning if missing).
- **`@onclick` vs `OnClick` hazard (verified).** The editor uses `@onclick="HandlePublishAsync"`. On a component this becomes a splatted attribute that overrides the button's internal `HandleClickAsync`, **bypassing its `Disabled`/`Loading` guard**, so a double-click can publish twice. Recommend: detect `onclick` in `AdditionalAttributes` and warn in DEBUG, or strip it and document `OnClick`.
- Add `Type="Submit|Button|Reset"` discoverability and a `Confirm` helper (see `CyConfirmDialog`, §7).

### 5.2 `CyIcon` / `IconRegistry`
- **Unknown icon names throw `ArgumentException`**, which takes down the render. A typo or a not-yet-available icon crashes a page.
  Fallback to a neutral placeholder icon in Production; throw only in Development.
- **The registry is closed.** There is no public `Register(name, svg)`, so consuming apps cannot add domain icons.
  Add `IconRegistry.Register` (or an `IIconProvider` in DI).
- **Missing icons needed for editing UIs:** `grip-vertical`, `arrow-up`, `arrow-down`, `git-branch`, `layers`, `panel-right`, `monitor`, `tablet`, `smartphone`, `list-checks`, `text-cursor-input`, `hash`, `calendar-days`, `circle-dot`, `check-square`, `undo`, `redo`, `save`, `search-check`.

---

## 6. Components the design system specifies that the library lacks

The app's `docs/figma` folder contains design-system specs for components with **no CymruBlazor implementation**, which is why the app hand-rolled them:

| Design-system spec | Hand-rolled in app | Proposed component |
|---|---|---|
| `progress-stepper`, `progress-vertical-step`, `progress-timeline` | wizard stepper in `NewOrderWizard` | **`CyStepper`** (horizontal, vertical, clickable steps, `aria-current="step"`) |
| `stat-card` | `dashboard-kpi-card` | **`CyStatCard`** |
| `toggles-switch` | none yet (needed for Required / Enabled) | **`CySwitch`** (`role="switch"`) |
| segmented / tab-like toggles | none | **`CySegmentedControl`** |
| avatar / avatar group | initials circle in header | **`CyAvatar`**, **`CyAvatarGroup`** |
| notification bell | none | **`CyNotificationBell`** |

---

## 7. New components needed for the questionnaire redesign

These are proposed in priority order; the redesign document (`Questionnaire-Designer-Redesign.md`) shows where each is used.

| # | Component | Why | Key API |
|---|---|---|---|
| 1 | **`CySortableList<TItem>`** + `CyDragHandle` | Drag-and-drop reordering, including **between lists** (sections). Library currently has no drag support at all. | `Items`, `ItemTemplate`, `KeySelector`, `Group` (for cross-list moves), `OnReorder(ReorderEventArgs)`, `Disabled`, `HandleOnly` |
| 2 | **`CyWorkspace`** (+ `CyWorkspacePane`) | Multi-pane tool layout that uses the full width: palette / canvas / inspector, collapsible and optionally resizable. | `Panes`, `Collapsible`, `Resizable`, `MinWidth`, persisted sizes |
| 3 | **`CyDrawer`** (side panel) | Inspector or "add question" panel without losing context (dialog is modal and centred). | `Open`, `Side`, `Modal`, focus trap when modal |
| 4 | **`CySwitch`** | Required / Enabled toggles on each row. | `Checked`, `Label`, `OnChange` |
| 5 | **`CyCheckboxGroup`** | Multi-select question rendering. | see §2 |
| 6 | **`CyStepper`** | Section-by-section runtime preview and the order wizard. | `Steps`, `Current`, `Orientation`, `OnStepSelected` |
| 7 | **`CySummaryList`** | "Check your answers" page (NHS pattern: key, value, change link). | `Rows`, `ChangeHref` |
| 8 | **`CyEmptyState`** | Empty designer canvas, empty list. | `Icon`, `Title`, `Description`, `Actions` |
| 9 | **`CyMenu` / overflow menu** | Per-row "more" actions (duplicate, move to section, remove). | menu-button pattern with roving tabindex |
| 10 | **`CyConfirmDialog`** (helper over `CyDialog`) | "Publish", "Discard draft", "Remove question" confirmations with consistent copy and focus return. | `ConfirmAsync(...)` service |
| 11 | **`CyToolbar`** | Sticky action bar (Save status, Undo/Redo, Preview, Publish). | `role="toolbar"`, roving tabindex |
| 12 | **`CySkeleton`** | Loading state for the designer and list. | `Lines`, `Shape` |
| 13 | **`CyUnsavedChanges`** | Navigation guard + "Saving… / Saved" indicator. | `Dirty`, `SaveState`, integrates with `NavigationManager` |
| 14 | **`CyRuleBuilder`** (optional, later) | Condition → action logic editing. | `Fields`, `Operators`, `Value` editors |

### 7.1 Accessibility requirements for `CySortableList` (non-negotiable)

Drag-and-drop is the classic accessibility trap. To stay consistent with the library's WCAG 2.2 and axe-tested approach:

- **WCAG 2.5.7 Dragging Movements:** every drag operation needs a single-pointer alternative. Provide built-in **Move up / Move down** buttons and a **Move to section…** menu.
- **Keyboard:** `Space` to pick up, `↑/↓` to move, `←/→` or menu to change list, `Space`/`Enter` to drop, `Esc` to cancel.
- **Announcements:** use the existing `CyLiveRegion` / `ILiveRegionRegistry` ("Picked up *Disease status*, position 1 of 5", "Moved to position 2").
- **Minimum 24×24 px target** for the handle (WCAG 2.5.8).
- **Reduced motion:** respect `prefers-reduced-motion`.
- **Implementation:** a small on-demand ES module (same pattern as the dialog overlay module) using pointer events; no third-party dependency. Keep reorder state in C#, raise `OnReorder` with old/new index and source/target group.

---

## 8. Design tokens and theming

1. **Undocumented, easy-to-guess-wrong names.** The app used tokens that do not exist and silently fall back to the hard-coded default:

   | Used by app | Exists in library? | Closest existing |
   |---|---|---|
   | `--cymru-color-text-secondary` (8×) | No | `--cymru-color-text-muted` |
   | `--cymru-color-surface-hover` (4×) | No | (none) |
   | `--cymru-radius-full` (3×) | No | `--cymru-radius-pill` / `-circle` |
   | `--cymru-shadow-2xl` | No | (none) |
   | `--cymru-font-mono`, `--cymru-font-family-mono` | No | `--cymru-font-family-monospace` |
   | `--cymru-colour-*` (UK spelling, 2×) | No | `--cymru-color-*` |
   | `--cymru-color-primary-dark`, `--cymru-color-neutral-dark` | No | (none) |

   Because `var(--x, #fallback)` hides the mistake, the typo is invisible until dark mode breaks.
2. **Ship a token reference** (generated table, plus `tokens.json`/`tokens.d.ts`), and expose the library's existing `UndefinedCssVariable` test as a **consumer-side build check or stylelint rule**.
3. **Add missing semantic tokens** at minimum: `surface-hover`, `surface-subtle`, `text-secondary` (alias of muted), `radius-full`, `shadow-xl/2xl`, `font-family-mono` alias, `page-max-width-*`, `focus-ring-*`, status colours (`success/warning/danger/info` × `bg/border/text`).
4. **Pick one spelling.** Tokens are `color`; docs and some consumers use `colour`. Either alias both or document the rule.
5. **Dark mode must be token-driven.** The app has 276 dark-mode override selectors and 614 `!important`s, largely because raw elements and custom classes use hard-coded hex. If semantic tokens flip automatically and CSS primitives (§2.2 item 8) use them, consumer dark-mode CSS should trend to zero.

---

## 9. Developer experience and documentation

1. **Adoption gap between the library and the app.** The app pins 1.5.0 while the repository is at 1.6.0 (unreleased). Publish a release (or preview feed) and a one-page "what changed that affects consumers".
2. **"Page recipes"** (list page, editor page, wizard, tool workspace) showing the intended composition of `CyPageHeader` + `CyCard` + `CyTable`/form fields. The `StarterApp` is a good base; add a recipe for the editor pattern this app needs.
3. **Dev-mode diagnostics** (DEBUG only): warn on unknown tokens in `style`, `onclick` splatting on `CyButton`, missing `AriaLabel` on icon-only buttons, missing `Label` on fields, heading-level skips.
4. **Razor parsing trap worth documenting in the recipes:** the Version column in screenshot 1 renders `v@q.Version.Major.0` because Razor treats `v@q` as an email address. Sample code that shows `@(…)` for interpolated text next to static characters would prevent this class of bug. (App fix is in the redesign document.)
5. **Localisation:** the docs describe the `ILocalisationService` flow; add a worked example for consumer-owned strings alongside library strings (the app ends up with parallel `*Strings` classes).
6. **bUnit + axe helpers** as a small `CymruBlazor.Testing` package so consumers can reuse the library's accessibility assertions.

---

## 10. Suggested delivery order

| Phase | Items | Unblocks |
|---|---|---|
| **A: quick wins** | `CyIcon` fallback + `Register`, missing icons, token reference + aliases, `CyButton` onclick guard + `Icon`, `CyTable` wrap | Immediate de-risking; removes crashes and silent failures |
| **B: form adoption** | `CyField`, standalone mode, `CyNumberInput`, `CyCheckboxGroup`, `Items` API | Replaces the 92 raw elements; runtime renderer |
| **C: editing experience** | `CySortableList`, `CySwitch`, `CyWorkspace`, `CyDrawer`, `CyEmptyState`, `CyMenu`, `CyToolbar` | Questionnaire Designer redesign |
| **D: flow and review** | `CyStepper`, `CySummaryList`, `CyStatCard`, `CyConfirmDialog`, `CyUnsavedChanges`, `CySkeleton` | Runtime preview, order wizard, dashboard |
| **E: advanced** | `CyCombobox`, `CyFileUpload`, `CyDataTable<T>`, `CyRuleBuilder` | Coded-term and file questions; logic authoring |
