# Changelog

All notable changes to this project are documented here. This project
follows [Semantic Versioning](https://semver.org/); version numbers are
derived automatically from git tags by [MinVer](https://github.com/adamralph/minver) -
see `CONTRIBUTING.md` for the release process.

Full detail for every release is also available as auto-generated
[GitHub Releases](https://github.com/gncube/CymruBlazor/releases).

## [Unreleased]

## [1.3.0] - Unreleased

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
## [1.2.1] - Unreleased

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
