---
title: Coding Standards and Principles
description: Foundation engineering philosophy and design principles applied to all code generation
applies_to: ["**/*.cs", "**/*.tsx", "**/*.ts", "**/*.js"]
requires:
  - .github/agents/architect.md
  - skills/dotnet-modern-development/SKILL.md
---

# Coding Standards and Principles

This skill captures the engineering philosophy and design principles that guide all code generation and review. These principles transcend language and framework.

## Core Principle: Code is Read More Than Written

Optimize for clarity and understanding. A clever solution that requires explanation is a failed solution.

> "Any fool can write code that a computer can understand. Good programmers write code that humans can understand."
> — Martin Fowler

## SOLID Principles

### S — Single Responsibility Principle

Each class, method, function should have one reason to change.

```csharp
// ❌ TOO MANY RESPONSIBILITIES
public class PatientService
{
    public async Task<PatientResponse> GetPatientAsync(Guid id)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);
        var audit = new AuditLog { PatientId = id, Action = "VIEW", Timestamp = DateTime.UtcNow };
        await _db.AuditLogs.AddAsync(audit);
        await _db.SaveChangesAsync();

        var email = new EmailMessage { To = patient.Email, Subject = "Access Log" };
        await _emailService.SendAsync(email);

        return new PatientResponse { Id = patient.Id, Name = patient.Name };
    }
}

// ✅ SINGLE RESPONSIBILITY
public sealed class GetPatientQueryHandler(IRepository repository) : IQueryHandler<GetPatientQuery, PatientResponse>
{
    public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
    {
        var patient = await repository.GetByIdAsync(query.Id, ct);
        return patient is null
            ? Result<PatientResponse>.Failure(Error.NotFound())
            : Result<PatientResponse>.Success(patient.ToResponse());
    }
}

// Logging is handled via ILogger (cross-cutting concern)
// Auditing is handled via an audit event handler
// Email is handled via event publishing
```

### O — Open/Closed Principle

Classes should be open for extension but closed for modification.

```csharp
// ❌ CLOSED FOR EXTENSION
public class DiscountCalculator
{
    public decimal Calculate(Patient patient, decimal amount)
    {
        if (patient.Type == "Gold") return amount * 0.9m;
        if (patient.Type == "Silver") return amount * 0.95m;
        if (patient.Type == "Bronze") return amount * 0.98m;
        return amount;
    }
    // Adding a new tier requires editing this class
}

// ✅ OPEN FOR EXTENSION
public interface IDiscountPolicy
{
    decimal Apply(decimal amount);
}

public sealed class GoldDiscountPolicy : IDiscountPolicy
{
    public decimal Apply(decimal amount) => amount * 0.9m;
}

// New tiers are added without modifying existing classes
```

### L — Liskov Substitution Principle

Derived types must be substitutable for their base types.

```csharp
// ❌ VIOLATES LISKOV
public interface IRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
}

public sealed class CachedRepository : IRepository
{
    // Throws NotImplementedException because cache is write-only
    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct) =>
        throw new NotImplementedException("Use cache instead");
}

// ✅ RESPECTS LISKOV
public interface IRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
}

public sealed class CachedRepository : IRepository
{
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _cache.TryGetValue(id, out var patient) ? patient : null;
    }
    // Cache miss is valid behavior, not an error
}
```

### I — Interface Segregation Principle

Many client-specific interfaces are better than one general-purpose interface.

```csharp
// ❌ FAT INTERFACE
public interface IPatientService
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task CreateAsync(CreatePatientCommand cmd);
    Task UpdateAsync(UpdatePatientCommand cmd);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Patient>> SearchAsync(string query);
    Task<int> CountAsync();
    Task ExportToCsvAsync(Stream stream);
    Task ImportFromCsvAsync(Stream stream);
}

// ✅ SEGREGATED INTERFACES
public interface IPatientQueryHandler
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Patient>> SearchAsync(string query, CancellationToken ct);
}

public interface IPatientCommandHandler
{
    Task<Result> CreateAsync(CreatePatientCommand cmd, CancellationToken ct);
    Task<Result> UpdateAsync(UpdatePatientCommand cmd, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}

// Clients depend only on what they use
```

### D — Dependency Inversion Principle

