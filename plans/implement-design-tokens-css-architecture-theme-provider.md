---
goal: Implement Design Tokens, CSS Architecture, and Theme Provider Foundation for CymruBlazor
version: 1.0.0
date_created: 2026-07-14
owner: CymruBlazor Core Architecture Team
status: Planned
tags: [architecture, design, css, theming, blazor, accessibility]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This implementation plan outlines the step-by-step execution strategy to build the core design tokens, modern CSS architecture, and theming provider infrastructure for CymruBlazor (Phase 4). The goal is to establish a decoupled, highly performant, accessible (WCAG 2.2 AA), and PWA-compatible styling architecture optimized for Blazor WebAssembly and Razor Class Libraries, completely independent of Bootstrap.

---

## 1. Requirements & Constraints

- **REQ-CSS-001**: Zero Bootstrap dependency. All styling must rely on custom design tokens and native CSS isolation.
- **REQ-CSS-002**: Single output bundled stylesheet must be served at `_content/CymruBlazor/css/cymrublazor.css` for consumers, containing no active run-time `@import` rules to minimize HTTP requests.
- **REQ-CSS-003**: Maintain strict Razor CSS Isolation (`Component.razor.css`) side-by-side with Razor files to prevent global style leaks.
- **REQ-CSS-004**: Work natively with the offline caching capabilities of Blazor WebAssembly PWAs by automatically compiling output assets.
- **REQ-ACC-001**: Align with WCAG 2.2 AA contrast standards, including responsive focus styles, reduced motion flags, and high-contrast/forced-colors media support.
- **REQ-THM-001**: Implement a C# `ThemeService` alongside CSS variables enabling runtime switching between Light/NHS Wales, Dark, and High Contrast themes without breaking components.
- **CON-CSS-001**: CSS custom properties cannot be used directly within CSS `@media` queries; responsive breakpoints must be handled via standard CSS variables in component utilities and documented clearly.

---

## 2. Implementation Steps

### Implementation Phase 4.1: CSS Design Tokens & Base Architecture

- GOAL-001: Establish the semantic variables, font scaling, layout helpers, reset system, and accessibility overrides using native CSS properties.

| Task | Description | Completed | Date |
| :--- | :--- | :---: | :--- |
| **TASK-001** | Create `wwwroot/css/tokens/colours.css` defining semantic color variables mapping NHS Wales color ranges to functional roles (primary, secondary, success, warning, danger, surface, text). | | |
| **TASK-002** | Create `wwwroot/css/tokens/typography.css` defining system font fallbacks (`Segoe UI`, `Arial`, `sans-serif`), sizes (`xs` to `xl`), line heights, and weights. | | |
| **TASK-003** | Create `wwwroot/css/tokens/spacing.css` establishing an 8-point modular spacing scale (`--cymru-space-0` through `--cymru-space-6`). | | |
| **TASK-004** | Create `wwwroot/css/tokens/elevation.css` defining shadows (`--cymru-shadow-sm` through `--cymru-shadow-lg`) and border-radii. | | |
| **TASK-005** | Create `wwwroot/css/tokens/breakpoints.css` declaring the reference sizes for media queries (mobile, tablet, desktop, wide). | | |
| **TASK-006** | Create `wwwroot/css/base/accessibility.css` containing WCAG 2.2 AA compliant focus indicators, `@media (prefers-reduced-motion)` resets, and `@media (forced-colors)` overrides. | | |
| **TASK-007** | Create `wwwroot/css/base/reset.css` containing a minimal, un-opinionated box-sizing and tag reset (no bootstrap residue). | | |

### Implementation Phase 4.2: Layout, Utilities, Cascade Layers & Single Bundling

- GOAL-002: Develop structural layouts, modern CSS cascade layers, lightweight helper utilities, and the bundling compilation tooling.

