# CymruBlazor

An open-source Blazor component library implementing the NHS Wales Design System.

[![NuGet](https://img.shields.io/nuget/v/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![CI](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml/badge.svg)](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/gncube/CymruBlazor/blob/main/LICENSE)

> **Status:** Stable (2.x)
>
> CymruBlazor follows [Semantic Versioning](https://semver.org/): new
> components and features arrive in minor releases, and breaking changes are
> reserved for major releases. Upgrading from 1.x? See the
> [v2.0.0 Migration Guide](docs/MIGRATION-2.0.md). See the
> [CHANGELOG](CHANGELOG.md) for release history.

## Getting Started

Targets .NET 10. Install the package from NuGet:

```shell
dotnet add package CymruBlazor
```

Add the stylesheet to your `App.razor` (Blazor Web App) or `wwwroot/index.html` (Blazor WebAssembly):

```html
<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
```

Register CymruBlazor services in `Program.cs`:

```csharp
builder.Services.AddCymruBlazor();
```

See the [Scaffold Guide](docs/CymruBlazor-Scaffold-Guide.md) for a step-by-step setup walkthrough, and the [Demo application](https://github.com/gncube/CymruBlazor/tree/main/src/CymruBlazor.Demo) for live usage.

## Testing Packages

CymruBlazor provides dedicated testing packages to test your own applications with the same checks the library uses on itself:

```shell
dotnet add package CymruBlazor.Testing        # bUnit helpers and structural markup checks
dotnet add package CymruBlazor.Testing.Axe    # Playwright and axe-core browser accessibility scans
```

- **`CymruBlazor.Testing`**: Provides `CymruTestContext` for bUnit tests and `CymruMarkupChecks` for verifying form control labeling, ARIA references, and heading hierarchy without needing a browser.
- **`CymruBlazor.Testing.Axe`**: Provides `CymruAxeTestBase` for automated WCAG accessibility scans against Chromium in light, dark, and high-contrast themes.

See the [Testing Guide](docs/Testing.md) for complete details and examples.

## Documentation

The full component catalogue, with live previews and code samples, is
published from the Demo application to GitHub Pages:

**[gncube.github.io/CymruBlazor](https://gncube.github.io/CymruBlazor/)**

Additional guides:
- [v2.0.0 Migration Guide](docs/MIGRATION-2.0.md) - Upgrading from 1.x to 2.0.0.
- [Scaffold Guide](docs/CymruBlazor-Scaffold-Guide.md) - Project setup, configuration, and architectural guidance.
- [Testing Guide](docs/Testing.md) - Component tests with bUnit and browser-based accessibility scans with axe-core.
- [Recipes](docs/Recipes.md) - Common page templates, layouts, and Welsh localisation examples.
- [Diagnostics](docs/Diagnostics.md) - Compiler warnings and runtime diagnostic codes.

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.
