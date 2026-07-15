---
goal: Implement Core Component Foundation and Base Component Architecture for CymruBlazor
version: 1.0.0
date_created: 2026-07-14
owner: CymruBlazor Core Architecture Team
status: Planned
tags: [architecture, components, blazor, accessibility, design-system]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This implementation plan establishes the reusable component foundation for CymruBlazor. The goal is to create a consistent, accessible, strongly typed component model that all future UI components inherit from. This phase introduces the component infrastructure, shared abstractions, CSS isolation conventions, accessibility primitives, and common APIs required to build a maintainable enterprise component library.

---

## 1. Requirements & Constraints

- **REQ-CMP-001**: Every public component shall inherit from a common base class where appropriate.
- **REQ-CMP-002**: All components shall use Razor CSS Isolation (`Component.razor.css`).
- **REQ-CMP-003**: Components shall not depend on Bootstrap classes or JavaScript frameworks.
- **REQ-CMP-004**: Components shall consume only semantic `--cymru-*` design tokens.
- **REQ-CMP-005**: Components shall support WCAG 2.2 AA keyboard navigation.
- **REQ-CMP-006**: Components shall expose strongly typed parameters rather than string-based APIs wherever possible.
- **REQ-CMP-007**: Components shall support theme switching without requiring re-rendering.
- **ACC-001**: Every interactive component shall expose the correct ARIA attributes.
- **PAT-001**: Follow the Composition over Inheritance principle where possible.
- **PAT-002**: Follow Microsoft's Razor Component Authoring guidance.
- **PAT-003**: Use C#-first APIs. JavaScript shall only be introduced when browser APIs are unavailable.
- **CON-001**: Components shall not manipulate the DOM directly.
- **CON-002**: All browser interaction shall occur through service abstractions.

---

# 2. Implementation Steps

## Implementation Phase 5.1: Component Infrastructure

- GOAL-001: Create the reusable component foundation that all CymruBlazor components inherit.

| Task     | Description                                                                                                                                                                                | Completed | Date       |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------- | ---------- |
| TASK-001 | Create `src/CymruBlazor/Components/Core/CymruComponentBase.cs` deriving from `ComponentBase` and providing common lifecycle functionality.                                                 | ✅        | 2026-07-14 |
| TASK-002 | Create `src/CymruBlazor/Components/Core/CymruInteractiveComponentBase.cs` for interactive controls supporting Disabled, Id, Class, Style, AdditionalAttributes, and accessibility helpers. | ✅        | 2026-07-14 |
| TASK-003 | Create `src/CymruBlazor/Components/Core/CssBuilder.cs` for deterministic CSS class composition.                                                                                            | ✅        | 2026-07-14 |
| TASK-004 | Create `src/CymruBlazor/Components/Core/StyleBuilder.cs` for deterministic inline style generation.                                                                                        | ✅        | 2026-07-14 |
| TASK-005 | Create `src/CymruBlazor/Components/Core/ComponentIdGenerator.cs` for deterministic element IDs.                                                                                            | ✅        | 2026-07-15 |
| TASK-006 | Create `src/CymruBlazor/Components/Core/AriaAttributes.cs` helper methods.                                                                                                                 | ✅        | 2026-07-15 |

---

## Implementation Phase 5.2: Common Component Contracts

- GOAL-002: Define reusable component interfaces.

| Task     | Description                   | Completed | Date       |
| -------- | ----------------------------- | --------- | ---------- |
| TASK-007 | Create `IHasSize`.            | ✅        | 2026-07-15 |
| TASK-008 | Create `IHasColour`.          | ✅        | 2026-07-15 |
| TASK-009 | Create `IHasVariant`.         | ✅        | 2026-07-15 |
| TASK-010 | Create `IHasIcon`.            | ✅        | 2026-07-15 |
| TASK-011 | Create `IHasDisabledState`.   | ✅        | 2026-07-15 |
| TASK-012 | Create `IHasValidationState`. | ✅        | 2026-07-15 |

---

## Implementation Phase 5.3: Shared Enumerations

- GOAL-003: Eliminate magic strings throughout the component library.

