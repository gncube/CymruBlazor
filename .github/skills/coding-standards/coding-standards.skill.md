---
title: HealthPassport Coding Standards
description: C# naming, structure, and code style standards for HealthPassport
applies_to: ["**/*.cs"]
requires:
  - .github/skills/healthpassport-architecture.skill.md
---

# HealthPassport Coding Standards

Standards ensure code consistency, readability, and maintainability across the entire codebase.

## Naming Conventions

### Types (Classes, Records, Interfaces, Enums)

**Rule:** PascalCase, singular noun

```csharp
// Classes
public sealed class PatientRepository { }
public sealed class CreatePatientCommandHandler { }
public sealed class PatientValidator { }

// Records
public sealed record PatientResponse { }
public sealed record CreatePatientCommand { }

// Interfaces
public interface IRepository { }
public interface IPatientRepository : IRepository { }
public interface IQuery<out TResponse> { }
public interface ICommand<TResponse> { }

// Enums
public enum PatientStatus { Active, Inactive, Suspended }
```

### Properties and Local Variables

**Rule:** PascalCase for properties, camelCase for local variables

```csharp
public sealed class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    public void UpdateName(string firstName, string lastName)
    {
        var trimmedFirstName = firstName.Trim();  // local variable
        var trimmedLastName = lastName.Trim();    // local variable

        FirstName = trimmedFirstName;  // property
        LastName = trimmedLastName;    // property
    }
}
```

### Private Fields

**Rule:** \_camelCase prefix

```csharp
public sealed class PatientService(IRepository repository, ILogger<PatientService> logger)
{
    private readonly IRepository _repository = repository;
    private readonly ILogger<PatientService> _logger = logger;

    // ✓ Or with primary constructor (preferred)
}
```

### Methods

**Rule:** PascalCase, verb + noun

```csharp
public async Task<Patient> GetPatientAsync(Guid id, CancellationToken ct) { }
public void ValidateEmail(string email) { }
public bool TryParseDate(string dateString, out DateTime result) { }
public async Task CreatePatientAsync(Patient patient, CancellationToken ct) { }
```

### Constants

**Rule:** UPPER_SNAKE_CASE

```csharp
public sealed class PaginationDefaults
{
    public const int DEFAULT_PAGE_SIZE = 10;
    public const int MAX_PAGE_SIZE = 100;
    public const int MIN_PAGE_NUMBER = 1;
}
```

### Namespaces

**Rule:** Company.Project.Feature or Company.Project.Layer.Feature

```csharp
// By feature (preferred)
namespace HealthPassport.Features.Patients;
namespace HealthPassport.Features.Patients.Commands;
namespace HealthPassport.Features.Patients.Queries;
namespace HealthPassport.Features.Patients.Handlers;

// By layer
namespace HealthPassport.Domain.Patient;
namespace HealthPassport.Application.Patient;
namespace HealthPassport.Contracts.Patients;
```

## Modern C# Idioms

### File-Scoped Namespaces

**Use file-scoped namespaces (no closing brace):**

```csharp
namespace HealthPassport.Features.Patients.Commands;

public sealed record CreatePatientCommand(string Email, string FirstName) : ICommand<PatientResponse>;
```

### Primary Constructors

**Prefer primary constructors for dependency injection:**

```csharp
// ✓ Preferred
public sealed class PatientRepository(ApplicationDbContext context) : IRepository
{
    public async Task<Patient> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}

// ❌ Avoid
public sealed class PatientRepository : IRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}
```

### Records for Immutable Data

**Use records for data transfer objects and commands:**

```csharp
// ✓ Preferred - Record
public sealed record CreatePatientCommand(
    string Email,
    string FirstName,
    string LastName,
    DateTime DateOfBirth) : ICommand<PatientResponse>;

// ❌ Avoid - Class with properties
public sealed class CreatePatientCommand
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    // ...
}
```

### Collection Expressions

**Use collection expressions for array/list initialization:**

```csharp
// ✓ Preferred (C# 12+)
var ids = [1, 2, 3, 4, 5];
var combined = [..existing, ..new];

// ❌ Avoid
var ids = new List<int> { 1, 2, 3, 4, 5 };
var combined = existing.Concat(new).ToList();
```

### Sealed Classes by Default

**Seal classes unless specifically designed for inheritance:**

```csharp
// ✓ Preferred
public sealed class PatientRepository : IRepository { }
public sealed class PatientValidator { }

// ❌ Avoid (unless designed for inheritance)
public class PatientRepository : IRepository { }
```

### Expression-Bodied Members

**Use for simple, single-line operations:**

```csharp
// ✓ Preferred
public string FullName => $"{FirstName} {LastName}";
public bool IsActive => Status == PatientStatus.Active;

// For methods
public Patient Create(string email, string firstName) =>
    new() { Email = email, FirstName = firstName };

// ❌ Don't overuse
public Task<Patient> GetAsync(Guid id) =>
    context.Patients
        .Where(p => p.Id == id)
        .FirstOrDefaultAsync();
```

