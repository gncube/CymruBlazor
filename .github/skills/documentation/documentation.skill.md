---
title: HealthPassport Documentation Standards
description: Documentation formats, ADRs, architecture diagrams, and knowledge capture
applies_to: ["docs/**/*", "**/*.md"]
requires:
  - .github/skills/healthpassport-architecture.skill.md
---

# Documentation Standards

Documentation is not optional—it's a first-class requirement. Every feature, change, and decision must be documented. Documentation should explain "why," not just "what."

## Core Principle

Good documentation makes the codebase easier to understand and maintain. Poor documentation forces future developers to reverse-engineer intent from code.

## Required Documentation by Change Type

### New Feature

- **Feature documentation** (`docs/[FeatureName].md`): Overview, user flows, configuration
- **API documentation** (`docs/API.md`): HTTP endpoints, request/response examples
- **Code comments** (XML docs, inline): Non-obvious decisions
- **README update:** How to use the feature
- **ADR (if architectural decision):** Why the design was chosen

### Architectural Change

- **ADR (Architectural Decision Record):** Why the decision, alternatives considered, consequences
- **Update CODEBASE_ARCHITECTURE.md:** Reflect the new architecture
- **C4 diagram** (if visible change): System context and containers
- **Arc42 sections:** Update relevant sections

### Bug Fix

- **Inline comment:** Why the bug occurred and how it's fixed
- **Test:** Regression test demonstrating the fix
- **Changelog:** Reference to issue
- **ADR (if systemic issue):** Root cause and prevention strategy

### Refactoring

- **No new documentation needed** (behavior unchanged)
- **Inline comments:** Only if non-obvious changes
- **Tests pass:** Prove behavior is preserved
- **Update docs if public API changes:** Mark deprecations

## Architectural Decision Records (ADRs)

Every architectural decision **must** have an ADR. ADRs live in `docs/architecture/adr/`.

### ADR File Naming

```
docs/architecture/adr/ADR-[SEQUENCE]-[decision-title].md

Example:
docs/architecture/adr/ADR-0004-vertical-slices-with-scoped-mediator.md
```

### ADR Template

```markdown
# ADR-[SEQUENCE]: [Decision Title]

## Status

Accepted | Proposed | Deprecated | Superseded by ADR-[X]

## Context

The context section describes the problem we're trying to solve.
What forces are acting upon us?

- Force 1
- Force 2
- Force 3

Include relevant constraints, requirements, and stakeholder concerns.

## Decision

The decision we have made to address the forces.

We will...

- Decision point 1
- Decision point 2

## Consequences

### Positive

- Benefit 1
- Benefit 2

### Negative

- Trade-off 1
- Trade-off 2

### Risks

- Risk 1 and mitigation
- Risk 2 and mitigation

## Alternatives Considered

### Alternative 1: [Name]

Why we didn't choose this...

### Alternative 2: [Name]

Why we didn't choose this...

## Related ADRs

- ADR-0001: Related decision
- ADR-0002: Related decision

## Implementation Notes

When and how this ADR was implemented.

## See Also

- [CODEBASE_ARCHITECTURE.md](../CODEBASE_ARCHITECTURE.md)
- [Feature Documentation](../[Feature].md)
```
