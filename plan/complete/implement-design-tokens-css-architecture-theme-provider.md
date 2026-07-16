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

| Task         | Description                                                                                                                                                                                 | Completed | Date       |
| :----------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------: | :--------- |
| **TASK-001** | Create `wwwroot/css/tokens/colours.css` defining semantic color variables mapping NHS Wales color ranges to functional roles (primary, secondary, success, warning, danger, surface, text). |     ✓     | 2026-07-14 |
| **TASK-002** | Create `wwwroot/css/tokens/typography.css` defining system font fallbacks (`Segoe UI`, `Arial`, `sans-serif`), sizes (`xs` to `xl`), line heights, and weights.                             |     ✓     | 2026-07-14 |
| **TASK-003** | Create `wwwroot/css/tokens/spacing.css` establishing an 8-point modular spacing scale (`--cymru-space-0` through `--cymru-space-6`).                                                        |     ✓     | 2026-07-14 |
| **TASK-004** | Create `wwwroot/css/tokens/elevation.css` defining shadows (`--cymru-shadow-sm` through `--cymru-shadow-lg`) and border-radii.                                                              |     ✓     | 2026-07-14 |
| **TASK-005** | Create `wwwroot/css/tokens/breakpoints.css` declaring the reference sizes for media queries (mobile, tablet, desktop, wide).                                                                |     ✓     | 2026-07-14 |
| **TASK-006** | Create `wwwroot/css/base/accessibility.css` containing WCAG 2.2 AA compliant focus indicators, `@media (prefers-reduced-motion)` resets, and `@media (forced-colors)` overrides.            |     ✓     | 2026-07-14 |
| **TASK-007** | Create `wwwroot/css/base/reset.css` containing a minimal, un-opinionated box-sizing and tag reset (no bootstrap residue).                                                                   |     ✓     | 2026-07-14 |

### Implementation Phase 4.2: Layout, Utilities, Cascade Layers & Single Bundling

- GOAL-002: Develop structural layouts, modern CSS cascade layers, lightweight helper utilities, and the bundling compilation tooling.

| Task         | Description                                                                                                                                                                                                                                                                                                     | Completed | Date       |
| :----------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------: | :--------- |
| **TASK-008** | Create `wwwroot/css/layout/grid.css` and `containers.css` to handle responsive containers, flex structures, and CSS Grid templates.                                                                                                                                                                             |     ✓     | 2026-07-14 |
| **TASK-009** | Add standard utility lists under `wwwroot/css/utilities/` covering margin/padding offsets, display toggles, and visibility classes.                                                                                                                                                                             |     ✓     | 2026-07-14 |
| **TASK-010** | Add theme base placeholders for default `:root`, `[data-theme="dark"]`, and `[data-theme="high-contrast"]` selectors to accommodate the CSS variables.                                                                                                                                                          |     ✓     | 2026-07-14 |
| **TASK-011** | Define CSS Cascade Layers (`@layer reset, tokens, base, layout, components, utilities, overrides;`) to establish robust priority handling.                                                                                                                                                                      |     ✓     | 2026-07-14 |
| **TASK-012** | Create a lightweight MSBuild-integrated CSS bundler that produces a single `cymrublazor.css`, removes development-only directives (`@import` and duplicate `@layer` declarations), preserves comments and source order, and requires no external tooling. Automatically runs during both `Build` and `Publish`. |     ✓     | 2026-07-14 |

### Implementation Phase 4.3: Component CSS Isolation & Blazor Theme Provider Foundation

- GOAL-003: Interface the CSS variables with Blazor components via a decoupled C# service, enforce Razor CSS isolation, and confirm PWA asset caching.

| Task         | Description                                                                                                                                                                         | Completed | Date       |
| :----------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------: | :--------- |
| **TASK-013** | Audit and isolate existing component styles (e.g., `Button.razor.css`, `Alert.razor.css`) to consume semantic `--cymru-` design tokens instead of direct colors.                    |     ✓     | 2026-07-14 |
| **TASK-014** | Verify WebAssembly PWA compatibility by checking that `service-worker-assets.js` automatically maps the compiled `cymrublazor.css`.                                                 |     ✓     | 2026-07-14 |
| **TASK-015** | Create C# `ThemeService.cs`, `ThemeDefinition.cs`, and `ThemeMode.cs` within the project's Services and Themes directories to programmatically toggle HTML `data-theme` attributes. |     ✓     | 2026-07-14 |
| **TASK-016** | Document client-side integration via the simple `<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet">` reference in consuming apps.                              |     ✓     | 2026-07-14 |

