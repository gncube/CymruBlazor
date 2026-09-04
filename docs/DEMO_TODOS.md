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

### 🟡 `CyTabs` / `CyTabPanel`

- **Needed by**: Every component page — Examples | API | Accessibility tab bar
- **Priority**: MEDIUM — downgraded from HIGH. The demo's hand-rolled tab bar
  now has correct ARIA semantics (`role="tablist"`/`role="tab"`/`aria-selected`/
  `role="tabpanel"`/`aria-controls`/`aria-labelledby`, retrofitted across all 18
  tabbed pages), so this is no longer an accessibility blocker — but it's still
  duplicated by hand in every one of those 18 pages. A real `CyTabs` component
  would eliminate that duplication and reduce the risk of the same mistake
  recurring elsewhere; it just isn't blocking anything anymore.
- **Spec**: unchanged from the original — see git history for the full spec if
  reviving this.
- **Current workaround**: `<div>` tab list with Blazor `bool` state, manual
  ARIA attributes, and manual CSS, hand-copied into each component doc page.

---

### 🟡 `CyAccordion` / `CyAccordionItem`

- **Needed by**: Sidebar navigation section expand/collapse
- **Priority**: MEDIUM — unchanged. `DemoSidebar.razor` still uses native
  `<details>`/`<summary>`, which remains a perfectly accessible interim (no JS
  needed, native keyboard/screen-reader support) — this is a "nice to have
  reusable component" gap, not a functional one.

---

### 🟡 `CyButton` — Icon-only variant

- **Needed by**: Header theme toggle button, header search button
- **Priority**: MEDIUM — downgraded from HIGH. `CyButton` itself gained a lot
  since this was written (`Variant`, `Size`, `Disabled`, `Loading`, `Href`,
  `Type`, `OnClick` — see CHANGELOG `0.1.0-preview.7`), but there's still no
  dedicated icon-only mode, and `DemoHeader.razor`'s search/theme-toggle
  buttons remain hand-coded `<button aria-label="...">` elements wrapping a
  `CyIcon` rather than using `CyButton` at all.
- **Current workaround**: hand-coded `<button>` + `CyIcon` in `DemoHeader.razor`.

---

### ✅ `CyButton` — `Href` / anchor rendering — **RESOLVED** (`0.1.0-preview.7`)

`CyButton.Href` now renders as `<a>` (instead of `<button>`) when set and the
button isn't disabled. **Follow-up still open**: `Home.razor`'s hero CTAs
(`Explore components →`, `Get started`) haven't been migrated to use it yet —
they still use the `cb-demo-btn` demo CSS utility on plain `<a>` tags. Since
the capability now exists in the library, migrating those two buttons is a
small, low-risk cleanup rather than something blocked on the library.

---

### 🟢 `CyBadge` / `CyTag`

- **Needed by**: Technology badge strip on homepage hero, category labels on
  component cards
- **Priority**: MEDIUM — unchanged. `Home.razor` still uses inline `<span>`
  with the `cb-badge`/`cb-badge--pill` demo utility classes.

---

### 🟢 `CyCodeBlock`

- **Needed by**: All component demo pages — code snippets with copy
  functionality
- **Priority**: MEDIUM — downgraded from HIGH now that a solid demo-level
  workaround exists and is used consistently everywhere. `Shared/
  DemoCodeBlock.razor` provides language label, a working copy-to-clipboard
  button (via `IJSRuntime` + `navigator.clipboard.writeText`), and dark-surface
  styling, and is used across every component doc page. What it doesn't have:
  the "Copied!" confirmation is a plain text-swap on the button, not announced
  via `CyLiveRegion`/a Mediator message as originally specced, and there's no
  CSS-only syntax highlighting. A real `CyCodeBlock` library component would
  still be valuable to avoid every consuming app rebuilding this, but it's no
  longer a hole in the demo's own experience.
- **Current workaround**: `Shared/DemoCodeBlock.razor` (demo-specific).

---

### ✅ `CySearchModal` / `CyCommandPalette` — feature resolved, component still doesn't exist

Full Ctrl+K search (global shortcut, results grouped/ranked by title →
category → description, arrow-key/Enter/Escape keyboard navigation, focus
trapped via the existing `CyFocusTrap`) is built and working, as
`Shared/DemoSearchModal.razor` + `Shared/DemoNavigationIndex.cs` +
`wwwroot/js/search.js` (demo-specific — the global-shortcut listener needed a
small JS interop bridge, since Blazor has no native way to observe a
document-level keydown regardless of focus). The **feature** gap this item
described is closed; whether a generalised `CySearchModal` library component
is worth extracting from the demo-specific implementation is a separate,
lower-priority question, since the current implementation is tightly coupled
to `DemoNavigationIndex`'s page list.

---

### ⚪ `CyDivider`

- **Needed by**: Sidebar section separators, footer visual divider
- **Priority**: LOW — unchanged. Still a CSS `border-bottom` on sidebar
  section headings.

---

## Existing Component Enhancements

### ✅ `CyNavigation` / `CyNavigationItem` — active link state — **already correct**

Checked directly: `CyNavigationItem.razor` already wraps a Blazor `NavLink`
with `ActiveClass="cy-navigation__link--active"` and
`Match="NavLinkMatch.Prefix"`. This was not actually blocking anything by the
time it was checked — unclear whether it was fixed before this note was
written or the original note was simply inaccurate. `DemoHeader.razor` still
uses plain `NavLink` elements directly rather than `CyNavigation`/
`CyNavigationItem` for its primary nav, but that's a deliberate choice (see
`/navigation/header`'s own docs) to keep the demo's off-canvas mobile drawer
as the single mobile-nav mechanism, rather than composition trouble — not
because the active-state capability doesn't work.

