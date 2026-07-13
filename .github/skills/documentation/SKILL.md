---
title: Documentation Standards
description: Documentation requirements, formats, and expectations for all code and architectural changes
applies_to: ["docs/**/*", "**/*.md", "**/*.xml"]
requires:
  - .github/agents/architect.md
  - skills/coding-standards/SKILL.md
---

# Documentation Standards

Documentation is not optional—it's a first-class requirement. Every feature, change, and decision must be documented. Documentation should explain "why," not just "what."

## Core Principle

Good documentation makes the codebase easier to understand and maintain. Poor documentation (or none) forces future developers to reverse-engineer intent from code.

## Required Documentation by Change Type

### New Feature

- **README update:** How to use the feature
- **Code comments:** Why non-obvious decisions were made
- **XML documentation:** All public APIs
- **OpenAPI/Swagger:** If it's an HTTP API

### Architectural Change

- **ADR (Architectural Decision Record):** Why the decision was made
- **Update CODEBASE_ARCHITECTURE.md:** Reflect the new architecture
- **C4 diagram:** If the architecture changed visibly
- **Arc42 section:** If applicable

### Bug Fix

- **Comment in code:** Why the bug occurred and how it's fixed
- **Update changelog:** Link to the issue
- **Reproduction test:** Verify the bug doesn't regress

### Refactoring

- **No new documentation needed** (behavior unchanged)
- **Code comments:** Only if non-obvious changes were made
- **Existing tests pass:** Prove behavior is preserved

## Architectural Decision Records (ADRs)

Every architectural decision **must** have an ADR. ADRs live in `docs/ADR/` and `docs/architecture/adr/`.

### ADR Structure

```markdown
---
status: Proposed | Accepted | Deprecated
---

# ADR-XXXX: Title

## Status

Accepted

## Context

Explain the problem. What drove this decision? What are the constraints?

## Decision

What was decided? Be specific and clear.

## Consequences

What are the implications? Trade-offs? Performance impact? Team impact?

## Alternatives Considered

What other approaches were evaluated? Why were they rejected?

## References

- Related ADR-XXX
- External documentation
- Issue #123

## Examples

Show code or architecture diagrams if helpful.
```

### ADR Example: Vertical Slice Architecture

```markdown
---
status: Accepted
---

# ADR-0004: Vertical Slices with Scoped Mediator

## Status

Accepted

## Context

The Health Passport application handles patient data with complex business logic across multiple features (patients, appointments, diagnostics).
Initial layered architecture created coupling between features and made deployment risky (one bug in a shared layer affects all features).

Team wanted:

- Fast feature delivery with minimal coordination
- Ability to deploy features independently
- Clear ownership boundaries
- Reduced cross-team dependencies

## Decision

Adopt Vertical Slice Architecture where each feature is a complete, self-contained vertical:

- Separate command, query, validator, handler, and endpoint per feature
- Features grouped in `Features/{FeatureName}/` folder
- Mediator configured with `ServiceLifetime.Scoped` (not Transient)
- No shared business logic between features

## Consequences

✅ Faster feature development; developers own entire vertical
✅ Independent deployability; one broken feature doesn't affect others
✅ Clear ownership; team can be assigned per vertical
⚠️ Code duplication possible if not careful; establish shared utilities
⚠️ Requires discipline; easy to violate boundaries if not vigilant
⚠️ Mediator MUST be Scoped; Transient breaks DbContext resolution

## Alternatives Considered

1. **Layered Architecture** – Rejected: High coupling between features; shared layer breaks create risk across all features
2. **CQRS with Event Sourcing** – Rejected: Overkill for current domain complexity; add when temporal history becomes a requirement
3. **Microservices** – Rejected: Team too small; operational overhead outweighs benefits

## References

- Jimmy Bogard: Vertical Slice Architecture
- ADR-0013: Presentation Dependency Registration
- docs/CODEBASE_ARCHITECTURE.md

## Code Example
```

Features/
├── Patients/
│ ├── Commands/
│ │ ├── CreatePatientCommand.cs
│ │ ├── CreatePatientCommandHandler.cs
│ │ └── CreatePatientCommandValidator.cs
│ ├── Queries/
│ │ ├── GetPatientQuery.cs
│ │ └── GetPatientQueryHandler.cs
│ └── Endpoints/
│ ├── GetPatientEndpoint.cs
│ └── CreatePatientEndpoint.cs
├── Appointments/
│ ├── Commands/
│ │ ├── ScheduleAppointmentCommand.cs
│ │ └── ScheduleAppointmentCommandHandler.cs
│ ├── Queries/
│ │ └── ...
│ └── Endpoints/
│ └── ...

```

```

### ADR Checklist

