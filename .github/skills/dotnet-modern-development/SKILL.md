---
title: Modern .NET Development
description: Foundation skill establishing modern .NET and C# development practices for all .NET agents
applies_to: ["*.csproj", "*.cs", "**/*.cs"]
requires:
  - skills/coding-standards/SKILL.md
---

# Modern .NET Development

This skill establishes the baseline for modern .NET and C# development across all agents. Every .NET-focused agent should reference this skill for consistency.

## SDK and Language Versions

### Target Latest Stable .NET

- **Current minimum:** .NET 8 LTS (EOL: Nov 2026)
- **Preferred:** .NET 9+ unless project constraints require 8
- **Never use:** .NET 5, 6, 7 (out of support)
- **Check:** `global.json` and `TargetFramework` in `.csproj`

### C# Language Version

- **Match TFM default** unless there's a specific reason
- **Enable latest features** the TFM supports
- **C# 13+:** Collection expressions, primary constructors, required members, file-scoped types
- **Don't:** Set `<LangVersion>` higher than the TFM's default unless adding a feature flag

### Check First

```bash
dotnet --version                 # Current SDK
dotnet --list-runtimes          # Installed runtimes
cat global.json                 # Project-level SDK constraint
```

## Project Configuration

### Enable Nullable Reference Types

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

**Why:** Catches null-related bugs at compile time. Non-nullable by default.

### Enable Implicit Usings

```xml
<PropertyGroup>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

**Rationale:**

- Reduces boilerplate (`System`, `System.Collections.Generic`, etc.)
- Still explicit when adding domain-specific usings

### Use File-Scoped Namespaces

```csharp
// ✅ CORRECT (C# 10+)
namespace HealthPassport.Application.Patients;

public sealed class GetPatientQuery { }

// ❌ AVOID
namespace HealthPassport.Application.Patients
{
    public sealed class GetPatientQuery { }
}
```

## Modern Language Features

### Primary Constructors

Use when they improve clarity and reduce boilerplate:

```csharp
// ✅ GOOD: Primary constructor for dependency injection
public sealed class GetPatientQueryHandler(IRepository repository) : IQueryHandler<GetPatientQuery, PatientResponse>
{
    public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct) =>
        await repository.GetByIdAsync(query.Id, ct);
}

// ✅ GOOD: Record with primary constructor
public sealed record GetPatientQuery(Guid Id) : IQuery<PatientResponse>;

// ❌ AVOID: Unnecessary for trivial classes
public sealed class SimpleUtility(string value);
```

### Collection Expressions (C# 12+)

```csharp
// ✅ MODERN (C# 12+)
var items = [item1, item2, item3];
int[] numbers = [1, 2, 3, 4, 5];
var list = new List<string> { ..existing };

// ❌ LEGACY
var items = new List<string> { item1, item2, item3 };
var numbers = new[] { 1, 2, 3, 4, 5 };
```

### Records for Immutable Models

```csharp
// ✅ USE for value objects, DTOs, responses
public sealed record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);

// ❌ AVOID: Classes only for mutable domain entities or service classes
```

### Required Members

```csharp
// ✅ Enforce complete initialization
public sealed record CreatePatientCommand
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public DateTime? DateOfBirth { get; init; }
}

// ❌ AVOID: Properties that must be set via reflection or builder
```

### Init-Only Properties

```csharp
// ✅ Immutable once constructed
public sealed record Patient
{
    public Guid Id { get; init; }
    public string Email { get; init; }
}

// ❌ AVOID: Public setters on immutable models
public class Patient { public string Email { get; set; } }
```

### Target-Typed New Expressions

```csharp
// ✅ TYPE IS CLEAR FROM CONTEXT
PatientResponse patient = new(id: Guid.NewGuid(), firstName: "John", lastName: "Doe", email: "john@example.com");
Dictionary<string, Patient> patients = new() { ["key"] = patient };

// ❌ AVOID: When type is unclear
var result = new();  // What type is this?
```

## Serialization

### System.Text.Json (NOT Newtonsoft.Json)

```csharp
// ✅ STANDARD
using System.Text.Json;
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var json = JsonSerializer.Serialize(data, options);

// Configure in Program.cs:
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

