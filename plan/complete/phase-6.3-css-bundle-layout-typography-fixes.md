# Phase 6.3 - CSS Bundle, Layout & Typography Regression Fixes

## Objective

Phase 6.1/6.2 shipped the Demo application shell (header, sidebar, footer,
home page) against the Phase 4 design-token/CSS architecture, but the two
never got fully wired together. This phase closes that gap:

- The Demo sidebar layout renders correctly at all breakpoints.
- Base typography (headings, body copy, links, lists) is styled out of the
  box, matching the PRD's "add one CSS reference" developer experience.
- The header/footer/hero visually align with the NHS Wales / DHCW component
  library reference (blue `#005eb8` + navy `#1b365d`).

Branch: `feature/phase-6.3-css-bundle-layout-typography-fixes`

---

## Root Cause Analysis

### 1. Sidebar layout was not showing properly

Two independent bugs compounded:

**a) The layout utility stylesheets targeted the wrong class names.**
`wwwroot/css/layout/containers.css` and `layout/grid.css` defined classes
like `.cymru-container`, `.cymru-stack`, `.cymru-sidebar` - but the actual
components (`CyContainer`, `CyStack`, `CySidebar`, `CyCluster`, `CyGrid`)
generate `.cy-container`, `.cy-stack`, `.cy-sidebar`, `.cy-cluster`,
`.cy-grid` via `CssBuilder` in their `*.razor.cs` files (see
`Components/Layout/CyStack.razor.cs`, `BaseCssClass => "cy-stack"`). No CSS
file anywhere defined the classes the components actually render, so every
layout primitive rendered as an unstyled `<div>`/`<aside>`.

**b) The dev-mode entry point never imported the layout stylesheets.**
`wwwroot/css/cymrublazor.css` (the file referenced directly by
`_content/CymruBlazor/css/cymrublazor.css` during `dotnet run`) had a
"Future imports" comment listing `layout/grid.css` and `layout/containers.css`
as *not yet wired up*. The Release/Publish path (`build/BundleCss.props` +
`build/BundleCss.targets`, which runs `CymruBlazor.CssBundler` before every
build) did already list the layout files - but was missing
`base/typography.css` entirely, and both paths were undermined by bug (a).

**c) Four layout components' CSS-isolation files were dead code.**
`CyContainer.razor.css`, `CyStack.razor.css`, `CyCluster.razor.css`, and
`CyGrid.razor.css` all contained a single rule using the `:host` selector
(e.g. `:host { display: contents; }`). Blazor CSS isolation does not
support `:host` - that is a Shadow DOM concept, and Blazor scopes plain
element/class selectors instead. These rules silently matched nothing.
(`CyCenter.razor.css` was written correctly, using a real `.cy-center`
class selector, and worked as intended - which is why it wasn't reported
as broken.)

**d) `<main>` had no flex sizing next to the sidebar.**
`MainLayout.razor` puts `<CySidebar>` and `<main class="cb-demo-main">`
inside a horizontal `<CyStack>`. Nothing gave `.cb-demo-main` `flex: 1`, so
once (a)-(c) are fixed the main column would still not claim the remaining
row width.

### 2. Typography styling was missing

`wwwroot/css/tokens/typography.css` only declares CSS custom properties
(`--cymru-font-size-heading-1`, `--cymru-line-height-body`, etc.). No
stylesheet ever *applied* those tokens to `h1`-`h6`, `body`, `p`, `a`, or
list elements - there was no `base/typography.css`, only a comment
referencing one that didn't exist. Every page fell back to unstyled
browser defaults for text.

### 3. NHS Wales colours didn't match the DHCW reference

- `--cymru-blue-700: #005eb8` already matches the DHCW header colour
  exactly, so the primary brand colour was correct.
- The reference site (`site/header-footer.html`) pairs that blue with a
  dark navy, `#1b365d`, used for the header's search affordance and other
  structural chrome. CymruBlazor had no equivalent token, so the header
  border and footer fell back to a plain grey/light palette with no navy
  accent at all, reading as flatter and less "on brand" than the reference.
