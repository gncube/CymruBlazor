# Changelog

All notable changes to this project are documented here. This project
follows [Semantic Versioning](https://semver.org/); version numbers are
derived automatically from git tags by [MinVer](https://github.com/adamralph/minver) -
see `CONTRIBUTING.md` for the release process.

Full detail for every release is also available as auto-generated
[GitHub Releases](https://github.com/gncube/CymruBlazor/releases).

## [Unreleased]

### Added (demo workbench redesign)

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

### Added (Phase 3+4 of the next release - see plan/plan-next-release-components.md)

- `CyFormFieldComponentBase<TValue>` (built on `InputBase<TValue>`), `CyTextBox`, `CySelect<TValue>`, `CyCheckbox`, `CyValidationSummary`.
- `CySkipLink`, `CyBreadcrumb`/`CyBreadcrumbItem`, `CyPageHeader`, `CyNavigation`/`CyNavigationItem` (with mobile menu + `FocusTrap` integration), `CyHeroBanner`, `CyFooter`.
- `IFocusManager` is now registered in `AddCymruBlazor()` - previously only ever registered manually by consuming apps; `CyNavigation`'s mobile menu depends on it transitively via `FocusTrap`.

### Fixed (demo workbench redesign)

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

### Fixed

- Dark and High Contrast themes now render correctly. `CyThemeProvider` applies `data-theme` to a `display: contents` wrapper *inside* `<body>` so it never adds an extra layout box - but that meant `<body>`'s own background/text colour (set in `base/typography.css`) and any `color: inherit`/`currentColor` usage (e.g. `CyCard`) never actually picked up the dark/high-contrast palette, since both are resolved at or above `<body>`, outside the attribute's reach. Components deeper in the tree that referenced `var(--cymru-color-text)` directly looked themed, while plain text and card backgrounds silently stayed light - typically rendering as low/no-contrast text on a dark surface. Fixed via a `body:has(.cy-theme-provider[data-theme="..."])` selector in `themes/dark.css`/`themes/high-contrast.css`, so `<body>` reacts correctly with no JavaScript required. See `Components/Theming/CyThemeProvider.razor.cs` and `wwwroot/css/components/theming.css` for the full explanation.
- `CyScreenReaderOnly` rendered CSS class `"sr-only"`, but the stylesheet only ever defined `.u-sr-only` - the component has been visually non-functional (not actually hiding its content) since `0.1.0-preview.1`. Corrected to `u-sr-only`.

### Added (Phase 1+2, previously listed)

- `IThemeService` is now registered in DI (`AddCymruBlazor()`) - previously implemented but never resolvable.
- `ThemeService` gained optional JS interop: `localStorage` persistence and live OS `prefers-color-scheme` detection, via `wwwroot/js/theme.js`. Fully backward compatible - the parameterless constructor path is unchanged.
- `CyThemeProvider` - new component; applies the active theme via a `data-theme` wrapper and re-renders on theme change.
- `CyTypography` - NHS Wales typography scale (`H1`-`H6`, `Body`, `BodyLarge`, `BodySmall`, `Caption`), with an `As` override to decouple visual style from semantic heading level.
- `CyCard` - content container with optional header/footer and whole-card-link (`Href`) support.
- `CyAlert` - status/alert banner with severity-driven ARIA role (`alert` vs `status`) and optional dismiss button.
- Removed `wwwroot/js/theme-service.js` - pre-existing, unreferenced scaffolding superseded by the newly-wired `theme.js` + `ThemeService` interop.

## [0.1.0-preview.1] - Pending

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
