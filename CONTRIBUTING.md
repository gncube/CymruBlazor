# Contributing to CymruBlazor

Thanks for your interest in contributing. CymruBlazor is an open-source
component library implementing the NHS Wales Design System.

## Before you start

- For anything beyond a small fix, please open an issue first to discuss
  the change - especially for new components, since they need to align
  with the NHS Wales Design System rather than introduce a new visual
  language (see `PRD.md`, section 3, "Non Goals").
- This repo's coding standards, testing standards, and modern .NET
  conventions are documented as Copilot/agent skills under
  `.github/skills/`. They apply to human contributors just as much as to
  AI-assisted changes - skim them before your first PR.

## Getting set up

Requires the .NET 10 SDK.

```bash
git clone [https://github.com/gncube/CymruBlazor.git](https://github.com/gncube/CymruBlazor.git)
cd CymruBlazor
dotnet restore CymruBlazor.slnx
dotnet build CymruBlazor.slnx
dotnet run --project src/CymruBlazor.Demo
