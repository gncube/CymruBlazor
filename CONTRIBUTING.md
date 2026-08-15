# Contributing to CymruBlazor

Thanks for your interest in contributing. CymruBlazor is an early-stage,
pre-1.0 project - see the [README](README.md) for what's currently
implemented versus planned.

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
git clone https://github.com/gncube/CymruBlazor.git
cd CymruBlazor
dotnet restore CymruBlazor.slnx
dotnet build CymruBlazor.slnx
dotnet run --project src/CymruBlazor.Demo
```

## Making a change

1. Branch from `main`.
2. Follow the existing component structure (see `Spec.md`, section 2,
   "Component Architecture") - `.razor`, `.razor.css`, `.razor.cs`
   code-behind, and a matching test file per component.
3. Every new CSS class a component's `CssBuilder` generates must have a
   corresponding rule in the appropriate `wwwroot/css/**` stylesheet, and
   that stylesheet must be listed in **both**
   `src/CymruBlazor/wwwroot/css/cymrublazor.css` (`@import`, used in dev)
   **and** `src/CymruBlazor/build/BundleCss.props` (used for
   Release/Publish builds). These two lists drifting out of sync is a
   recurring source of bugs in this project - please keep them identical.
4. Add or update tests:
   - `tests/CymruBlazor.Tests` - bUnit component tests (the primary,
     actively-used test project right now).
   - `tests/CymruBlazor.ApprovalTests` / `tests/CymruBlazor.AccessibilityTests` -
     scaffolded but not yet populated; if you're adding the first tests to
     either, please call that out in your PR description so it gets extra
     review attention.
5. Run the full suite before opening a PR:

   ```bash
   dotnet test CymruBlazor.slnx -c Release
   ```

6. Follow [Conventional Commits](https://www.conventionalcommits.org/) for
   commit messages (`feat:`, `fix:`, `docs:`, `refactor:`, etc.) - CI's
   generated release notes are built from these.

## Pull requests

- Keep PRs focused and reasonably small; large, multi-concern PRs are
  harder to review carefully in an accessibility-focused library.
- CI (`.github/workflows/ci.yml`) must pass: build, test, and a
  pack-verification step.
- Accessibility is not optional here - WCAG 2.2 AA, keyboard navigation,
  and screen reader compatibility are requirements, not nice-to-haves (see
  `PRD.md`, section 5).

## Releasing

Maintainers only. Versioning is fully derived from git tags via
[MinVer](https://github.com/adamralph/minver) - there is no manual version
bump anywhere in the codebase. Pushing a tag like `v0.2.0` triggers
`.github/workflows/release.yml`, which builds, tests, packs, publishes to
NuGet, and creates the GitHub Release automatically.

## Code of conduct

Be respectful and constructive. This project supports NHS Wales and public
sector engineering teams building software that people rely on for
healthcare - treat contributions and reviews with the same care.
