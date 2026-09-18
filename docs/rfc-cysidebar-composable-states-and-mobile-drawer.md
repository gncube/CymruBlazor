## Summary

`CySidebar` already does more out of the box than a first read suggests — its
default styling is correctly theme-reactive, `CollapseMode.Compact` already
ships the icon-over-label rail, and the toggle button is already borderless.
But three real gaps in the current API forced a consuming team
(`LabReporting.Dashboard`) to abandon `CySidebar` entirely and hand-roll an
`AppSidebar` from scratch, reproducing — and, in their first pass, getting
wrong — work this component should provide for free. This issue proposes
closing those three gaps.

## Evidence

1. **No way to cycle through multiple collapsed appearances from one
   control.** `SidebarCollapseMode` (`src/CymruBlazor/Enums/SidebarCollapseMode.cs`)
   is a single value fixed for the sidebar's lifetime; `Collapsed` is a plain
   bool. There's no supported way to expose "click once for Compact, again
   for IconOnly, again for Hidden" from one button. Our own
   `samples/Dashboard/Layout/MainLayout.razor` sidesteps this by exposing
   `CollapseMode` as a design-time `<select>` dropdown rather than a runtime
   control — it doesn't actually demonstrate cycling. `LabReporting`, which
   needed exactly that cycling UX, built an entirely separate
   `SidebarDisplayState` enum and state machine to get it.

2. **`CollapseMode.Hidden` has no way back in.** When `Collapsed=true` and
   `CollapseMode=Hidden`, `CySidebar.razor`'s `ShowHeader`/toggle-button logic
   still renders the header (brand + the expand chevron) inside the
   `<aside>`, but `.cy-sidebar--collapsed` (`wwwroot/css/layout/grid.css`)
   clips that same element to `width: 0; overflow: hidden`. The sidebar's own
   built-in toggle becomes invisible and unreachable the moment it's needed
   most. Nothing marks the clipped content `aria-hidden`/`inert` either, so
   it also sits in the tab order while invisible. `LabReporting` discovered
   this the hard way and had to add a second, external toggle button in
   their own app header, bound via `@ref` + a public method, just to have a
   way to re-open a `Hidden` sidebar.

3. **No first-class mobile off-canvas drawer.** `CySidebar`'s only responsive
   behaviour (`@media (max-width: 63.99rem)` in `grid.css`) switches it to a
   full-width stacked block — not an overlay drawer with a backdrop.
   Tellingly, this exact drawer pattern (an `IsOpen` bool, a CSS
   open/closed class, a click-to-close backdrop `<div>`) already exists once
   in this repo, hand-built for the docs site
   (`src/CymruBlazor.Demo/Layout/DemoSidebar.razor` +
   `.cb-shell-backdrop` in `MainLayout.razor`), and is called out in
   `plan/known-issues-and-backlog.md` as never having been generalised.
   `LabReporting` then reinvented the identical pattern a third time
   (`MobileOpen`/`MobileOpenChanged` + a backdrop `<div>`) because
   `CySidebar` doesn't offer it. Three independent implementations of the
   same ~40 lines of state/markup is a strong signal this belongs in the
   component, not in every consumer.

**Minor naming nit:** `SidebarCollapseMode.Disabled` is the only member that
describes a *capability* ("collapsing is turned off") rather than an
*appearance* (`Compact`/`IconOnly`/`Hidden` all describe what "collapsed"
looks like). It reads at a glance like "this sidebar is disabled/inert,"
which it isn't — it's the *opposite*, a permanently-expanded sidebar.

## Proposed API

```csharp
// New: an ordered, composable state instead of a fixed CollapseMode + bool.
public enum SidebarState { Expanded, Compact, IconOnly, Hidden }

public partial class CySidebar : CyLayoutComponentBase
{
    // Replaces Collapsed/CollapseMode as the primary API. Consumers list
    // only the states they want to be reachable, in cycle order.
    [Parameter] public IReadOnlyList<SidebarState> States { get; set; }
        = [SidebarState.Expanded, SidebarState.Compact];

    [Parameter] public SidebarState State { get; set; } = SidebarState.Expanded;
    [Parameter] public EventCallback<SidebarState> StateChanged { get; set; }

    public Task CycleNextAsync();     // advances through States, wraps around
    public Task ExpandAsync();        // -> States[0], typically Expanded
    public Task SetStateAsync(SidebarState state);

    // First-class mobile drawer, modelled on DemoSidebar + LabReporting's
    // own workaround. No effect above MobileBreakpoint.
    [Parameter] public bool MobileOpen { get; set; }
    [Parameter] public EventCallback<bool> MobileOpenChanged { get; set; }
    [Parameter] public string MobileBreakpoint { get; set; } = "47.99rem";
    // Renders its own backdrop + close (X) button when true.
    [Parameter] public bool ShowMobileBackdrop { get; set; } = true;

    // Back-compat: existing Collapsed/CollapseMode keep working, mapped
    // onto States=[Expanded, CollapseMode] / State internally, marked
    // [Obsolete] pointing at States/State.
    [Obsolete("Use States and State instead.")]
    [Parameter] public bool Collapsed { get; set; }
    [Obsolete("Use States and State instead.")]
    [Parameter] public SidebarCollapseMode CollapseMode { get; set; }
}
```

For gap 2 (Hidden trap), independent of the API above: render a small
persistent "reveal" handle positioned just outside the sidebar's own clipped
boundary whenever the effective state is `Hidden`/collapsed-to-zero, so
there's always a visible, focusable way back in without requiring the
consumer to wire up an external button — and mark the clipped header
`aria-hidden="true"` (or use `inert`) so it drops out of the tab order while
invisible, matching the care already taken elsewhere in this file for
`IconOnly` label visibility (clipped visually, not `aria-hidden`, which is
correct there but is the wrong choice for a fully hidden container).

`SidebarCollapseMode` can stay for one deprecation cycle as the back-compat
path described above rather than being removed outright; only the
`Disabled` member needs a rename (`NonCollapsible`, forwarded via
`[Obsolete]`).

## Backward compatibility

- `Collapsed`/`CollapseMode`/`CollapsedChanged` keep working unchanged;
  they become a thin compatibility layer over `States`/`State`.
- CSS classes `cy-sidebar--compact`/`--icon-only`/`--collapsed` keep their
  current meaning; `SidebarState` maps onto the same class names so no
  consumer CSS needs to change.
- No existing consumer (including our own samples) breaks without opting in
  to `States`/`MobileOpen`.

## Alternatives considered

- **Leave `CySidebar` as-is and only document the workaround.** Rejected —
  the pattern has now been built three separate times in this codebase's own
  history (Demo, `samples/Dashboard`'s absence of it, `LabReporting`); that's
  the definition of something the component should own.
- **A new wrapper component (`CyNavigationRail`) instead of extending
  `CySidebar`.** Considered, but `CySidebar` already owns 90% of the needed
  surface (positioning, width tokens, theming, the toggle button) — adding a
  second component would fragment rather than consolidate.

## Non-goals

- Changing `SidebarPosition`/`SidebarWidth` semantics.
- Building a full navigation-item component set (`cy-sidebar__item`/`__label`
  conventions stay as plain CSS conventions, not new sub-components) — out
  of scope for this issue.