---

## 3. Alternatives

- **ALT-001: Tailwind CSS / CSS-in-JS Utility Frameworks**: Considered for rapid styling but rejected due to high bundle size, dependency complexities in Razor Class Libraries, and conflicts with the clean offline PWA philosophy.
- **ALT-002: Traditional CSS imports (`@import`)**: Considered keeping individual stylesheets loaded at runtime. Rejected because multiple `@import` requests cause render-blocking network roundtrips, degrading mobile/PWA performance.
- **ALT-003: BundlerMinifier / LibMan**: Considered as external bundling tools but rejected. MSBuild-only approach keeps CymruBlazor dependency-free, requires no Node.js, npm, webpack, or external CLI tools, and integrates natively with `dotnet build` and `dotnet publish` for GitHub Actions and local development.

---

## 3.1 Build Architecture Strategy

### Pure MSBuild CSS Bundling

The CSS bundling process uses **pure MSBuild** with no external tooling dependencies—no Node.js, npm, webpack, LibMan, or BundlerMinifier. This approach:

- **Keeps CymruBlazor dependency-free** and cross-platform compatible.
- **Integrates natively** with `dotnet build` and `dotnet publish`.
- **Works seamlessly** on GitHub Actions and local development machines.
- **Generates a single deterministic output** (`cymrublazor.css`) with no runtime `@import` statements.

### CSS Bundler Tool

A lightweight C# bundler (~150 lines) at `tools/CymruBlazor.CssBundler/Program.cs` performs the bundling:

1. **Reads all CSS files** in defined order (reset → tokens → base → layout → components → utilities → overrides).
2. **Removes duplicate `@layer` declarations** (only the initial declaration in the output remains).
3. **Removes all `@import` statements** (development-only directives).
4. **Preserves comments and source formatting** for debuggability.
5. **Outputs a single `cymrublazor.css`** with a single global `@layer` declaration at the top.

### Generated Stylesheet Structure

```
@layer reset,tokens,base,layout,components,utilities,overrides;

/* reset.css */
* { box-sizing: border-box; }
...

/* colours.css */
:root { --cymru-primary: #003087; }
...

/* typography.css */
:root { --cymru-font-family: 'Segoe UI', Arial, sans-serif; }
...

/* accessibility.css */
:focus-visible { ... }
...

/* [all remaining CSS in order] */
```

### MSBuild Integration

Two simple files orchestrate the bundling:

**`build/BundleCss.props`**: Declares the CSS file list.

```xml
<ItemGroup>
  <CymruCss Include="wwwroot\css\base\reset.css"/>
  <CymruCss Include="wwwroot\css\tokens\*.css"/>
  <CymruCss Include="wwwroot\css\base\*.css" Exclude="wwwroot\css\base\reset.css"/>
  <CymruCss Include="wwwroot\css\themes\*.css"/>
  <CymruCss Include="wwwroot\css\layout\*.css"/>
  <CymruCss Include="wwwroot\css\components\*.css"/>
  <CymruCss Include="wwwroot\css\utilities\*.css"/>
  <CymruCss Include="wwwroot\css\overrides\*.css"/>
</ItemGroup>
```

**`build/BundleCss.targets`**: Defines the MSBuild target that invokes the bundler.

```xml
<Target Name="BundleCymruCss" BeforeTargets="AssignTargetPaths">
  <Exec Command="dotnet run --project tools\CymruBlazor.CssBundler\CymruBlazor.CssBundler.csproj -- src\CymruBlazor\wwwroot\css\cymrublazor.css @(CymruCss)"/>
</Target>
```

**CymruBlazor.csproj**: One-line integration.

```xml
<Import Project="build\BundleCss.targets" />
```

### Entry Point Renaming