### Nullable Reference Types

**Always enabled. Explicitly mark nullable:**

```csharp
// ✓ Preferred (explicitly nullable)
public sealed class Patient
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;  // Non-null
    public string? MiddleName { get; set; }             // Nullable
    public string? Notes { get; set; }
}

// Constructor
public Patient(string email, string? middleName = null)
{
    Email = email ?? throw new ArgumentNullException(nameof(email));
    MiddleName = middleName;
}
```

### Async/Await

**All I/O operations must be async:**

```csharp
// ✓ Preferred
public async Task<Patient> GetPatientAsync(Guid id, CancellationToken ct)
{
    return await repository.GetByIdAsync(id, ct);
}

// ❌ Never block
public Patient GetPatient(Guid id)
{
    return repository.GetByIdAsync(id, CancellationToken.None).Result;  // WRONG!
}
```

### Null-Coalescing and Pattern Matching

**Use modern null handling:**

```csharp
// ✓ Null coalescing
var name = firstName ?? "Unknown";

// ✓ Pattern matching
var result = patient switch
{
    null => Result.Failure(PatientError.NotFound()),
    _ when !patient.IsActive => Result.Failure(PatientError.Inactive()),
    _ => Result.Success(patient)
};
```

## Code Structure

### Class Organization

```csharp
namespace HealthPassport.Features.Patients.Commands;

/// <summary>
/// Creates a new patient in the system.
/// </summary>
public sealed record CreatePatientCommand(
    string Email,
    string FirstName,
    string LastName,
    DateTime DateOfBirth) : ICommand<PatientResponse>;

/// <summary>
/// Validates create patient command input.
/// </summary>
public sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

/// <summary>
/// Handles patient creation.
/// </summary>
public sealed class CreatePatientCommandHandler(
    IRepository repository,
    ILogger<CreatePatientCommandHandler> logger)
    : ICommandHandler<CreatePatientCommand, PatientResponse>
{
    public async Task<Result<PatientResponse>> Handle(
        CreatePatientCommand command,
        CancellationToken ct)
    {
        var patient = Patient.Create(
            command.Email,
            command.FirstName,
            command.LastName,
            command.DateOfBirth);

        await repository.AddAsync(patient, ct);

        logger.LogInformation("Patient created: {PatientId}", patient.Id);

        return Result.Success(new PatientResponse(patient));
    }
}
```

## Result Pattern

Always return `Result<T>` from handlers:

```csharp
// Success
return Result.Success(data);

// Failure with predefined error
return Result.Failure<T>(PatientError.NotFound(id));

// Failure with message
return Result.Failure<T>(new Error("Patient.Inactive", "Patient is not active"));

// Chaining
var result = await handler.Handle(command, ct);
return result.IsSuccess
    ? Results.Ok(result.Value)
    : result.ToHttpResult();
```

## XML Documentation

**All public members must have XML comments:**

```csharp
/// <summary>
/// Gets a patient by their ID.
/// </summary>
/// <param name="id">The patient ID.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The patient if found; otherwise null.</returns>
public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
}
```

## Dependency Injection

Always use primary constructors for DI:

```csharp
public sealed class PatientService(
    IRepository repository,
    ILogger<PatientService> logger)
{
    // Use repository and logger directly
    public async Task<Patient> GetAsync(Guid id, CancellationToken ct)
    {
        var patient = await repository.GetByIdAsync(id, ct);
        logger.LogInformation("Retrieved patient: {Id}", id);
        return patient;
    }
}
```

## Code Analyzers

Project includes static code analysis. Follow these rules:

- **CA1510:** Use ArgumentNullException.ThrowIfNull() instead of explicit null checks
- **CA2227:** Collection properties should be read-only (no mutable setters)
- **CA1805:** Remove redundant explicit initialization to default values
- **CA1707:** Remove underscores from member names (unless private fields)
- **CA1062:** Validate arguments of public methods (use ThrowIfNull)

**Example:**

```csharp
// ✓ CA1510 correct
public sealed class Repository(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));
}

// Or better:
ArgumentNullException.ThrowIfNull(context);
```

## Anti-Patterns

❌ **Mixing case styles:** Use consistent conventions  
❌ **Magic strings/numbers:** Extract to named constants  
❌ **Static methods for services:** Inject dependencies instead  
❌ **Null forgiveness (!)** Validate properly, don't suppress warnings  
❌ **Synchronous wrapper methods:** Async all the way  
❌ **Comments explaining obvious code:** Good code is self-documenting

## Formatting

- **Indentation:** 4 spaces
- **Line length:** Keep under 120 characters where practical
- **Braces:** Allman style (opening brace on new line) for class/method definitions
- **Using statements:** Alphabetically ordered, file-scoped namespace removes need for many
