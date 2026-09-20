---
status: Accepted
---

# ADR-0001: Build CyDialog on the native `<dialog>` element

## Status

Accepted (v1.3.0)

## Context

The library needs a modal dialog, and `CyFocusTrap` (1.2.x) did not contain
focus at all. The demo's search modal and the sidebar drawer both hand-rolled
a backdrop plus `role="dialog"`, with no inert background and no reliable
Escape or focus return.

A correct modal needs: focus moved in, `Tab` contained, the rest of the page
inert (for keyboard and assistive technology), `Escape` to close, focus
returned on close, and rendering above every stacking context. Reimplementing
all of that in JavaScript is large and easy to get subtly wrong.

## Decision

`CyDialog` renders a native `<dialog>` and opens it with `showModal()`. The
browser then provides the inert background, `Tab` containment, `Escape`
(`cancel` event), the top layer and the `::backdrop`. A small ES module
(`wwwroot/js/cymru-overlay.js`), imported on demand by the component itself
(`import("./_content/CymruBlazor/js/cymru-overlay.js")`, so consumers add no
`<script>` tag), does only what Blazor cannot:

- calls `showModal()` and reports every close back to .NET (`OnNativeClosed`),
- makes non-dismissible dialogs ignore `Escape` (`cancel` is cancelled),
- optional backdrop-click close,
- returns focus to the opener explicitly, because native restoration is not
  reliable everywhere (Safari, removed elements).

`CyFocusTrap` is *not* stretched into a modal. It stays a focus-only
primitive (used by the navigation menu and, via `IFocusManager.TrapAsync`, the
sidebar drawer) with real containment provided by the same module.
`AddCymruBlazor()` now registers `JsFocusManager` as the default
`IFocusManager`.

Body content is rendered only while the dialog is open, so closed dialogs cost
nothing and cannot leak duplicate ids into the page.

### Browser support bar

`HTMLDialogElement.showModal()` is supported in Chrome/Edge 37+, Firefox 98+
and Safari 15.4+ (all evergreen browsers since March 2022). That matches the
support bar of the rest of the library (CSS `@layer`, `:has()`, `dvh` units
already require it or better). Browsers without `<dialog>` are not supported;
the component does not polyfill.

## Consequences

- Very little JavaScript (~250 lines including the trap and tooltip helpers),
  covered by real-browser tests (`OverlayModuleBrowserTests`).
- Behaviour follows the platform and improves as browsers do.
- Known platform quirks: a browser may close a dialog on a repeated `Escape`
  even when `cancel` was prevented (Chromium's anti-abuse close watcher), so
  `Dismissible="false"` cannot be a hard guarantee; the docs say so.
- `::backdrop` does not reliably inherit custom properties, so the backdrop
  colour is a literal.
- `html:has(dialog[open])` is used to stop background scrolling.

## Alternatives Considered

- **Stretch `CyFocusTrap` into a modal** (div + `role="dialog"` + `aria-modal`):
  needs our own inert handling, Escape, top-layer/z-index management and
  focus restoration; more code, more bugs. Rejected.
- **A third-party dialog/focus-trap library:** adds a dependency and a script
  the consumer must load; the native element already does the hard parts.
- **Render a closed `<dialog>` with all content:** simpler, but duplicates ids
  and keeps hidden form controls in the DOM. Rejected.

## References

- WAI-ARIA Authoring Practices: Dialog (Modal) pattern
- WCAG 2.4.3 Focus Order, 2.1.2 No Keyboard Trap
- `plan/plan-post-1.2-roadmap.md` D2, D3
