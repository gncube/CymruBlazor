# Create CymruBlazor Component

Design and implement a new Blazor component for CymruBlazor (NHS Wales Design System implementation).

## Workflow

```
Component Request
    ↓
Analyse NHS Wales Design System Impact
    ↓
Select Skills & Prompts
    ↓
Generate Component Specification
    ↓
Architect (Component Design)
    ↓
Blazor Expert (Implementation & Optimization)
    ↓
C# Expert (Code Quality)
    ↓
Testing & Validation (bUnit)
    ↓
Documentation & Demo
    ↓
Deliver
```

## Workflow Phases

### Phase 1: Component Request Analysis

**Input:** Component requirement (e.g., "Create a Badge component")

**Analysis:**

- Which NHS Wales Design System component does this align with?
- Is this a new component or variation of existing?
- What are the accessibility requirements (WCAG 2.2 AA)?
- What design tokens are needed?
- Which component category? (Layout, Content, Forms, Infrastructure)
- Existing similar components to learn from?

**Deliverable:** Component analysis document

### Phase 2: Select Skills & Prompts

Automatically select:

- `skills/dotnet-modern-development/SKILL.md` — Modern .NET practices
- `skills/coding-standards/SKILL.md` — Code standards
- `skills/testing/SKILL.md` — Component testing with bUnit
- `skills/documentation/SKILL.md` — Component documentation
- `.github/agents/architect.md` — Component design patterns
- `.github/agents/blazor-expert.md` — Blazor best practices

**Reference Documentation:**

- NHS Wales Design System guidelines
- Existing component examples in `src/CymruBlazor/Components/`
- Demo app examples in `src/CymruBlazor.Demo/`

### Phase 3: Generate Component Specification

**Input:** Component analysis and NHS requirements

**Deliverable:** Structured component specification (`plans/component-[name]-v1.md`)

**Specification Contents:**

- Component purpose & use cases
- Props/Parameters (required, optional, defaults)
- Event callbacks & communication patterns
- Accessibility requirements (WCAG 2.2 AA)
- Design tokens used (colors, spacing, typography)
- Component variants (e.g., Button: Primary, Secondary, Danger)
- Composition patterns (How does it work with other components?)
- Keyboard navigation & screen reader support
- Implementation phases with goals (GOAL-\*)
- Atomic tasks (TASK-\*)
- Affected files (FILE-\*)
- Test scenarios (TEST-\*)
- Demo/documentation examples
- Risks and assumptions (RISK-_, ASM-_)
- Dependencies on other components

**Note:** Specification is complete before any code is written.

### Phase 4: Architect (Component Design)

**Input:** Component specification

**Responsibilities:**

- Component API design (props, events, slots)
- Accessibility requirements (WCAG 2.2 AA)
- Design token mapping
- Composition with other components
- CSS class structure and theming
- CymruComponentBase usage

**Deliverable:** Component design document

**Ensures:**

- Follows NHS Wales Design System patterns
- Single responsibility principle
- Accessibility-first design
- Composable with other components
- Clear prop contracts
- No prop explosion (keep API simple)

### Phase 5: Blazor Expert (Implementation & Optimization)

**Input:** Design from Phase 4 and Specification from Phase 3

**Execution:** Implement following TASK-\* definitions:

- Razor component file in appropriate folder (`src/CymruBlazor/Components/[Category]/[ComponentName].razor`)
- Component base class (CymruComponentBase) inheritance
- Props with EditorRequired where needed
- Event callbacks for parent communication
- CSS class builder methods
- Child content rendering
- Cascading parameter injection if needed
- Performance optimizations (@key, ShouldRender)

**Follows:**

- Coding standards (coding-standards.skill)
- Modern C# idioms (primary constructors, records)
- Blazor best practices
- CymruBlazor conventions
- Task-specific implementation details

### Phase 6: C# Expert (Code Quality)

**Input:** Implementation from Phase 5

**Review:**

- Component parameter clarity
- Event callback patterns
- CSS class composition logic
- Null safety and nullable reference types
- Resource disposal if needed
- Performance concerns

