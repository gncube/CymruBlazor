# Git Branch Housekeeping & Consolidation Plan

**Repository:** `https://github.com/gncube/CymruBlazor`  
**Audit Date:** 2026-10-20  
**Baseline Branch:** `main` (`783ffc9` — `docs(changelog): finalize release 2.1.0 notes`, tagged `v2.1.0`)  
**Status:** Planning Only (No branches modified, merged, or deleted during audit)

---

## 1. Executive Summary

A full inspection of local (`refs/heads/*`) and remote (`refs/remotes/origin/*`) Git references was conducted against `main` (`783ffc9`, release `v2.1.0`) following `git fetch --all --prune`.

### Key Takeaways
1. **Unmerged Branches (`git branch -a --no-merged main`):**
   - **0 remote branches** on `origin` have unmerged commits.
   - **3 local-only branches** contain commits not reachable from `main` via Git ancestry:
     - `chore/v2.0.0-docs-and-housekeeping` (`4c526fe`, **1 commit ahead, 14 behind `main`**) — Contains valuable documentation and project-file housekeeping from the `v2.0.0` release alongside changes that have since been superseded by `main` (`208ce7e`, `a09caa8`). **Recommended Action: Selectively Port.**
     - `feature/phase-3-4-component-integration` (`0bd12bd`, **3 commits ahead, 332 behind `main`**) — Pre-history-import branch from July–August 2026 (`merge-base` `a593e42`). All source code and tests were imported into `main` via `f21a00d` / `03d1906` (PR #6) and heavily evolved through `v2.1.0`. Only 49 raw design screenshot PNGs (`~4.4 MB`) under `docs/Foundations/` are unique to this branch. **Recommended Action: Archive (optional bundle/export of screenshots) then Delete.**
     - `fix/warning-cleanup-and-test-activations` (`fe2cc1e`, **2 commits ahead, 332 behind `main`**) — Pre-history-import branch from July 2026 (`merge-base` `a593e42`). 100% of its code and test changes were incorporated into `f21a00d` and subsequently superseded on `main`. **Recommended Action: Delete.**
2. **Merged Stale Branches (`git branch -a --merged main`):**
   - **10 local branches** and **8 remote branches** on `origin` (representing **14 unique branch names** besides `main`) are **0 commits ahead** of `main`—their tips are already direct ancestors of `main`.
   - 1 local branch (`phase-f/v1.12.1-phase-f1`) tracks a remote branch that is already `[gone]`.
   - **Recommended Action:** **Delete** locally (and on `origin` when maintainer remote cleanup is authorised).

---

## 2. Verified Findings vs. Assumptions & Manual Review Flags

### 2.1 Verified Findings (Grounded in Git Telemetry)
- **Working tree state:** Clean (`On branch main`, aligned with `origin/main` at `783ffc9`, 0 stashes, 1 worktree).
- **Release state on `main`:** `v2.1.0` is the latest tag on `main` (`783ffc9`), preceded by `v2.0.1` (`dfa515b`), `v2.0.0` (`773830a`), and `v1.13.0` (`f6a1755`).
- **Why `feature/phase-3-4-component-integration` and `fix/warning-cleanup-and-test-activations` appear unmerged:** On `2026-08-15`–`2026-08-17`, repository history was consolidated via commits `f21a00d` (`fix(tests) ready to deliver dhcw single record`), `03d1906` (`Prepare import: track local changes`), and `9cadb33` (PR #6). Those commits imported the working tree of the Phase 3/4 and warning-cleanup work onto `main` without recording a Git merge parent to `384fbf6`, `fe2cc1e`, `98f4492`, or `0bd12bd`.
- **Scaffold Guide version drift on `main`:** `docs/CymruBlazor-Scaffold-Guide.md` on `main` still references `v1.5.0` and `Stable (1.x)` in Sections 0, 2, and 8. Updating this file (originally started in `chore/v2.0.0-docs-and-housekeeping`) is still needed on `main` and should target `v2.1.0`.
- **Conflicting file deletions in `chore/v2.0.0-docs-and-housekeeping`:**
  - `chore/v2.0.0-docs-and-housekeeping` deletes `RELEASE-NOTES-v1.13.0.md`. However, subsequent commits on `main` (`208ce7e` and `a09caa8`) added `RELEASE-NOTES-v2.0.0.md` at the repository root and updated `docs/IMPLEMENTATION-PLAN.md` (line 38) to explicitly cite `RELEASE-NOTES-v1.13.0.md and RELEASE-NOTES-v2.0.0.md` as completed deliverables.
  - `chore/v2.0.0-docs-and-housekeeping` deletes `docs/Spike-G-A2-rule-builder-drag-reorder.md`, which is still referenced three times in `docs/IMPLEMENTATION-PLAN.md` (lines 683, 811, and 864).

### 2.2 Assumptions
- **Assumption A1:** No external developer or open pull request is actively pushing to the 8 merged remote branches on `origin` (the newest commit among them is `2026-10-12`, and several date back to September 2026).
- **Assumption A2:** The 49 binary PNG files (`~4.4 MB`) in `docs/Foundations/` on `feature/phase-3-4-component-integration` were temporary design-handoff screenshots (including files named `Screenshot 2026-07-23 111741.png` and `icon-priview.png`) and were intentionally excluded when `03d1906` was imported to avoid bloating the Git repository and NuGet/source archives.
- **Assumption A3:** Root-level `RELEASE-NOTES-v*.md` files (`RELEASE-NOTES-v1.13.0.md`, `RELEASE-NOTES-v2.0.0.md`) are intended to remain tracked in the repository alongside `CHANGELOG.md`.

### 2.3 Branches Flagged for Manual Maintainer Review
| Branch | Flag Reason | Decision Needed from Maintainer |
|---|---|---|
| `chore/v2.0.0-docs-and-housekeeping` | Deletes `RELEASE-NOTES-v1.13.0.md` and `docs/Spike-G-A2-rule-builder-drag-reorder.md` | Confirm that `RELEASE-NOTES-v1.13.0.md` and `docs/Spike-G-A2-rule-builder-drag-reorder.md` should **remain** on `main` (recommended) while `docs/PhaseG-continue-prompt.md` is deleted. |
| `feature/phase-3-4-component-integration` | Contains 49 binary PNG design screenshots (`~4.4 MB`) under `docs/Foundations/` not present on `main` | Confirm whether `docs/Foundations/*.png` can be discarded or archived outside Git before deleting the local branch. |

---

## 3. Complete Branch Inventory

### 3.1 Unmerged Branches (`git branch -a --no-merged main`)

| Branch | Scope | Tip Commit | Last Commit Date | Merge Base with `main` | Ahead / Behind `main` | Recommended Action | Priority |
|---|---|---|---|---|---|---|---|
| `chore/v2.0.0-docs-and-housekeeping` | Local only | `4c526fe` | 2026-10-14 | `773830a` (`v2.0.0`) | **+1 / -14** | **Selectively Port** (then Delete) | **P1 (High)** |
| `feature/phase-3-4-component-integration` | Local only | `0bd12bd` | 2026-08-17 | `a593e42` | **+3 / -332** | **Archive & Delete** | **P2 (Medium)** |
| `fix/warning-cleanup-and-test-activations` | Local only | `fe2cc1e` | 2026-07-22 | `a593e42` | **+2 / -332** | **Delete** | **P2 (Medium)** |

### 3.2 Merged Stale Branches (`git branch -a --merged main`)

All branches listed below have `0` commits ahead of `main` (`git cherry main <branch>` is empty, and `git merge-base main <branch>` equals the branch tip):

| Branch Name | Presence | Tip Commit | Last Commit Date | Ahead / Behind `main` | Upstream Tracking State | Recommended Action | Priority |
|---|---|---|---|---|---|---|---|
| `chore/release-1.13.0-baselines` | Local + `origin` | `f6a1755` | 2026-10-12 | 0 / -16 | `origin/chore/release-1.13.0-baselines` | **Delete** | **P3 (Low)** |
| `phase-f/v1.12.1-phase-f1` | Local only | `2e38773` | 2026-10-09 | 0 / -41 | `origin/phase-f/v1.12.1-phase-f1: gone` | **Delete** | **P3 (Low)** |
| `fix/starterapp-cysidbar-state-api` | Local only | `679cbbb` | 2026-10-08 | 0 / -42 | None | **Delete** | **P3 (Low)** |
| `release/1.8.0-forms` | Local only | `423e8e1` | 2026-10-02 | 0 / -86 | None | **Delete** | **P3 (Low)** |
| `feature/deep-welsh-translation-high-traffic-components` | Local + `origin` | `c710876` | 2026-09-24 | 0 / -116 | `origin/feature/deep-welsh-translation-high-traffic-components` | **Delete** | **P3 (Low)** |
| `feature/bilingual-content-parity-and-tab-wiring` | Local + `origin` | `9827643` | 2026-09-24 | 0 / -118 | None (local ref matches remote tip) | **Delete** | **P3 (Low)** |
| `feature/docs-cross-linking-and-404-localisation` | Local + `origin` | `e81d021` | 2026-09-24 | 0 / -119 | None (local ref matches remote tip) | **Delete** | **P3 (Low)** |
| `phase2/information-architecture-sidebar-sequencing` | Local only | `550d45d` | 2026-09-24 | 0 / -126 | None | **Delete** | **P3 (Low)** |
| `chore/documentation-information-architecture-review` | Local only | `873ae18` | 2026-09-24 | 0 / -127 | None | **Delete** | **P3 (Low)** |
| `origin/bugfix/v1.3.0-localization-overlays-tooltips` | `origin` only | `37a69c0` | 2026-09-20 | 0 / -149 | Remote branch | **Delete** | **P3 (Low)** |
| `feature/v1.2.1-phase1-package-validation-cleanup` | Local only | `a1a6208` | 2026-09-19 | 0 / -163 | None | **Delete** | **P3 (Low)** |
| `origin/feature/cysidebar-rfc-states-mobile-drawer` | `origin` only | `202bb26` | 2026-09-18 | 0 / -188 | Remote branch | **Delete** | **P3 (Low)** |
| `origin/copilot/fix-failing-github-actions-job` | `origin` only | `93f697e` | 2026-09-11 | 0 / -246 | Remote branch | **Delete** | **P3 (Low)** |
| `origin/tests/add-accessibility-tests` | `origin` only | `0e5d2e4` | 2026-09-08 | 0 / -266 | Remote branch | **Delete** | **P3 (Low)** |

---

## 4. Detailed Branch-by-Branch Audit & Recommendations

### 4.1 `chore/v2.0.0-docs-and-housekeeping` (Unmerged — Local Only)

- **Recommended Action:** **Selectively Port** (then **Delete** local branch)
- **Commit History (`main..chore/v2.0.0-docs-and-housekeeping`):**
  - `4c526fe` (`2026-10-14 22:38:15 +0100`): `docs: update documentation for v2.0.0 release and clean up redundant files`
- **File-Level Comparison Against Current `main` (`783ffc9`):**

| File Touched in `4c526fe` | Change in `chore/v2.0.0-docs-and-housekeeping` | Status on `main` (`783ffc9`) | Porting Recommendation |
|---|---|---|---|
| `CONTRIBUTING.md` | Replaces `1.x patch and minor releases` with `patch and minor releases`, and replaces `ahead of a 2.0.0 removal? Add it to docs/MIGRATION-2.0.md` with version-neutral guidance. | Still contains `1.x` / `2.0.0` wording on `main`. | **Port to `main`.** Clean, accurate post-2.0 contributor guidance. |
| `src/CymruBlazor/CymruBlazor.csproj` | Updates XML comment on line 62 from `(1.x promises semver)` to `(promises semver)`. | Still says `(1.x promises semver)` on `main`. | **Port to `main`.** Non-functional comment cleanup. |
| `docs/Current-Solution-Structure.md` | Adds `ADR-0003-testing-package-proposal.md`, `CymruBlazor-Improvement-Recommendations.md`, `design-tokens.md`, `Diagnostics.md`, `IMPLEMENTATION-PLAN.md`, `Recipes.md`, `Testing.md`, and `CompatibilitySuppressions.xml` to the solution tree diagram. | Missing all 8 files in the tree diagram on `main`. | **Port to `main`.** Also verify whether any `v2.1.0` additions need listing. |
| `docs/CymruBlazor-Scaffold-Guide.md` | Updates stale `v1.5.0` / `Stable (1.x)` references to `v2.0.0` / `Stable (2.x)`, adds link to `MIGRATION-2.0.md`, and adds `CyDataTable<TItem>` to Section 8 (`Data Display`). | **Stale on `main`** (still says `1.5.0` and `Stable (1.x)`). | **Port to `main` & update version to `2.1.0`** so it matches the current release on `main`. |
| `docs/PhaseG-continue-prompt.md` | Deletes temporary 32-line AI session continuation prompt created during Phase G (`1.12.3` → `1.13.0`). | Still present on `main`; unreferenced anywhere else in the repo. | **Port deletion (`git rm`) to `main`.** |
| `docs/IMPLEMENTATION-PLAN.md` | Appends `### 2.0.0 Major Release shipped (2026-10-14)` summary section at the bottom of the file. | `main` (`a09caa8`) updated the top status table for R1, `2.0.0`, and `Post-2.0`, but lacks this narrative section at the bottom. | **Port only the bottom section** (`### 2.0.0 Major Release shipped (2026-10-14)`). Keep `main`'s top status table intact. |
| `README.md` | Updates status callout to `2.x`, adds `Testing Packages` and `Documentation` links, and changes `CHANGELOG.md` / `CONTRIBUTING.md` links to relative paths. | **Superseded on `main`** by `208ce7e`, which includes `dotnet add package` commands for the testing packages. | **Keep `main`'s version** (optionally port only the relative link cleanups for `CHANGELOG.md` and `CONTRIBUTING.md`). |
| `RELEASE-NOTES-v1.13.0.md` | Deletes the 79-line v1.13.0 release notes file from the repo root. | Tracked on `main` alongside `RELEASE-NOTES-v2.0.0.md` and cited as deliverable `0.6` in `docs/IMPLEMENTATION-PLAN.md`. | **Do NOT delete** (unless maintainer explicitly decides to remove all root `RELEASE-NOTES-*.md` files and update `docs/IMPLEMENTATION-PLAN.md`). |
| `docs/Spike-G-A2-rule-builder-drag-reorder.md` | Deletes the 49-line Phase G spike report. | Still referenced in `docs/IMPLEMENTATION-PLAN.md` (lines 683, 811, 864). | **Keep on `main`** to prevent broken internal doc references (or update `IMPLEMENTATION-PLAN.md` if deleting). |

- **Dependencies:** None.
- **Risks:**
  - Directly merging `chore/v2.0.0-docs-and-housekeeping` with `git merge` would cause merge conflicts in `README.md` and `docs/IMPLEMENTATION-PLAN.md`, regress `README.md`'s `Testing Packages` code block, and delete `RELEASE-NOTES-v1.13.0.md`.
  - Selective porting onto a fresh branch off `main` eliminates all conflict and regression risks.
- **Acceptance Criteria:**
  - `CONTRIBUTING.md`, `src/CymruBlazor/CymruBlazor.csproj`, `docs/Current-Solution-Structure.md`, and `docs/CymruBlazor-Scaffold-Guide.md` (updated for `2.1.0`) reflect the 2.x architecture.
  - `docs/PhaseG-continue-prompt.md` is removed.
  - `docs/IMPLEMENTATION-PLAN.md` retains `main`'s Post-2.0 status table and includes the `### 2.0.0 Major Release shipped (2026-10-14)` section at the end.
  - No broken markdown links exist across `README.md`, `CONTRIBUTING.md`, or `docs/`.
  - `dotnet build -c Release` and `dotnet test -c Release` pass with zero warnings.

---

### 4.2 `feature/phase-3-4-component-integration` (Unmerged — Local Only)

- **Recommended Action:** **Archive & Delete** (Create a lightweight archive tag or export `docs/Foundations/*.png` if needed externally, then delete the local branch)
- **Commit History (`main..feature/phase-3-4-component-integration`):**
  - `384fbf6` (`2026-07-22 11:58:55 +0100`): `feat: complete Phase 3 & 4 component integration`
  - `98f4492` (`2026-07-24 19:28:25 +0100`): `fix(tests): resolve nullable mismatch in TestFormModel properties`
  - `0bd12bd` (`2026-08-17 12:49:56 +0100`): `chore(dependencies): update package versions and remove unused using directives`
- **Audit Findings & Rationale:**
  - **Code & Tests:** Every component introduced in `384fbf6` and `98f4492` (`CyCheckbox`, `CySelect`, `CyTextBox`, `CyValidationSummary`, `CyBreadcrumb`, `CyFooter`, `CyHeroBanner`, `CyNavigation`, `CyPageHeader`, `CySkipLink`, `CyIcon`, `IconRegistry`, CSS tokens, and bUnit tests) was imported into `main` via commits `f21a00d` and `03d1906` on `2026-08-15`–`2026-08-17`.
  - **Massive Divergence:** `main` is **332 commits ahead** and has since shipped `v1.5.0` through `v2.1.0` (including the R1 `CyFormFieldComponentBase` refactor, `v2.0.0` breaking API cleanup, and Central Package Management updates).
  - **Unique Files on Branch:** Commit `98f4492` committed **49 binary PNG images (`~4.4 MB`)** under `docs/Foundations/` (`Brandings/`, `Colours/`, `Grid & Layout System/`, `Icons/`, `Select/`, `Spacing & Elevation/`, `Tags/`, `Toggles/`, `Typography/`). Several filenames contain typos or raw desktop timestamps (`docs/Foundations/Icons/icon-priview.png`, `docs/Foundations/Icons/Screenshot 2026-07-23 111741.png`, `docs/Foundations/Spacing & Elevation/sematic-tokens.png`).
  - **Why Not Merge or Port to `main`:** All design token specifications are now canonically documented in `docs/design-tokens.md` and interactive Demo pages (`src/CymruBlazor.Demo`). Merging 4.4 MB of unreferenced static PNGs into `main` would bloat repository clone size and distribution archives (`scripts/Pack-Solution.ps1`).
- **Dependencies:** Confirm with maintainer whether `docs/Foundations/` PNGs should be exported to an external folder or Git bundle before deleting the branch.
- **Risks:** Deleting without archiving would permanently lose the 49 PNG screenshots once Git garbage collection runs (`git gc`). Creating a `git bundle` backup outside the repository before branch deletion eliminates data-loss risk completely.
- **Acceptance Criteria:**
  - Backup bundle created (or `docs/Foundations/` exported outside repo if desired by maintainer).
  - Local branch `feature/phase-3-4-component-integration` deleted.

---

### 4.3 `fix/warning-cleanup-and-test-activations` (Unmerged — Local Only)

- **Recommended Action:** **Delete** (optionally included in the pre-cleanup Git bundle backup)
- **Commit History (`main..fix/warning-cleanup-and-test-activations`):**
  - `384fbf6` (`2026-07-22 11:58:55 +0100`): `feat: complete Phase 3 & 4 component integration`
  - `fe2cc1e` (`2026-07-22 14:40:39 +0100`): `feat: add core accessibility, theming, and form components with focus management support`
- **Audit Findings & Rationale:**
  - Shares commit `384fbf6` with `feature/phase-3-4-component-integration`.
  - Commit `fe2cc1e` modified 15 files: nullability/analyzer cleanups in `FocusManager.cs`, `CyLiveRegion.razor.cs`, `FocusTrap.razor.cs`, `CyFormFieldComponentBase.cs`, `CssBundler/Program.cs`, `CA1859` attributes in `ComponentIdGeneratorTests.cs`, and initial test classes in `CymruBlazor.AccessibilityTests` and `CymruBlazor.ApprovalTests`.
  - **100% Superseded:** Diffing `fe2cc1e` against `f21a00d` (the August 15 import commit on `main`) confirms that every single change from `fe2cc1e` was carried verbatim into `f21a00d`. Subsequently on `main`, `CymruBlazor.ApprovalTests` was intentionally replaced by bUnit structural DOM snapshot tests (`a1a6208`, `TASK-107`), and `CymruBlazor.AccessibilityTests` was expanded into the 219-test Playwright + axe-core suite.
- **Dependencies:** None.
- **Risks:** None (zero unique unported work).
- **Acceptance Criteria:**
  - Local branch `fix/warning-cleanup-and-test-activations` is deleted cleanly after creating the safety bundle.

---

### 4.4 Merged Stale Local & Remote Branches (10 Local, 8 Remote)

- **Recommended Action:** **Delete**
- **Branches Covered:**
  - **Local + Remote (4 pairs):**
    - `chore/release-1.13.0-baselines` (`f6a1755`)
    - `feature/bilingual-content-parity-and-tab-wiring` (`9827643`)
    - `feature/deep-welsh-translation-high-traffic-components` (`c710876`)
    - `feature/docs-cross-linking-and-404-localisation` (`e81d021`)
  - **Local Only (6 branches):**
    - `phase-f/v1.12.1-phase-f1` (`2e38773`, upstream `[gone]`)
    - `fix/starterapp-cysidbar-state-api` (`679cbbb`)
    - `release/1.8.0-forms` (`423e8e1`)
    - `phase2/information-architecture-sidebar-sequencing` (`550d45d`)
    - `chore/documentation-information-architecture-review` (`873ae18`)
    - `feature/v1.2.1-phase1-package-validation-cleanup` (`a1a6208`)
  - **Remote Only (4 branches):**
    - `origin/bugfix/v1.3.0-localization-overlays-tooltips` (`37a69c0`)
    - `origin/feature/cysidebar-rfc-states-mobile-drawer` (`202bb26`)
    - `origin/copilot/fix-failing-github-actions-job` (`93f697e`)
    - `origin/tests/add-accessibility-tests` (`0e5d2e4`)
- **Audit Findings & Rationale:**
  - Every branch in this group has `merge-base(main, branch) == tip(branch)`.
  - Zero commits exist on any of these branches that are not already part of `main`'s commit graph.
  - Safe deletion can be enforced using `git branch -d` (lowercase `-d`), which Git refuses to execute unless the branch is confirmed merged into the current HEAD.
- **Dependencies:** Remote branch deletion on `origin` should only be executed by the maintainer after verifying no open PRs reference those branches on GitHub.
- **Risks:** None for local deletion (`git branch -d` guarantees merge safety).
- **Acceptance Criteria:**
  - `git branch` lists only `main` once housekeeping is complete.

---

## 5. Prioritised Implementation Plan & Order of Execution

Note: No critical bugfix branches exist among the unmerged refs; all prior bugfix branches (`fix/starterapp-cysidbar-state-api`, `origin/bugfix/v1.3.0-localization-overlays-tooltips`, `origin/copilot/fix-failing-github-actions-job`) are already merged into `main`. The execution order therefore prioritises **preserving and porting active documentation/housekeeping updates first**, followed by **archiving/cleaning up unmerged historical branches**, and finally **pruning merged stale branches**.

### Phase 1 (P0 — Pre-Flight Safety Backup)
1. **Create an offline Git bundle backup of all unmerged branches** outside the working tree before touching any refs.
   - Ensures 100% recoverability of `chore/v2.0.0-docs-and-housekeeping`, `feature/phase-3-4-component-integration` (including `docs/Foundations/*.png`), and `fix/warning-cleanup-and-test-activations` without polluting repository tags or remotes.

### Phase 2 (P1 — Valuable Documentation & Housekeeping Port)
2. **Selectively port `chore/v2.0.0-docs-and-housekeeping` (`4c526fe`) onto a new branch from `main` (`docs/v2.1-housekeeping-port`):**
   - Port `CONTRIBUTING.md`, `src/CymruBlazor/CymruBlazor.csproj`, and `docs/Current-Solution-Structure.md` from `4c526fe`.
   - Port `docs/CymruBlazor-Scaffold-Guide.md` from `4c526fe` and update the target package version examples from `2.0.0` to `2.1.0`.
   - Append the `### 2.0.0 Major Release shipped (2026-10-14)` narrative block to `docs/IMPLEMENTATION-PLAN.md` while keeping `main`'s updated top status table.
   - Remove `docs/PhaseG-continue-prompt.md` (`git rm docs/PhaseG-continue-prompt.md`).
   - Retain `RELEASE-NOTES-v1.13.0.md`, `docs/Spike-G-A2-rule-builder-drag-reorder.md`, and `main`'s `README.md` (applying only relative link cleanups if desired).
   - Run full build, test, and package validation checks, then open PR and merge into `main`.

### Phase 3 (P2 — Resolve Unmerged Pre-Import Branches)
3. **Delete local unmerged branches after Phase 1 backup & Phase 2 merge:**
   - Delete `chore/v2.0.0-docs-and-housekeeping` (`git branch -D chore/v2.0.0-docs-and-housekeeping`).
   - Delete `fix/warning-cleanup-and-test-activations` (`git branch -D fix/warning-cleanup-and-test-activations`).
   - Delete `feature/phase-3-4-component-integration` (`git branch -D feature/phase-3-4-component-integration`) after confirming the Phase 1 bundle backup is saved.

### Phase 4 (P3 — Prune Merged Stale Branches)
4. **Delete the 10 local branches already merged into `main`** using safe `git branch -d`.
5. **(Optional / Maintainer-Only) Delete the 8 merged remote branches on `origin`** after verifying no open GitHub PRs depend on them, followed by `git fetch --all --prune`.

---

## 6. Validation Strategy

After executing Phase 2 (selective port of `chore/v2.0.0-docs-and-housekeeping` onto a fresh branch from `main`), run the following validation suite before merging to `main`:

```powershell
# 1. Verify working tree and diff scope (only expected docs and .csproj comment changed)
git status
git diff --stat main

# 2. Verify LF line endings on modified Markdown files and CRLF on .csproj per repository conventions
git diff --check main

# 3. Build solution in Release configuration (warnings-as-errors enforced by CI/Directory.Build.props)
dotnet build CymruBlazor.slnx -c Release

# 4. Run full bUnit / xUnit unit test suite
dotnet test tests/CymruBlazor.Tests/CymruBlazor.Tests.csproj -c Release --no-build

# 5. Run NuGet package build & API baseline package validation
dotnet pack src/CymruBlazor/CymruBlazor.csproj -c Release --no-build
dotnet pack src/CymruBlazor.Testing/CymruBlazor.Testing.csproj -c Release --no-build
dotnet pack src/CymruBlazor.Testing.Axe/CymruBlazor.Testing.Axe.csproj -c Release --no-build
```

---

## 7. Safe Execution & Cleanup Commands Reference

> [!IMPORTANT]
> The commands below are provided for **execution after plan approval**. None of these mutating commands have been executed during this planning audit.

### 7.1 Phase 1: Create Non-Destructive Offline Backup Bundle
```powershell
# Save a complete Git bundle of the 3 unmerged branches outside the repo before any deletion
$backupDir = Join-Path $HOME "CymruBlazor-branch-backups"
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

git bundle create "$backupDir/cymrublazor-unmerged-2026-10-20.bundle" `
    main `
    chore/v2.0.0-docs-and-housekeeping `
    feature/phase-3-4-component-integration `
    fix/warning-cleanup-and-test-activations

# Verify the bundle integrity
git bundle verify "$backupDir/cymrublazor-unmerged-2026-10-20.bundle"
```

### 7.2 Phase 2: Selectively Port `chore/v2.0.0-docs-and-housekeeping`
```powershell
# 1. Create a fresh branch from latest main
git switch -c docs/v2.1-housekeeping-port main

# 2. Checkout the cleanly portable files directly from 4c526fe
git checkout 4c526fe -- `
    CONTRIBUTING.md `
    src/CymruBlazor/CymruBlazor.csproj `
    docs/Current-Solution-Structure.md `
    docs/CymruBlazor-Scaffold-Guide.md

# 3. Remove the obsolete Phase G session continuation prompt
git rm docs/PhaseG-continue-prompt.md

# 4. After updating v2.0.0 -> v2.1.0 in docs/CymruBlazor-Scaffold-Guide.md and appending
#    the "2.0.0 Major Release shipped (2026-10-14)" section to docs/IMPLEMENTATION-PLAN.md:
git add CONTRIBUTING.md src/CymruBlazor/CymruBlazor.csproj docs/
git commit -m "docs: port v2.x housekeeping updates and remove stale Phase G prompt"
```

### 7.3 Phase 3: Delete Unmerged Local Branches (After Backup & Port)
```powershell
# Switch back to main first
git switch main

# Delete the 3 unmerged local branches (requires -D because their original commit SHAs are not ancestors of main)
git branch -D chore/v2.0.0-docs-and-housekeeping
git branch -D fix/warning-cleanup-and-test-activations
git branch -D feature/phase-3-4-component-integration
```

### 7.4 Phase 4: Delete Merged Local Branches (Safe `-d` Flag)
```powershell
# Safe delete (-d) succeeds ONLY because all 10 branches are verified ancestors of main
git branch -d `
    chore/documentation-information-architecture-review `
    chore/release-1.13.0-baselines `
    feature/bilingual-content-parity-and-tab-wiring `
    feature/deep-welsh-translation-high-traffic-components `
    feature/docs-cross-linking-and-404-localisation `
    feature/v1.2.1-phase1-package-validation-cleanup `
    fix/starterapp-cysidbar-state-api `
    phase-f/v1.12.1-phase-f1 `
    phase2/information-architecture-sidebar-sequencing `
    release/1.8.0-forms
```

### 7.5 Optional Maintainer-Only Remote Cleanup (Only If Explicitly Authorised)
```powershell
# Optional: Only run if maintainer decides to prune the 8 merged branches on origin
# (Constraint reminder: Never force-push or delete remote branches without maintainer execution)
git push origin --delete `
    bugfix/v1.3.0-localization-overlays-tooltips `
    chore/release-1.13.0-baselines `
    copilot/fix-failing-github-actions-job `
    feature/bilingual-content-parity-and-tab-wiring `
    feature/cysidebar-rfc-states-mobile-drawer `
    feature/deep-welsh-translation-high-traffic-components `
    feature/docs-cross-linking-and-404-localisation `
    tests/add-accessibility-tests

git fetch --all --prune
```
