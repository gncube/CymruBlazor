# Changelog

All notable changes to this project are documented here. This project
follows [Semantic Versioning](https://semver.org/); version numbers are
derived automatically from git tags by [MinVer](https://github.com/adamralph/minver) -
see `CONTRIBUTING.md` for the release process.

Full detail for every release is also available as auto-generated
[GitHub Releases](https://github.com/gncube/CymruBlazor/releases).

## [Unreleased]

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