| Task     | Description                     | Completed | Date       |
| -------- | ------------------------------- | --------- | ---------- |
| TASK-013 | Create `ComponentSize.cs`.      | ✅        | 2026-07-15 |
| TASK-014 | Create `ComponentVariant.cs`.   | ✅        | 2026-07-15 |
| TASK-015 | Create `ComponentColour.cs`.    | ✅        | 2026-07-15 |
| TASK-016 | Create `ComponentElevation.cs`. | ✅        | 2026-07-15 |
| TASK-017 | Create `IconPosition.cs`.       | ✅        | 2026-07-15 |
| TASK-018 | Create `ValidationState.cs`.    | ✅        | 2026-07-15 |

---

## Implementation Phase 5.4: Layout Components

- GOAL-004: Wrap the Phase 4 CSS layout primitives in strongly typed Razor components.

| Task     | Description           | Completed | Date |
| -------- | --------------------- | --------- | ---- |
| TASK-019 | Create `CyContainer`. | ✅        | 2026-07-15 |
| TASK-020 | Create `CyStack`.     | ✅        | 2026-07-15 |
| TASK-021 | Create `CyGrid`.      | ✅        | 2026-07-15 |
| TASK-022 | Create `CyCluster`.   |           |      |
| TASK-023 | Create `CySidebar`.   |           |      |
| TASK-024 | Create `CyCenter`.    |           |      |

---

## Implementation Phase 5.5: Accessibility Infrastructure

- GOAL-005: Standardise accessibility support across all components.

| Task     | Description                                | Completed | Date |
| -------- | ------------------------------------------ | --------- | ---- |
| TASK-025 | Create keyboard navigation helper service. |           |      |
| TASK-026 | Create FocusManager service abstraction.   |           |      |
| TASK-027 | Create FocusTrap component.                |           |      |
| TASK-028 | Create LiveRegion component.               |           |      |
| TASK-029 | Create ScreenReaderOnly component.         |           |      |

---

## Implementation Phase 5.6: Component Testing Foundation

- GOAL-006: Establish reusable testing infrastructure.

| Task     | Description                                   | Completed | Date |
| -------- | --------------------------------------------- | --------- | ---- |
| TASK-030 | Create bUnit test infrastructure.             |           |      |
| TASK-031 | Create ApprovalTests snapshot infrastructure. |           |      |
| TASK-032 | Create accessibility verification helpers.    |           |      |
| TASK-033 | Create shared component test base.            |           |      |

---

# 3. Alternatives

- **ALT-001**: Create each component independently without a shared base. Rejected due to duplication and inconsistent APIs.
- **ALT-002**: Use Bootstrap utility classes internally. Rejected because CymruBlazor must remain framework-independent.
- **ALT-003**: Use reflection-based component metadata. Rejected due to runtime cost and complexity.

---

# 4. Dependencies

- **DEP-001**: Phase 4 CSS Architecture.
- **DEP-002**: ThemeService.
- **DEP-003**: Design Tokens.
- **DEP-004**: .NET 10 Razor Components.

---

# 5. Files

- **FILE-001**: `src/CymruBlazor/Components/Core/*`
- **FILE-002**: `src/CymruBlazor/Components/Layout/*`
- **FILE-003**: `src/CymruBlazor/Contracts/*`
- **FILE-004**: `src/CymruBlazor/Enums/*`
- **FILE-005**: `tests/CymruBlazor.Tests/*`

---

# 6. Testing

- **TEST-001**: Verify all components derive from the correct base class.
- **TEST-002**: Verify CSS isolation files are generated.
- **TEST-003**: Verify deterministic CSS class generation.
- **TEST-004**: Verify accessibility attributes.
- **TEST-005**: Verify keyboard navigation.
- **TEST-006**: Verify layout component rendering using bUnit.
- **TEST-007**: Verify snapshot output using ApprovalTests.
- **TEST-008**: Execute Axe accessibility verification.

---

# 7. Risks & Assumptions

- **RISK-001**: Over-engineering the base component model may reduce flexibility. Mitigation: keep abstractions minimal and focused.
- **RISK-002**: Accessibility requirements may evolve. Mitigation: centralise ARIA and focus management.
- **ASSUMPTION-001**: All future components will follow the common architecture introduced in this phase.

---

# 8. Related Specifications / Further Reading

- Microsoft ASP.NET Core Razor component authoring guidance
- WAI-ARIA Authoring Practices Guide 1.2
- WCAG 2.2 Recommendation
- Fluent UI Blazor Architecture
- Carbon Design System Component Guidelines
- NHS Wales Design System
