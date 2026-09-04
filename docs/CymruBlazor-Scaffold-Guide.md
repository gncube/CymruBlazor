# CymruBlazor — Scaffold Guide

> .NET 10 · C# · NHS Wales Design System · Open Source Blazor Component Library

This guide answers one question: **I have a new .NET/Blazor application and
want to use the current CymruBlazor package — what do I need to install,
configure and implement to get started correctly?**

It documents the package as it is currently published, not the eventual
v1.0 scope described in `PRD.md`. Where the two differ, this guide follows
what you can actually install today.

---

## 0. Current status

> **CymruBlazor is pre-release.** The latest published version is
> `0.1.0-preview.8`. The public API — component names, parameters and CSS
> class names — may still change before `1.0.0`. Treat this guide as
> describing "the current preview", not a stable contract.

- GitHub: <https://github.com/gncube/CymruBlazor>
- NuGet: <https://www.nuget.org/packages/CymruBlazor/>
- Live component catalogue (built from the Demo app, kept current
  automatically on every push to `main`): <https://gncube.github.io/CymruBlazor/>
- Release history: `CHANGELOG.md` in the repository, and
  [GitHub Releases](https://github.com/gncube/CymruBlazor/releases)

Versioning is derived entirely from git tags by
[MinVer](https://github.com/adamralph/minver) (tag prefix `v`, e.g. tag
`v0.1.0-preview.8` → package version `0.1.0-preview.8`) — there is no
manually maintained version number and no GitVersion tooling involved.
This is a repository-maintenance detail, not something a consumer needs to
configure.

---

## 1. Prerequisites

```bash
dotnet --version   # .NET 10 SDK or later
```

CymruBlazor targets `net10.0` and works in both Blazor WebAssembly and
Blazor Web App (interactive Server/WASM) projects. No Node.js or npm step
is required to consume the package — the design tokens and NHS Wales
styling are shipped as pre-built CSS inside the NuGet package itself.

---

## 2. Install the package

Because no `1.0.0` has been published yet, the default `dotnet add
package` resolution (which ignores prerelease versions) will not find it.
Pass `--prerelease`, or pin an explicit version:

```bash
dotnet add package CymruBlazor --prerelease
```

or, pinned:

```bash
dotnet add package CymruBlazor --version 0.1.0-preview.8
```

Using [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)?
Add the version to `Directory.Packages.props` and reference it without a
version in the project file:

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="CymruBlazor" Version="0.1.0-preview.8" />
```

```xml
<!-- YourApp.csproj -->
<PackageReference Include="CymruBlazor" />
```

That single package is everything you need. Icons (`CyIcon`) and theming
(`IThemeService`, `CyThemeProvider`) ship as part of the core package —
there is currently no separate `CymruBlazor.Icons` or
`CymruBlazor.Theming` package on NuGet, regardless of what the
long-term repository layout in `Spec.md` describes.

Installing the package also brings in two transitive dependencies you
should be aware of:

- **`Mediator.Abstractions`** — a lightweight, source-generated
  mediator/notification library. CymruBlazor uses it internally for its
  accessibility live-region announcement pipeline (see
  [§7](#7-accessibility-services)); you don't need to configure it
  yourself, but you will see it in your dependency tree and it's
  available to `@inject` (`IMediator`) if you want to publish your own
  announcements.
- **`Microsoft.JSInterop`** — required for the small, optional
  `cymrublazor.js` script described in [§6](#6-theming).

---

## 3. Reference the stylesheet

Add a single `<link>` to `wwwroot/index.html` (Blazor WebAssembly) or the
relevant host page/`App.razor` (Blazor Web App):

```html
<link rel="stylesheet" href="_content/CymruBlazor/css/cymrublazor.css" />
```

This one file is the complete design system — NHS Wales colour, spacing,
and typography tokens, base resets, layout primitives, every shipped
component's styles, utility classes, and the light/dark/high-contrast
theme variants. It is generated at build time from a layered source tree
(`tokens/`, `base/`, `layout/`, `components/`, `themes/`, `utilities/`)
and concatenated into one file using CSS
[`@layer`](https://developer.mozilla.org/en-US/docs/Web/CSS/@layer) for
predictable cascade ordering — you only ever need to reference the single
bundled file shown above.

If you use `CyThemeProvider` with persisted/OS-aware theme switching
(recommended — see [§6](#6-theming)), also reference the small companion
script:

```html
<script src="_content/CymruBlazor/js/cymrublazor.js"></script>
```

This script is optional. Without it, runtime theme switching still works
for the current session; it just won't remember the user's choice across
page loads or react live to OS light/dark-mode changes.

---

## 4. Register services

In `Program.cs`:

```csharp
using CymruBlazor.Extensions;

builder.Services.AddCymruBlazor();
```

This call is **required**, not optional infrastructure — several
components resolve services from DI at render time and will throw if
`AddCymruBlazor()` hasn't been called. It registers:

| Service | Used by |
|---|---|
| `IComponentIdGenerator` | Deterministic element IDs across all components (labels, `aria-describedby`, etc.) |
| `IThemeService` | `CyThemeProvider`, and any component you build that reacts to theme changes |
| `IFocusManager` | `CyFocusTrap`, and transitively `CyNavigation`'s mobile menu |
| `IPackageVersionService` | `CyFooter`'s optional `ShowVersion` parameter (looks up the published NuGet version) |
| Mediator pipeline (`IMediator`, notification handlers) | `CyLiveRegion`'s screen-reader announcement handling |

All are registered `Scoped`, matching Blazor's per-circuit/per-session
lifetime.

---

## 5. Add recommended global usings

Component and enum types are split across several namespaces. The
library's own `_Imports.razor` only pre-imports
`CymruBlazor.Components.Core`, `CymruBlazor.Components.Layout`, and
`CymruBlazor.Enums` — everything else needs an explicit `@using` in your
own app. At minimum, add the ones you're using; a reasonable starting set
mirrors what the Demo application itself recommends:

```razor
@using CymruBlazor.Components.Layout
@using CymruBlazor.Components.Accessibility
@using CymruBlazor.Enums
@using CymruBlazor.Accessibility.Notifications
@using Mediator
```

Add these as needed for the areas you use:

| Namespace | Contains |
|---|---|
| `CymruBlazor.Components.Layout` | `CyContainer`, `CyStack`, `CySidebar`, `CyCluster`, `CyGrid`, `CyCenter`, `CyHeader`, `CyNavigation`, `CyNavigationItem`, `CyHeroBanner`, `CyFooter`, `CyBreadcrumb`, `CyBreadcrumbItem`, `CyPageHeader`, `CySkipLink` |
| `CymruBlazor.Components.Content` | `CyCard`, `CyAlert`, `CyIcon`, `CyTypography` |
| `CymruBlazor.Components.Forms` | `CyTextBox`, `CySelect<TValue>`, `CyCheckbox`, `CyValidationSummary` |
| `CymruBlazor.Components.Theming` | `CyThemeProvider` |
| `CymruBlazor.Components.Branding` | `CyBrandLogo`, `CyLanguageToggle` |
| `CymruBlazor.Components.Accessibility` | `CyFocusTrap`, `CyLiveRegion`, `CyScreenReaderOnly` |
| `CymruBlazor.Components.Button` | `CyButton` (`Variant`, `Size`, `Disabled`, `Loading`, `Href`, `Type`, `OnClick` — see note below) |
| `CymruBlazor.Enums` | `ComponentSize`, `ComponentColour`, `ComponentElevation`, `ContainerSize`, `Orientation`, `AlignItems`, `JustifyContent`, `GridColumns`, `GridGap`, `HeroBackground`, `SidebarPosition`, `SidebarWidth`, `SidebarCollapseMode`, `TypographyVariant`, `ValidationState`, `AppLanguage`, `BrandLogoVariant`, `LiveRegionPoliteness`, `IconPosition` |
| `CymruBlazor.Themes` | `IThemeService`, `ThemeMode`, `ThemeDefinition`, `ThemeChangedEventArgs` |
| `CymruBlazor.Accessibility.Focus` | `IFocusManager`, `IKeyboardNavigationService` |
| `CymruBlazor.Accessibility.Notifications` | `LiveRegionAnnouncement` |

> **Naming note:** every component uses a `Cy` prefix (`CyContainer`,
> `CyNavigation`, `CyTextBox`, `CyButton`, `CyFocusTrap`, …), **not** the
> `Cymru`-prefixed names shown in `PRD.md`/`PROMPT.md` (those describe an
> earlier planning document, not the shipped API). `CyButton` and
> `CyFocusTrap` were originally shipped unprefixed (`Button`, `FocusTrap`)
> and later renamed - if you're looking at an example predating that
> rename, add the `Cy` prefix. `CyButton` is no longer minimal: it
> supports `Variant` (`ComponentColour`), `Size` (`ComponentSize`),
> `Disabled`, `Loading` (shows a spinner, blocks `OnClick`), `Href`
> (renders as `<a>` instead of `<button>` when set and not disabled),
> `Type`, and `OnClick`.

---

## 6. Your first component

Layout primitives compose to build any page structure:

```razor
<CyContainer Size="ContainerSize.Large">
    <CyStack Orientation="Orientation.Vertical" Gap="ComponentSize.Medium">
        <h1>Hello, NHS Wales</h1>
        <p>Built with CymruBlazor.</p>
    </CyStack>
</CyContainer>
```

### Page chrome

The NHS Wales-specific layout components compose the same way:

```razor
<CySkipLink TargetId="main-content" />

<CyNavigation>
    <CyNavigationItem Text="Home" Href="/" />
    <CyNavigationItem Text="Appointments" Href="/appointments" />
</CyNavigation>

<CyPageHeader Title="Appointments" Subtitle="Manage upcoming clinics">
    <Breadcrumb>
        <CyBreadcrumb>
            <CyBreadcrumbItem Text="Home" Href="/" />
            <CyBreadcrumbItem Text="Appointments" />
        </CyBreadcrumb>
    </Breadcrumb>
</CyPageHeader>

<main id="main-content" tabindex="-1">
    @Body
</main>

<CyFooter Copyright="© 2026 Digital Health and Care Wales"
          ShowVersion="true" />
```

Place `CySkipLink` before `CyNavigation` in markup, not after — it must be
the first focusable element on the page to satisfy WCAG 2.4.1 (Bypass
Blocks).

---

## 7. Forms

Form fields (`CyTextBox`, `CySelect<TValue>`, `CyCheckbox`) derive from
Blazor's own `InputBase<TValue>`, so they behave like the framework's
built-in `<InputText>`/`<InputSelect>` — use them inside an `<EditForm>`
with `@bind-Value`, and validation follows normal `DataAnnotations`/
`EditContext` rules:

```razor
<EditForm Model="_model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <CyValidationSummary Title="There is a problem" />

    <CyTextBox @bind-Value="_model.PatientName"
               Label="Patient name"
               HintText="As shown on the NHS number card"
               Required="true" />

    <CySelect @bind-Value="_model.Ward" Label="Ward">
        <option value="">Select a ward</option>
        <option value="cardiology">Cardiology</option>
        <option value="paediatrics">Paediatrics</option>
    </CySelect>

    <CyCheckbox @bind-Value="_model.ConsentGiven" Label="Consent given" />

    <button type="submit">Save</button>
</EditForm>
```

`Label` is `[EditorRequired]` on every field component — there is no
placeholder-as-label option, since relying on placeholder text as a label
is a well-known accessibility failure. Hints and validation errors are
wired up automatically via `aria-describedby`.

---

## 8. Theming

Wrap your root layout content in `CyThemeProvider` to enable runtime light
/ dark / high-contrast theme switching:

```razor
@inherits LayoutComponentBase
@inject IThemeService Theme

<CyThemeProvider>
    ... your layout markup ...
</CyThemeProvider>
```

`CyThemeProvider` applies the active theme via a `data-theme` attribute
and re-renders its subtree whenever the theme changes. To switch themes
programmatically:

```csharp
await Theme.ToggleDarkModeAsync();
// or
await Theme.SetThemeAsync(ThemeMode.HighContrast);
```

Subscribe to `IThemeService.ThemeChanged` if a component outside the
provider's own subtree needs to react to theme changes.

If you referenced `cymrublazor.js` in [§3](#3-reference-the-stylesheet),
the chosen theme persists across page loads (`localStorage`) and the app
will also follow the OS `prefers-color-scheme` setting live until the
user makes an explicit in-app choice.

---

## 9. Accessibility services

Beyond the WCAG-aligned markup baked into every component, CymruBlazor
ships two accessibility-specific building blocks worth knowing about:

**`CyFocusTrap`** — wrap transient UI (mobile menus, dialogs) to contain
keyboard focus and optionally restore it on close:

```razor
<CyFocusTrap Enabled="_menuOpen" AutoFocus="true" RestoreFocus="true">
    ...
</CyFocusTrap>
```

**`CyLiveRegion`** — announces dynamic content changes to screen readers.
Place one instance in your layout, then publish announcements from
anywhere in your app via the `Mediator` package's `IMediator`:

```razor
<CyLiveRegion Politeness="LiveRegionPoliteness.Polite" />
```

```csharp
@inject IMediator Mediator

await Mediator.Publish(new LiveRegionAnnouncement("Appointment saved"));
```

This publish/subscribe pattern (rather than a direct method call) is why
the `Mediator` package appears in your dependency tree even though you
never call `AddMediator` yourself — `AddCymruBlazor()` does that for you.

---

## 10. Icons and branding

`CyIcon` renders from a built-in, named SVG registry that includes both
general-purpose icons (`chevron-down`, `close`, `edit`, `filter`, …) and
healthcare-specific ones (`ambulance`, `clinical`, `gp`, `ward`,
`critical`, …):

```razor
<CyIcon Name="appointment" Size="20" />
```

`CyBrandLogo` and `CyLanguageToggle` support NHS Wales's bilingual
(English/Welsh) branding requirements:

```razor
<CyBrandLogo Href="/" Text="DHCW" Variant="BrandLogoVariant.Full" />
<CyLanguageToggle @bind-CurrentLanguage="_language" />
```

---

## 11. Worked example

The repository's `samples/Dashboard` project is currently the only sample
that actually references and uses CymruBlazor end-to-end — package
reference, `AddCymruBlazor()`, `CyThemeProvider`, `CySidebar`,
`CyBrandLogo`, `CyLanguageToggle`, `CyIcon`, and the theming/live-region
patterns above are all demonstrated there. Use it as your reference
implementation rather than `samples/StarterApp` or
`samples/HealthcarePortal`, which are currently unmodified Blazor
WebAssembly templates with no CymruBlazor integration.

For a live, always-current catalogue of every shipped component with
interactive previews, see the Demo application published at
<https://gncube.github.io/CymruBlazor/>.

---

## 12. Verifying your setup

```bash
dotnet build
dotnet run
```

If a component throws a DI resolution error at runtime, double-check step
4 (`AddCymruBlazor()`); if styles are missing or components render
unstyled, double-check step 3 (the stylesheet `<link>`).

---

## Summary checklist

1. `dotnet add package CymruBlazor --prerelease`
2. `<link rel="stylesheet" href="_content/CymruBlazor/css/cymrublazor.css" />`
3. *(optional, for persisted theming)* `<script src="_content/CymruBlazor/js/cymrublazor.js"></script>`
4. `builder.Services.AddCymruBlazor();`
5. Add `@using` directives for the component namespaces you need
6. Wrap your layout in `<CyThemeProvider>`
7. Build pages from `CyContainer`/`CyStack` and the NHS Wales chrome
   components; use `CyTextBox`/`CySelect`/`CyCheckbox` inside `<EditForm>`
   for forms