Depend on abstractions, not concretions.

```csharp
// ❌ DEPENDS ON CONCRETE CLASS
public sealed class PatientService
{
    private readonly SqlPatientRepository _repo = new();

    public async Task<Patient?> GetAsync(Guid id) =>
        await _repo.GetByIdAsync(id, CancellationToken.None);
}

// ✅ DEPENDS ON ABSTRACTION
public sealed class PatientService(IPatientRepository repository)
{
    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct);
}

// Repository is injected; can swap implementations for testing or other databases
```

## DRY, KISS, YAGNI

### DRY — Don't Repeat Yourself

Identical or similar code in multiple places indicates missing abstraction.

```csharp
// ❌ REPEATED CODE
public async Task<Result<PatientResponse>> GetPatientAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id is required"));

    var patient = await _repository.GetByIdAsync(id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}

public async Task<Result<DoctorResponse>> GetDoctorAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return Result<DoctorResponse>.Failure(Error.Validation("Id is required"));

    var doctor = await _doctorRepository.GetByIdAsync(id, ct);
    return doctor is null
        ? Result<DoctorResponse>.Failure(Error.NotFound())
        : Result<DoctorResponse>.Success(doctor.ToResponse());
}

// ✅ ABSTRACTED
public async Task<Result<T>> GetByIdAsync<T>(Guid id, IRepository<T> repository, CancellationToken ct)
    where T : class
{
    ArgumentException.ThrowIfNullOrEmpty(id.ToString(), nameof(id));

    var entity = await repository.GetByIdAsync(id, ct);
    return entity is null
        ? Result<T>.Failure(Error.NotFound())
        : Result<T>.Success(entity);
}
```

### KISS — Keep It Simple, Stupid

Simpler solutions are easier to understand, test, and maintain.

```csharp
// ❌ OVER-ENGINEERED
public sealed class PatientSpecification : Specification<Patient>
{
    public PatientSpecification(string firstName, string lastName, DateTime? dob)
    {
        Query
            .Where(p => p.FirstName == firstName)
            .Where(p => p.LastName == lastName);

        if (dob.HasValue)
            Query.Where(p => p.DateOfBirth == dob.Value);
    }
}

// ✅ SIMPLE
var patients = await _repository.GetAsync(
    firstName: "John",
    lastName: "Doe",
    dateOfBirth: new DateTime(1990, 1, 1),
    cancellationToken: ct);
```

### YAGNI — You Aren't Gonna Need It

Don't add functionality until it's actually needed. Speculative code is technical debt.

```csharp
// ❌ SPECULATIVE: "We might need this someday"
public interface IPatientService
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Patient?> GetByIdWithCacheAsync(Guid id, CancellationToken ct);  // Not used
    Task<Patient?> GetByIdWithLockAsync(Guid id, CancellationToken ct);   // Not used
    Task<IEnumerable<Patient>> GetByNameAsync(string name, CancellationToken ct);  // Not planned
}

// ✅ JUST WHAT WE NEED
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
}

// Add when needed; nothing speculative
```

## Clean Code Practices

### Meaningful Names

Names should reveal intent without requiring comments.

```csharp
// ❌ CRYPTIC
public class pd
{
    public async Task<pd?> g(Guid id) => await d.Get(id);
}

// ✅ CLEAR
public sealed class PatientDetails
{
    public async Task<PatientDetails?> GetByIdAsync(Guid id) =>
        await repository.GetByIdAsync(id);
}
```

### Small, Focused Methods

Methods should do one thing and do it well. If a method is more than ~20 lines, consider refactoring.

