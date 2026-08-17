# Changelog

All notable changes to this project are documented here. This project
follows [Semantic Versioning](https://semver.org/); version numbers are
derived automatically from git tags by [MinVer](https://github.com/adamralph/minver) -
see `CONTRIBUTING.md` for the release process.

Full detail for every release is also available as auto-generated
[GitHub Releases](https://github.com/gncube/CymruBlazor/releases).

## [Unreleased]

### Added (Phase 3+4 of the next release - see plan/plan-next-release-components.md)

- `CyFormFieldComponentBase<TValue>` (built on `InputBase<TValue>`), `CyTextBox`, `CySelect<TValue>`, `CyCheckbox`, `CyValidationSummary`.
- `CySkipLink`, `CyBreadcrumb`/`CyBreadcrumbItem`, `CyPageHeader`, `CyNavigation`/`CyNavigationItem` (with mobile menu + `FocusTrap` integration), `CyHeroBanner`, `CyFooter`.
- `IFocusManager` is now registered in `AddCymruBlazor()` - previously only ever registered manually by consuming apps; `CyNavigation`'s mobile menu depends on it transitively via `FocusTrap`.

### Fixed

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
