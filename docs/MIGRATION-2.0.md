# Migrating to CymruBlazor 2.0.0

CymruBlazor follows [Semantic Versioning](https://semver.org/): members
scheduled for removal are marked `[Obsolete]` first, in a 1.x release, so
existing code keeps compiling (with a warning) until 2.0.0. This document
lists every member currently marked `[Obsolete]` in the library, why, and
what to use instead.

This is a living document: it is updated as new `[Obsolete]` members are
added in 1.x releases, and each entry is removed once the member itself is
removed in 2.0.0.

> **Status:** all members below are warnings only (`error: false` or no
> `error` argument), so they do not fail your build today. They will be
> deleted in 2.0.0.

## `CySidebar`

The following `CySidebar` members are superseded by the `States` /
`State` parameter pair, which support cycling through more than the two
states the legacy API assumed (`Expanded` / `Collapsed`):

| Obsolete member | Replacement |
| --- | --- |
| `CySidebar.Collapsed` (`bool`) | `CySidebar.State` (`SidebarState`) - compare against `SidebarState.Expanded` instead of `!Collapsed` |
| `CySidebar.CollapsedChanged` (`EventCallback<bool>`) | `CySidebar.StateChanged` (`EventCallback<SidebarState>`) |
| `CySidebar.CollapseMode` (`SidebarCollapseMode`) | `CySidebar.States` (`IReadOnlyList<SidebarState>`) - configure which states the sidebar cycles through, e.g. `[SidebarState.Expanded, SidebarState.IconOnly]`, instead of describing one fixed collapsed appearance |

**Before:**

```razor
<CySidebar
    Collapsed="@_collapsed"
    CollapsedChanged="@(v => _collapsed = v)"
    CollapseMode="SidebarCollapseMode.Compact" />
```

**After:**

```razor
<CySidebar
    States="@(new[] { SidebarState.Expanded, SidebarState.Compact })"
    State="@_state"
    StateChanged="@(v => _state = v)" />
```

### `SidebarCollapseMode.Disabled`

`SidebarCollapseMode.Disabled` is an obsolete alias for
`SidebarCollapseMode.NonCollapsible`. Replace `Disabled` with
`NonCollapsible` (same underlying value; renamed for clarity). This applies
whether or not you have already migrated off `CollapseMode` above.

## Unimplemented keyboard-navigation primitives (Roadmap D5)

Nothing in CymruBlazor has ever registered or called these types - they
predate the components that would have used them, and no component consumes
them today. There is no in-place replacement; if `CyDropdown`/a menu
component is ever built, keyboard navigation for it should be designed
against that component's actual requirements rather than by resurrecting
this API.

- `CymruBlazor.Accessibility.Focus.IKeyboardNavigationService`
- `CymruBlazor.Accessibility.Focus.KeyboardNavigationService`
- `CymruBlazor.Accessibility.Focus.KeyboardNavigationResult`
- `CymruBlazor.Accessibility.Focus.KeyboardNavigationOptions`
- `CymruBlazor.Accessibility.Focus.FocusNavigationMode`

If you built something against these types directly, there is no
replacement API in CymruBlazor; you will need to keep your own copy of the
logic you depend on, or open an issue describing your use case.

## Unimplemented contracts and enums

No shipped component implements or returns these; they were speculative
groundwork for variants/icon placement that were never built the way these
types anticipated. There is no replacement.

- `CymruBlazor.Contracts.IHasIcon`
- `CymruBlazor.Contracts.IHasVariant`
- `CymruBlazor.Enums.ComponentVariant`
- `CymruBlazor.Enums.IconPosition`

If a future component needs an icon-position or variant contract, it is
expected to get a fresh, purpose-built type rather than reusing these.

## How to check what your project still uses

Build with warnings visible (the default) - every obsolete member above
produces a `CS0618` warning at each call site. To find them all at once:

```bash
dotnet build CymruBlazor.slnx -warnaserror:CS0618
```

This turns every remaining obsolete usage into a build error so none are
missed before you upgrade to 2.0.0.
