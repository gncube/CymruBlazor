# Changelog

All notable changes to this project are documented here. This project
follows [Semantic Versioning](https://semver.org/); version numbers are
derived automatically from git tags by [MinVer](https://github.com/adamralph/minver) -
see `CONTRIBUTING.md` for the release process.

Full detail for every release is also available as auto-generated
[GitHub Releases](https://github.com/gncube/CymruBlazor/releases).

## [1.12.0] - Unreleased

Roadmap v1.12.0 ("Rules and page scaffolding", Phase F of `docs/IMPLEMENTATION-PLAN.md`).
This first part adds the page scaffolding; `CyRuleBuilder` follows in the same release.
Everything here is additive: no existing member is removed or changed, no new
`[Obsolete]` members, so `docs/MIGRATION-2.0.md` is untouched. No new required member on
`CyLocalizedStrings`. No new JavaScript.

### Added

- **`CyPage` and `CyPageSection`** (Layout): `CyPage` centres page content and caps its
  width with `Width` (`CyPageWidth.Narrow`, `Default`, `Wide`, `Full`), using the existing
  `--cymru-page-max-width-*` tokens and the standard gutters (`RemovePadding` to drop
  them). It renders a `div`; the layout keeps the single `main` landmark. `CyPageSection`
  adds vertical spacing and an optional `Heading` (`HeadingLevel`, default 2); with a
  heading it is a `section` labelled by it, without one a plain `div`.
- **`CyPageHeader`**: `Eyebrow` (text above the title), `Badges` (status content beside
  the title) and `TitleLevel` (1 to 6, default 1; the look does not change).
- **`CyCard`**: `Title` with `HeadingLevel` (default 3), `Padding` (`CyCardPadding.Default`,
  `Compact`, `None`), `Variant` (`CyCardVariant.Raised`, `Outlined`, `Flat`) and
  `Collapsible` with a two-way `Expanded` / `ExpandedChanged`. A collapsible card's title is
  a button (`aria-expanded`, `aria-controls`) over a `hidden` body; no script. It needs a
  `Title` and cannot be combined with `Href`.
- Demo page `/layouts/page`, a new example block and API rows on the `CyCard` page, with
  sidebar, search-index and English and Welsh overview entries.

### Changed

- `CyCard` and `CyPageHeader` produce the same markup as 1.11.0 when none of the new
  parameters is set. One edge case: a `title="..."` attribute passed to `CyCard` used to fall
  through to the `div` as a tooltip; the parameter is now `Title` and renders a heading.
  Use `Class`/`AdditionalAttributes` with another attribute if you relied on the tooltip.
- The disabled option of `CyCombobox` / `CyMultiCombobox` now uses `surface-alt` with the
  normal text colour (and italics) instead of the `disabled-*` tokens, which stay light in
  the dark and high-contrast themes and failed contrast there.

### Tests

- New guard test that every library service injected by a component resolves from
  `AddCymruBlazor()` (plan item TASK-074).

## [1.11.0] - 2026-10-07

Roadmap v1.11.0 ("Advanced inputs and data", Phase E of `docs/IMPLEMENTATION-PLAN.md`):
a searchable combobox, a file upload and a data table. Everything here is
additive: no existing member is removed or changed, no new `[Obsolete]`
members, so `docs/MIGRATION-2.0.md` is untouched. No new required member on
`CyLocalizedStrings`: every new phrase lives in a `Cy<Name>Text` record with
English defaults. `CyRuleBuilder` is deferred to 1.12.0.

### Added

- **`CyCombobox<TValue>` and `CyMultiCombobox<TValue>`** (Forms): WAI-ARIA 1.2
  list-autocomplete combobox for choosing one item or several from a long list.
  Items are `CyOption<TValue>`, either in memory (`Items`, `Filter`) or from an
  async `ItemsProvider` (debounced; a slower, older answer never replaces a newer
  one). `MinSearchLength`, `MaxResults` (the user is told when the list is
  capped), `AllowClear`, `SelectedText` / `SelectedOptions` for values not in the
  loaded list, `MaxListHeight`. Results, loading, no results and each choice are
  spoken through a polite status region. Chosen items of the multiple version are
  a list of chips with "Remove {item}" buttons. `CyComboboxText`.
- **`CyFileUpload`** (Forms): a labelled native file input as the drop area, with
  `Accept`, `Multiple`, `MaxFileSize` (default 5 MiB), `MaxFiles`, `MaxTotalSize`,
  rejection messages that name the file and the limit, a file list with progress,
  Cancel, Retry and Remove (each named with the file), and an `Upload` callback
  (`CyFileUploadContext` / `CyUploadResult`). `AutoUpload`, `MaxParallel`,
  `OnFilesChanged`, `OnRejected`, `CyFileUploadText`.
- **`CyDataTable<TItem>`** (Data): composed from the unchanged `CyTable`. Optional
  sorting (`aria-sort` only on the sorted column, announced, stable), row selection
  (checkbox or radio per row named with `RowLabel`, select-page header checkbox,
  `KeySelector` so a selection survives paging) and paging (range line, page
  announcements), over in-memory `Items` or an async `ItemsProvider` with loading,
  stale-response discard and an error row with Retry. `CyDataColumn<TItem>`,
  `CyDataTableRequest`, `CyDataTableResult<TItem>`, `CyDataTableText`.
- Demo pages `/forms/combobox`, `/forms/file-upload` and `/data/data-table`, with
  sidebar, search-index and English and Welsh overview entries.
- One new on-demand script, `wwwroot/js/cymru-inputs.js`, loaded only by the
  combobox: it prevents the browser default for Up, Down, Enter and Escape while
  the list is in use. No npm package or third-party code; existing scripts are
  unchanged. `CyFileUpload` and `CyDataTable` need no script.

### Security

- `CyFileUpload`'s size, count and type checks are advisory and exist to catch
  honest mistakes early; a browser can report anything. The library ships no upload
  endpoint: your `Upload` callback decides where a file goes, and the server must
  check size, type and content again. `MaxFileSize` is also passed to
  `OpenReadStream`, file names are always HTML-encoded, and exception text is
  never shown to the user.

### Not included

- `CyDataTable`: virtualisation, inline editing, column resizing, reordering or
  hiding, stacked rows on small screens, grouping, expandable rows, a filter UI,
  export and ARIA grid keyboard navigation. Use `QuickGrid` or build on the
  component for those.

## [1.10.0] - Unreleased

Roadmap v1.10.0 ("Flow and review", Phase D of `docs/IMPLEMENTATION-PLAN.md`):
the components for multi-step journeys, review pages, dashboards and safe
editing. Everything here is additive: no existing member is removed or
changed, no new `[Obsolete]` members, so `docs/MIGRATION-2.0.md` is untouched.
No new required member on `CyLocalizedStrings`: every new phrase lives in a
`Cy<Name>Text` record with English defaults. No third-party script or npm
package is introduced, and no existing JavaScript file is changed.

### Added

- **`CyStepper` and `CyStepItem`** (Layout): horizontal or vertical progress
  indicator. A `nav` with an ordered list; the current step has
  `aria-current="step"`, other states are spoken as hidden text and shown by
  icon or number. `Clickable` steps are buttons (or links with `Href`);
  upcoming steps need `AllowForwardNavigation`. `@bind-Current`,
  `OnStepSelected`, `CyStepperText`. Below 40rem the horizontal layout becomes
  a "Step 2 of 4: Title" summary.
- **`CySummaryList` and `CySummaryRow`** (Content): "check your answers" list
  as a `dl` with key, value and a change link or button. The change action's
  name includes the key ("Change Date of birth", WCAG 2.4.4). `Borders`,
  `Card`, `CySummaryListText` ("Change", "Not provided").
- **`CyStatCard`** (Content): key figure with `Unit`, `Trend` and `TrendText`
  (always written out; the arrow is decorative), `TrendTone`, `Colour` accent,
  `Href` (whole card is a link), `Loading` (skeleton plus `aria-busy`) and
  optional `HeadingLevel`. `CyStatCardText`.
- **`ICyConfirmService`, `CyConfirmService`, `CyConfirmOptions` and
  `CyConfirmDialog`** (Accessibility): `await ConfirmAsync(...)` over
  `CyDialog` (ADR-0001). One `CyConfirmDialog` host in the layout. Escape, the
  close button, Cancel, a cancelled token and removing the host all answer
  `false`, so the task never hangs; concurrent requests queue; a destructive
  request focuses Cancel and uses the danger button. Without a host,
  `ConfirmAsync` throws a clear `InvalidOperationException`. Registered
  (scoped) by `AddCymruBlazor()`. `CyConfirmText`.
- **`CyUnsavedChanges`** (Feedback): navigation guard plus a Saving / Saved /
  Unsaved / Error indicator (`role="status"`, always present, announces on
  change). Uses `NavigationManager.RegisterLocationChangingHandler` and asks
  through `ICyConfirmService` when a host exists; otherwise cancels the
  navigation and raises `OnNavigationBlocked`. Fragment-only changes are not
  guarded. `CySaveState`, `CyUnsavedChangesText`.
  *Limit:* reload, closing the tab and external links are not guarded (that
  needs a `beforeunload` script).
- **`CySkeleton`** (Feedback): text bars, rectangle or circle placeholder.
  Shapes are `aria-hidden`; one hidden "Loading" text per skeleton (turn off
  with `Announce` inside a group). `Width`/`Height` accept plain lengths only.
  Shimmer stops under `prefers-reduced-motion`.
- **`CySegmentedControl<TValue>`** (Forms): joined single-choice control on
  native radios (arrow-key behaviour from the browser), `@bind-Value`, items
  as `CyOption<TValue>`.
- **`CyAvatar`, `CyAvatarGroup`, `CyAvatarItem`** (Content): photo or initials
  on one of six theme-token colours chosen from the name; `Decorative` for use
  beside a visible name; group with "+N" badge read as "and N more".
- **`CyNotificationBell`** (Layout): button or link with an unread count in
  its accessible name ("Notifications, 3 unread"), `Max` ("99+"), `Expanded`
  for `aria-expanded`, optional polite `Announce`.
- Stylesheets `flow.css` and `controls.css`, token-only so light, dark and
  high-contrast follow automatically; `forced-colors` and
  `prefers-reduced-motion` handled.
- Demo pages for all of the above, registered in the sidebar, search index and
  the Forms, Content, Feedback and Navigation overviews (English and Welsh).
  `MainLayout` now hosts `<CyConfirmDialog />`.

### Notes for consumers

- To use `ICyConfirmService` (or the leave-the-page prompt in
  `CyUnsavedChanges`), add `<CyConfirmDialog />` once to your layout.
- The Welsh overview-card descriptions are first drafts and should be checked
  by a Welsh speaker before release.

---

### Fixed

- **`CyButton` Danger in the dark theme** failed colour contrast (white text on the pale red fill). The label now uses the dark surface colour in the dark theme (`CyButton.razor.css`).

## [1.9.0] - Unreleased

Roadmap v1.9.0 ("Editing experience", Phase C of
`docs/IMPLEMENTATION-PLAN.md`): the primitives the Questionnaire Designer
needs. Everything here is additive: no existing member is removed or changed,
no new `[Obsolete]` members, so `docs/MIGRATION-2.0.md` is untouched. R1 (the
shared field-chrome refactor) stays deferred, so the DOM of existing form
fields is unchanged. No third-party script or npm package is introduced.

### Added

- **`CySortableList<TItem>` and `CyDragHandle`.** Reorderable list with
  `Items`, `ItemTemplate`, `KeySelector`, `Group`, `OnReorder`, `Disabled`,
  `HandleOnly`, `ShowMoveActions`, `EmptyContent` and localisable `Text`. The
  list never mutates `Items`; it raises `CyReorderEventArgs<TItem>` (item, old
  and new index, source and target list ids) and the page applies the move.
  Lists sharing a `Group` and item type can exchange items.
  - Pointer drag uses Pointer Events in an on-demand module
    (`sortable-list.js`), so mouse, touch and pen behave the same.
  - **WCAG 2.5.7:** built-in Move up, Move down and "Move to..." buttons do
    everything dragging does with a single click.
  - **Keyboard:** Space picks up, Up/Down/Home/End move, Space or Enter drops,
    Escape (or leaving the handle) cancels. The move is previewed with CSS
    `order`, so the DOM and focus do not jump until the drop.
  - **Announcements** for pick-up, each move, drop, cancel and cross-list
    moves go through `ILiveRegionRegistry`, with a local live region as the
    fallback when it is not registered.
  - Handle and buttons are 44px (WCAG 2.5.8); `prefers-reduced-motion` and
    forced-colours are respected.
- **`CySwitch`** (`role="switch"`, `aria-checked`, `OnChange`), a
  `CyFormFieldComponentBase<bool>` that works with or without an `EditForm`.
- **`CyWorkspace` / `CyWorkspacePane`**: multi-pane tool layout with `Fill`,
  `Width`, `MinWidth`, `Collapsible` / `@bind-Collapsed`. Collapsed content
  stays in the DOM. `Width`/`MinWidth` accept plain lengths only. Panes stack
  below 48rem.
- **`CyDrawer`**: slide-over panel with `Open`, `Side`, `Size`, `Modal`,
  `Footer`, `Dismissible`, `CloseOnBackdropClick`. Modal drawers are a native
  `<dialog>` (ADR-0001: inert background, contained Tab, Escape); non-modal
  drawers are an `<aside>` that takes and returns focus.
- **`CyEmptyState`**: icon, title, description (always encoded), actions.
- **`CyMenu` / `CyMenuItem`**: WAI-ARIA menu-button pattern with roving
  `tabindex`, Arrow/Home/End/Escape/Tab handling, disabled and destructive
  items.
- **`CyToolbar`**: `role="toolbar"`, one tab stop with arrow-key navigation
  (arrows left alone in text fields), `Orientation`, `Sticky` top/bottom.
- Stylesheets `switch.css`, `sortable-list.css` and `editing.css`, token-only
  so light, dark and high-contrast follow automatically.
- Demo pages for all of the above, registered in the sidebar, search index and
  the Forms, Content and Layout overviews.

### Fixed

- `SortableGroupRegistry` was not registered by `AddCymruBlazor()`, which
  silently disabled every cross-list move. It is now registered (scoped).
- The topmost changelog section was headed `[1.7.0]` but describes 1.8.0
  (Phase B); relabelled `[1.8.0]`.

### Notes

- Verified: the solution builds; the unit tests pass (no failures in
  `CymruBlazor.Tests`); the containerised Playwright/axe suite passes 168 of
  168, including every new demo route in light, dark and high-contrast. The
  first axe run found one real violation (heading order on the sortable-list
  demo), fixed before release.
- `sortable-list.js` and `cymru-editing.js` were also exercised in jsdom
  (toolbar roving focus, key guard, `focusIn`, dispose).
- `scripts/Run-AccessibilityTests.Container.ps1` now builds from a clean copy
  of the sources, so host `bin/` and `obj/` output (Windows paths) cannot break
  the Linux container, and the container no longer writes into the host's
  `obj/`.
- Still to do before tagging: a visual pass in all three themes (the CSS has
  not been looked at in a browser by a person) and `dotnet pack` against the
  1.8.0 baseline.

## [1.8.0] - 02/10/2026

Roadmap v1.8.0 ("Make forms adoptable", Phase B of
`docs/IMPLEMENTATION-PLAN.md`). Everything here is additive: no existing
member is removed or changed (reflection diff against the 1.7.0 build: 0
removed/changed, 6 new types), no new `[Obsolete]` members, so
`docs/MIGRATION-2.0.md` is untouched. The existing form fields render
byte-identical markup inside an `EditForm` (checked on 14 scenarios).

### Added

- **Standalone mode (B3).** `CyTextBox`, `CySelect`, `CyCheckbox`,
  `CyRadioGroup`, `CyTextArea` and `CyDateInput` now work with **no**
  `<EditForm>`. `InputBase` itself tolerates a missing `EditContext`; the
  library's own validation helpers dereferenced it unguarded and threw
  `NullReferenceException`. They are now null-safe, so
  `<CyTextBox @bind-Value="x" Label="Name" />` works on any page. Without an
  `EditContext` there is no validation store, so a field shows only errors it
  raised itself (see `CyNumberInput`). `@bind-Value` is still required (the
  framework needs the `ValueExpression`).
- **`CyField` (B1).** Label, hint, required marker and error around *any*
  control, with the `for`/`id`/`aria-describedby`/`aria-invalid`/`aria-required`
  wiring done for you via `context.Attributes`. Needs no `EditContext`: pass
  `Error="..."`, or inside an `EditForm` pass `For="@(() => model.X)"` and it
  shows and clears that field's messages as the form validates (`Error` wins
  when both are set). `Group="true"` renders a `fieldset`/`legend` for a set of
  controls that answer one question.
- **`CyNumberInput<TValue>` (B4).** `int`/`long`/`short`/`float`/`double`/
  `decimal` and nullable forms, with `Unit`, `Min`, `Max`, `Step`, `Culture`
  and overridable messages. Renders `type="text"` with an `inputmode` hint (not
  `type="number"`), parsing and range/step checks in C#. The unit is part of the
  field's accessible description. Works with or without an `EditForm`.
- **`CyCheckboxGroup<TValue>` (B4).** "Select all that apply" bound to an
  `IEnumerable<TValue>`, rendered as `fieldset`/`legend`, with per-item hint
  and disabled state and an optional `Comparer`.
- **`Items` on `CySelect` and `CyRadioGroup` (B5)**, plus `Placeholder` on
  `CySelect`. Both take `IEnumerable<CyOption<TValue>>` (`Value`, `Text`,
  optional `Hint`, `Disabled`); `CyOptions.ToCyOptions(...)` projects any
  sequence and `CyOptions.FromEnum<T>()` builds one from an enum. A single
  shared record means **no new type parameter**, so existing
  `CySelect<TValue>`/`CyRadioGroup<TValue>` usages compile unchanged;
  `ChildContent` still works and renders after `Items`.
- **CSS primitives for raw markup (B2):** `.cy-input`, `.cy-label` and
  `.cy-hint` (and `.cy-select`/`.cy-textarea` for raw controls) are now
  supported, theme-aware classes, including an `aria-invalid="true"` style, so
  pages that keep a native control need no dark-mode overrides.

### Changed

- `CyFormFieldComponentBase<TValue>` gains protected `ValidationMessages`,
  `ValidationMessageText` and `LocalError`; `HasValidationError` is now
  null-safe. Derived components outside this library are unaffected unless
  they declare members with these exact names.

### Notes

- The new bUnit/xunit and axe tests for the Phase B work have passed in CI.
  This includes `StandaloneFieldTests`, `CySelectItemsTests`,
  `CyNumberInputTests`, `CyCheckboxGroupTests`, `CyFieldTests`, and the axe
  scans for the new components. The library compiles with no new diagnostics
  and a 97-check render harness passes.

## [1.7.0] - 2026/10/02

Roadmap v1.7.0 ("Quick wins and de-risking", Phase A of
`docs/IMPLEMENTATION-PLAN.md`). Everything here is additive: no existing
API, default or rendering changes, and no new `[Obsolete]` members, so
`docs/MIGRATION-2.0.md` is untouched. Package validation runs against 1.6.0.

### Added

- **Diagnostics mode.** `services.AddCymruBlazor(options => ...)` (new
  overload) accepts `CymruBlazorOptions.Diagnostics`: `Strict` (the default,
  today's behaviour: misuse throws) or `Lenient` (log a warning, render a safe
  fallback). The package ships as a Release build and so cannot detect
  Development itself; set it from your own environment check, e.g.
  `o.Diagnostics = env.IsDevelopment() ? Strict : Lenient`. Each distinct
  warning is logged once per process. Components resolve the service
  optionally, so hosts that never register it keep `Strict`.
- **`CyIcon` in `Lenient` mode** renders a neutral `unknown` placeholder for an
  unrecognised name instead of throwing (`CY0001`).
- **`IconRegistry.Register(name, markup, domain?, overwrite?)`** for
  app-specific icons. Thread-safe. Markup is rendered as raw HTML, so it is
  validated against an allow-list (SVG shape elements and geometry/presentation
  attributes only; scripts, event handlers, styles, links, namespaces, DTDs and
  text are rejected). Built-in icons can never be replaced. `AllNames` now
  returns a snapshot and includes registered icons.
- **18 icons** (Lucide, ISC): `grip-vertical`, `arrow-up`, `arrow-down`,
  `git-branch`, `layers`, `panel-right`, `monitor`, `tablet`, `smartphone`,
  `list-checks`, `text-cursor-input`, `hash`, `calendar-days`, `circle-dot`,
  `check-square`, `redo`, `search-check`, `unknown`. (`save` and `undo` were
  already present.) Generic UI glyphs keep their Lucide names; domain concepts
  keep semantic names.
- **`CyButton`**: `Icon`, `IconPlacement` (new `ButtonIconPlacement`:
  `Start`/`End`, logical so it mirrors in RTL) and `IconOnly`. An icon-only
  button needs an accessible name from `AriaLabel`, `AriaLabelledBy`, or
  `ChildContent` (kept visually hidden); without one it throws in `Strict` and
  warns (`CY0004`) in `Lenient`.
- **`CyButton.AutoLoading`** (opt-in): the button enters its loading state for
  as long as `OnClick` is running, so an impatient second click cannot re-enter
  an `async` handler. Off by default because existing handlers may rely on
  re-entrancy.
- **`CyTable.Wrap`** (opt-in) plus per-cell `cy-table__cell--wrap`,
  `--nowrap` and `--truncate` classes. Cells still default to one line.
- **Design tokens**: `--cymru-color-surface-subtle`, `-surface-hover`,
  `-text-secondary`; status `-success-border`, `-warning-border`,
  `-warning-text`, `-danger-border`, `-info-border`, `-info-text`;
  `--cymru-focus-ring-{width,color,offset}`; `--cymru-font-family-mono` and
  `--cymru-font-mono`; `--cymru-radius-full`; `--cymru-shadow-2xl`;
  `--cymru-page-max-width-{narrow,default,wide,full}`; and a generated
  `--cymru-colour-*` (UK spelling) alias for every `--cymru-color-*` token.
  All are defined on every theme scope, so they follow dark and high-contrast.
- **`scripts/GenerateTokens.cs`** generates `tokens.json`, `tokens.d.ts`,
  `docs/design-tokens.md` and the colour aliases from the token CSS. With
  `--check` (now a CI step) it fails if those are stale, or if a token derived
  from a theme-overridden token is declared only on `:root`.

### Fixed

- **Accessibility smoke tests failed at browser launch.**
  `docker/playwright/Dockerfile` was pinned to Playwright 1.62.0 while the
  `Microsoft.Playwright` package is 1.63.0, so Chromium was missing in the
  container (`scripts/Run-AccessibilityTests.ps1` and the `Accessibility CI
  Gate` workflow both failed). The image version is now derived from
  `Directory.Packages.props` (Dockerfile `ARG PLAYWRIGHT_VERSION`, passed by
  the script and by `accessibility.yml`), so a package bump cannot desync it
  again.

### Changed

- `PackageValidationBaselineVersion` is now **1.7.0** (was 1.2.0), the last
  published release, so package validation guards everything added since 1.2.0.
- CHANGELOG housekeeping: the 1.5.2 release was filed under a duplicate
  `[1.5.1]` heading (and carried 1.5.1's intro); `1.4.0` now precedes `1.3.0`.

### Notes

- **`@onclick` on `<CyButton>` was already guarded.** The improvement
  recommendations described it as a splatted attribute that bypasses the
  `Disabled`/`Loading` guard. It does not: Blazor binds parameters
  case-insensitively, so `@onclick` binds to the `OnClick` parameter. Verified
  against the unmodified 1.6.0 code (0 handler calls while `Loading`). The real
  gap was re-entrancy of an in-flight `async` handler, addressed by
  `AutoLoading`; a regression test now locks the existing behaviour.
- `CyIcon`, `CyButton` and `IconRegistry` changes are source- and
  binary-compatible with 1.6.0.

## [1.6.0] - 2026-09-28

Roadmap v1.6.0 ("Localisation step 2"), plus a full illustrative Welsh
pass across the Demo site while the translator review is arranged. No
breaking changes; no new `[Obsolete]` members, so
`docs/MIGRATION-2.0.md` is untouched this release.

### Added

- `ICyLocalizer` (ADR-0002): a central, cascadable source of the strings
  CymruBlazor components fall back to when their own override parameter
  is left unset. Register the default implementation with
  `AddCymruBlazor()`, wrap your layout's body in the new
  `CyLocalizationProvider` to cascade it, and `CyAlert`, `CyBreadcrumb`,
  `CyNavigation`, `CyToastContainer`, `CyDialog`, `CyCodeBlock`,
  `CySidebar`, `CyPagination` and `CySpinner` pick it up automatically.
  An explicit override parameter always wins, unchanged from the 1.3.0
  step-1 pattern; a consumer who registers nothing, or never adds
  `CyLocalizationProvider`, sees no behaviour change at all.
- `CyLocalizedStrings`: the record of built-in English/Welsh fallback
  text `ICyLocalizer` serves. Deliberately does not cover
  `CyDateInput`'s day/month/year and range-error parameters or
  `CyTextArea`'s character-count formats - those are plain (non-nullable)
  `string` parameters with a hardcoded default rather than the
  `string? ?? "..."` shape, so adding a localiser fallback there would
  mean a breaking type change. Bind those directly to your own
  catalogue instead (see the Demo's `AppStrings.Library` for the
  pattern); no library change was needed for that.
- Demo: `AppStrings.EnglishLibrary`/`WelshLibrary` now source every
  field `ICyLocalizer` covers directly from `CyLocalizer.English`/
  `CyLocalizer.Welsh`, so the built-in strings live in exactly one
  place. The eleven `CyDateInput`/`CyTextArea` fields it doesn't cover
  stay as Demo literals, unchanged.
- `LocalisationPage`: new "Step 2" section replacing the old "proposed
  in ADR-0002 (not built yet)" note.
- Illustrative Welsh added, following the existing per-page
  `Strings.Language == AppLanguage.Welsh ? ... : ...` convention, to
  `Home`, `DesignTokens`, and their code-behind sample content
  (previously English-only). Combined with pages already bilingual
  from earlier releases, roughly 20 of the Demo's ~57 pages are now
  fully bilingual; see the translation review pack's "Coverage status"
  sheet for exactly which, and `Known limitations` below for what's
  queued next.

### Tests

- `CyLocalizerTests`: the new service's language-switching behaviour in
  isolation - defaults to English, `SetLanguage` switches `Strings`/
  `LangTag` and raises `LanguageChanged` exactly once per real change
  (a no-op setting the current language again does not re-raise, matching
  `AppStrings.SetLanguage`'s existing contract), unsubscribed handlers
  are not invoked, and `English`/`Welsh` are complete and distinct from
  each other (same shape as `AppStringsTests`'s equivalent check).
- `CyLocalizationProviderTests`: cascades to a nested component with no
  override, an explicit override still wins over the cascaded value, a
  language change re-renders, and - the important negative case - a
  component *not* wrapped in the provider ignores a registered (but
  never cascaded) `ICyLocalizer` and keeps its old English literal.
- `AppStringsTests`/`LocalisationOverrideTests` needed no changes:
  neither wraps its components in `CyLocalizationProvider`, so
  `Localizer` cascades as `null` in both and every fallback still
  resolves to the same English literal as before.

### Known limitations

- **All Welsh text across the Demo site remains illustrative, not
  translator-reviewed** - proceeding without it per an explicit
  decision, so the whole bilingual surface can be reviewed in one pass
  once ready rather than piecemeal. A `CymruBlazor-Welsh-Translation-Review-Pack.xlsx`
  work package (source English, illustrative Welsh draft, and blank
  columns for the reviewed replacement and reviewer notes, organised
  by shared strings vs. per-page copy, plus a coverage-status sheet)
  has been handed to the translator. Expected back within a fortnight;
  will land as a follow-up patch, expected to touch only string
  literals (`CyLocalizer.Welsh`, `AppStrings`'s Welsh records, and the
  per-page Welsh branches/ternaries) with no code-shape changes.
- Roughly 37 of the Demo's ~57 pages (the individual `CyXxxPage`
  component-reference pages, e.g. `CyBadgePage`, `CyDateInputPage`,
  `CyTabsPage`) are still English-only and were out of scope for this
  pass - see the review pack's "Coverage status" sheet for the full
  list. Queued for a follow-up pass and a second, smaller review pack.
- `ICyLocalizer` only wires the nine components above. A handful of
  labels remain "Not covered by a parameter" for reasons noted on
  `LocalisationPage` (e.g. `CyLanguageToggle`, `CyBrandLogo`).
- Housekeeping items from the v1.6.0 roadmap entry were triaged (see
  `docs/Current-Solution-Structure.md` for the repository map referenced
  below); still non-gating for this release:
  - `docs/MIGRATION-2.0.md` has been drafted, documenting every current
    `[Obsolete]` member (`CySidebar.Collapsed`/`CollapsedChanged`/
    `CollapseMode`, `SidebarCollapseMode.Disabled`, the unconsumed
    `Accessibility.Focus` keyboard-navigation types, and the unimplemented
    `IHasIcon`/`IHasVariant`/`ComponentVariant`/`IconPosition` types) and
    their replacements.
  - The ARIA `tabpanel`/`aria-controls` retrofit is confirmed complete in
    code (`CyTabs`/`CyTabPanel`: `role="tab"`/`"tabpanel"`, `aria-controls`,
    `aria-selected`, `aria-labelledby`, roving `tabindex`) and covered by
    `CyTabsAccessibilityTests`. The manual NVDA/VoiceOver/JAWS spot-check
    itself still needs a person with that assistive tech - it isn't
    something that can be completed without one.
  - Heading-level configurability: confirmed `CyCard`'s `Header` slot is a
    generic `RenderFragment?` with no heading semantics of its own (unlike
    `CyAlert.Title`/`TitleLevel` and `CyAccordionItem.Title`/
    `HeadingLevel`), so it remains the leading candidate; no API change
    made yet pending a design decision on the parameter shape.
  - Icon semantic-naming pass and the visual-regression spike remain
    unstarted - both need design/tooling decisions (a design-system
    naming review; a spike proposal for screenshot-diff tooling built on
    the existing Playwright/Docker accessibility harness) rather than a
    mechanical fix.
- Written without being able to run the .NET test projects (no `dotnet`
  in the authoring environment), same caveat as v1.3.0/v1.4.0/v1.5.x. The
  cascading-parameter fallback chain in each component was checked by
  tracing it by hand against the existing (unmodified) assertions in
  `AppStringsTests`/`LocalisationOverrideTests`. **Since verified:** the
  unit/component suite (`scripts/Run-TestsLocal.ps1`) has been run against
  1.6.0 and passes.


## [1.5.2] - 2026-09-27

Roadmap v1.5.2 ("Close the small stuff"). No new public API; two
`[Obsolete]` deprecations. Package validation runs against 1.5.1.

### Added

- `CyTooltip`: flips to the opposite side of its declared `Placement`
  (top&#8596;bottom, start&#8596;end) when it would otherwise render
  off-screen. Detected client-side by measuring the (already laid out,
  even while hidden) tooltip content against the viewport on
  hover/focus, and re-checked on resize for any tooltip currently open;
  requires the same JS as Escape-to-dismiss.
- `CySidebar`: the mobile drawer now makes the rest of the page `inert`
  while open (unreachable by Tab, click or assistive tech), the same
  containment a native `<dialog>` gets for free from the browser's top
  layer. This is a direct call into the overlay module, independent of
  whichever `IFocusManager` is registered.
- `CyDateInput`: `DayRangeErrorMessage`, `MonthRangeErrorMessage`,
  `YearRangeErrorMessage` and `InvalidDateErrorMessage` parameters
  (English defaults, overridable for Welsh), appended to the field's
  error message when a specific segment (or the day/month pair, for an
  impossible combination such as 31 February) is what's actually wrong.

### Changed

- `CyDateInput`: `aria-invalid` and `aria-describedby` are now applied to
  whichever segment(s) are actually at fault, not all three together.
  When the segments are individually well-formed and combine into a real
  date, yet the field is still invalid (e.g. a custom "must be in the
  past" rule), there is nothing segment-specific to blame, so all three
  fall back to `aria-invalid="true"` together, as before.
- Demo: `.cy-theme-provider` now has a `[data-theme="high-contrast"]`
  token block. It didn't exist before, so every `--cb-*` shell/chrome
  token silently kept its light-theme value in high contrast while
  genuine CymruBlazor components correctly switched via
  `high-contrast.css`'s `--cymru-color-*` tokens - anywhere the two mixed
  (`CyCard`'s `color: inherit` against its own now-black background;
  `.cb-api-table__name`'s `--cymru-color-link` against the still-white
  shell background) broke badly (computed 1.18:1 and 1.25:1; both need
  4.5:1). The new block maps `--cb-*` onto the library's own
  `--cymru-color-*` tokens rather than a third parallel set of values.
- Demo: `.cb-shell-header` is now opaque instead of a semi-transparent
  `backdrop-filter: blur()` panel, in both light and dark. Its actual
  contrast previously depended on whatever page content happened to
  render behind it at a given scroll position - not something a static
  or automated check can verify - and a real user scrolling dark content
  underneath it could hit a genuinely low-contrast moment.

### Fixed

- **Stale CSS bundle could ship pre-fix component styles.**
  `build/BundleCss.targets`'s `BundleCymruCss` target declared
  `Inputs="wwwroot/css/cymrublazor.css"` - the single entry-point file,
  not the ~25 `@import`ed partials it actually concatenates. Editing a
  partial (e.g. `components/navigation.css`, `themes/dark.css`) left the
  entry point's own timestamp unchanged, so MSBuild's up-to-date check
  considered the target current and silently reused whatever bundle
  already existed in `obj/` on any Release build/publish against a
  working tree that hadn't been cleaned since a prior build. `Inputs`
  now globs `wwwroot/css/**/*.css`, so editing any partial correctly
  invalidates the cached output. Found via a live Docker/Playwright run
  of `DemoSmokeTests` reporting a dark-mode `color-contrast` failure on
  `CyNavigation`'s active link that a diff against source confirmed was
  already fixed in `navigation.css` but absent from the actual published
  bundle. `Run-AccessibilityTests.ps1` bind-mounts the host repo's
  `obj`/`bin` straight into its container, so this could reproduce
  locally even though a clean CI checkout (no `obj`/`bin` to reuse) was
  never affected; run `git clean -fdx` once if local CSS edits don't
  seem to take effect.
- **`CyButton`'s default (primary) variant used the wrong text-colour
  token in dark theme.** `color: var(--cymru-color-surface)` instead of
  `color: var(--cymru-color-primary-text)`. In light theme
  `--cymru-color-surface` happens to resolve to white, so the button
  read correctly by coincidence and passed every existing (light-only)
  `CyButton` axe suite; in dark theme `--cymru-color-surface` is navy,
  giving near-invisible navy text on a teal button background (computed
  ~2:1 against the 4.5:1 minimum). Corrected to
  `--cymru-color-primary-text`, which was already themed white in both
  light and dark and simply wasn't being referenced.
- **`--cymru-color-primary` had no dark-mode-safe value for use as
  foreground text on a surface, only as a background.**
  `.cy-button--secondary`/`--tertiary` set
  `color: var(--cymru-color-primary)`; that token is appropriately dark
  in dark theme for background use (the case the previous fix relies
  on) but unusable as text against another dark surface - e.g.
  `CySidebar`'s "Widen" button read at ~2.1:1 against real sidebar
  chrome. Added `--cymru-color-primary-on-surface` (defaults to
  `--cymru-color-primary` in light theme, no visual change;
  `--cymru-cyan-400` in dark, matching `--cymru-color-link`'s existing
  dark-mode treatment; `#00ffff` in high contrast, matching
  `--cymru-color-primary`/`--cymru-color-link` there) and applied it to
  `CyButton`'s secondary/tertiary variants plus four more instances of
  the same pattern found by grepping for the old token's use as text
  rather than background: `CySkipLink`'s focus state (text and border),
  `CyNavigation`'s hover link, `CySidebar`'s reveal-handle hover/focus,
  and `CySpinner`'s primary variant. `CyTabs`'s bespoke
  `[data-theme="dark"] .cy-tabs__tab--active` hand-rolled override (same
  `cyan-400` value) was collapsed into the new token rather than kept as
  a one-off. `CyBadge`'s and `CySidebar__item[aria-current="page"]`'s
  primary-on-`--cymru-color-primary-subtle` pairings were checked and
  deliberately left alone: that token isn't overridden in dark theme, so
  both stay dark-text-on-light-tint and were never actually broken.
- The three fixes above were found in sequence via a live
  `Run-AccessibilityTests.ps1` (Docker/Playwright) run of
  `DemoSmokeTests` against a real published build - each fix unmasked
  the next failure until the full 61-route x 3-theme sweep passed clean.
  This supersedes the "Known limitation" previously noted here (the two
  colour-contrast fixes above it are now confirmed by that same live
  run, not just static analysis) and closes the corresponding
  `known-issues-and-backlog.md` items on Demo dark/high-contrast colour
  contrast.

### Deprecated

- `IKeyboardNavigationService`, `KeyboardNavigationService`,
  `KeyboardNavigationResult`, `KeyboardNavigationOptions` and
  `FocusNavigationMode` are marked `[Obsolete]` (roadmap D5). Nothing in
  CymruBlazor has ever registered or consumed them; kept for one more
  cycle in case a consumer implemented against them directly. Scheduled
  for removal in 2.0.0.

### Documentation

- `cards.css`: recorded the `CyCard.Elevation` product decision (roadmap
  D4) - Small/Medium/Large intentionally rendering the same shadow is
  accepted as correct, not a bug, and the corresponding
  `known-issues-and-backlog.md` item is closed.

## [1.5.1] - 2026-09-26

Housekeeping, test coverage, documentation, and a bilingual
documentation-site rework. No new public API; package validation runs
against 1.5.0.

### Added

- `AppStrings.Catalogue` and `LibraryStrings` in the Demo app now include all 13
  text override parameters added across v1.4.0 (`CyDateInput`, `CyTextArea`) and
  v1.5.0 (`CyPagination`, `CySpinner`), with illustrative Welsh translations.
- `ComponentContractTests` extended to cover `CyProgress` and `CySpinner` under
  `IHasSize` and `IHasColour`.
- Demo: category overview pages for Data, Feedback, Foundations and Navigation,
  completing 100% information-architecture symmetry across all ten documentation
  sections (every category now has an overview page), plus a `CyHeroBanner`
  documentation page.
- Demo: deep, page-level bilingual (English/Welsh) content across all ten
  category overview pages, the Installation guide, and six high-traffic
  component pages (`CyButton`, `CyTextBox`, `CyTable`, `CyAlert`, `CyHeader`,
  `CyFooter`) - subtitles, breadcrumbs, interactive preview captions and
  workbench data, generated-code notes, `ApiDocs` parameter descriptions, and
  accessibility checklists all now re-render reactively on language toggle via
  `AppStrings.Changed`. As with localisation step 1, this Welsh text is
  illustrative and has not been translator-reviewed (see the backlog's
  localisation-step-2 item).
- Demo: all 36 component documentation pages' tab bars (Examples/API/
  Accessibility) now read their labels from `Strings.Nav` instead of hardcoded
  English, and site search indexing/ranking recognises Welsh terms.

### Changed

- `CyDialog`: `Size` parameter now gracefully clamps `ComponentSize.ExtraSmall`
  to `Small` and `ComponentSize.ExtraLarge` to `Large` rather than throwing an
  unhandled runtime exception.
- `CyTextArea`: parameter validation now enforces that `MaxLength` is supplied
  when `ShowCharacterCount="true"`.
- Demo navigation reorganised around a 10-step developer journey (Getting
  Started, Foundations, Branding, Layouts, Navigation, Forms, Content, Data,
  Feedback, Accessibility), replacing the previous ad hoc ordering; `Design
  Tokens` moved under Foundations.
- Demo: canonical routes established for `CyDialog` (now under Feedback) and
  `Focus Trap` (now under Foundations), with the previous Accessibility routes
  kept as aliases so bookmarked links and prev/next navigation keep working.
  `DemoNavigationIndex` gained alias support to resolve both canonical and
  legacy routes to the same navigation entry.
- Demo footer's Accessibility link now points at the Accessibility category
  overview instead of the old Focus Trap shortcut.

### Fixed

- `CyBrandLogo`: removed obsolete Tailwind prototype utility classes
  (`items-center`, `focus-visible:ring-*`, `h-8`, `h-15`) from computed class
  properties; logo styling and sizing are cleanly governed by BEM classes in
  `branding.css`.

### Documentation

- Overhauled `docs/CymruBlazor-Scaffold-Guide.md` for v1.5.0 stable release:
  updated package installation, central package management, service registrations
  (`IToastService`), complete component namespace directory (`.Data`, `.Feedback`,
  `.Forms`, etc.), and added dedicated usage sections for Forms, Data Display,
  Feedback, Overlays, and Localisation Strategy.
- Modernised `README.md` to reference modern `App.razor` (Blazor Web App) and
  `wwwroot/index.html` (Blazor WebAssembly) instead of obsolete `_Host.cshtml`,
  and corrected its pre-release status banner to describe the current v1.5.x
  scope instead of the original `0.1.0-preview.1` scope.
- Corrected Demo `Installation.razor` layout structure (`CyThemeProvider` wrapping
  layout `@Body` rather than `<Router>`), added a dependency-injection
  registration step, expanded the global usings snippet, and enhanced
  cross-links to related pages.
- Synchronised Demo `DesignTokens.razor` typography scale table with the DHCW
  discrete dual-tier desktop/mobile token specification in `typography.css`.
- Localised the Demo's 404 (`NotFound`) page into Welsh.

### Removed

- Unused Razor Class Library template asset `wwwroot/background.png` (it was
  shipped as `_content/CymruBlazor/background.png` but nothing referenced it).
  No public API is affected.

### Internal

- `ToastPauseTests` rewritten to use `TimeProvider`/`FakeTimeProvider` instead
  of real elapsed time, removing a source of CI flakiness in the pause-on-
  hover/focus timing assertions.

## [1.5.0] - 2026-09-24

Data display and feedback. Everything is additive; package validation
runs against 1.2.0. Deliberately does not include a data grid, sorting,
filtering or virtualisation (roadmap decision D5) - style
`Microsoft.AspNetCore.Components.QuickGrid` instead if you need those.

### Added

- **`CyTable`**: a styled semantic table wrapper. Parameters: `Caption`
  (required), `CaptionVisuallyHidden`, `ScrollContainer` (default `true` -
  wraps the table in a keyboard-focusable `role="region"` so a table
  wider than its container can be reached without a mouse instead of
  silently overflowing), `ChildContent` (your own `<thead>`/`<tbody>`
  markup, including `scope="col"`/`scope="row"` on your own `<th>`
  elements).
- **`CyPagination`**: page navigation for a result set too large to show
  at once. Parameters: `CurrentPage`/`CurrentPageChanged`
  (`@bind-CurrentPage`), `TotalPages`, `SiblingCount`, `BoundaryCount`
  (boundary/sibling truncation with an ellipsis for large page counts,
  following the same model as GOV.UK's and Material UI's pagination),
  `AriaLabel`, `PreviousLabel`, `NextLabel`, `PageAriaLabelFormat`,
  `CurrentPageAriaLabelFormat` (all with English defaults, overridable).
  Renders nothing when there is only one page.
- **`CyProgress`**: a progress indicator built on the native
  `<progress>` element (browsers expose this as `role="progressbar"`
  automatically, with `aria-valuenow`/`min`/`max` computed from
  `value`/`max`). Parameters: `Value` (`double?` - leave `null` for
  indeterminate), `Max`, `Colour`, `Size`, `Label`, `AriaLabel` (exactly
  one of `Label`/`AriaLabel` is required, so the bar always has an
  accessible name), `ShowValueText`, `ValueText` (overrides the computed
  percentage, e.g. "18 of 20 beds").
- **`CySpinner`**: an indeterminate loading indicator. `role="status"`
  with an accessible name (`Label`, default "Loading"), shown visually
  next to the spinner only when `ShowLabel` is set. `CyButton.Loading`'s
  own inline spinner is unchanged and does not use this component - use
  `CySpinner` where a loading state needs its own accessible name (e.g.
  a panel that is (re)loading data), not for a button's own busy state.
- Demo pages for all four components: `/data/table`, `/data/pagination`,
  `/feedback/progress`, `/feedback/spinner`. A new "Data" sidebar group
  holds the first two; "Feedback" gains the other two.

### Changed

- `samples/Dashboard`: `WaitingListWidget` now renders its table with
  `CyTable`, `OccupancyWidget` now renders its bed-occupancy bars with
  `CyProgress`, and all three widgets' status pills are now `CyBadge`
  instead of a hand-rolled `<span class="dashboard-status">`. The
  now-unused `.dashboard-status`, `.dashboard-progress__*` and
  `.dashboard-waiting-table` CSS was removed.
- Demo's `DemoNavigationIndex` (prev/next page navigation, search) gained
  entries for the four new pages - and, fixing a pre-existing gap found
  while doing so, for the existing "Toast Service" page, which had a
  sidebar entry but was never in the index.

### Tests

- Unit tests for all four new components.
- Axe suites (light, dark, high-contrast) for all four.
- `DashboardWidgetMigrationTests`: renders the three migrated Dashboard
  widgets with real sample data, asserting the old hand-rolled markup is
  gone and the new library components are in place - the regression
  harness the roadmap called for.

### Known limitations

- The new override strings on `CySpinner`/`CyPagination` were not added
  to the Demo's `AppStrings.Catalogue` (the 1.3.0 localisation-step-1
  reference implementation). Tracked in the backlog.
- Written without being able to run the .NET test projects (no `dotnet`
  in the authoring environment), same caveat as v1.3.0/v1.4.0. In
  particular, `CyPagination`'s boundary/sibling truncation algorithm was
  checked only by manually tracing several page/total combinations by
  hand, not by running `CyPaginationTests`.

## [1.4.0] - 2026-09-23

Forms. Everything is additive; package validation runs against 1.2.0.
Confirmed against the NHS Wales Design System (the DHCW component library)
in addition to GOV.UK/NHS.UK, per the roadmap's D-numeric/D4 decisions.

### Added

- **`CyRadioGroup<TValue>`** and **`CyRadio`**: a native `<fieldset>`/
  `<legend>` group of mutually exclusive options. `CyRadioGroup<TValue>`
  parameters: `Label`, `HintText`, `Required`, `Disabled`, `ChildContent`
  (one `CyRadio` per option), `Value`/`ValueChanged` (`@bind-Value`).
  `CyRadio` parameters: `Value`, `Label`, `HintText`, `Disabled`. Selection
  is reported up as a string and converted to `TValue` the same way
  `CySelect<TValue>`'s `<option>` values are.
- **`CyTextArea`**: a multi-line text field. Parameters: `Rows` (default
  5), `MaxLength`, `ShowCharacterCount` (a live, `aria-live="polite"`
  characters-remaining/too-many message, with correct singular/plural
  wording and localisable `Character[s]RemainingFormat`/
  `Character[s]OverLimitFormat` overrides), plus the usual
  `Label`/`HintText`/`Required`/`Disabled`/`Value`.
- **`CyDateInput`**: a three-field day/month/year date input (`DateOnly?`),
  built ahead of any calendar `CyDatePicker` per roadmap decision D4.
  Segments are plain `type="text" inputmode="numeric"` fields (not
  `type="number"` or a `<select>` of months), never auto-advance focus
  between each other, and are only combined into a date on `change`
  (blur), not on every keystroke. Parameters: `Label`, `HintText`,
  `DayLabel`/`MonthLabel`/`YearLabel` (English defaults, overridable),
  `AutocompleteDateOfBirth`, `Required`, `Disabled`, `Value`/`ValueChanged`.
- `CyTextBox.InputMode` and `CyTextBox.Autocomplete`: native `inputmode`
  and `autocomplete` attributes. No `type="number"` variant was added
  (roadmap D-numeric decision) - a native number input's spin buttons,
  locale-dependent decimal separator and habit of silently discarding
  non-numeric characters make it unsuitable for most of what looks like
  "a number" in a form (postcodes, phone numbers, NHS numbers); use
  `Type="text"` (the default) with `InputMode="numeric"` instead.

### Tests

- Unit tests for all three new components, plus `CyTextBox`'s new
  `InputMode`/`Autocomplete` parameters.
- Axe suites (light, dark, high-contrast) for `CyRadioGroup`/`CyRadio`,
  `CyTextArea` and `CyDateInput`.
- `ComponentContractTests` extended: `CyRadio`, `CyTextArea` and
  `CyDateInput` implement `IHasDisabledState`; `CyTextArea`, `CyDateInput`
  and `CyRadioGroup<TValue>` implement `IHasValidationState`.

### Known limitations

- `CyDateInput`'s `aria-invalid` applies to all three segments together
  when the field is invalid, not to whichever specific segment is wrong
  (the DHCW reference marks only the offending segment). Tracked in the
  backlog.

## [1.3.0] - 2026-09-21

Welsh strings and overlays. Everything is additive; package validation runs
against 1.2.0.

### Added

- **`CyDialog`**: a modal dialog on the native `<dialog>` element
  (`showModal()`): inert background, `Tab` containment, `Escape`, focus
  return, top layer. Parameters: `Open`/`OpenChanged`, `OnClosed`, `Title`,
  `Description`, `ChildContent`, `Footer`, `Dismissible`,
  `CloseOnBackdropClick`, `Size`, `CloseLabel`. See ADR-0001.
- **`CyTooltip`** (`TooltipPlacement`): plain-text tooltip on hover *and*
  focus, dismissible with `Escape`, hoverable (WCAG 1.4.13), with
  `aria-describedby`.
- **Localisation step 1**: every built-in English string has a `string?`
  override with an English default. `AriaLabel` on `CyBreadcrumb`,
  `CyNavigation`, `CyToastContainer` and `CyLanguageToggle`;
  `OpenMenuLabel`/`CloseMenuLabel` on `CyNavigation`; `DismissAriaLabel` on
  `CyAlert` and `CyToastContainer`; `CodeLabel`, `CopyLabel`, `CopiedLabel`,
  `CopyFailedLabel`, `CopiedMessage`, `CopyFailedMessage` on `CyCodeBlock`;
  `RevealLabel`, `CloseLabel`, `SizeGroupLabel`, `ExpandLabel`,
  `CollapseLabel`, `CompactLabel`, `IconOnlyLabel`, `HideLabel`,
  `ResizeLabel` on `CySidebar`. ADR-0002 proposes the step-2 localiser.
- `JsFocusManager` (registered by default) and
  `IFocusManager.TrapAsync(...)` with `FocusTrapOptions`. `TrapAsync` is a
  default interface member, so existing custom `IFocusManager`
  implementations keep compiling and keep getting `FocusAsync` /
  `RestoreFocusAsync` calls.
- `IToastService.PauseAutoDismiss` / `ResumeAutoDismiss` (default interface
  members). A toast's countdown pauses while it is hovered or focused and
  resumes with the time left (at least one second) - WCAG 2.2.1.
- `cymru-overlay.js`, an ES module the library imports on demand. No
  `<script>` tag is needed.
- `CyAlert.TitleLevel` and `CyAccordionItem.HeadingLevel` (default unchanged) so
  pages can keep an unbroken heading outline.
- `IHasSize`, `IHasColour`, `IHasDisabledState` and `IHasValidationState`
  are now implemented where the property already existed.

### Changed

- Demo: a **Localisation** page (`/foundations/localisation`) and an
  `AppStrings` service show the step-1 pattern end to end (language toggle,
  translated accessible names, `lang` on translated text, a catalogue of
  every override). The site's shell toast container now follows the selected
  language. The Welsh text is illustrative and not translator-reviewed.
- `CyFocusTrap` now really works: it moves focus in, contains `Tab`, pulls
  back escaped focus and restores focus when released. It also honours
  `Enabled` changing after the first render. It still does not make the rest
  of the page inert; use `CyDialog` for a modal.
- The `CySidebar` mobile drawer closes on `Escape`, traps focus while open on
  small screens and returns focus afterwards. Without a registered
  `IFocusManager` it still closes on `Escape`.
- `AddCymruBlazor()` registers `JsFocusManager` instead of the logging-only
  `FocusManager`, which remains available as a no-op implementation.
- The Demo search modal and Dashboard sample tooltips use `CyDialog` /
  `CyTooltip`.

### Fixed

- The published-demo smoke run found and fixed, in the demo: low-contrast muted
  text in the light theme, `aria-controls` pointing at tab panels that are not
  rendered (29 pages), heading order (TOC, properties panel, home features),
  duplicate unlabeled landmarks (sample sidebars, breadcrumbs) and the Legacy
  page (no `h1`, nested `main`).
- A Secondary/Tertiary `CyButton` in the Actions of a Primary or Secondary `CyHeader` was primary-on-accent
  and unreadable; it now inherits the header's text colour (found by the smoke run).
- `CyCodeBlock`'s scrollable `<pre>` is keyboard focusable (`tabindex="0"`).
- Demo API tables: the parameter-name colour was a fixed blue that failed
  contrast on the dark theme; it now uses the theme-aware link colour.
- `CyAlert` no longer makes the whole page scroll horizontally when its text
  contains a long unbroken string on a narrow screen (WCAG 1.4.10).

### Deprecated

- `IHasVariant`, `IHasIcon`, `ComponentVariant` and `IconPosition` are
  `[Obsolete]`: nothing implements or uses them. Removal in 2.0.0.

### Tests

- Real-browser suites for the overlay module, `CyDialog`, `CyTooltip` and
  computed-style/reflow checks, and a smoke run over the published demo
  (every route, light/dark/high-contrast, no console errors, axe clean).
  See `tests/CymruBlazor.AccessibilityTests/README.md`.

## [1.2.1] - 2026-09-19

### Fixed

- `CyToastContainer` now follows the active theme. It used `--cb-color-*`
  custom properties that the library never defined, plus hard-coded hex
  colours, so it ignored dark mode and showed pastel toasts in
  high-contrast. It now uses the same `--cymru-*` tokens as `CyAlert`.
- `CyToastContainer` z-index is now `--cymru-z-toast` (1080) instead of a
  hard-coded 1050 that tied with modals.
- Toasts are announced to screen readers: the container is a polite,
  non-atomic live region and each toast has `role="status"` (Info/Success) or
  `role="alert"` (Warning/Danger), matching `CyAlert`.
- `CyTabs`: the active tab's label failed WCAG AA contrast in the dark theme
  (2.34:1, `#0a6a84` on `#212b32`). It now uses `--cymru-color-link` there.
  Light and high-contrast are unchanged. Found by the new themed axe scans.
- Undefined CSS custom properties (each hidden by a fallback value):
  the `CySidebar` drawer and backdrop now use `--cymru-z-modal` /
  `--cymru-z-overlay`, the sticky `CyHeader` uses `--cymru-z-sticky`
  (changed together so the stack stays sticky header < backdrop < drawer <
  toast), `CyCodeBlock` uses `--cymru-font-family-monospace`, and the skip
  link uses a literal `150ms` transition.

### Changed

- Toast colours now come from the NHS Wales theme tokens instead of
  Tailwind pastels, and `CyCodeBlock` renders in the design system's
  monospace stack. Both are visible but intended theme-token alignments.

### Documentation

- `CyFocusTrap` docs corrected. It does not move, restore or contain focus
  by default: the registered `FocusManager` only logs at Debug level. XML
  docs (on `CyFocusTrap`, `IFocusManager`, `FocusManager`, `FocusOptions`,
  `FocusResult`), the Demo pages and the code samples (`<CyFocusTrap>`) now
  say so and describe how to register a custom `IFocusManager`. A working
  implementation is planned for 1.3.0.

### Internal

- Package validation (`EnablePackageValidation`, baseline 1.2.0) fails
  `dotnet pack` on accidental breaking API changes.
- `UndefinedCssVariableTests` fails on any library `var(--x)` with no
  definition, replacing the prefix-based token test.
- `CyToastContainerTests` and `CyToastContainerAccessibilityTests`.
- Dark and high-contrast axe scans for Accordion, Alert, Badge, Button,
  Card, Checkbox, CodeBlock, Icon, Select, Sidebar, Tabs, TextBox and the
  toast.
- Removed the empty `CymruBlazor.ApprovalTests` project, its package pin and
  its prompt reference.

## [1.2.0] - 2026-09-19

### Added

- `CySidebar` composable states: new `SidebarState` enum
  (`Expanded`, `Compact`, `IconOnly`, `Hidden`) with `States`/`State`/
  `StateChanged` parameters and `CycleNextAsync()`/`ExpandAsync()`/
  `SetStateAsync()`, so a single toggle can cycle through any ordered set
  of appearances. `Collapsed`/`CollapseMode`/`CollapsedChanged` keep
  working unchanged as an `[Obsolete]` compatibility layer.
- `CySidebar` mobile off-canvas drawer: `MobileOpen`/`MobileOpenChanged`/
  `MobileBreakpoint`/`ShowMobileBackdrop`, with a click-to-close backdrop
  and a close button replacing the desktop toggle while open.
- `SidebarCollapseMode.NonCollapsible`; `Disabled` is retained as an
  `[Obsolete]` alias for one release.
- `CySidebar` directional size controls: with three or more `States` the
  header shows two chevrons - one to narrow a step, one to widen a step
  (`NarrowAsync()`/`WidenAsync()`, no wrap-around) - instead of a single
  cycling button. Each button's accessible name says where it goes ("Show
  icons only", "Hide sidebar", "Expand sidebar"). One- and two-state
  sidebars keep the original single toggle.
- `CySidebar.CollapsedBrand` and `ShowBrandWhenCollapsed`: show the icon-sized
  logo in the Compact/IconOnly rails, or just the two chevrons when no icon
  logo is available.

### Changed

- `CyBrandLogo` size scale is now 18 / 32 / 48 / 64 / 72px (ExtraSmall,
  Small, Medium, Large, ExtraLarge; was 18 / 24 / 32 / 40 / 56) for both the
  built-in mark and image logos. Image logos now have their height *set* to
  the Size instead of capped with `max-height`, so a 48px SVG really renders
  at Large (64px); they still shrink without distortion when the container is
  narrower. Existing `Small` usages grow from 24px to 32px - check any
  fixed-height header or nav bars. Guarded by `BrandLogoSizeScaleTests`.
- Demo: the CySidebar page preview is polished (a framed app window with
  status chips, real navigation rows and styled buttons) and now exposes the
  `CyBrandLogo` properties - variant, size, text - plus the rail logo
  (`CollapsedBrand`) on/off and size, with the generated code following the
  selections.
- Demo: documented sidebar width customization (per-state defaults, the CSS
  override recipe and its cautions) and the logo size scale.
- `CySidebar` now always fills the full height of the row it sits in
  (`align-self: stretch`), even when the parent uses `align-items:
  flex-start`, so its background and border reach the bottom of the page.
- `CySidebar` `IconOnly` rail is wider (5rem, was 3.5rem) and draws
  navigation icons at 1.5rem (24px) with a 3rem minimum row height, so the
  icon-only rail is comfortable to read and tap.

### Fixed

- `CySidebar` navigation rows (`.cy-sidebar__item`) were never flex containers,
  so the Compact/IconOnly `flex-direction`/`align-items` rules were inert: a
  short label sat beside its icon while a long one wrapped beneath it, and
  IconOnly icons hugged the left edge. Rows are now flex containers in every
  state (with hover, focus-visible and `aria-current="page"` styles), Compact
  always centres the icon with its label directly beneath, and IconOnly
  centres the icon. Guarded by `CySidebarCssTests`.
- Demo: the CySidebar preview's action buttons used an undefined
  `cymru-btn` class and rendered as plain text; they now use `CyButton`.
- `CySidebar` in the `Hidden` state hid its contents with `display: none`
  inside a CSS layer, so any consumer rule such as `.nav { display: flex }`
  (consumer CSS is unlayered and always wins over layered CSS) left the
  navigation visible and overlapping the page. The collapsed aside now also
  uses `visibility: hidden` (which the reveal handle opts out of).
- `CySidebar` in the `Hidden` state no longer traps the user: a reveal
  handle is rendered beside the zero-width `<aside>` (which is now its
  positioned containing block and no longer clips it, so it neither
  disappears nor pushes the page into horizontal scrolling), and all other
  sidebar content is `display: none` so it leaves the tab order and
  accessibility tree. Hidden also now wins over the generic tablet
  full-width stacking rule, an open mobile drawer always shows its full
  contents regardless of `State`, and a closed mobile drawer is no longer
  keyboard-focusable while off-canvas.
- Demo: the `/content/icons` preview icon was invisible in dark mode (and
  the gallery's icon names and domain headings were near-invisible). The
  page's scoped stylesheet used `--cb-color-*` custom properties that are
  not defined anywhere, so every usage fell back to a hardcoded light-mode
  colour; its dark override could never apply because scoped CSS rewrites
  the `[data-theme]` selector to require a scope attribute the theme
  provider element does not have. Now uses the theme-reactive
  `--cb-bg-*`/`--cb-text-*`/`--cb-border-*` tokens, with a regression test
  (`DemoCssTokenTests`) guarding against reintroducing undefined tokens.
- `CySidebar`'s reveal handle, mobile backdrop and close button referenced
  `--cy-color-*`/`--cy-radius-*`/`--cy-shadow-*` custom properties that are
  defined nowhere in the library (the real design tokens are `--cymru-*`),
  so they always rendered their hardcoded light-mode fallback regardless
  of theme. Remapped to `--cymru-*` and guarded by
  `Library_Css_Should_Not_Reference_Undefined_Cy_Tokens`.

## [1.1.0 and earlier] - retroactively documented

These demo-site changes were listed under `[Unreleased]` and shipped before
v1.2.0, but were never given a version heading. They are recorded here
after the fact.

### Changed (demo only)

- Demo pages migrated from obsolete `Shared/DemoCodeBlock.razor` wrapper
  to direct usage of `CyCodeBlock` (library component). This removes the
  compatibility wrapper and eliminates the demo application's own code
  display abstraction, making all ~29 component documentation pages
  consume the library's own public component. No behavioral change to
  the demo site; this is purely a code organization improvement.
- `/content/icons` workbench redesigned: live search by name/domain,
  a global size toggle (16/20/24/32) driving the full gallery, friendly
  domain headings with badges and per-domain counts, and one-click
  copy of `<CyIcon Name="..." />` markup with an accessible live-region
  announcement, matching the pattern already used by `CyCodeBlock`.
- Fixed several dark-mode contrast bugs in the demo shell: the header's
  theme-toggle and mobile hamburger icons were hardcoded white and
  disappeared against the light-mode header; the main content column
  had no theme-aware background at all, so it silently inherited the
  page's static light background under dark mode while its text
  correctly switched to dark-mode colours, making page titles,
  breadcrumbs, tabs, and the "On This Page" panel unreadable. Also
  found and fixed the root cause behind both: five CSS custom
  properties (`--docs-text-primary`, `--docs-text-muted`,
  `--docs-card-surface`, `--docs-surface-wash`, `--docs-sidebar-active`)
  were referenced throughout `demo.css` but never actually defined
  anywhere, so every usage silently fell back to a hardcoded, non-theme-
  reactive value (most often literal white) - most visibly on the Theme
  Provider page's own System/Light/Dark/HighContrast control. All usages
  now map to the real, already theme-reactive `--cb-*`/`--cymru-*`
  tokens. No behavioral or visual change in light mode; demo-only.

## [0.1.0-preview.9] - 2026-09-08

### Added

- `CyBadge` - a small label for categorisation, status, or metadata.
  Covers the "tag"/chip use case too via `Dismissible`/`OnDismiss`,
  rather than shipping a separate `CyTag` component for what only
  differs by whether a dismiss affordance is present. Supports the
  full `ComponentColour` palette (except `Unspecified`) and a `Pill`
  toggle. See `/content/badge`.
- `CyAccordion` / `CyAccordionItem` - a vertically stacked set of
  expand/collapse sections implementing the WAI-ARIA Accordion
  pattern, including Up/Down/Home/End keyboard navigation between
  section headers via real `ElementReference.FocusAsync()` focus
  moves. Replaces the native `<details>`/`<summary>` this was
  previously only a "nice to have" wrapper around. See
  `/content/accordion`.
- `CyTabs` / `CyTabPanel` - a tabbed interface implementing the
  WAI-ARIA Tabs pattern with automatic activation and a roving
  `tabindex` (Left/Right/Home/End move focus and select). Intended to
  replace the hand-copied tab bar markup duplicated across the site's
  ~18 tabbed component documentation pages, though that migration
  itself is a separate follow-up - this release only adds the
  component and its own documentation page (`/navigation/tabs`).
- `CyCodeBlock` - a labelled, read-only code sample display with a
  copy-to-clipboard button, replacing `Shared/DemoCodeBlock.razor`
  (demo-only) as the library's own answer to this. Copies via a
  direct `navigator.clipboard.writeText` JS interop call (no custom
  JS module), and announces success/failure through the Mediator
  pipeline to any `CyLiveRegion` in the app - not just as a visual
  button-label swap, which was the specific gap the demo-only
  workaround had. Does not include syntax highlighting. See
  `/content/code-block`.
- Four missing component category overview pages: `/forms`,
  `/content`, `/branding`, `/accessibility` (`/layouts` already
  existed). Each is a `CyGrid`/`CyCard` grid linking to that
  category's component pages, per the original spec for these pages.
- "Open in GitHub" links added to the ~26 component documentation
  pages that didn't already have one (`CyButton`'s `/forms/button`
  was the only page with one before this release).

### Changed

- `Home.razor`'s hero CTAs ("Explore components", "Get started") now
  use `CyButton Href=...` instead of plain `<a>` tags, now that
  `CyButton.Href` exists (`0.1.0-preview.7`). Required re-scoping the
  hero's CSS overrides with the `::deep` combinator, since Blazor CSS
  isolation doesn't let `Home.razor.css` reach into a child
  component's own rendered markup by default - a plain find/replace
  of the anchor tag would have visibly broken the hero buttons'
  styling.

## [0.1.0-preview.8] - 2026-09-02

Demo UI/UX refinements plus one real library bug fix, found while
building them.

### Added

- `CyIcon` gained `StrokeWidth` (`double`, default `2`) and `Color`
  (`string?`, default `null` -> `currentColor`), wired into the
  rendered SVG's `stroke`/`stroke-width` attributes.

### Fixed

- `CyBrandLogo` rendered illegible dark-on-dark text when placed
  inside a dark-background header. Root cause: `.cy-brand-logo`/
  `.cy-brand-logo__wordmark` hardcoded `color: var(--cymru-color-text)`
  instead of inheriting from context, and the existing dark-background
  override (`.cy-hero-banner--inverse`/`.cy-navigation`) didn't cover
  `CyHeader`'s `.cy-header--primary`/`.cy-header--secondary` background
  variants, since `CyHeader` didn't exist when that override was
  written. Also corrected the override's colour token from
  `--cymru-color-text-inverse` (which flips to dark grey in dark mode
  and black in high-contrast mode) to `--cymru-color-accent-text`
  (which correctly stays white in every theme, matching
  `--cymru-color-accent`'s own fixed-across-themes navy) - the old
  token choice was a separate, pre-existing bug affecting any inverse
  text on the navy header/footer in dark mode, not just `CyBrandLogo`.
- `icons.css` switched from `display: inline-block` to `inline-flex`
  with centered alignment and `line-height: 1`, fixing inconsistent
  vertical centering of icons against adjacent text.

### Changed (demo only, no package impact)

- Home page restructured to semantic `<section>` elements with a BEM
  class scheme (`cb-home__*`) instead of inline styles, with a richer
  hero gradient treatment.

## [0.1.0-preview.7] - 2026-08-29

The demo workbench redesign: repositions `CymruBlazor.Demo` from a
basic showcase into a documented component workbench (Getting Started,
per-component Examples/API/Accessibility pages, search, prev/next
navigation), fixes several real library bugs found in the process, and
adds one new component. See the demo itself for the fullest picture -
most of this release's work is demo-only and has no NuGet package
impact; only the items below under "Added"/"Fixed" (not "Demo") do.

### Added

- `CyHeader` - new component; the header-side counterpart to `CyFooter`
  that was previously missing. `Brand`/`ChildContent`/`Actions` slots,
  the same `Background` (`ComponentColour`) convention as `CyFooter` so
  a matching header+footer pair reads as one deliberate pairing, and
  an optional `Sticky` position. Deliberately does not implement its
  own mobile nav collapse - compose a `CyNavigation` in `ChildContent`
  for that.
- `CyFooter.Background` (`ComponentColour`: `Primary`/`Secondary`/
  `Surface`/`Neutral`) - previously hardcoded to the navy `Primary`
  look. Default is unchanged. Plain `<a>` children now default to
  `color: inherit` with an underline so links stay readable against
  every background value.
- `CyButton` extended from a `ChildContent`-only wrapper to a full
  interactive component: `Variant` (`ComponentColour`), `Size`
  (`ComponentSize`), `Disabled`, `Loading`, `Href` (renders as `<a>`
  for navigation when set and not disabled), `Type`, `OnClick`.
- `IconRegistry` gained `moon`/`sun` entries (verified byte-for-byte
  against `lucide-static@1.34.0`) for theme-toggle UI.
- Standard Blazor WASM boilerplate CSS (`#blazor-error-ui`,
  `.loading-progress`/`.loading-progress-text`) - the SDK template
  ships these by default, but this project's `index.html` was
  customised early on and the stylesheet never carried them over.
  Without `display: none`, the error banner defaulted to the browser's
  normal `block` display for a `<div>` and showed permanently on every
  page load regardless of whether an error actually occurred.

### Fixed

- `CyStack`/`CyGrid`/`CyCluster`'s `Gap` parameter had no visible
  effect for any value except `Medium`: each component's base CSS
  class (`.cy-stack`/`.cy-grid`/`.cy-cluster`) declared its own
  hardcoded `gap: var(--cymru-layout-gap-md)`, which always won the
  cascade tie against the `.cy-gap-{size}` modifier class also applied
  to the same element (equal specificity, declared later in the same
  file). Removed the redundant hardcoded declarations; the modifier
  class already covers every value including `Medium`. Existing
  `CyStackTests`/`CyGridTests` only assert the class name is applied,
  not the resulting computed style, which is why this shipped
  unnoticed - bUnit can't evaluate real CSS cascade resolution.
- Doc comments in `ThemeService.cs` and `CyThemeProvider.razor.cs`
  referenced a nonexistent `wwwroot/js/theme.js` - the actual file is
  `cymrublazor.js`. No functional impact (the doc-comment path was
  never used by code), but corrected for anyone copying it.
- `CyHeader.razor.cs` referenced `<see cref="CyBrandLogo"/>` in a doc
  comment without importing `CymruBlazor.Components.Branding`, leaving
  the reference unresolvable.

## [0.1.0-preview.6 and earlier] - retroactively documented

The entries below accumulated under "Unreleased" across several
releases (at least as far back as `0.1.0-preview.5`, based on when the
components they describe first appear in git history) without ever
being moved into a versioned section when those releases actually
shipped. Consolidated here in one place rather than guessing which
exact preview tag introduced each item - if you need that precision,
`git log --diff-filter=A -- <path>` against the relevant file is more
trustworthy than this document for anything before `0.1.0-preview.7`.

### Added

- `IThemeService` is now registered in DI (`AddCymruBlazor()`) - previously implemented but never resolvable.
- `ThemeService` gained optional JS interop: `localStorage` persistence and live OS `prefers-color-scheme` detection (originally via `wwwroot/js/theme.js`, later renamed `cymrublazor.js` in `0.1.0-preview.6`). Fully backward compatible - the parameterless constructor path is unchanged.
- `CyThemeProvider` - applies the active theme via a `data-theme` wrapper and re-renders on theme change.
- `CyTypography` - NHS Wales typography scale (`H1`-`H6`, `Body`, `BodyLarge`, `BodySmall`, `Caption`), with an `As` override to decouple visual style from semantic heading level.
- `CyCard` - content container with optional header/footer and whole-card-link (`Href`) support.
- `CyAlert` - status/alert banner with severity-driven ARIA role (`alert` vs `status`) and optional dismiss button.
- `CyFormFieldComponentBase<TValue>` (built on `InputBase<TValue>`), `CyTextBox`, `CySelect<TValue>`, `CyCheckbox`, `CyValidationSummary`.
- `CySkipLink`, `CyBreadcrumb`/`CyBreadcrumbItem`, `CyPageHeader`, `CyNavigation`/`CyNavigationItem` (with mobile menu + `FocusTrap` integration), `CyHeroBanner`, `CyFooter`.
- `IFocusManager` is now registered in `AddCymruBlazor()` - previously only ever registered manually by consuming apps; `CyNavigation`'s mobile menu depends on it transitively via `FocusTrap`.
- Removed `wwwroot/js/theme-service.js` - pre-existing, unreferenced scaffolding superseded by the newly-wired `theme.js`/`cymrublazor.js` + `ThemeService` interop.

### Fixed

- Dark and High Contrast themes now render correctly. `CyThemeProvider` applies `data-theme` to a `display: contents` wrapper *inside* `<body>` so it never adds an extra layout box - but that meant `<body>`'s own background/text colour (set in `base/typography.css`) and any `color: inherit`/`currentColor` usage (e.g. `CyCard`) never actually picked up the dark/high-contrast palette, since both are resolved at or above `<body>`, outside the attribute's reach. Components deeper in the tree that referenced `var(--cymru-color-text)` directly looked themed, while plain text and card backgrounds silently stayed light - typically rendering as low/no-contrast text on a dark surface. Fixed via a `body:has(.cy-theme-provider[data-theme="..."])` selector in `themes/dark.css`/`themes/high-contrast.css`, so `<body>` reacts correctly with no JavaScript required. See `Components/Theming/CyThemeProvider.razor.cs` and `wwwroot/css/components/theming.css` for the full explanation.
- `CyScreenReaderOnly` rendered CSS class `"sr-only"`, but the stylesheet only ever defined `.u-sr-only` - the component has been visually non-functional (not actually hiding its content) since `0.1.0-preview.1`. Corrected to `u-sr-only`.



## [0.1.0-preview.1] - 2026-08-20

Initial pre-release. This is a **preview**, not a feature-complete 1.0 -
see `PRD.md` section 6 for the full v1 component scope and
`plan/plan-first-nuget-release.md` for what's deliberately deferred.

### Added

- Layout primitives: `CyContainer`, `CyStack`, `CySidebar`, `CyCluster`,
  `CyGrid`, `CyCenter`.
- Accessibility utilities: `FocusTrap`, `CyLiveRegion`, `CyScreenReaderOnly`.
- `Button` (minimal - no variants/sizes/disabled state/click handling yet).
- `ThemeService` for runtime theme switching.
- NHS Wales-aligned design tokens (colour, typography, spacing, elevation,
  breakpoints) and a layered CSS architecture consumed via a single
  `_content/CymruBlazor/css/cymrublazor.css` reference.
- Demo application covering all shipped components.

### Known limitations

- Content components (`Card`, `Alert`, `Typography`) and most form
  components (`TextBox`, `Select`, `Checkbox`, validation summary) are not
  yet implemented.
- `Button` does not yet support `@onclick`, variants, or a disabled state.
- `CymruBlazor.ApprovalTests` and `CymruBlazor.AccessibilityTests` are
  scaffolded but do not yet contain tests.