| Task | Description | Completed | Date |
| :--- | :--- | :---: | :--- |
| **TASK-008** | Create `wwwroot/css/layout/grid.css` and `containers.css` to handle responsive containers, flex structures, and CSS Grid templates. | | |
| **TASK-009** | Add standard utility lists under `wwwroot/css/utilities/` covering margin/padding offsets, display toggles, and visibility classes. | | |
| **TASK-010** | Add theme base placeholders for default `:root`, `[data-theme="dark"]`, and `[data-theme="high-contrast"]` selectors to accommodate the CSS variables. | | |
| **TASK-011** | Define CSS Cascade Layers (`@layer reset, tokens, base, layout, components, utilities, overrides;`) to establish robust priority handling. | | |
| **TASK-012** | Configure build-time concatenation/compilation inside the `.csproj` file (via MSBuild task or `BundlerMinifier` / `LibMan`) to compile all sheets into `wwwroot/css/cymrublazor.css` during both Build and Publish. | | |

### Implementation Phase 4.3: Component CSS Isolation & Blazor Theme Provider Foundation

- GOAL-003: Interface the CSS variables with Blazor components via a decoupled C# service, enforce Razor CSS isolation, and confirm PWA asset caching.

| Task | Description | Completed | Date |
| :--- | :--- | :---: | :--- |
| **TASK-013** | Audit and isolate existing component styles (e.g., `Button.razor.css`, `Alert.razor.css`) to consume semantic `--cymru-` design tokens instead of direct colors. | | |
| **TASK-014** | Verify WebAssembly PWA compatibility by checking that `service-worker-assets.js` automatically maps the compiled `cymrublazor.css`. | | |
| **TASK-015** | Create C# `ThemeService.cs`, `ThemeDefinition.cs`, and `ThemeMode.cs` within the project's Services and Themes directories to programmatically toggle HTML `data-theme` attributes. | | |
| **TASK-016** | Document client-side integration via the simple `<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet">` reference in consuming apps. | | |

---

## 3. Alternatives

- **ALT-001: Tailwind CSS / CSS-in-JS Utility Frameworks**: Considered for rapid styling but rejected due to high bundle size, dependency complexities in Razor Class Libraries, and conflicts with the clean offline PWA philosophy.
- **ALT-002: Traditional CSS imports (`@import`)**: Considered keeping individual stylesheets loaded at runtime. Rejected because multiple `@import` requests cause render-blocking network roundtrips, degrading mobile/PWA performance.

---

## 4. Dependencies

- **DEP-001**: `.NET 10.0 SDK` (WebAssembly and Razor Class Library compilation).
- **DEP-002**: Consuming Application index files (`index.html` or `App.razor`) to reference the newly bundled style asset.

---

## 5. Files

- **FILE-001**: `src/CymruBlazor/wwwroot/css/cymrublazor.css` (Main entry sheet/Output bundle).
- **FILE-002**: `src/CymruBlazor/wwwroot/css/tokens/colours.css` (Colours token sheet).
- **FILE-003**: `src/CymruBlazor/wwwroot/css/base/accessibility.css` (Accessibility and accessibility reset rules).
- **FILE-004**: `src/CymruBlazor/Services/ThemeService.cs` (Theme switching manager).
- **FILE-005**: `src/CymruBlazor/Themes/ThemeMode.cs` (Theme mode enumerations).

---

## 6. Testing

- **TEST-001**: **bUnit Component Tests**: Verify components correctly render with semantic class namespaces and do not leak global style identifiers.
- **TEST-002**: **Accessibility Verification**: Confirm that focus visible styling resolves correctly under high-contrast emulation using Axe-Core tests.
- **TEST-003**: **PWA Cash Manifest Validation**: Build the application and parse `service-worker-assets.js` to assert that `cymrublazor.css` is registered for offline use.

---

## 7. Risks & Assumptions

- **RISK-001**: Browser support for CSS Cascade Layers in exceptionally legacy environments. *Mitigation*: Ensure targets strictly meet modern browser standards (Edge, Safari, Chrome, Firefox) as mandated by modern Blazor WASM runtimes.
- **ASSUMPTION-001**: The hosting Blazor application retains responsibility for referencing the single stylesheet root; automatic injection will not be performed by the class library.

---

## 8. Related Specifications / Further Reading

- [NHS digital service manual - Colour guidelines](https://service-manual.nhs.uk/design-system/styles/colour)
- [W3C Web Content Accessibility Guidelines (WCAG) 2.2](https://www.w3.org/TR/WCAG22/)