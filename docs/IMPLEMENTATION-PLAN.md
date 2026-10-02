# CymruBlazor: Implementation Plan (1.6.0 → 1.11 → 2.0.0)

**Inputs reviewed:** `CymruBlazor-Improvement-Recommendations.md`, `MIGRATION-2.0.md`, the 1.6.0 source zip (library, tests, samples, CI, CSS, JS, ADRs, CHANGELOG).
**Suggested location:** `docs/IMPLEMENTATION-PLAN.md`

> **Caveat.** `dotnet` was not available where this plan was written, so nothing here has been built or run. Every claim marked *(verified)* was read directly in the source; anything else is flagged as an assumption or a spike. Each phase has a CI gate for exactly that reason.

---

## 0. Implementation status

| Phase | State | Evidence |
|---|---|---|
| 0 (pre-flight) | **Mostly done.** 0.1 (tools/ restored), 0.2 (full suite green: 623/623 unit tests + Playwright/axe smoke test), 0.3 (1.6.0 published 2026-09-28; baseline bumped to 1.6.0) and spike 0.4 done. 0.5 and 0.6 remain. | NuGet; local runs |
| A (1.7.0) | **Implemented**, A1–A9 | See below |
| B–F | Not started | |

**How A was verified (and what wasn't).** .NET 10 was installed in the sandbox, but nuget.org is blocked, so the repo's own test projects (xunit, bUnit, Shouldly, Playwright) could **not** be restored or run. Instead the library was compiled from source with a stub for `Mediator`, with the repo's `.editorconfig`, and exercised through a purpose-built harness (`HtmlRenderer` plus a small event-dispatching renderer): 69 checks, all passing. The library emits only the 14 `ASP0006` warnings already present in the untouched 1.6.0 build. A reflection diff of the public API against the original 1.6.0 build shows **0 removed or changed members** (44 additions). The new bUnit/xunit tests in `tests/` were written but **not run**; CI is their first real execution.

**Spike 0.4 result.** In .NET 10, `InputBase` itself does *not* require an `EditContext`. CymruBlazor's `HasValidationError` dereferences it unguarded and throws `NullReferenceException`. So standalone mode (B3) = make the base class null-safe; no synthetic `EditContext` needed.

**Corrections to the recommendations, found during implementation**

1. **The `@onclick` double-publish bug (§5.1) does not reproduce.** `@onclick` on `<CyButton>` binds to the `OnClick` parameter (Blazor binds parameters case-insensitively) and is guarded; verified against the unmodified 1.6.0 code (0 calls while `Loading`). A splat-capture fix was written, proven unnecessary, and removed. The genuine gap, re-entrancy of an in-flight async handler, is fixed by the opt-in `AutoLoading`. A7 was reworded accordingly.
2. `save` and `undo` already existed in the registry, so 17 icons were missing (plus `unknown`), not 19. `--cymru-shadow-xl` already existed; only `2xl` was added.
3. Theme scoping is the real dark-mode hazard: themes apply to `.cy-theme-provider` and `body:has()`, not `:root`, so a `var()`-derived token declared on `:root` silently keeps its light value. New tokens are declared on all theme scopes, and `scripts/GenerateTokens.cs --check` enforces it (negative-tested: it reports 18 violations when the aliases are `:root`-only). The 1.6.0 token CSS has no violations.
4. Several rows in §1.2 above stay valid; the `CyButton` `@onclick` row there is superseded by item 1.

**Phase 0 status**
- **0.1** Resolved: `tools/CymruBlazor.CssBundler` was a packing omission.
- **0.2/0.3** Done: 1.6.0 is on nuget.org (2026-09-28); the CHANGELOG now records it; `PackageValidationBaselineVersion` is 1.6.0. The Playwright image/package mismatch that failed the accessibility gate is fixed (the image version now derives from `Directory.Packages.props`).
- **0.5** Still to do: DOM snapshot tests for the existing form fields. Do this before Phase B's R1 refactor.
- **0.6** Still to do: consumer release note.
- Run `dotnet pack` once to confirm package validation passes against 1.6.0 (it needs nuget.org access).