- [ ] Decision has a clear problem statement
- [ ] Decision is specific (not vague)
- [ ] Consequences are realistic (don't minimize trade-offs)
- [ ] Alternatives were considered
- [ ] ADR explains **why**, not just **what**
- [ ] File named `ADR-XXXX-Title.md` or `XXXX-Title.md`
- [ ] Status is set (Proposed, Accepted, or Deprecated)

## Code-Level Documentation

### XML Documentation for Public APIs

Required for all public types and members.

```csharp
// ✅ COMPLETE
/// <summary>
/// Retrieves a patient by their unique identifier.
/// </summary>
/// <param name="id">The patient's unique identifier.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>
/// A task representing the asynchronous operation.
/// The result contains the patient if found; null otherwise.
/// </returns>
/// <exception cref="ArgumentException">Thrown when id is empty.</exception>
public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    ArgumentException.ThrowIfNullOrEmpty(id.ToString(), nameof(id));
    return await _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}

// ❌ MISSING: Public API with no documentation
public async Task<Patient?> GetPatient(Guid id) =>
    await _repository.GetByIdAsync(id, CancellationToken.None);

// ✓ NOT required: Private/internal methods
private Patient? GetFromCache(Guid id) => _cache.Get(id);
```

### Inline Comments Explain "Why"

```csharp
// ✅ EXPLAINS WHY
public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
{
    // Validate id early to fail fast before database query
    if (query.Id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));

    // Query returns null for missing patients, not exception.
    // This is intentional: not found is not an exceptional condition.
    var patient = await _repository.GetByIdAsync(query.Id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}

// ❌ REDUNDANT: Comment repeats what code says
public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
{
    // Check if id is empty
    if (query.Id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));

    // Get patient from repository
    var patient = await _repository.GetByIdAsync(query.Id, ct);

    // Return null if patient not found
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}
```

## API Documentation

### OpenAPI/Swagger

All HTTP endpoints must have complete OpenAPI documentation.

```csharp
// ✅ COMPLETE DOCUMENTATION
app.MapGet("/api/patients/{id}", GetPatientAsync)
    .WithName("GetPatient")
    .WithOpenApi()
    .WithSummary("Retrieve a patient by ID")
    .WithDescription("Returns the patient record with the specified ID. Returns 404 if not found.")
    .Produces<PatientResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized)
    .WithTags("Patients")
    .RequireAuthorization();

async Task<IResult> GetPatientAsync(Guid id, ISender mediator, CancellationToken ct)
{
    var query = new GetPatientQuery(id);
    var result = await mediator.Send(query, ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
}

// ❌ MISSING DOCUMENTATION
app.MapGet("/api/patients/{id}", async (Guid id, ISender mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetPatientQuery(id), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
});
```

### API.md Reference Documentation

Maintain a top-level `docs/API.md` documenting all endpoints.

````markdown
# API Documentation

## Patients

### Get Patient

- **Endpoint:** `GET /api/patients/{id}`
- **Authentication:** Required
- **Parameters:**
  - `id` (path, required): Patient ID (GUID)
- **Response:** 200 OK
  ```json
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com"
  }
  ```
````

- **Errors:**
  - 404 Not Found: Patient does not exist
  - 401 Unauthorized: Not authenticated

````

## Architecture Documentation

### CODEBASE_ARCHITECTURE.md

High-level overview of the system architecture. Updated whenever architecture changes.

```markdown
# Health Passport Architecture

## Overview
Health Passport is a Blazor WASM client with an ASP.NET Minimal APIs backend.

## Technology Stack
- **Client:** Blazor WebAssembly (.NET 9)
- **API:** ASP.NET Core Minimal APIs (.NET 9)
- **Database:** SQLite with EF Core
- **Patterns:** Vertical Slice Architecture, CQRS, MediatR

## Folder Structure
````

src/
├── Client/ # Blazor WASM app
├── HealthPassport.API/ # ASP.NET API
├── HealthPassport.Application/ # Business logic (handlers, validators)
├── HealthPassport.Contracts/ # DTOs and response types
├── HealthPassport.Domain/ # Domain entities and value objects
└── HealthPassport.Infrastructure/ # Database context, migrations

```

## Key Design Decisions
- **Vertical Slices:** Features organized as complete verticals
- **Mediator Pattern:** CQRS queries and commands dispatched via MediatR
- **Result<T> Pattern:** Explicit error handling without exceptions
- **Scoped Mediator:** Handlers can resolve scoped DbContext

See:
- ADR-0004: Vertical Slices with Scoped Mediator
- ADR-0013: Presentation Dependency Registration
```

### C4 Diagrams

Use C4 model diagrams to visualize architecture at different zoom levels.

```
System Context
└── System Landscape: How Health Passport fits in the broader ecosystem

Container Diagram
└── Client, API, Database, Auth Service, External Integrations

Component Diagram (per container)
└── API: Controllers, Handlers, Repositories, Services

Class Diagram (when helpful)
└── Domain entities and relationships
```

Store diagrams in `docs/architecture/diagrams/` as PlantUML or draw.io files.

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

Person(user, "User", "A patient or healthcare provider")
System(healthpassport, "Health Passport", "Patient health record management")
System_Ext(email, "Email System", "Sends email notifications")

Rel(user, healthpassport, "Uses")
Rel(healthpassport, email, "Sends emails via")

@enduml
```

## Project Structure Documentation

### DomainModels.md

Documents the domain entities and their relationships.

```markdown
# Domain Models

## Patient

The core entity representing a patient.

| Property    | Type      | Description                      |
| ----------- | --------- | -------------------------------- |
| Id          | Guid      | Unique patient identifier        |
| FirstName   | string    | Patient's first name             |
| LastName    | string    | Patient's last name              |
| Email       | string    | Contact email                    |
| DateOfBirth | DateTime? | Optional birth date              |
| IsActive    | bool      | Whether patient record is active |

### Relationships

- Patient → Appointments (one-to-many)
- Patient → Diagnostics (one-to-many)

### Invariants

- FirstName and LastName are required and non-empty
- Email must be valid and unique
- DateOfBirth cannot be in the future
```

## README Guidelines

Every feature should update the top-level README or feature-specific README.

````markdown
# Health Passport

Patient health record management system built with Blazor WASM and ASP.NET Core.

## Features

- Patient record management
- Appointment scheduling
- Diagnostic test results
- User authentication and authorization

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- SQLite (included with EF Core)

### Installation

```bash
git clone <repo>
cd healthpassport-tutorial
dotnet build
dotnet run --project src/HealthPassport.API
```
````

### Architecture

See [CODEBASE_ARCHITECTURE.md](docs/CODEBASE_ARCHITECTURE.md) for system design.

### Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

````

## Markdown Formatting Standards

### Headings
```markdown
# Level 1: Document Title (one per document)
## Level 2: Major Section
### Level 3: Subsection
#### Level 4: Sub-subsection (use sparingly)
````

### Code Blocks

Always specify language for syntax highlighting:

````markdown
```csharp
public sealed class MyClass { }
```

```json
{ "key": "value" }
```

```bash
dotnet build
```
````

### Lists

```markdown
- Item 1
- Item 2
- Item 3

OR

1. First step
2. Second step
3. Third step
```

### Links and References

```markdown
[Display text](path/to/file.md)
[Link to issue](#issue-123)
[External link](https://example.com)
```

### Tables

```markdown
| Header 1 | Header 2 | Header 3 |
| -------- | -------- | -------- |
| Value 1  | Value 2  | Value 3  |
| Value 4  | Value 5  | Value 6  |
```

## Git Commit Message Standards

Reference issues and ADRs in commit messages:

```
feat: Add patient search endpoint

- Implement GetPatientsByNameQuery handler
- Add SearchPatientEndpoint for /api/patients/search
- Add SearchPatientResponse contract
- Closes #123

See ADR-0004 for vertical slice structure
```

### Format

```
<type>: <subject>

<body>

<footer>
```

**Types:** feat, fix, docs, style, refactor, test, chore

**Subject:** Imperative mood, lowercase, no period

**Body:** Explain **what** and **why**, not **how**

**Footer:** Issue references and ADR links

## Changelog

Maintain a `CHANGELOG.md` documenting all releases.

```markdown
# Changelog

All notable changes to this project are documented here.

## [1.0.0] - 2024-01-15

### Added

- Patient record management
- Appointment scheduling
- User authentication

### Fixed

- Issue #123: Patient search returning incorrect results
- Issue #124: Login page timing out on slow connections

### Changed

- Migrated from Newtonsoft.Json to System.Text.Json
- Refactored patient repository to use EF Core async methods

### Security

- Updated authentication to use OAuth 2.0

See [ADR-0013](docs/architecture/adr/ADR-0013-user-context-endpoint.md) for authentication changes.

### Deprecated

- Old REST API endpoints (use GraphQL instead)
```

## Documentation Checklist

- [ ] README updated with feature description
- [ ] XML documentation on all public APIs
- [ ] OpenAPI documentation for HTTP endpoints
- [ ] Code comments explain "why," not "what"
- [ ] ADR created for architectural decisions
- [ ] CODEBASE_ARCHITECTURE.md updated if design changed
- [ ] C4 diagram added/updated if structure changed
- [ ] Commit message references issues and ADRs
- [ ] CHANGELOG.md updated with breaking changes
- [ ] API.md updated with new endpoints

## See Also

- `docs/architecture/adr/cheatsheet.md` – ADR quick reference
- `CODEBASE_ARCHITECTURE.md` – Current system architecture
- `docs/API.md` – Complete API reference
- `.github/agents/architect.md` – Architectural guidance