```csharp
// ❌ TOO LARGE
public async Task<Result<PatientResponse>> HandleAsync(CreatePatientCommand cmd, CancellationToken ct)
{
    // Validate
    if (string.IsNullOrWhiteSpace(cmd.Email))
        return Result<PatientResponse>.Failure(Error.Validation("Email required"));
    if (cmd.DateOfBirth > DateTime.UtcNow)
        return Result<PatientResponse>.Failure(Error.Validation("Invalid birth date"));

    // Check for duplicates
    var existing = await _repository.GetByEmailAsync(cmd.Email, ct);
    if (existing is not null)
        return Result<PatientResponse>.Failure(Error.Conflict("Email already registered"));

    // Create entity
    var patient = new Patient
    {
        Id = Guid.NewGuid(),
        FirstName = cmd.FirstName,
        LastName = cmd.LastName,
        Email = cmd.Email,
        DateOfBirth = cmd.DateOfBirth
    };

    // Persist
    await _repository.AddAsync(patient, ct);
    await _unitOfWork.SaveChangesAsync(ct);

    // Log and publish event
    _logger.LogInformation("Patient created: {PatientId}", patient.Id);
    await _eventPublisher.PublishAsync(new PatientCreatedEvent(patient.Id), ct);

    return Result<PatientResponse>.Success(patient.ToResponse());
}

// ✅ COMPOSED: Each method handles one thing
public sealed class CreatePatientCommandHandler(
    IRepository repository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher,
    ILogger<CreatePatientCommandHandler> logger) : ICommandHandler<CreatePatientCommand, Result<PatientResponse>>
{
    public async Task<Result<PatientResponse>> Handle(CreatePatientCommand cmd, CancellationToken ct)
    {
        var validationResult = ValidateCommand(cmd);
        if (!validationResult.IsSuccess)
            return validationResult;

        var existing = await repository.GetByEmailAsync(cmd.Email, ct);
        if (existing is not null)
            return Result<PatientResponse>.Failure(Error.Conflict("Email already registered"));

        var patient = CreatePatientEntity(cmd);

        await repository.AddAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Patient created: {PatientId}", patient.Id);
        await eventPublisher.PublishAsync(new PatientCreatedEvent(patient.Id), ct);

        return Result<PatientResponse>.Success(patient.ToResponse());
    }

    private static Result<PatientResponse> ValidateCommand(CreatePatientCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Email))
            return Result<PatientResponse>.Failure(Error.Validation("Email required"));
        if (cmd.DateOfBirth > DateTime.UtcNow)
            return Result<PatientResponse>.Failure(Error.Validation("Invalid birth date"));
        return Result<PatientResponse>.Success(null!);  // Placeholder; validates only
    }

    private static Patient CreatePatientEntity(CreatePatientCommand cmd) =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = cmd.FirstName,
            LastName = cmd.LastName,
            Email = cmd.Email,
            DateOfBirth = cmd.DateOfBirth
        };
}
```

### Guard Clauses

Check preconditions early and return/throw to avoid deep nesting.

```csharp
// ❌ NESTED
public async Task<Result<PatientResponse>> GetAsync(Guid id, CancellationToken ct)
{
    if (id != Guid.Empty)
    {
        var patient = await _repository.GetByIdAsync(id, ct);
        if (patient is not null)
        {
            return Result<PatientResponse>.Success(patient.ToResponse());
        }
        else
        {
            return Result<PatientResponse>.Failure(Error.NotFound());
        }
    }
    else
    {
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));
    }
}

// ✅ GUARD CLAUSES
public async Task<Result<PatientResponse>> GetAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));

    var patient = await _repository.GetByIdAsync(id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}
```

### Constructor Injection and Configuration

Use dependency injection; avoid service locators and singletons.

```csharp
// ✅ CORRECT: Dependencies explicit and testable
public sealed class PatientService(
    IRepository repository,
    IValidator validator,
    ILogger<PatientService> logger)
{
    public async Task<Result<Patient>> GetAsync(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Getting patient {PatientId}", id);
        return await repository.GetByIdAsync(id, ct);
    }
}

// ❌ AVOID: Hidden dependencies
public static class PatientService
{
    private static readonly IRepository _repo = ServiceLocator.Resolve<IRepository>();

    public static async Task<Patient?> Get(Guid id) =>
        await _repo.GetByIdAsync(id, CancellationToken.None);
}
```

### Comments Explain WHY, Not What

Code that needs a comment to explain what it does is unclear. Comments should explain the "why."

```csharp
// ❌ REDUNDANT: The code already says what it does
public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
{
    // Get the patient by id
    var patient = await _repository.GetByIdAsync(id, ct);
    // Return the patient
    return patient;
}

// ✅ HELPFUL: Explains the design decision
public async Task<Result<PatientResponse>> GetAsync(Guid id, CancellationToken ct)
{
    // Validate id early to fail fast before database query
    if (id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));

    // Query returns null for missing patients, not exception.
    // This is intentional: not found is not an exceptional condition.
    var patient = await _repository.GetByIdAsync(id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}
```