- Separately (and this is what's most visible in the screenshot): the
  "Browse components" hero button uses the shared `.cb-demo-btn--secondary`
  class, which is styled for *light* backgrounds (`color:
  var(--cymru-color-primary)` - blue text). Reused unmodified on the hero's
  blue gradient, the text is blue-on-blue and effectively invisible.

---

## Fixes Applied

| # | File | Change |
|---|------|--------|
| 1 | `wwwroot/css/layout/containers.css` | Rewritten to target `.cy-container` and its `--sm/--md/--lg/--xl/--fluid/--no-padding` modifiers (previously `.cymru-container`, unused by any component). |
| 2 | `wwwroot/css/layout/grid.css` | Rewritten to back `.cy-stack`, `.cy-cluster`, `.cy-grid`, `.cy-sidebar` and their real modifier/gap/align/justify classes, generated 1:1 from each component's `*.razor.cs`. Legacy `.cymru-*` utility classes were dropped since nothing renders them. |
| 3 | `wwwroot/css/base/typography.css` | **New file.** Applies the existing typography tokens to `html`, `body`, `h1`-`h6`, `p`, `a`, lists, `code`, etc. |
| 4 | `wwwroot/css/cymrublazor.css` | Added `@import` for `base/typography.css`, `layout/containers.css`, `layout/grid.css` (dev-mode entry point). |
| 5 | `build/BundleCss.props` | Added `base/typography.css` and `utilities/screen-reader.css` (existed on disk, was never bundled) to the authoritative Release/Publish file list. |
| 6 | `Components/Layout/CyContainer.razor.css`, `CyStack.razor.css`, `CyCluster.razor.css`, `CyGrid.razor.css` | Removed the non-functional `:host` rules and documented why, so the pattern isn't copy-pasted into future components. |
| 7 | `CymruBlazor.Demo/wwwroot/css/demo.css` | Added `.cb-demo-main { flex: 1 1 0%; min-width: 0; padding: var(--cymru-space-4); }` so the content column fills the remaining width next to the sidebar. |
| 8 | `wwwroot/css/tokens/colours.css` | Added `--cymru-navy-900: #1b365d` (primitive) and `--cymru-color-accent` / `--cymru-color-accent-text` (semantic), matching the DHCW reference's navy accent. |
| 9 | `CymruBlazor.Demo/Layout/DemoHeader.razor.css` | Header bottom border now uses `--cymru-color-accent` (navy) instead of a plain darker blue. |
| 10 | `CymruBlazor.Demo/Layout/DemoFooter.razor.css` | Footer restyled as a navy band (`--cymru-color-accent` background, `--cymru-color-accent-text` foreground) instead of light grey, matching the NHS Wales pattern of pairing a blue header with a darker footer. |
| 11 | `CymruBlazor.Demo/Pages/Home.razor.css` | Scoped an inverse treatment to `.cb-home__hero .cb-demo-btn--secondary` so "Browse components" is readable on the blue gradient, without changing the shared button's light-background appearance used elsewhere (`FocusTrapPage`, `CyLiveRegionPage`). |

---

## Verification Performed

- Brace-balance / structural check on every edited CSS file.
- Reproduced the `CymruBlazor.CssBundler` concatenation logic against the
  updated `BundleCss.props` file list to confirm the Release/Publish bundle
  resolves all 17 referenced files and produces balanced, syntactically
  intact CSS (no dotnet SDK available in this environment to run the real
  `dotnet build`/`bunit` suite - see **Remaining Work**).

## Risks

- The navy footer/header-accent treatment is a best-effort match against
  the reference's documented colours (`#005eb8` blue, `#1b365d` navy); the
  reference site's footer markup itself wasn't fully renderable from the
  fetched content, so the exact footer structure (single band vs. two
  bands) hasn't been reproduced 1:1 - only the colour pairing.
- `layout/grid.css` and `layout/containers.css` class contracts are now
  tightly coupled to the `BaseCssClass` values in each `*.razor.cs` file.
  Any future rename of a layout component's base class must update both.

## Remaining Work

- Run `dotnet build` / `dotnet test` (bUnit + ApprovalTests +
  AccessibilityTests) in a real .NET 9 environment to confirm no
  regressions - not possible in this sandbox (no .NET SDK / NuGet network
  access here).
- Visually diff the Demo header/footer against
  `dhcw-digital-health-and-care-wales.github.io/nhsw-component-library/site/header-footer.html`
  once buildable, and consider adding the reference's secondary nav row
  and mobile bottom navigation pattern as a follow-up phase.
- Consider adding a `tests/CymruBlazor.ApprovalTests` snapshot for
  `MainLayout` so a sidebar/main regression like this fails CI next time.

## Recommended Next Iteration

1. Build and visually verify in a real environment.
2. Add an approval/snapshot test asserting `CySidebar` + `CyStack` render
   with non-empty computed layout classes, to catch "class name drift"
   between a component's code-behind and its backing CSS automatically.
3. Extend `base/typography.css` coverage to blockquotes, `hr`, and form
   labels as those components are built out.
