# CymruBlazor Demo — Library Gap TODO List

> **Purpose**: This file tracks every gap in the `CymruBlazor` component library
> discovered by treating `CymruBlazor.Demo` as a genuine reference implementation.
>
> The demo should use **only `CymruBlazor` components** for structural elements.
> Where a gap is found, a temporary workaround is used in the demo and this file
> is updated. Library PRs should reference items here when resolving gaps.
>
> **Status key**: 🔴 Blocking · 🟡 High priority · 🟢 Medium priority · ⚪ Low priority

---

## Missing Components

### 🔴 `CyTabs` / `CyTabPanel`

- **Needed by**: Every component page — Examples | API | Accessibility tab bar
- **Priority**: HIGH — blocks the core component page UX pattern
- **Spec**:
  - Props: `Items` (list of `{ Id, Label }` tab definitions), `ActiveTab` (two-way bindable string)
  - Keyboard: Left/Right arrow keys navigate between tabs, Enter/Space activates, Home/End jump to first/last
  - ARIA: `role="tablist"`, `role="tab"`, `aria-selected`, `role="tabpanel"`, `aria-labelledby`
  - Variants: underline (default), pill
  - Each tab panel uses `role="tabpanel"` with `tabindex="0"` and `aria-labelledby`
- **Temporary workaround**: Demo uses `<div>` tab list with Blazor `bool` state and manual CSS

---

### 🟡 `CyAccordion` / `CyAccordionItem`

- **Needed by**: Sidebar navigation section expand/collapse
- **Priority**: MEDIUM — sidebar is functional with `<details>/<summary>` as interim
- **Spec**:
  - `CyAccordion`: optional `AllowMultipleOpen` bool
  - `CyAccordionItem`: `Header` (string or RenderFragment), `IsExpanded` (bindable), `Id`
  - ARIA: `aria-expanded`, `aria-controls` on trigger; `role="region"` on panel
  - Animation: CSS `max-height` transition (no JS)
- **Temporary workaround**: Native HTML `<details>`/`<summary>` (accessible without JS)

---

### 🔴 `CyButton` — Icon-only variant

- **Needed by**: Header theme toggle button (☾/☀), header search button (🔍), code block copy button
- **Priority**: HIGH — all currently use hand-coded `<button>` elements in the demo
- **Spec**:
  - New variant mode: `ButtonVariant.Icon` or a separate `CyIconButton` component
  - Props: `AriaLabel` (required, no default — WCAG 4.1.2), icon via `ChildContent` or `Icon` param
  - Sizes: match existing `ComponentSize` enum
  - Note: `AriaLabel` **must be required** — icon-only buttons without an accessible name violate WCAG 2.1 SC 4.1.2
- **Temporary workaround**: Hand-coded `<button aria-label="...">` in `DemoHeader.razor`

---

### 🔴 `CyButton` — `Href` / anchor rendering

- **Needed by**: Homepage hero CTA buttons ("Explore components →", "Get started")
- **Priority**: HIGH — demo currently uses `<a class="cb-demo-btn">` (demo CSS utility)
- **Spec**:
  - When `Href` is set, render `<a>` instead of `<button>`
  - All existing button styling/variants apply unchanged
  - Add `Target` and `Rel` params for external links
- **Temporary workaround**: `cb-demo-btn` utility class in `demo.css`

---

### 🟡 `CyBadge` / `CyTag`

- **Needed by**: Technology badge strip on homepage hero, category labels on component cards
- **Priority**: MEDIUM
- **Spec**:
  - Small inline label pill — purely presentational, not interactive
  - Props: `Variant` (neutral, primary, success, warning, danger, info), `Size` (sm/md)
  - Renders as `<span>` with appropriate ARIA role if used as a status indicator
- **Temporary workaround**: Inline `<span>` with utility classes in `Home.razor`

---

### 🔴 `CyCodeBlock`

