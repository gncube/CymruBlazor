# Review Architecture

Review a pull request or codebase against CymruBlazor component-library standards.

## Workflow

```
Load coding-standards.skill
    +
Load skills/testing/SKILL.md
    +
Load documentation.skill
    ↓
Review Against Standards
    ├─ Component API clarity (props/parameters)
    ├─ Accessibility (WCAG 2.2 AA, ARIA, keyboard)
    ├─ Design token & theming usage
    ├─ Composition & RenderFragment patterns
    ├─ CSS class composition and deterministic class builders
    ├─ Event callbacks and EventCallback usage
    ├─ bUnit testing coverage and meaningful scenarios
    ├─ Documentation & demo examples
    └─ Architecture consistency for component library
    ↓
Generate Review Report
    ↓
Suggest Improvements
    ↓
Identify Missing Tests
    ↓
Identify Missing ADRs
    ↓
Identify Documentation Gaps
```

## Review Criteria

### 1. Component API & Organization

**Check:**

- ✓ Component placed in the appropriate `src/CymruBlazor/Components/[Category]/` folder
- ✓ Parameters are minimal, well-documented, and use sensible defaults
- ✓ Child content exposed as `RenderFragment` where applicable
- ✓ Tests and demo examples co-located or referenced in `src/CymruBlazor.Demo/`

**Violations:**

- ❌ Components with large parameter sets or boolean-flag explosion
- ❌ Multiple responsibilities (UI + data-fetching) in a single component
- ❌ Missing demo or usage examples

### 2. Accessibility

**Check:**

- ✓ ARIA roles and labels present where needed
- ✓ Keyboard navigation and focus management verified
- ✓ Contrast and color token usage meets WCAG AA
- ✓ Screen reader behavior tested in demos or bUnit

**Violations:**

- ❌ Missing or incorrect ARIA attributes
- ❌ Keyboard traps or missing keyboard support
- ❌ Color token misuse causing low contrast

### 3. SOLID Principles

**Check:**

- ✓ **S**ingle Responsibility: Each class has one reason to change
- ✓ **O**pen/Closed: Open for extension, closed for modification
- ✓ **L**iskov Substitution: Implementations are substitutable
- ✓ **I**nterface Segregation: Clients depend on specific interfaces
- ✓ **D**ependency Inversion: Depend on abstractions, not concretions

**Anti-Patterns:**

- ❌ God objects with multiple responsibilities
- ❌ Static dependencies
- ❌ Tight coupling to concrete classes
- ❌ Fat interfaces

### 4. Design Tokens & Theming

**Check:**

- ✓ Components consume design tokens (colors, spacing, typography)
- ✓ Theming switch works through cascading parameters or provider
- ✓ No hard-coded colors or spacing values

**Violations:**

- ❌ Hard-coded visual values
- ❌ Tokens used inconsistently across variants

### 5. Coding Standards

**Check:**

- ✓ File-scoped namespaces
- ✓ Primary constructors for DI
- ✓ Records for immutable data
- ✓ Sealed classes by default
- ✓ PascalCase for public, \_camelCase for private
- ✓ XML documentation for public APIs
- ✓ No static analyzer warnings (CA1510, CA1805, etc.)
- ✓ Async/await for all I/O
- ✓ Nullable reference types enabled
- ✓ Modern C# idioms (collection expressions, pattern matching)

**Violations:**

- ❌ Class-based namespaces
- ❌ Property injection
- ❌ Mutable classes where records should be used
- ❌ Public-facing unsealed classes
- ❌ Inconsistent naming
- ❌ Missing documentation
- ❌ Static analyzer warnings

### 6. Performance & Rendering

**Check:**

- ✓ Avoid unnecessary renders for high-frequency updates
- ✓ Use `@key` where list identity matters
- ✓ Consider `ShouldRender()` for expensive components

**Violations:**

- ❌ Frequent full-tree re-renders for minor prop changes
- ❌ Heavy synchronous work on render path
- ❌ JSInterop causing layout thrashing

### 7. Testing Coverage (bUnit + Unit)

**Check:**

- ✓ bUnit tests for render outcomes and interactions
- ✓ Unit tests for helper classes and token mappers
- ✓ Accessibility assertions via testing library or bUnit
- ✓ Demo scenarios covered in storybook-like pages or demo app

**Violations:**

- ❌ No bUnit tests for interactive behavior
- ❌ Missing accessibility assertions
- ❌ Only snapshot tests without behavior assertions

### 8. Documentation & Demo

**Check:**

- ✓ Component docs exist in `docs/components/[ComponentName].md`
- ✓ Demo pages in `src/CymruBlazor.Demo/` showcase variants and accessibility
- ✓ XML comments for public APIs where applicable

