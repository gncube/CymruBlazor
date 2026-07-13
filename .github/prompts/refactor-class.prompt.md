# Refactor Class

Refactor the selected C# class into production-quality code.

## Workflow

```
Architect
    ↓
Refactoring Plan Generation Mode
    ↓
C# Expert (Refactoring)
    ↓
Testing
    ↓
Documentation Updates
    ↓
Deliver
```

## Core Principles

- Preserve behaviour and public API compatibility (unless instructed otherwise)
- Improve readability and reduce complexity
- Follow SOLID principles
- Prefer modern C# features
- Remove duplication
- Improve naming clarity
- Reduce allocations where practical
- Prefer primary constructors and file-scoped namespaces
- Prefer collection expressions
- When refactoring components, ensure `RenderFragment` usage and parameters remain stable
- Maintain accessibility behavior and ARIA attribute support

## Workflow Phases

### Phase 1: Architect

- Analyze current class or component responsibilities
- Identify violations of single-responsibility in components (too many UI concerns)
- Document complexity hotspots and places where composition is preferable
- Propose refactoring strategy without making changes
- Identify dependencies, demo impacts, and accessibility implications

### Phase 2: Refactoring Plan Generation Mode

**Input:** Analysis and refactoring strategy from Phase 1

**Deliverable:** Structured refactoring plan saved to `/plan/` directory

**Plan File Naming:** `refactor-[class-name]-[version].md`

**Plan Template Requirements:**

- Current state analysis (complexity metrics, SOLID violations, issues)
- Refactoring objectives and success criteria
- Detailed refactoring steps in logical sequence
- New class structure and responsibilities
- Method extractions and consolidations
- Naming improvements
- Dependency injection changes if applicable
- Breaking changes to public API (if any)
- Backwards compatibility strategy
- Test coverage requirements
- Validation approach

**Note:** Do NOT modify code in this phase; only generate the plan.

### Phase 3: C# Expert (Refactoring)

**Input:** Refactoring plan from Phase 2

**Deliverable:** Refactored production-quality code

**Execution:** Apply refactoring steps following the plan:

- Extract methods and responsibilities
- For components: extract child content into `RenderFragment` or helper components where appropriate
- Rename for clarity
- Consolidate duplication
- Apply modern C# idioms
- Optimize allocations where beneficial
- Preserve or clearly document public API/parameter changes

### Phase 4: Testing

- Verify behaviour preservation through existing tests
- Add tests for newly extracted methods if uncovered
- Validate complexity reduction measurably
- Confirm SOLID improvements
- Test edge cases and error paths

### Phase 5: Documentation Updates

**Deliverables:**

1. **Code Documentation**
   - Update XML doc comments for refactored methods
   - Document new internal helper methods
   - Clarify complex logic with comments

2. **Component & Architecture Documentation**
   - Update `CODEBASE_ARCHITECTURE.md` if refactoring establishes new pattern
   - Update `docs/components/[ComponentName].md` for components
   - Update demo pages in `src/CymruBlazor.Demo/` to reflect component changes
   - Document naming conventions applied

3. **ADR (if significant architectural change)**
   - Create `docs/architecture/adr/ADR-[sequence]-[decision-title].md` if refactoring represents significant change

### Phase 6: Deliver

- All tests passing
- Code follows conventions in CODEBASE_ARCHITECTURE.md
- Documentation complete
- Public API changes communicated
- Ready for PR review