---

### ✅ `CySidebar` — mobile off-canvas drawer — resolved at the demo level, not in the library

A real off-canvas drawer with backdrop, focus trapping, and Escape-to-close
exists and works — but it's implemented in the **demo's** `MainLayout.razor`
(Blazor component state + `NavigationManager.LocationChanged` to auto-close on
navigation) and `DemoSidebar.razor` (an `IsOpen` parameter toggling a CSS
transform), not as a `CollapseMode.Drawer`/`CollapseMode.OffCanvas` mode on
the library's `CySidebar` itself. `CySidebar`'s own `CollapseMode` enum is
unchanged. If another consuming app wants this exact off-canvas behaviour,
they'd currently need to reimplement it rather than opt into it via a
parameter — that's the real remaining gap.

---

### 🟢 `CyFooter` — typed link group slots

- **Priority**: MEDIUM — unchanged; not what actually got built. `CyFooter`
  gained a `Background` parameter instead (`0.1.0-preview.7` — see CHANGELOG),
  which was a different, real gap (no way to use it on a light page without
  the hardcoded navy). The originally-requested typed `LinkGroups`/
  `FooterLink` API for configuring columns without raw `ChildContent` markup
  is still open — `DemoFooter.razor` still passes four hand-written `<div>`
  groups as `ChildContent`.

---

### ⚪ `CyHeroBanner` — verify navy gradient rendering

- **Priority**: LOW, and arguably moot now. `Home.razor` no longer uses
  `CyHeroBanner` at all — the homepage hero was rebuilt as custom
  `<section class="cb-home__hero">` markup with its own gradient treatment
  (see `Home.razor.css`), trading dogfooding this component for more visual
  control. `CyHeroBanner` itself is unchanged and unverified either way; there
  just isn't a live reference usage of it left in the demo to verify against.

---

### ✅ `CyButton` — `Loading` state visual indicator — **RESOLVED** (`0.1.0-preview.7`)

`Loading="true"` shows a spinner and disables interaction (blocks `OnClick`)
without changing the button's width. Live, working example: `/forms/button`.

---

## Documentation / Site Gaps

### 🟡 Design Principles page

- **Route**: `/design-principles`
- **Priority**: HIGH — unchanged, still doesn't exist. Pure documentation
  prose page, no library gap.

---

### 🟡 Component category overview pages

- **Routes**: `/layouts` (**exists** — `LayoutsOverview.razor`, predates this
  list), `/forms`, `/content`, `/branding`, `/accessibility` (**still don't
  exist**)
- **Content**: Grid of `CyCard` components linking to individual component
  pages within that category
- **Library requirement**: `CyCard` ✅, `CyGrid` ✅ — both exist and are
  unblocked; this is purely demo page-authoring work.

---

### ✅ Prev/next component navigation — **RESOLVED**

`Shared/DemoPageNav.razor` + `Shared/DemoNavigationIndex.cs`, wired into
`MainLayout.razor` once (not per-page), so it appears automatically at the
bottom of every page in the index and correctly shows nothing on pages
outside it (e.g. Home). Exactly the demo-specific, no-library-component-needed
shape originally specced.

---

### 🟢 "Edit on GitHub" links

- **Priority**: MEDIUM — unchanged, and only partially done. `/forms/button`
  has a working "Open in GitHub" link to `CyButton.razor`'s source; no other
  component page has one yet. Still no library component needed — this is
  purely a matter of adding the same link, with the right per-component path,
  to the remaining ~28 pages.

---

## Resolved Gaps

| Item | Resolved in | Notes |
|---|---|---|
| `CyButton` — `Href`/anchor rendering | `0.1.0-preview.7` | Library. `Home.razor`'s hero CTAs not yet migrated to use it — see note above. |
| `CyButton` — `Loading` state | `0.1.0-preview.7` | Library. |
| `CyButton` — `Variant`/`Size`/`Disabled`/`Type`/`OnClick` | `0.1.0-preview.7` | Library. Not originally listed as a separate item, but was previously a `ChildContent`-only wrapper. |
| `CySearchModal` (feature, not as a reusable component) | `0.1.0-preview.7` | Demo-only (`DemoSearchModal.razor`). Global Ctrl+K shortcut needed a small JS interop bridge (`wwwroot/js/search.js`). |
| `CySidebar` mobile off-canvas drawer (feature, not as a `CySidebar` mode) | `0.1.0-preview.7` | Demo-only (`MainLayout.razor` + `DemoSidebar.razor`). |
| Prev/next component navigation | `0.1.0-preview.7` | Demo-only (`DemoPageNav.razor`), as originally specced. |
| `CyTabs` ARIA semantics (not the component itself) | `0.1.0-preview.7` | Demo-only; retrofitted across all 18 tabbed pages. Component itself still doesn't exist — see updated entry above. |
| `CyNavigation`/`CyNavigationItem` active link state | Already correct when checked | Turned out not to be broken; note may always have been inaccurate. |
| `CyIcon` — `StrokeWidth`/`Color` parameters | `0.1.0-preview.8` | Library. Not originally listed as a gap, but a real addition. |
| `CyBrandLogo` illegible on dark headers | `0.1.0-preview.8` | Library bug fix, found while building `CyHeader`. |
| `CyHeader` (new component) | `0.1.0-preview.7` | Library. Not originally listed here as a missing component, but genuinely didn't exist before. |
| `CyFooter.Background` | `0.1.0-preview.7` | Library. Different gap than the "typed link group slots" item below, which is still open. |

---

*Last updated: 2026-09-05, following the `0.1.0-preview.7`/`0.1.0-preview.8`
releases. Every status above was re-verified against the actual current code
(not assumed from memory) before being marked resolved, downgraded, or left
open.*
