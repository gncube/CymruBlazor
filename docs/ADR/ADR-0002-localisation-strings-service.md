---
status: Proposed
---

# ADR-0002: Localisation step 2 - an optional strings service keyed by `AppLanguage`

## Status

Proposed. Needs review before any code is written (public API design).

## Context

Every built-in user-facing string is English. `AppLanguage` documents that
NHS Wales services must offer Welsh on equal terms (Welsh Language Standards),
so a bilingual service has to be able to translate what the library renders.

Step 1 (v1.3.0, shipped) gives every built-in string a `string?` override
parameter with an English default, following the existing
`CyBadge.DismissAriaLabel` pattern. That is non-breaking and sufficient for a
single call site, but scales badly: an app with many `CyAlert`s, `CyDialog`s and
a sidebar must repeat the Welsh strings at every use.

Naming convention already in use: accessible names of dismiss buttons are
`DismissAriaLabel`; landmark names are `AriaLabel`; other accessible names are
`*Label`; visible text and announcements are `*Label` / `*Message`.

The library currently has no notion of "the current language" beyond the
`CyLanguageToggle` component's parameters, and it ships no Welsh text.

The demo's Localisation page (`/foundations/localisation`, `AppStrings`) is
the working reference for step 1 and shows the per-component wiring that this
ADR would remove.

## Proposal

Add an optional service; components fall back through three levels:

1. the component's own parameter (step 1, always wins),
2. `ICyLocalizer` if one is registered, for the current `AppLanguage`,
3. the built-in English default.

```csharp
public interface ICyLocalizer
{
    /// Returns the text for a library string, or null to use the English default.
    string? Get(CyString key, AppLanguage language);
}

public enum CyString { AlertDismiss, ToastRegion, ToastDismiss, BreadcrumbLabel, ... }
```

- Keys are an enum (or constants) so the full set is discoverable and a
  test can assert every hard-coded default has a key.
- Components obtain the current language from a small `ILanguageState`
  (scoped, change notification) that `CyLanguageToggle` updates; without it,
  language is `English` and behaviour is exactly step 1.
- Registration is opt-in: `services.AddCymruLocalizer<MyLocalizer>()`.
  Apps that translate elsewhere (resx, a CMS) implement the interface over
  their own source.
- Whether the library also ships an official Welsh string set is a separate
  decision (below).

## Open questions (for review)

1. **Ship Welsh text?** Accurate NHS Wales Welsh needs vetted translation
   (Welsh Language Standards; DHCW translation service), not machine output.
   Proposal: do not ship Welsh strings in the core package until reviewed;
   offer a `CymruBlazor.Localisation.Cy` companion package if DHCW supplies them.
2. **Language state.** Reuse `ThemeService`-style scoped service, or read
   `CultureInfo.CurrentUICulture`? Server-hosted apps want per-circuit state;
   WebAssembly wants a persisted choice. Likely both behind `ILanguageState`.
3. **Pluralisation / formatting.** Current strings have none. If a future
   string needs it (`"{0} results"`), keys need format arguments.
4. **Attribute `lang`.** Welsh text inside an English page should carry
   `lang="cy"` (WCAG 3.1.2). The localiser could return the language of the
   text so components can emit it.
5. **Calendars.** `CyDatePicker` (deferred) needs Welsh month/day names and
   week start, so it depends on this decision.

## Consequences

- Fully additive: no parameter is removed, and no service registered means no
  change in behaviour.
- One extra optional service resolution per component that renders strings
  (cheap; cache per component instance).
- Adds a second source of truth for strings, so the guard tests added in 1.3.0
  (`HardcodedAriaLabelTests`, per-parameter override tests) must be extended to
  cover key completeness.

## Alternatives Considered

- **`IStringLocalizer` / resx in the library:** ties consumers to
  `Microsoft.Extensions.Localization` and to culture rather than the app's
  explicit English/Welsh switch; resource lookup by culture also does not map
  cleanly to `AppLanguage`.
- **Parameters only (step 1 forever):** simple, but repetitive and easy to miss.
- **Cascading parameter carrying a dictionary:** no DI, but every consumer must
  wrap their app and the type is untyped.

## References

- `src/CymruBlazor/Enums/AppLanguage.cs`
- `plan/plan-post-1.2-roadmap.md` D1, D4
- Welsh Language Standards (Welsh Language (Wales) Measure 2011)
