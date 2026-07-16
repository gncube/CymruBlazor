# Phase 4 - Design Tokens, CSS Architecture and Theming Foundation

## Objective

Establish a production-ready styling architecture for CymruBlazor that:

- Is completely independent of Bootstrap.
- Works natively with Blazor WebAssembly.
- Supports Blazor WebAssembly PWA applications.
- Scales to 50+ components.
- Supports future theming without breaking components.
- Minimises CSS payloads and HTTP requests.
- Aligns with modern CSS architecture and WCAG 2.2 AA accessibility.

---

Branch: feature/phase-4-design-tokens-css-theming ✅ 2026-07-13

# Architectural Principles

## Principle 1 - Semantic Design Tokens

Components must **never** reference NHS colours directly.

Instead, components consume semantic tokens.

Example:

```
Primary
Secondary
Success
Warning
Danger
Info
Surface
Background
Border
Text
Muted
```

Semantic tokens are mapped to NHS Wales colours within the theme.

This allows:

- NHS Wales theme
- NHS England theme
- Corporate theme
- Dark theme
- High contrast theme

without changing component CSS.

---

## Principle 2 - Global vs Component Styles

Global CSS is only used for:

- Design tokens
- Reset
- Accessibility
- Typography
- Layout
- Utility classes

Every component owns its own isolated CSS.

Example:

```
Button.razor
Button.razor.css

Alert.razor
Alert.razor.css

Card.razor
Card.razor.css
```

This avoids selector collisions and keeps components self-contained.

---

## Principle 3 - Single Bundled Stylesheet

Do not use CSS `@import`.

Instead, produce a single bundled stylesheet during the build.

Benefits:

- Faster first render
- Smaller request count
- Better PWA caching
- Better cache invalidation
- Simpler consumer experience

Consumers reference only:

```
_content/CymruBlazor/css/cymrublazor.css
```

---

## Principle 4 - PWA First

Every sample application was scaffolded using

```
dotnet new blazorwasm --empty --pwa
```

Therefore the component library must:

- Work without Bootstrap.
- Work without JavaScript frameworks.
- Integrate cleanly with the generated Service Worker.
- Produce static assets compatible with offline caching.

---

# Folder Structure

```
src/
    CymruBlazor/
        Components/

        Themes/

        Services/

        wwwroot/
            css/

                tokens/
                    colours.css
                    spacing.css
                    typography.css
                    elevation.css
                    breakpoints.css

                base/
                    reset.css
                    accessibility.css
                    typography.css

                layout/
                    grid.css
                    containers.css

                utilities/
                    spacing.css
                    display.css
                    flex.css
                    visibility.css

                cymrublazor.css
```

Component CSS lives beside the component.

Example:

```
Components/

    Button/
        Button.razor
        Button.razor.cs
        Button.razor.css

    Alert/
        Alert.razor
        Alert.razor.css

    Card/
        Card.razor
        Card.razor.css
```

---

# Step 1 - Design Tokens

Create

```
wwwroot/css/tokens/colours.css
```

Define semantic colours only.

Example

```
--cymru-color-primary
--cymru-color-primary-hover

--cymru-color-secondary

--cymru-color-success

--cymru-color-warning

--cymru-color-danger

--cymru-color-info

--cymru-color-surface

--cymru-color-surface-alt

--cymru-color-background

--cymru-color-border

--cymru-color-text

--cymru-color-text-muted

--cymru-color-focus
```

No component should reference NHS colours directly.

---

# Step 2 - Typography Tokens

Create

```
tokens/typography.css
```

Example

```
--cymru-font-family

--cymru-font-size-xs
--cymru-font-size-sm
--cymru-font-size-md
--cymru-font-size-lg
--cymru-font-size-xl

--cymru-font-weight-normal
--cymru-font-weight-semibold
--cymru-font-weight-bold

--cymru-line-height
```

Prefer

```
system-ui,
Segoe UI,
Arial,
sans-serif
```

Avoid hardcoded licensed fonts such as Frutiger.

---

# Step 3 - Spacing Tokens

Create

```
tokens/spacing.css
```

Example

```
--cymru-space-0
--cymru-space-1
--cymru-space-2
--cymru-space-3
--cymru-space-4
--cymru-space-5
--cymru-space-6
```

