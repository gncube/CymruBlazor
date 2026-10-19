# CymruBlazor 2.0.0

A major release. It executes the breaking changes and obsolete API removals scheduled across the 1.x series to modernise component contracts, eliminate dead code, and streamline the public API surface.

Upgrading from 1.x? See the complete [v2.0.0 Migration Guide](docs/MIGRATION-2.0.md) for step-by-step instructions and code replacements.

## Highlights

### Major Release Milestone: v2.0.0

CymruBlazor 2.0.0 establishes a clean, stable baseline for the 2.x lifecycle. Obsolete APIs that were deprecated during the 1.x roadmap have been removed in accordance with [Semantic Versioning](https://semver.org/), ensuring a leaner library footprint and unambiguous component APIs.

### `CySidebar` Modernisation

The legacy two-state boolean parameters on `CySidebar` have been replaced by a modern multi-state model supporting cycling through arbitrary sequences of layout states (such as `Expanded`, `Compact`, and `Hidden`):

- **Removed parameters**: `Collapsed` (`bool`), `CollapsedChanged` (`EventCallback<bool>`), and `CollapseMode` (`SidebarCollapseMode`).
- **Replacements**: Use `State` (`SidebarState`), `StateChanged` (`EventCallback<SidebarState>`), and `States` (`IReadOnlyList<SidebarState>`).
- **`SidebarCollapseMode.Disabled` removed**: Replaced with `SidebarCollapseMode.NonCollapsible` for clarity.
- **Styling and compatibility**: Internal legacy parameter synchronisation and compatibility bridging have been cleaned up. The `<aside>` element retains the `data-collapse-mode` attribute for stylesheet compatibility and adds `data-state`.

```razor
<!-- Before (1.x) -->
<CySidebar
    Collapsed="@_collapsed"
    CollapsedChanged="@(v => _collapsed = v)"
    CollapseMode="SidebarCollapseMode.Compact" />

<!-- After (2.0.0) -->
<CySidebar
    States="@(new[] { SidebarState.Expanded, SidebarState.Compact })"
    State="@_state"
    StateChanged="@(v => _state = v)" />
```

### Removal of Speculative Unused Contracts

Early prototype contracts and enums that were never implemented by shipped components have been removed to prevent developer confusion:

- `CymruBlazor.Contracts.IHasIcon`
- `CymruBlazor.Contracts.IHasVariant`
- `CymruBlazor.Enums.ComponentVariant`
- `CymruBlazor.Enums.IconPosition`

Any future variant or icon positioning features will use purpose-built, component-specific designs rather than generic contracts.

### Removal of Unimplemented Keyboard Navigation Primitives (Roadmap D5)

Unused keyboard navigation primitives in `CymruBlazor.Accessibility.Focus` that predated component requirements and were never wired up to any shipped component have been removed:

- `IKeyboardNavigationService`
- `KeyboardNavigationService`
- `KeyboardNavigationResult`
- `KeyboardNavigationOptions`
- `FocusNavigationMode`

Focus management continues to be provided through `IFocusManager`, `JsFocusManager`, and `FocusTrap`.

### Strict Compiler Hygiene and Package Validation

- **Zero compiler warnings**: The library and test suite build cleanly with zero warnings, enforced under `-warnaserror:CS0618`.
- **Package validation baseline**: Validated against baseline `1.13.0`. All breaking changes and API removals are verified and catalogued in `src/CymruBlazor/CompatibilitySuppressions.xml`.

## Upgrade notes

- Consult the [v2.0.0 Migration Guide](docs/MIGRATION-2.0.md) for complete details and code migration examples.
- To identify any remaining obsolete API usage in your project prior to upgrading, build with warnings as errors:
  ```bash
  dotnet build YourSolution.slnx -warnaserror:CS0618
  ```

## Quality

Unit tests 1355 of 1355. Accessibility tests 219 of 219, including automated axe-core scans in light, dark, and high-contrast themes.

**Full changelog**: [`CHANGELOG.md`](CHANGELOG.md)