**Violations:**

- ❌ Missing usage examples or demo
- ❌ Docs not updated to reflect props/variants

### 9. ADR Compliance

**Check:**

- ✓ Architectural decisions documented in ADRs
- ✓ ADRs follow template (Status, Context, Decision, Consequences, Alternatives)
- ✓ Related ADRs linked
- ✓ Implementation notes included
- ✓ Alternatives considered documented

**Violations:**

- ❌ No ADR for significant decisions
- ❌ Incomplete ADR information
- ❌ ADR not linked from code/docs

### 10. Component Library Consistency

**Check:**

- ✓ Follows component folder and naming conventions
- ✓ Reuses existing helper components (Button, Icon, etc.) where appropriate
- ✓ Styling and tokens consistent across variants
- ✓ Tests and demos follow established examples

**Violations:**

- ❌ New visual patterns without documentation or ADR
- ❌ Inconsistent naming or token usage
- ❌ Duplicate implementations of same component

## Review Report Structure

### Executive Summary

**Overall Assessment:** ✓ Approve | ⚠️ Needs Changes | ✗ Request Changes

**Key Strengths:**

- Strength 1
- Strength 2

**Major Concerns:**

- Concern 1
- Concern 2

---

### Detailed Findings

#### 1. Vertical Slice

**Status:** ✓ Pass | ⚠️ Partial | ✗ Fail

**Findings:**

- Finding ID: VSA-001
- Category: Folder Organization
- Severity: Medium
- Description: ...
- Recommendation: ...
- Trade-offs: ...

#### 2. CQRS

**Status:** ✓ Pass

#### 3. SOLID Principles

**Status:** ⚠️ Partial

**Findings:**

- Finding ID: SOLID-001
- Principle: Single Responsibility
- Severity: Medium
- Description: ...

#### 4. Domain-Driven Design

**Status:** ✓ Pass

#### 5. Coding Standards

**Status:** ✓ Pass

#### 6. Authorization & Authentication

**Status:** ✓ Pass

#### 7. Testing

**Status:** ⚠️ Partial

**Findings:**

- Missing integration tests for endpoint
- Need tests for error scenarios
- Authorization not tested

#### 8. Documentation

**Status:** ✗ Fail

**Findings:**

- No ADR for architectural decision
- XML documentation missing
- Feature docs not updated

#### 9. ADR Compliance

**Status:** ⚠️ Partial

#### 10. Architecture Consistency

**Status:** ✓ Pass

---

### Statistics

| Metric                 | Value |
| ---------------------- | ----- |
| Files Changed          | 25    |
| Lines Added            | 1,240 |
| Lines Removed          | 340   |
| Complexity Change      | +2.3  |
| Test Coverage          | 82%   |
| Documentation Coverage | 88%   |

---

### Required Actions

**Before Merge:**

1. [ ] Create ADR for architectural decision (TASK-001)
2. [ ] Add integration tests for endpoints (TASK-002)
3. [ ] Update feature documentation (TASK-003)
4. [ ] Add XML comments to public APIs (TASK-004)

**Suggested Improvements:**

1. [ ] Refactor PatientService to single responsibility (OPTIONAL)
2. [ ] Add caching for patient queries (OPTIONAL)
3. [ ] Consider repository pattern for... (OPTIONAL)

---

### Additional Notes

- Architecture is sound
- Follows conventions well
- Good test coverage
- Documentation needs attention

**Approved by:** [Architect Name]  
**Date:** [Date]  
**PR:** [Link]

## When to Use This Prompt

Use `review-architecture.prompt` for:

- Pull request architectural review
- Feature branch readiness check
- Architecture compliance audit
- Code review before merge
- Codebase health check
- New team member onboarding review

## Example Invocation

```
Review this pull request against HealthPassport Architecture:

PR: https://github.com/...
Branch: feature/appointment-scheduling

Check against:
✓ Vertical Slice organization
✓ CQRS pattern
✓ SOLID principles
✓ Domain-Driven Design
✓ Coding standards
✓ Authorization/Authentication
✓ Testing coverage
✓ Documentation
✓ ADR compliance
✓ Architecture consistency

Return structured review report with:
- Executive summary
- Detailed findings by category
- Required actions
- Suggested improvements
- Statistics
```

## Resources

- [HealthPassport Architecture Skill](../../skills/healthpassport-architecture.skill.md)
- [Vertical Slice Skill](../../skills/vertical-slice.skill.md)
- [Coding Standards Skill](../../skills/coding-standards.skill.md)
- [DDD Skill](../../skills/ddd.skill.md)
- [Documentation Standards](../../skills/documentation.skill.md)
- [CODEBASE_ARCHITECTURE.md](../../CODEBASE_ARCHITECTURE.md)
