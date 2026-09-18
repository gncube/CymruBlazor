# CySidebar: Composable States, a Working Hidden-State Escape, and a Mobile Drawer

**Status:** Proposed
**Owner:** Gerald Ncube
**Created:** 2026-09-17
**Target:** Next minor (adds APIs; no breaking change to `Collapsed`/`CollapseMode`)
**Depends on:** —

## Objective

Close the three gaps that forced a real consumer (`LabReporting.Dashboard`)
to abandon `CySidebar` and hand-build a replacement: no supported way to
cycle through more than one collapsed appearance from a single control, a
`Hidden` collapse mode whose own toggle becomes unreachable once triggered,
and no first-class mobile off-canvas drawer. See
`rfc-cysidebar-composable-states-and-mobile-drawer.md` for the evidence and
proposed public API this plan implements.

## Scope

- `src/CymruBlazor/Components/Layout/CySidebar.razor` and `.razor.cs`
- `src/CymruBlazor/Enums/SidebarCollapseMode.cs` (new `SidebarState` enum
  alongside it; `Disabled` rename)
- `src/CymruBlazor/wwwroot/css/layout/grid.css` (`.cy-sidebar` rules)
- `tests/CymruBlazor.Tests/Components/CySidebarTests.cs`
- `src/CymruBlazor.Demo/Pages/Components/Layout/CySidebarPage.razor` (docs)
- `samples/Dashboard/Layout/MainLayout.razor` (update to demonstrate the new
  API instead of the `<select>` workaround)

