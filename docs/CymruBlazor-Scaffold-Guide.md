# CymruBlazor — Scaffold Guide

> .NET 10 · C# · NHS Wales Design System · Open Source Blazor Component Library

This guide answers one question: **I have a new .NET/Blazor application and
want to use the current CymruBlazor package — what do I need to install,
configure and implement to get started correctly?**

It documents the package as it is currently published in **v1.5.0**.

---

## 0. Current status

> **Status: Stable (1.x).** The current published version is `1.5.0`.
> CymruBlazor follows [Semantic Versioning](https://semver.org/): new
> components and features arrive in minor releases, and breaking changes
> are reserved for major releases.

- GitHub: <https://github.com/gncube/CymruBlazor>
- NuGet: <https://www.nuget.org/packages/CymruBlazor/>
- Live component catalogue (built from the Demo app, kept current
  automatically on every push to `main`): <https://gncube.github.io/CymruBlazor/>
- Release history: `CHANGELOG.md` in the repository, and
  [GitHub Releases](https://github.com/gncube/CymruBlazor/releases)

Versioning is derived entirely from git tags by
[MinVer](https://github.com/adamralph/minver) (tag prefix `v`, e.g. tag
`v1.5.0` → package version `1.5.0`).

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

Install the package via the .NET CLI:

```bash
dotnet add package CymruBlazor
```

or with an explicit version:

```bash
dotnet add package CymruBlazor --version 1.5.0
```

Using [Central Package Management (CPM)](https://learn.microsoft.com/nuget/consume-packages/central-package-management)?
Add the package version to `Directory.Packages.props` and reference it without a
version in your project file:

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="CymruBlazor" Version="1.5.0" />
```

```xml
<!-- YourApp.csproj -->
<PackageReference Include="CymruBlazor" />
```

That single package contains everything you need: design tokens, components,
icons (`CyIcon`), overlays (`CyDialog`, `CyTooltip`), and theming
(`IThemeService`, `CyThemeProvider`).

Installing the package also brings in two transitive dependencies:
- **`Mediator.Abstractions`** — lightweight, source-generated mediator used
  internally for `CyLiveRegion` screen-reader announcements and toast notifications.
- **`Microsoft.JSInterop`** — required for DOM event listeners, theme persistence,
  and native overlay interop (`cymru-overlay.js`).

---

## 3. Reference the stylesheet

Add a single `<link>` to `App.razor` (Blazor Web App) or `wwwroot/index.html` (Blazor WebAssembly):

```html
<link rel="stylesheet" href="_content/CymruBlazor/css/cymrublazor.css" />
```

This bundle contains the complete design system — NHS Wales colour, spacing,
and typography tokens, base resets, layout primitives, component styles,
and light/dark/high-contrast themes organised using CSS `@layer` rules.

If you use `CyThemeProvider` with persisted theme preferences and OS
`prefers-color-scheme` synchronization, also include the companion script:

```html
<script src="_content/CymruBlazor/js/cymrublazor.js"></script>
```

*(Note: Overlay components like `CyDialog` and `CyTooltip` import their own
ES module `cymru-overlay.js` on demand automatically without requiring an
explicit script tag).*

---

## 4. Register services

In `Program.cs`:

```csharp
using CymruBlazor.Extensions;

builder.Services.AddCymruBlazor();
```

This registration is **mandatory**. Several components resolve scoped
services from dependency injection during rendering. `AddCymruBlazor()` registers:

| Service | Lifetime | Used by |
|---|---|---|
| `IComponentIdGenerator` | `Scoped` | Deterministic element IDs across all components (labels, `aria-describedby`, form inputs) |
| `IThemeService` | `Scoped` | `CyThemeProvider`, and components reacting to runtime theme changes |
| `IFocusManager` (`JsFocusManager`) | `Scoped` | `CyFocusTrap`, `CyDialog`, and `CyNavigation` mobile drawer |
| `IToastService` (`ToastService`) | `Scoped` | Injected into components to trigger accessible notifications rendered by `CyToastContainer` |
| `IPackageVersionService` | `Scoped` | `CyFooter`'s optional `ShowVersion` parameter |
| Mediator pipeline (`IMediator`) | `Scoped` | `CyLiveRegion` announcements and internal notification dispatch |

---

## 5. Add recommended global usings

Add the following to your root `_Imports.razor` to make CymruBlazor components
and enums available across all pages and layouts:

```razor
@using CymruBlazor.Components.Button
@using CymruBlazor.Components.Content
@using CymruBlazor.Components.Data
@using CymruBlazor.Components.Feedback
@using CymruBlazor.Components.Forms
@using CymruBlazor.Components.Layout
@using CymruBlazor.Components.Theming
@using CymruBlazor.Components.Branding
@using CymruBlazor.Components.Accessibility
@using CymruBlazor.Enums
@using CymruBlazor.Themes
@using CymruBlazor.Accessibility.Notifications
@using Mediator
```

### Namespace Directory

| Namespace | Key Components and Types |
|---|---|
| `CymruBlazor.Components.Layout` | `CyContainer`, `CyStack`, `CySidebar`, `CyCluster`, `CyGrid`, `CyCenter`, `CyHeader`, `CyNavigation`, `CyNavigationItem`, `CyHeroBanner`, `CyFooter`, `CyBreadcrumb`, `CyBreadcrumbItem`, `CyPageHeader`, `CySkipLink`, `CyTabs`, `CyTabPanel` |
| `CymruBlazor.Components.Content` | `CyCard`, `CyAlert`, `CyIcon`, `CyTypography`, `CyBadge`, `CyAccordion`, `CyAccordionItem`, `CyCodeBlock`, `CyTooltip` |
| `CymruBlazor.Components.Forms` | `CyTextBox`, `CySelect<TValue>`, `CyCheckbox`, `CyRadioGroup<TValue>`, `CyRadio`, `CyTextArea`, `CyDateInput`, `CyValidationSummary` |
| `CymruBlazor.Components.Data` | `CyTable`, `CyPagination` |
| `CymruBlazor.Components.Feedback` | `IToastService`, `CyToastContainer`, `CyProgress`, `CySpinner` |
| `CymruBlazor.Components.Button` | `CyButton` (`Variant`, `Size`, `Disabled`, `Loading`, `Href`, `Type`, `OnClick`) |
| `CymruBlazor.Components.Accessibility` | `CyDialog`, `CyFocusTrap`, `CyLiveRegion`, `CyScreenReaderOnly` |
| `CymruBlazor.Components.Theming` | `CyThemeProvider` |
| `CymruBlazor.Components.Branding` | `CyBrandLogo`, `CyLanguageToggle` |
| `CymruBlazor.Enums` | `ComponentSize`, `ComponentColour`, `ContainerSize`, `Orientation`, `TypographyVariant`, `ThemeMode`, `AppLanguage`, etc. |

> **Icon-Only Buttons:** To create an accessible icon-only button, compose
> `CyButton` with `CyIcon` and supply an `aria-label`:
> ```razor
> <CyButton Variant="ComponentColour.Tertiary" aria-label="Search" OnClick="HandleSearch">
>     <CyIcon Name="search" Size="20" />
> </CyButton>
> ```

---

## 6. Layout and Shell Structure

In `MainLayout.razor`, wrap the application shell inside `<CyThemeProvider>`:

```razor
@inherits LayoutComponentBase
@inject IThemeService Theme

<CyThemeProvider>
    <!-- 1. Skip link MUST be first focusable element (WCAG 2.4.1) -->
    <CySkipLink TargetId="main-content" />

    <div class="app-shell">
        <CyHeader Title="NHS Wales" />

        <CyNavigation>
            <CyNavigationItem Text="Home" Href="/" Match="NavLinkMatch.All" />
            <CyNavigationItem Text="Patients" Href="/patients" />
            <CyNavigationItem Text="Clinics" Href="/clinics" />
        </CyNavigation>

        <!-- 2. Main content landmark with matching id and tabindex for focus transfer -->
        <main id="main-content" tabindex="-1">
            @Body
        </main>

        <CyFooter Copyright="© 2026 Digital Health and Care Wales" ShowVersion="true" />
    </div>

    <!-- 3. Global accessible toast container and screen reader live region -->
    <CyToastContainer />
    <CyLiveRegion />
</CyThemeProvider>
```

---

## 7. Forms

Form components integrate with Blazor's `<EditForm>` and support `DataAnnotations`:

```razor
<EditForm Model="_model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <CyValidationSummary Title="There is a problem" />

    <!-- Text input with HTML5 inputmode and autocomplete -->
    <CyTextBox @bind-Value="_model.NhsNumber"
               Label="NHS Number"
               HintText="10-digit number shown on medical card"
               InputMode="numeric"
               Required="true" />

    <!-- 3-Field Date Input (Day / Month / Year) -->
    <CyDateInput @bind-Value="_model.DateOfBirth"
                 Label="Date of birth"
                 HintText="For example, 31 3 1980"
                 AutocompleteDateOfBirth="true"
                 Required="true" />

    <!-- Radio group with fieldset/legend semantics -->
    <CyRadioGroup @bind-Value="_model.ContactPreference"
                  Label="How should we contact you?"
                  Required="true">
        <CyRadio Value="@("email")" Label="Email" />
        <CyRadio Value="@("sms")" Label="Text message (SMS)" />
        <CyRadio Value="@("letter")" Label="Letter" />
    </CyRadioGroup>

    <!-- Multi-line text area with live character counter -->
    <CyTextArea @bind-Value="_model.ClinicalNotes"
                Label="Clinical notes"
                Rows="5"
                MaxLength="500"
                ShowCharacterCount="true" />

    <CyCheckbox @bind-Value="_model.ConsentGiven" Label="Consent confirmed" />

    <CyButton Type="submit" Variant="ComponentColour.Primary">
        Submit Record
    </CyButton>
</EditForm>
```

---

## 8. Data Display (CyTable & CyPagination)

CymruBlazor v1.5.0 provides accessible table and pagination primitives:

```razor
<!-- Accessible table with mandatory Caption and horizontal scroll region -->
<CyTable Caption="Scheduled Outpatient Clinics" ScrollContainer="true">
    <thead>
        <tr>
            <th scope="col">Clinic</th>
            <th scope="col">Specialty</th>
            <th scope="col">Status</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var clinic in _pagedClinics)
        {
            <tr>
                <th scope="row">@clinic.Name</th>
                <td>@clinic.Specialty</td>
                <td><CyBadge Variant="@clinic.StatusColour">@clinic.Status</CyBadge></td>
            </tr>
        }
    </tbody>
</CyTable>

<!-- Pagination with boundary/sibling truncation -->
<CyPagination TotalPages="@_totalPages"
              @bind-CurrentPage="_currentPage"
              BoundaryCount="1"
              SiblingCount="1" />
```

---

## 9. Feedback (CyProgress, CySpinner & IToastService)

Provide clear, accessible state feedback during loading or async operations:

```razor
<!-- Determinate Progress Bar with accessible name -->
<CyProgress Value="18" Max="20" Label="Bed Occupancy" ShowValueText="true" ValueText="18 of 20 beds occupied" />

<!-- Indeterminate Loading Spinner -->
<CySpinner Label="Loading patient records..." ShowLabel="true" Size="ComponentSize.Medium" />
```

### Toast Notifications

Inject `IToastService` anywhere in your application:

```csharp
@inject IToastService Toasts

private void SaveRecord()
{
    // Countdown pauses automatically on hover or keyboard focus (WCAG 2.2.1)
    Toasts.Show("Record updated successfully", ToastVariant.Success);
}
```

---

## 10. Overlays & Accessibility Services

### Modal Dialogs (`CyDialog`)

Native `<dialog>` with inert backdrop, top-layer elevation, focus containment, and Escape handling:

```razor
<CyButton OnClick="() => _dialogOpen = true">View Discharge Summary</CyButton>

<CyDialog @bind-Open="_dialogOpen"
          Title="Discharge Summary"
          Description="Confirm details before finalising discharge"
          Size="ComponentSize.Large">
    <p>Patient is medically fit for discharge.</p>

    <Footer>
        <CyButton Variant="ComponentColour.Primary" OnClick="ConfirmDischarge">Confirm</CyButton>
        <CyButton Variant="ComponentColour.Secondary" OnClick="() => _dialogOpen = false">Cancel</CyButton>
    </Footer>
</CyDialog>
```

### Accessible Tooltips (`CyTooltip`)

WCAG 1.4.13 compliant tooltip triggering on both hover and focus:

```razor
<CyTooltip Text="National Health Service identifier">
    <span tabindex="0">NHS Number</span>
</CyTooltip>
```

---

## 11. Localisation Strategy (Welsh / English)

Every component exposing user-facing text provides overridable string parameters
(e.g., `AriaLabel`, `CloseLabel`, `DayLabel`, `MonthLabel`, `YearLabel`,
`PreviousLabel`, `NextLabel`).

In consuming applications, inject an application string service (patterned after
the Demo app's `AppStrings`) to cleanly map bilingual resources:

```razor
<CyDateInput DayLabel="@Strings.DateDay"
             MonthLabel="@Strings.DateMonth"
             YearLabel="@Strings.DateYear"
             Label="@Strings.DateOfBirth" />

<CyPagination PreviousLabel="@Strings.PaginationPrevious"
              NextLabel="@Strings.PaginationNext"
              AriaLabel="@Strings.PaginationLabel"
              @bind-CurrentPage="_page"
              TotalPages="10" />
```

---

## Summary Checklist

1. `dotnet add package CymruBlazor`
2. Add `<link rel="stylesheet" href="_content/CymruBlazor/css/cymrublazor.css" />` to host page
3. *(Optional)* Add `<script src="_content/CymruBlazor/js/cymrublazor.js"></script>` for persisted theming
4. `builder.Services.AddCymruBlazor();` in `Program.cs`
5. Add component `@using` directives to `_Imports.razor`
6. Wrap `MainLayout.razor` in `<CyThemeProvider>`
7. Build pages using semantic primitives (`CyTable`, `CyPagination`, `CyProgress`, `CyDialog`, and forms)
