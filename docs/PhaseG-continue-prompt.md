# Phase G, continuing in a new session

You are continuing Phase G of CymruBlazor (.NET 10 Blazor library for the NHS Wales Design System, baseline 1.12.3, building towards 1.13.0).

## Read first (do not re-audit)

1. `docs/IMPLEMENTATION-PLAN.md`: the end of the file ("Phase G" onwards) has the scope, every hand-off result and the remaining tasks (TASK-114 to TASK-120).
2. `docs/Spike-G-A2-rule-builder-drag-reorder.md` and `docs/ADR/ADR-0003-testing-package-proposal.md`: the two decisions are made (build drag reorder; documentation first, then two packages).
3. `CHANGELOG.md` `[1.13.0] - Unreleased`.

## How we work

- There is no .NET SDK in your sandbox. You write the code; the maintainer runs `dotnet build`, `dotnet test` and `.\scripts\Run-AccessibilityTests.ps1` and pastes the output.
- Each hand-off is one zip of new and modified files only. Keep replies short and do not recap steps.
- The checkout is CRLF; preserve it when editing existing files (docs are LF).
- Warnings are errors in CI (watch CA1822, CA1859, CA1861, CA1720, CA1305, CA1826). Tokens only in CSS, only tokens defined in all three themes, no `opacity` dimming. Additive API only.
- Internal components cannot be used as Razor tags (RZ10012); create them with `RenderTreeBuilder.OpenComponent<T>`.
- Each phase ends with a conventional-commit message ending with these trailers:
  `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`
  `Claude-Session: <the new session's URL>`

## Done so far (all green)

Hand-offs 1 to 5: hygiene and baseline, CY0005 diagnostics, `CyRuleField.ValueEditor`, recipes documentation, `CyDataTable` `PageSizeOptions` / `OnSortChanged`. Hand-off 6 (spike report and ADR) is documents only; its commit is `docs: add G-A2 spike report and ADR-0003 testing package proposal`.

## Next

1. TASK-117: `docs/Testing.md` with a copyable bUnit base class and axe base class.
2. TASK-114 to 116: drag reorder in `CyRuleBuilder` (opt-in `AllowDragReorder`) with Playwright tests.
3. TASK-118 to 120: the two testing packages and the workflow changes.

Start with a one-sentence statement of what you are about to do, create the task list, then begin TASK-117.