**Delivers:**

- Code quality improvements
- API refinements
- Performance optimizations

### Phase 7: Testing & Validation (bUnit)

**Input:** Implementation from Phase 5 & 6

**Testing:**

- Render tests (does component render with different props?)
- Event callback tests (do callbacks fire correctly?)
- CSS class composition tests
- Accessibility tests (focus states, ARIA labels)
- Child content rendering tests
- Cascading parameter tests
- Error state handling

**Deliverables:**

- `tests/CymruBlazor.Tests/Components/[Category]/[ComponentName]Tests.cs`
- `tests/CymruBlazor.ApprovalTests/Components/[Category]/[ComponentName]ApprovalTests.cs`
- Code coverage > 80% for new component
- All tests passing
- No static analyzer warnings

### Phase 8: Documentation & Demo

**Deliverables:**

1. **Component Documentation**
   - Create `docs/components/[ComponentName].md`
   - Document props with examples
   - Document event callbacks
   - Document usage patterns
   - Document accessibility features
   - Document design tokens used

2. **Demo Component**
   - Add to `src/CymruBlazor.Demo/Pages/[Category]/[ComponentName].razor`
   - Show all variants
   - Show different states
   - Show composition examples
   - Show accessibility features
   - Interactive examples

3. **Update README (if first component of category)**
   - Add component to features list
   - Link to documentation

4. **Update AGENTS.md**
   - Reference new component in component library section

### Phase 9: Final Validation

**Checklist:**

- ✅ Component renders correctly
- ✅ All props work as documented
- ✅ All events fire correctly
- ✅ Accessible (WCAG 2.2 AA)
- ✅ Follows NHS Wales Design System
- ✅ Composable with other components
- ✅ Tests pass and cover > 80%
- ✅ Documentation complete
- ✅ Demo shows all variants
- ✅ No analyzer warnings
- ✅ Git commits follow Conventional Commits

### Phase 10: Deliver

**Outputs:**

- Merged PR with component code
- Component visible in Demo app
- Documentation published
- Tests passing in CI/CD
- Ready for NuGet package

**Deliverable:** Test plan and suggestions

- Unit test cases suggested (not implemented)
- Integration test scenarios
- Edge cases to consider
- Authorization test cases
- Error path scenarios

**Note:** Tests are already implemented in Phase 7.

### Phase 11: Deliver

**Quality Gates:**

✓ Implementation plan complete  
✓ All code implemented per plan  
✓ All tests passing (unit and integration)  
✓ Code review complete (architecture verified)  
✓ ADRs created/updated  
✓ Documentation complete  
✓ Architecture diagrams updated  
✓ No static analyzer warnings  
✓ Ready for PR and merge

**Output:**

- Complete feature branch with implementation
- PR ready with description linking to plan
- All documentation updated
- AOD updated
- Test coverage metrics
- Ready for team review

## Skills Used

This prompt works with these skills:

- `healthpassport-architecture.skill` — HealthPassport architecture reference
- `vertical-slice.skill` — Feature organization patterns
- `coding-standards.skill` — C# standards and idioms
- `ddd.skill` — Domain-Driven Design patterns
- `documentation.skill` — Documentation and ADR standards

## Example Invocations

### Feature 1: Patient Search

```
Use generate-feature prompt to implement:

"Create a patient search feature allowing admin to find patients
by email, name, or date of birth with pagination.
Authorization: Admin only.
Result should integrate with existing patient domain model."
```

### Feature 2: Appointment Scheduling

```
Use generate-feature prompt to implement:

"Implement appointment scheduling for patients and doctors.
Patients can request appointments, doctors can approve/reject/reschedule.
Notify both parties via email on state changes.
Store in database, expose via API."
```

## When to Use This Prompt

Use `generate-feature.prompt` when:

- Adding a new feature to HealthPassport
- Feature requires new domain logic
- Feature requires new API endpoints
- Feature requires new database schema
- Feature impacts architecture

**Do not use** for:

- Simple bug fixes
- Refactoring without new behavior
- Documentation-only changes
- Configuration changes