// ❌ AVOID: Newtonsoft.Json (legacy, slower)
using Newtonsoft.Json;
JsonConvert.SerializeObject(data);
```

**Why:** Built into .NET, faster, minimal allocations, native AOT compatible.

## Dependency Injection

### Constructor Injection

```csharp
// ✅ CORRECT
public sealed class PatientService(IRepository repository, ILogger<PatientService> logger)
{
    public async Task<Patient?> GetAsync(Guid id) =>
        await repository.GetByIdAsync(id);
}

// ❌ AVOID: Service locator pattern
public sealed class PatientService
{
    private readonly IServiceProvider _provider;
    public PatientService(IServiceProvider provider) => _provider = provider;
    public async Task<Patient?> GetAsync(Guid id) =>
        await _provider.GetRequiredService<IRepository>().GetByIdAsync(id);
}
```

### Register in DependencyInjection.cs

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddScoped<IPatientRepository, PatientRepository>();
    services.AddScoped<IPatientService, PatientService>();
    // ...
    return services;
}
```

### Lifetime Rules (Health Passport)

- **Singleton:** `IDateTimeProvider` only (stateless)
- **Scoped:** `IMediator`, `DbContext`, repositories, `ICurrentUserProvider`
- **Transient:** `IEndpoint` implementations **only**

## Asynchronous APIs

### Always Async (No Sync-Over-Async)

```csharp
// ✅ CORRECT: Async end-to-end
public async Task<Patient?> GetAsync(Guid id, CancellationToken ct) =>
    await _repository.GetByIdAsync(id, ct);

// ❌ AVOID: Sync wrapper over async
public Patient? Get(Guid id) => GetAsync(id).Result;

// ❌ AVOID: Sync that blocks
public Patient? Get(Guid id) => GetAsync(id).GetAwaiter().GetResult();
```

### CancellationToken Parameter

```csharp
// ✅ ALWAYS include ct parameter
public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
{
    var patient = await _repository.GetByIdAsync(query.Id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}

// ❌ AVOID: Missing cancellation support
public async Task<Result<PatientResponse>> Handle(GetPatientQuery query)
{
    var patient = await _repository.GetByIdAsync(query.Id);
    // ...
}
```

### ConfigureAwait(false) in Libraries

```csharp
// ✅ In library/infrastructure code: prevent unnecessary context switch
public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await _dbContext.Patients
        .FirstOrDefaultAsync(p => p.Id == id, ct)
        .ConfigureAwait(false);
}

// ✅ In application entry points: use default context
public async Task Main(string[] args)
{
    await app.RunAsync();  // No ConfigureAwait needed
}
```

## Null Handling

### Guard Clauses

```csharp
// ✅ CHECK EARLY
public void SetEmail(string email)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
    // Safe to use email now
}

// ✅ FOR NULLABLE REFERENCE TYPES
public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
{
    var patient = await _repository.GetByIdAsync(id, ct);
    return patient;  // Nullable<Patient>, caller must null-check
}

// ❌ AVOID: Implicit null tolerance
public void SetEmail(string? email)
{
    _email = email;  // Dangerous—might be null later
}
```

### Null Coalescing

```csharp
// ✅ EXPLICIT: Use ?? when you have a fallback
var name = user.FirstName ?? "Unknown";

// ✅ EXPLICIT: Use ?. for safe navigation
var email = user?.Email;

// ❌ AVOID: Blanket null suppression
public Patient! GetPatient() => _patients.FirstOrDefault()!;  // Lies about safety
```

## Error Handling

### Use Result<T> Pattern

```csharp
// ✅ CORRECT: Explicit error handling
public async Task<Result<PatientResponse>> Handle(GetPatientQuery query, CancellationToken ct)
{
    var patient = await _repository.GetByIdAsync(query.Id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound("Patient not found"))
        : Result<PatientResponse>.Success(patient.ToResponse());
}

// ✅ MAPPING TO HTTP
app.MapGet("/patients/{id}", async (Guid id, ISender mediator, CancellationToken ct) =>
{
    var query = new GetPatientQuery(id);
    var result = await mediator.Send(query, ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Problem(statusCode: 404, detail: "Patient not found");
});

// ❌ AVOID: Throwing exceptions for flow control
public async Task<PatientResponse> GetPatient(Guid id)
{
    try
    {
        var patient = await _repository.GetByIdAsync(id);
        return patient.ToResponse();
    }
    catch (KeyNotFoundException ex)
    {
        throw new InvalidOperationException("Patient not found", ex);
    }
}
```