- **Development**: Edit `wwwroot/css/cymrublazor.entry.css` (never manually edit).
- **Generated Output**: `wwwroot/css/cymrublazor.css` (automatically created during Build/Publish).
- **Public Reference**: Consuming apps link `_content/CymruBlazor/css/cymrublazor.css` as always.

---

## 4. Dependencies

- **DEP-001**: `.NET 10.0 SDK` (WebAssembly and Razor Class Library compilation).
- **DEP-002**: Consuming Application index files (`index.html` or `App.razor`) to reference the newly bundled style asset.

---

## 5. Files

- **FILE-001**: `src/CymruBlazor/wwwroot/css/cymrublazor.entry.css` (Development entry point—never edited manually).
- **FILE-002**: `src/CymruBlazor/wwwroot/css/cymrublazor.css` (Generated output bundle—auto-generated during Build and Publish).
- **FILE-003**: `src/CymruBlazor/wwwroot/css/tokens/colours.css` (Colours token sheet).
- **FILE-004**: `src/CymruBlazor/wwwroot/css/base/accessibility.css` (Accessibility and accessibility reset rules).
- **FILE-005**: `src/CymruBlazor/build/BundleCss.targets` (MSBuild target for CSS bundling orchestration).
- **FILE-006**: `src/CymruBlazor/build/BundleCss.props` (MSBuild properties for CSS bundler configuration).
- **FILE-007**: `tools/CymruBlazor.CssBundler/Program.cs` (~150 lines—lightweight bundler that removes `@import` and deduplicates `@layer` declarations).
- **FILE-008**: `src/CymruBlazor/Services/ThemeService.cs` (Theme switching manager).
- **FILE-009**: `src/CymruBlazor/Themes/ThemeMode.cs` (Theme mode enumerations).

---

## 6. Testing

- **TEST-001**: **bUnit Component Tests**: Verify components correctly render with semantic class namespaces and do not leak global style identifiers.
- **TEST-002**: **Accessibility Verification**: Confirm that focus visible styling resolves correctly under high-contrast emulation using Axe-Core tests.
- **TEST-003**: **PWA Cash Manifest Validation**: Build the application and parse `service-worker-assets.js` to assert that `cymrublazor.css` is registered for offline use.

---

## 7. Definition of Done: TASK-012

TASK-012 is complete when:

- ✓ `cymrublazor.entry.css` acts as the development entry point.
- ✓ An MSBuild target runs automatically during both `Build` and `Publish`.
- ✓ A generated `wwwroot/css/cymrublazor.css` contains all CSS in the correct layer order.
- ✓ All runtime `@import` statements are removed from the generated output.
- ✓ Only a single global `@layer reset, tokens, base, layout, components, utilities, overrides;` declaration remains.
- ✓ No Node.js, npm, LibMan, BundlerMinifier, or external bundling tools are required.
- ✓ The generated stylesheet is deterministic, debuggable, and suitable for inclusion in Blazor WebAssembly service worker asset manifests.

---

## 8. Progress Update

### Completed

- Phase 4.1 - Design Tokens & Base Architecture.
- Phase 4.2 - Cascade layer entry point.
- Task 008 - Layout primitives.
- Task 009 - Focused utility styles.
- Task 010 - Theme definitions.
- Task 012 - MSBuild bundling architecture.

At this stage, the CSS pipeline is effectively complete. The next logical milestone is **Phase 4.3**, implementing the `ThemeService`, `ThemeMode`, and `ThemeDefinition` classes to connect the semantic CSS themes with runtime Blazor theme switching.

---

## 9. Risks & Assumptions

- **RISK-001**: Browser support for CSS Cascade Layers in exceptionally legacy environments. _Mitigation_: Ensure targets strictly meet modern browser standards (Edge, Safari, Chrome, Firefox) as mandated by modern Blazor WASM runtimes.
- **ASSUMPTION-001**: The hosting Blazor application retains responsibility for referencing the single stylesheet root; automatic injection will not be performed by the class library.

---

## 10. Related Specifications / Further Reading

- [NHS digital service manual - Colour guidelines](https://service-manual.nhs.uk/design-system/styles/colour)
- [W3C Web Content Accessibility Guidelines (WCAG) 2.2](https://www.w3.org/TR/WCAG22/)