Out of scope: `SidebarPosition`/`SidebarWidth`, a new navigation-item
component, and migrating `LabReporting.Dashboard`'s own `AppSidebar` back
onto `CySidebar` (that's a follow-up for that repo once this ships).

## Decisions

- Add `SidebarState { Expanded, Compact, IconOnly, Hidden }` and a
  `States`/`State`/`StateChanged` API rather than extending
  `SidebarCollapseMode` in place — `CollapseMode` describes an *appearance
  while collapsed*, not a full state machine including "not collapsed";
  overloading it further would make the existing member names lie.
- Keep `Collapsed`/`CollapseMode`/`CollapsedChanged` working as a
  back-compat shim (computed from `States`/`State`) rather than a breaking
  removal, since `samples/Dashboard` and any external consumers already
  depend on them.
- Fix the `Hidden`-state trap with a visible reveal handle rendered outside
  the clipped `<aside>`, not by leaving the header rendered-but-invisible —
  a handle is discoverable; documentation alone (telling consumers to wire
  an external button, as `LabReporting` had to) is not.
- Promote the mobile drawer pattern from `CymruBlazor.Demo/Layout/DemoSidebar.razor`
  into `CySidebar` itself (as `MobileOpen`/backdrop/breakpoint), rather than
  leaving `DemoSidebar` as the only working example — once `CySidebar`
  supports it natively, `DemoSidebar` can optionally be rebuilt on top of it
  in a follow-up, but that rebuild isn't required for this plan.
- Rename `SidebarCollapseMode.Disabled` → `NonCollapsible`, keeping
  `Disabled` as an `[Obsolete]` alias for one release rather than removing
  it outright.

## Implementation

### Phase 1 — `SidebarState` and the back-compat shim

- Add `Enums/SidebarState.cs` (`Expanded`, `Compact`, `IconOnly`, `Hidden`).
- Add `States`/`State`/`StateChanged` parameters to `CySidebar.razor.cs`.
- Reimplement `EffectiveCollapsed`/`CollapseModeAttribute`/`BuildCssClass`
  in terms of `State` instead of `Collapsed`/`CollapseMode`.
- Make `Collapsed`/`CollapseMode`/`CollapsedChanged` a thin compatibility
  layer: when set, they compute an equivalent `States`/`State` pair
  (`States = [Expanded, CollapseMode]`, `State = Collapsed ? CollapseMode : Expanded`)
  so existing markup (`samples/Dashboard`) keeps working unchanged. Mark
  them `[Obsolete("Use States and State instead.", error: false)]`.

### Phase 2 — Cycling

- `CycleNextAsync()`: advances `State` to the next entry in `States`,
  wrapping to the first after the last.
- `ExpandAsync()`: sets `State` to `States[0]` (normally `Expanded`).
- `SetStateAsync(SidebarState)`: sets `State` directly if it's a member of
  `States`; no-ops (or throws in DEBUG, TBD during review) otherwise.
- Update the sidebar's own header toggle button to call `CycleNextAsync()`
  when `States.Count > 1`, and keep the current expand/collapse
  binary-toggle behaviour when `States.Count == 2` (so existing two-state
  consumers see no behaviour change, only new capability when they opt in
  to more states).

### Phase 3 — Fix the `Hidden` trap

- In `CySidebar.razor`, when the effective `State` is `Hidden`: mark the
  existing header `aria-hidden="true"` (children get `tabindex="-1"` via a
  small `CyInteractiveComponentBase`-style helper, or `inert` where
  supported — confirm current browser-support bar for `inert` against this
  repo's supported-browsers list before choosing between the two).
- Render a new small reveal handle (a `<button>` with the same
  `chevron`/expand semantics as the normal toggle) positioned just outside
  the sidebar's clipped boundary — e.g. `position: absolute` against a
  positioned ancestor, `inset-inline-start: 100%` from the zero-width
  `<aside>` — visible and focusable whenever `State == Hidden`. Calls
  `ExpandAsync()`.
- Add the corresponding CSS to `grid.css` alongside the existing
  `.cy-sidebar--collapsed` rules.

### Phase 4 — Mobile drawer

- Add `MobileOpen`/`MobileOpenChanged`/`MobileBreakpoint`/`ShowMobileBackdrop`
  parameters.
- When `MobileOpen` and below `MobileBreakpoint`: render the existing
  `<aside>` as a fixed-position off-canvas panel (`transform: translateX`)
  and, if `ShowMobileBackdrop`, an internal backdrop `<div>` that closes on
  click — same shape as `CymruBlazor.Demo`'s `.cb-shell-backdrop` /
  `DemoSidebar.IsOpen`, and as `LabReporting.Dashboard`'s independently
  built `MobileOpen`/backdrop.
- Render a close (`X`) button in the header when `MobileOpen` is true,
  matching the existing toggle button's visual language, and ensure it
  — not the desktop cycle toggle — is what's shown in that state (mirrors
  the mutual-exclusivity `LabReporting.Dashboard` had to build by hand).
- Add the breakpoint media query to `grid.css`, parameterised only to the
  extent CSS custom properties allow (`MobileBreakpoint` sets a CSS
  variable consumed by the media query's `@container` equivalent, or falls
  back to the default `47.99rem` if browser support for range media
  queries as variables isn't there — confirm during implementation).

### Phase 5 — Naming cleanup

- Add `SidebarCollapseMode.NonCollapsible`; keep `Disabled` as
  `[Obsolete("Use NonCollapsible.", error: false)]` mapped to the same
  behaviour.

### Phase 6 — Tests, demo, docs

- Extend `tests/CymruBlazor.Tests/Components/CySidebarTests.cs`: cycling
  through an arbitrary `States` list (including a 3- and 4-entry list),
  the `Hidden` reveal handle rendering/being focusable/calling
  `ExpandAsync`, `aria-hidden` on the clipped header, `MobileOpen`
  rendering the backdrop and close button and excluding the desktop
  toggle, and that `Collapsed`/`CollapseMode` still work unchanged
  (regression coverage for the back-compat shim).
- Update `CySidebarPage.razor` in the demo to show the new `States`
  cycling API and the mobile drawer, replacing/augmenting the current
  Compact/IconOnly/Disabled/Hidden static examples.
- Update `samples/Dashboard/Layout/MainLayout.razor` to use
  `States="[Expanded, Compact, IconOnly, Hidden]"` on the single toggle
  button instead of the `<select>` dropdown, so the sample actually
  demonstrates the cycling UX rather than a design-time switch.
- Add a `CHANGELOG.md` entry under `[Unreleased]`.

## Verification

- `dotnet test` — all of `CymruBlazor.Tests` (including the new
  `CySidebarTests` cases) and `CymruBlazor.AccessibilityTests` green.
- Manual: cycle a 4-state sidebar with keyboard only, confirm the `Hidden`
  reveal handle is reachable by Tab and announced correctly by a screen
  reader; confirm `Collapsed`/`CollapseMode` consumers (the `Dashboard`
  sample, pre-migration) still render and behave identically.
- Manual, real mobile viewport (not devtools emulation alone, per the
  caution already logged in `plan/known-issues-and-backlog.md` for the
  Demo's own drawer): open/close the drawer, confirm the backdrop closes
  it, confirm it doesn't appear above `MobileBreakpoint`.
- `dotnet build` the package project and confirm `BundleCss.targets` still
  produces a valid `cymrublazor.css` including the new `grid.css` rules.

## Acceptance Criteria

- [ ] `SidebarState` + `States`/`State`/`StateChanged`/`CycleNextAsync`/
      `ExpandAsync`/`SetStateAsync` implemented and tested.
- [ ] `Collapsed`/`CollapseMode`/`CollapsedChanged` still work, unchanged
      in behaviour, with `[Obsolete]` guidance pointing at the new API.
- [ ] A sidebar in `Hidden` state has a visible, focusable, keyboard- and
      screen-reader-accessible way back to `Expanded`, and its clipped
      header is excluded from the tab order.
- [ ] `MobileOpen`/`MobileOpenChanged`/`MobileBreakpoint`/`ShowMobileBackdrop`
      implemented, with backdrop and close-button behaviour matching the
      pattern already proven in `DemoSidebar`/`LabReporting.Dashboard`.
- [ ] `SidebarCollapseMode.NonCollapsible` added; `Disabled` still works,
      marked obsolete.
- [ ] `samples/Dashboard` demonstrates cycling via the toggle button, not
      the `<select>` workaround.
- [ ] `CHANGELOG.md` updated.

## Risks

- **`inert` browser support** may not meet this repo's support bar —
  confirm before Phase 3 lands; `aria-hidden` + manual `tabindex="-1"` is
  the fallback and is already this plan's documented alternative.
- **CSS variable range media queries** for a parameterised
  `MobileBreakpoint` may not be broadly supported; falling back to a fixed
  default breakpoint is an acceptable de-scope for Phase 4 if so.
- **API surface growth on an already-large component** — `CySidebar` picks
  up seven new parameters. If review finds this makes the component hard
  to reason about, the mobile-drawer piece (Phase 4) is the most separable
  and could ship as a follow-up `CySidebarMobileDrawer` in a later plan
  instead, per the "alternatives considered" note in the RFC.
- **Migrating `LabReporting.Dashboard`'s hand-rolled `AppSidebar` back onto
  this API** is explicitly out of scope here and will surface its own
  gaps once attempted — treat this plan's acceptance criteria as
  "sufficient for a fresh consumer," not as pre-verified against that
  specific app.

## Out of Scope

- Migrating `LabReporting.Dashboard` itself onto the new API.
- Rebuilding `CymruBlazor.Demo/Layout/DemoSidebar.razor` on top of
  `CySidebar` (optional future follow-up once this ships).
- Any change to `SidebarPosition`, `SidebarWidth`, or the
  `cy-sidebar__item`/`cy-sidebar__label` CSS conventions.

## Completion

**Status:** Not started
**Completed:** —