### Precise Exception Types

```csharp
// ✅ SPECIFIC
ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

// ❌ VAGUE
throw new Exception("Invalid email");
```

## Obsolete APIs to Avoid

❌ **Never use:**

- `Activator.CreateInstance()` without validation
- `AppDomain.CurrentDomain` (obsolete in .NET Core)
- `System.Collections.ArrayList` (use generic `List<T>`)
- `Hashtable` (use `Dictionary<K,V>`)
- `.Result` / `.Wait()` on tasks (deadlock risk)
- `JsonConvert` (use `System.Text.Json`)
- `ConfigurationBuilder` without `.AddEnvironmentVariables()` (hard to configure in production)

## Performance Considerations

### Minimize Allocations Where Practical

```csharp
// ✅ GOOD: Reuse allocated buffers
var buffer = ArrayPool<byte>.Shared.Rent(1024);
try
{
    // Use buffer
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);
}

// ✅ GOOD: Use Span<T> for stack-allocated data
Span<int> numbers = stackalloc int[10];

// ❌ AVOID: Unnecessary allocations in hot paths
var list = new List<string>();
for (int i = 0; i < 1000000; i++)
{
    list.Add(i.ToString());  // Allocates 1M strings
}
```

### Async Streams

```csharp
// ✅ GOOD: Stream large results asynchronously
public async IAsyncEnumerable<Patient> GetPatientsByRegionAsync(string region, CancellationToken ct)
{
    await foreach (var patient in _repository.QueryByRegionAsync(region, ct))
    {
        yield return patient;
    }
}

// ✅ CONSUMING
await foreach (var patient in service.GetPatientsByRegionAsync("US-CA", ct))
{
    await ProcessPatientAsync(patient, ct);
}

// ❌ AVOID: Loading everything into memory
public async Task<List<Patient>> GetPatientsByRegion(string region)
{
    return await _repository.QueryByRegionAsync(region).ToListAsync();  // All at once
}
```

## Logging

### Use ILogger<T>

```csharp
// ✅ STANDARD
public sealed class PatientService(IRepository repository, ILogger<PatientService> logger)
{
    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Retrieving patient {PatientId}", id);
        try
        {
            return await repository.GetByIdAsync(id, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve patient {PatientId}", id);
            throw;
        }
    }
}

// ❌ AVOID: Console.WriteLine or custom logging
Console.WriteLine($"Patient: {id}");
```

## XML Documentation

### Public APIs

```csharp
// ✅ REQUIRED: Public methods and types
/// <summary>
/// Retrieves a patient by their unique identifier.
/// </summary>
/// <param name="id">The patient's unique identifier.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>
/// A task representing the asynchronous operation.
/// The result contains the patient if found; otherwise null.
/// </returns>
public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
}

// ✅ NOT required: Internal or trivial implementations
private string Normalize(string input) => input.Trim().ToLower();
```

## Migration Checklist

When generating new code, verify:

- [ ] Target framework is .NET 8+ (or project minimum)
- [ ] `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` in `.csproj`
- [ ] File-scoped namespaces (`namespace Foo;`)
- [ ] Primary constructors for dependency injection
- [ ] Records for immutable models (DTOs, responses)
- [ ] Async/await throughout, with `CancellationToken` parameters
- [ ] `Result<T>` for error handling (never throw for domain errors)
- [ ] Guard clauses for null checks
- [ ] `System.Text.Json` (not Newtonsoft.Json)
- [ ] `ILogger<T>` for logging
- [ ] XML documentation for public APIs
- [ ] No obsolete APIs

## References

- [What's new in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new)
- [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Async/await best practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [System.Text.Json documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json)

## Related Skills

- `skills/coding-standards/SKILL.md` – General engineering philosophy
- `skills/testing/SKILL.md` – Testing strategies
- `skills/documentation/SKILL.md` – Documentation standards
