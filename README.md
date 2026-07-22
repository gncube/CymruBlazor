# CymruBlazor

An open-source Blazor component library implementing the NHS Wales Design System.

[![NuGet](https://img.shields.io/nuget/v/CymruBlazor)](https://www.nuget.org/packages/CymruBlazor)
[![Build](https://github.com/gncube/CymruBlazor/actions/workflows/ci.yml/badge.svg)](https://github.com/gncube/CymruBlazor/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Status: pre-release.** The first published version is `0.1.0-preview.1`,
> covering layout primitives, accessibility utilities, a minimal `Button`,
> and the NHS Wales design-token/CSS foundation. Content components
> (`Card`, `Alert`, `Typography`) and most form controls are not
> implemented yet - see [`CHANGELOG.md`](CHANGELOG.md) for exactly what's
> in this release and [`PRD.md`](PRD.md) for the full v1 target scope.

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

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

