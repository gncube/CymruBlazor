# CymruBlazor

An open-source Blazor component library implementing the NHS Wales Design System.

[![NuGet](https://img.shields.io/nuget/v/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![CI](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml/badge.svg)](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/gncube/CymruBlazor/blob/main/LICENSE)

> **Status:** Stable (1.x)
>
> CymruBlazor follows [Semantic Versioning](https://semver.org/): new
> components and features arrive in minor releases, and breaking changes are
> reserved for major releases. APIs that are being replaced are marked
> `[Obsolete]` first. The library is under active development, so the component
> set keeps growing. See the
> [CHANGELOG](https://github.com/gncube/CymruBlazor/blob/main/CHANGELOG.md)
> for release history.

## Getting Started

Targets .NET 10. Install the package from NuGet:

```shell
dotnet add package CymruBlazor
```

Add the stylesheet to your `App.razor` (Blazor Web App) or `wwwroot/index.html` (Blazor WebAssembly):

```html
<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
```

See the [Demo application](https://github.com/gncube/CymruBlazor/tree/main/src/CymruBlazor.Demo)
for full usage.

## Documentation

The full component catalogue, with live previews and code samples, is
published from the Demo application to GitHub Pages:

**[gncube.github.io/CymruBlazor](https://gncube.github.io/CymruBlazor/)**

## Contributing

Read [CONTRIBUTING.md](https://github.com/gncube/CymruBlazor/blob/main/CONTRIBUTING.md)
before opening a pull request.