## Performance Without Sacrificing Maintainability

### Profile Before Optimizing

Premature optimization is the root of all evil. Measure first.

```csharp
// ❌ PREMATURE: Unclear if this micro-optimization matters
public static List<T> FastCopy<T>(List<T> source)
{
    var result = new T[source.Count];
    Array.Copy(source.ToArray(), result, source.Count);
    return result.ToList();
}

// ✅ MEASURED: Only optimize hot paths with evidence
public sealed class PatientSearchService(IRepository repository)
{
    // This method is called 1000x per second. Optimization is justified.
    public async Task<List<Patient>> SearchAsync(string query, CancellationToken ct)
    {
        // Use early filtering in database query, not client-side
        return await repository.SearchAsync(
            predicate: p => p.FirstName.Contains(query) || p.LastName.Contains(query),
            cancellationToken: ct);
    }
}
```

### Readability + Performance

When both are possible, choose both. When they conflict, readability wins unless profiling proves otherwise.

```csharp
// ✅ READABLE AND PERFORMANT
public sealed record GetPatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);

public sealed class GetPatientQueryHandler(IRepository repository) : IQueryHandler<GetPatientQuery, PatientResponse>
{
    public async Task<Result<GetPatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
    {
        var patient = await repository.GetByIdAsync(query.Id, ct);
        return patient is null
            ? Result<GetPatientResponse>.Failure(Error.NotFound())
            : Result<GetPatientResponse>.Success(new GetPatientResponse(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.Email));
    }
}
```

## No Magic Strings

Constants and enums beat magic strings. The string values are scattered through code and hard to maintain.

```csharp
// ❌ MAGIC STRINGS
public class PatientRoles
{
    public static void CheckPermission(User user, string endpoint)
    {
        if (endpoint == "/admin/patients" && user.Role != "Admin")
            throw new UnauthorizedAccessException();

        if (endpoint == "/patient/profile" && user.Role != "Patient" && user.Role != "Admin")
            throw new UnauthorizedAccessException();
    }
}

// ✅ CONSTANTS AND ENUMS
public enum UserRole
{
    Patient,
    Doctor,
    Admin
}

public class EndpointPermissions
{
    public static readonly Dictionary<string, UserRole[]> Requirements = new()
    {
        ["/admin/patients"] = [UserRole.Admin],
        ["/patient/profile"] = [UserRole.Patient, UserRole.Admin]
    };
}

public static void CheckPermission(User user, string endpoint)
{
    if (!EndpointPermissions.Requirements.TryGetValue(endpoint, out var allowed))
        throw new KeyNotFoundException($"Endpoint {endpoint} not configured");

    if (!allowed.Contains(user.Role))
        throw new UnauthorizedAccessException($"Role {user.Role} cannot access {endpoint}");
}
```

## Strong Typing Over Primitive Types

Domain concepts should be types, not primitives.

```csharp
// ❌ PRIMITIVE TYPES: Easy to mix up
public async Task<bool> IsEmailRegisteredAsync(string email) =>
    await _repository.EmailExistsAsync(email);

public async Task<Patient> GetByEmailAsync(string email) =>
    await _repository.GetByEmailAsync(email);

// Caller can't tell email from name
await repository.CheckAsync("john@example.com", email: true);
await repository.CheckAsync("John Doe", email: false);

// ✅ STRONG TYPING
public sealed record EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            return Result<EmailAddress>.Failure(Error.Validation("Invalid email"));

        return Result<EmailAddress>.Success(new EmailAddress(value));
    }
}

public async Task<bool> IsEmailRegisteredAsync(EmailAddress email) =>
    await _repository.EmailExistsAsync(email.Value);

// Compiler prevents misuse
var email = EmailAddress.Create("john@example.com");
if (!email.IsSuccess)
    return email;  // Type-safe error handling
```

## Related Skills

- `skills/dotnet-modern-development/SKILL.md` – Modern .NET/C# practices
- `skills/testing/SKILL.md` – Testing philosophy
- `.github/agents/architect.md` – Architectural patterns and decisions