---

# Step 4 - Elevation Tokens

```
tokens/elevation.css
```

```
--cymru-shadow-sm
--cymru-shadow-md
--cymru-shadow-lg

--cymru-radius-sm
--cymru-radius-md
--cymru-radius-lg
```

---

# Step 5 - Breakpoint Tokens

```
tokens/breakpoints.css
```

Document responsive breakpoints.

Although CSS custom properties cannot currently be used inside media queries, keeping breakpoint values documented in one place improves maintainability.

---

# Step 6 - Accessibility Base

Create

```
base/accessibility.css
```

Include

- Focus styles
- Reduced motion
- Forced colours
- High contrast
- Screen reader helpers

WCAG 2.2 AA compliance begins here.

---

# Step 7 - Reset

Create

```
base/reset.css
```

Minimal reset only.

Do not include opinionated framework resets.

---

# Step 8 - Layout

Create

```
layout/grid.css
layout/containers.css
```

Provide

- Container widths
- Responsive layout helpers
- CSS Grid helpers
- Flex utilities

---

# Step 9 - Utility Classes

Create

```
utilities/
```

Include

- spacing
- flex
- display
- visibility

Utilities should remain intentionally small.

---

# Step 10 - Theme Support

Create the initial themes.

```
:root
```

contains the default NHS Wales theme.

Also define empty placeholders for

```
[data-theme="dark"]

[data-theme="high-contrast"]
```

Even if only the default theme is implemented initially.

This avoids future breaking changes.

---

# Step 11 - CSS Cascade Layers

Use modern CSS Layers.

```
@layer reset;
@layer tokens;
@layer base;
@layer layout;
@layer components;
@layer utilities;
@layer overrides;
```

This greatly reduces specificity problems.

---

# Step 12 - Bundled Stylesheet

Generate

```
wwwroot/css/cymrublazor.css
```

This file is the only stylesheet referenced by consumers.

Do not use CSS `@import`.

Instead, bundle the CSS during the build process.

---

# Step 13 - Razor CSS Isolation

Every component owns its own stylesheet.

Example

```
Button.razor
Button.razor.css

Card.razor
Card.razor.css

Alert.razor
Alert.razor.css
```

Component styling should never leak globally.

---

# Step 14 - PWA Compatibility

Verify that published static assets are automatically included in

```
service-worker-assets.js
```

No manual changes should be required.

Validate:

- First load
- Offline mode
- Hard refresh
- Version upgrades

---

# Step 15 - Theme Provider Foundation

Introduce the infrastructure for theming now, even if only the default theme is available.

Create

```
Services/
    ThemeService.cs

Themes/
    ThemeDefinition.cs

Themes/
    ThemeMode.cs
```

Responsibilities:

- Current theme
- Theme switching
- Notify components of theme changes
- Persist preference (future enhancement)

This establishes the contract between components and styling before component development begins.

---

# Step 16 - Consumer Integration

Document how consuming applications reference the bundled stylesheet.

For Blazor WebAssembly:

```
<link href="_content/CymruBlazor/css/cymrublazor.css"
      rel="stylesheet">
```

The library must not attempt to inject assets automatically.

Consumers remain responsible for referencing the stylesheet.

---

# Deliverables

At the end of Phase 4 the solution will contain:

✓ Semantic design tokens

✓ Global CSS architecture

✓ Component CSS isolation

✓ Theme infrastructure

✓ PWA-compatible static assets

✓ Single bundled stylesheet

✓ Accessibility foundation

✓ Responsive layout utilities

✓ Dark theme placeholders

✓ High contrast placeholders

✓ Modern CSS Cascade Layers

✓ Zero Bootstrap dependency

✓ Optimised for Blazor WebAssembly and Razor Class Libraries

---

# Acceptance Criteria

- Builds successfully on .NET 10.
- Works in all `--empty --pwa` sample applications.
- Requires only one stylesheet reference.
- No Bootstrap dependency.
- No JavaScript dependency.
- All component styles are isolated.
- Global CSS contains only tokens, base styles, layout and utilities.
- Service Worker caches all static assets correctly.
- Theme architecture supports future expansion without breaking changes.
- Meets WCAG 2.2 AA accessibility requirements.