- **Needed by**: All component demo pages — code snippets with copy functionality
- **Priority**: HIGH — code display and copy are core to any documentation experience
- **Spec**:
  - Props: `Language` (string: "razor", "csharp", "bash"), `Code` (string), `Label` (optional header string)
  - Copy button triggers `navigator.clipboard.writeText` via `IJSRuntime`
  - "Copied!" confirmation announced via `CyLiveRegion` (Mediator message)
  - Syntax highlighting: CSS-only token colouring applied to HTML escaped content
    (no JS syntax highlighter dependency — respects the library's minimal-dependency principle)
  - Dark code surface using `--cymru-navy-900` background token
- **Temporary workaround**: Raw `<pre><code>` styled via `demo.css` + per-page clipboard JS interop

---

### ⚪ `CySearchModal` / `CyCommandPalette`

- **Needed by**: Header search (Ctrl+K shortcut) — PR 4 scope
- **Priority**: LOW
- **Spec**:
  - Triggered by Ctrl+K keyboard shortcut or clicking the search icon
  - Full-screen overlay modal
  - Text `<input>` with instant Blazor-side filtering (no JS search library)
  - Results grouped by: Components / Documentation
  - Keyboard: Arrow keys navigate results, Enter to follow link, Escape to close
  - Uses `CyFocusTrap` internally (already exists in library ✅)
- **Temporary workaround**: Search button is a non-functional stub in PR 1 (aria-disabled)

---

### ⚪ `CyDivider`

- **Needed by**: Sidebar section separators, footer visual divider
- **Priority**: LOW
- **Spec**: Horizontal `<hr>` or vertical variant using `--cymru-color-border` token
- **Temporary workaround**: CSS `border-bottom` on sidebar section headings

---

## Existing Component Enhancements

### 🔴 `CyNavigation` / `CyNavigationItem` — active link state

- **Current**: `CyNavigationItem` renders an `<a>` tag but has no Blazor `NavLink` active-matching
- **Needed**: `CyNavigationItem` should behave like Blazor's `NavLink` — applying an active CSS class
  when the current route matches `Href` (with optional `Match` param: `All` vs `Prefix`)
- **Why blocking**: `DemoHeader.razor` cannot switch to `CyNavigation` until active state works correctly
- **Priority**: HIGH (PR 1 blocker for the header)

---

### 🔴 `CySidebar` — mobile off-canvas drawer

- **Current**: `CySidebar` supports `CollapseMode.Compact`, `CollapseMode.IconOnly`, `CollapseMode.Hidden`
  but has no off-canvas/drawer pattern for mobile viewports
- **Needed**: A `CollapseMode.Drawer` (or `CollapseMode.OffCanvas`) mode where:
  - At mobile widths, the sidebar renders as an absolutely-positioned overlay drawer
  - Overlay dims the main content when the drawer is open
  - `CyFocusTrap` is applied when drawer is open
  - `Collapsed = true` closes it; hamburger button in header sets `Collapsed = false`
- **Priority**: HIGH (PR 1 blocker for correct mobile layout)

---

### 🟢 `CyFooter` — typed link group slots

- **Current**: `CyFooter` accepts `Copyright`, `Version`, `ShowVersion`, `PackageId`
  but the link columns are not yet configurable without `ChildContent` raw markup
- **Needed**: A `LinkGroups` parameter accepting a typed collection:
  ```csharp
  record FooterLinkGroup(string Title, IReadOnlyList<FooterLink> Links);
  record FooterLink(string Label, string Href, bool External = false);
  ```
  so `DemoFooter.razor` can be simplified to a single `<CyFooter>` configuration
- **Priority**: MEDIUM

---

### 🟢 `CyHeroBanner` — verify navy gradient rendering

- **Current**: `HeroBackground.Primary` background is documented for dark header use
- **Needed**: Confirm the gradient `linear-gradient(135deg, #1b294a, #325083)` is applied or that
  `--cymru-color-accent` tokens produce the correct dark navy hero for the new homepage design
- **Priority**: LOW — likely works, just needs visual verification in the demo

---

### 🟢 `CyButton` — `Loading` state visual indicator

- **Current**: `CyButton` may or may not have a `Loading` prop — needs verification
- **Needed for playground**: Loading state (`Loading="true"`) should show a spinner and disable interaction
- **Priority**: MEDIUM (needed for component playground in PR 3)

---

## Documentation / Site Gaps

### 🟡 Design Principles page

- **Route**: `/design-principles`
- **Content**: The CymruBlazor design principles:
  1. Accessible by default
  2. C# first
  3. Composable components
  4. Minimal dependencies
  5. Predictable APIs
  6. Themeable by design
  7. Tested components
- **No library gap** — pure documentation prose page

---

### 🟡 Component category overview pages

- **Routes**: `/layouts`, `/forms`, `/content`, `/branding`, `/accessibility`
- **Content**: Grid of `CyCard` components linking to individual component pages
- **Library requirement**: `CyCard` (exists ✅) + `CyGrid` (exists ✅)
- **Missing**: These overview pages don't exist yet — each shows a card per component in that category

---

### 🟢 Prev/next component navigation

- **Needed by**: Bottom of each component page — "← CyStack" / "CyGrid →" links
- **No library component needed** — demo-specific `DemoComponentNav.razor` shared component
- **Priority**: PR 4 scope

---

### 🟢 "Edit on GitHub" links

- **Needed by**: Each component page — links to source `.razor` file on GitHub
- **No library component needed** — simple anchor with constructed URL
- **Priority**: MEDIUM

---

## Resolved Gaps

*(Move items here when the corresponding library PR is merged)*

| Component | Fixed in PR | Date |
|---|---|---|
| — | — | — |

---

*Last updated: 2026-08-24. Maintained as part of the `feature/demo-workbench-shell` branch.*
