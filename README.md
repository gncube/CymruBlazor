# CymruBlazor

An open-source Blazor component library implementing the NHS Wales Design System.

[![NuGet](https://img.shields.io/nuget/v/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CymruBlazor.svg)](https://www.nuget.org/packages/CymruBlazor/)
[![CI](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml/badge.svg)](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Status:** Pre-release
>
> CymruBlazor is currently under active development. The API and
> component set may change between preview releases.
>
> The current published package version is shown by the NuGet badge
> above. See [CHANGELOG.md](CHANGELOG.md) for release history and
> [PRD.md](PRD.md) for the planned v1 scope.

## Getting Started

Pre-release versions aren't resolved by default, so pass `--prerelease`
(or an explicit version) until a `1.0.0` is published:

```shell
dotnet add package CymruBlazor --prerelease
```

Add the stylesheet to your `App.razor` or `_Host.cshtml`:

```html
<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
```

See the [Demo application](src/CymruBlazor.Demo) and [documentation](docs/) for full usage.

## Documentation

The full component catalogue, with live previews and code samples, is
published from the Demo application to GitHub Pages:

**[gncube.github.io/CymruBlazor](https://gncube.github.io/CymruBlazor/)**

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

