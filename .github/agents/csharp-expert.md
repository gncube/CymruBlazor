---
title: C# Expert Agent
description: Specialized guidance for C# language features, patterns, and best practices
responsibilities:
  - Guide modern C# language feature usage and idioms
  - Review type system decisions and generic constraints
  - Recommend pattern matching, records, and immutability patterns
  - Advise on async/await, error handling, and performance
  - Guide LINQ and extension method design
requires:
  - .github/agents/architect.md
  - skills/dotnet-modern-development/SKILL.md
  - skills/coding-standards/SKILL.md
  - skills/testing/SKILL.md
---

# C# Expert Agent

Specialized guidance for writing idiomatic, performant, and maintainable C# code in CymruBlazor component library implementing the NHS Wales Design System.

## Responsibilities

### Language Feature Selection

- Recommend modern C# idioms (primary constructors, records, init-only properties)
- Guide use of generics, constraints, and type inference
- Advise on nullable reference types and null-safety
- Review component prop definitions and cascading parameters
- Recommend performance-conscious features (Span<T>, stackalloc for string handling)

### Component API Design

- Design clean component parameter contracts
- Guide event callback definitions
- Recommend child content and render fragment patterns
- Advise on component base class usage (CymruComponentBase)
- Guide CSS class composition and theming

### Error Handling Patterns

- Review exception usage vs. graceful degradation
- Guide defensive programming (null checks, validation)
- Recommend logging for component lifecycle issues
- Advise on validation error display in components

### Performance & Rendering

- Identify unnecessary re-renders
- Recommend Span<T> for string parsing in CSS class builders
- Guide memory allocation in event handlers
- Recommend ShouldRender() overrides for optimization
- Advise on component lifecycle efficiency

### Async & Interop

- Guide async method design in component initialization
- Advise on Task vs. ValueTask for component callbacks
- Guide CancellationToken usage in component methods
- Review JavaScript interop for accessibility concerns

## Language Feature Decision Tree

### Do You Need to Return Multiple Values?

**Yes** → Use tuple, record, or dedicated type

```csharp
// ✅ TUPLE FOR SIMPLE DATA
public async Task<(Patient Patient, int AppointmentCount)> GetPatientWithCountAsync(Guid id)
{
    var patient = await _repository.GetByIdAsync(id);
    var count = await _appointmentRepository.CountByPatientAsync(id);
    return (patient, count);
}

var (patient, count) = await GetPatientWithCountAsync(id);

// ✅ RECORD FOR SEMANTIC MEANING
public sealed record PatientWithCount(Patient Patient, int AppointmentCount);

public async Task<PatientWithCount> GetPatientWithCountAsync(Guid id)
{
    var patient = await _repository.GetByIdAsync(id);
    var count = await _appointmentRepository.CountByPatientAsync(id);
    return new PatientWithCount(patient, count);
}

// ❌ AVOID: Output parameters (C# 1.0 style)
public bool TryGetPatient(Guid id, out Patient? patient)
{
    patient = await _repository.GetByIdAsync(id);
    return patient is not null;
}
```

### Does This Type Represent a Concept That Should Be Immutable?

**Yes** → Use `record` (immutable by default)

```csharp
// ✅ RECORD FOR COMPONENT PARAMETERS
public sealed record ButtonComponentParams(
    ButtonVariant Variant = ButtonVariant.Primary,
    bool Disabled = false,
    string? CssClass = null,
    string? AriaLabel = null);

var buttonParams = new ButtonComponentParams
{
    Variant = ButtonVariant.Secondary,
    Disabled = false
};
var (variant, disabled, css, label) = buttonParams;  // Deconstruction

// ❌ MUTABLE CLASS: Risk of accidental state changes
public class ButtonParams
{
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    public bool Disabled { get; set; } = false;
    public string? CssClass { get; set; }
}
```

**No** → Use `class` only for stateful services or builders

```csharp
// ✅ CLASS: Mutable builder for complex construction
public sealed class PatientBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "";

    public PatientBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
    public Patient Build() => new() { Id = _id, FirstName = _firstName };
}

// ✅ CLASS: Stateful service with responsibilities
public sealed class PatientService
{
    private readonly IRepository _repository;
    private readonly IValidator _validator;

    public PatientService(IRepository repository, IValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<PatientResponse>> GetAsync(Guid id, CancellationToken ct) =>
        await _repository.GetByIdAsync(id, ct);
}
```

### Do You Have Conditional Logic That Depends on Type?

**Yes** → Use pattern matching instead of casting and null checks

```csharp
// ✅ PATTERN MATCHING: Clear, concise, exhaustive
public string Describe(object obj) => obj switch
{
    Patient p => $"Patient: {p.FirstName} {p.LastName}",
    Appointment a => $"Appointment on {a.DateTime:g}",
    Diagnostic d => $"Diagnostic: {d.TestName}",
    string s => $"Text: {s}",
    null => "Unknown",
    _ => obj.ToString() ?? "Object"
};

// ✅ NULL-COALESCING PATTERN
public void Process(Patient? patient)
{
    if (patient is null)
        throw new ArgumentNullException(nameof(patient));

    // Use patient safely
}

// ✅ PROPERTY PATTERN
public bool IsValidPatient(Patient patient) => patient switch
{
    { FirstName.Length: > 0, LastName.Length: > 0, Email.Length: > 0 } => true,
    _ => false
};

// ❌ AVOID: Old-style casting
if (obj is Patient)
{
    var patient = (Patient)obj;
    Console.WriteLine(patient.FirstName);
}
```

### Should This Be a struct (Value Type) or class (Reference Type)?

**Value Type (struct)** → Small, immutable, hot path allocation, no identity needed

```csharp
// ✅ STRUCT: Small immutable value (< 128 bits typically)
public sealed record struct PatientId(Guid Value)
{
    public static PatientId Empty => default;
    public bool IsEmpty => Value == Guid.Empty;
}

// ✅ STRUCT: Enumerator pattern
public struct PatientEnumerator : IEnumerator<Patient>
{
    private readonly Patient[] _patients;
    private int _index;

    public Patient Current => _patients[_index];
    public bool MoveNext() => ++_index < _patients.Length;
}
```

**Reference Type (class)** → Large objects, mutable state, identity needed, long-lived

```csharp
// ✅ CLASS: Large object with identity and state
public sealed class PatientContext
{
    public Guid Id { get; init; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public List<Appointment> Appointments { get; } = [];
    public List<Diagnostic> Diagnostics { get; } = [];
}
```

### How Should This Collection Be Serialized/Passed?

**Read-only** → `IEnumerable<T>` or `IReadOnlyList<T>`

```csharp
// ✅ RETURNS ENUMERABLE: Caller can't modify
public IEnumerable<Patient> GetPatients(Guid departmentId)
{
    return _repository.GetByDepartmentAsync(departmentId);
}

// ✅ ACCEPTS ENUMERABLE: Flexible on input
public async Task CreateBatchAsync(IEnumerable<CreatePatientCommand> commands, CancellationToken ct)
{
    foreach (var cmd in commands)
    {
        await CreateAsync(cmd, ct);
    }
}

// ❌ AVOID: Returning mutable lists (caller can modify internal state)
public List<Patient> GetPatients(Guid departmentId)
{
    return _repository.GetByDepartmentAsync(departmentId).ToList();
}
```

**Writable** → Specific collection type (List<T>, Dictionary<K,V>)

```csharp
// ✅ RETURNS LIST: Caller can add/remove
public async Task<List<Patient>> GetPatientsAsync(Guid departmentId)
{
    var patients = await _repository.GetByDepartmentAsync(departmentId);
    return patients.ToList();  // Explicit copy
}
```

## Modern C# Idioms

### Primary Constructors (C# 12+)

```csharp
// ✅ PRIMARY CONSTRUCTOR: Parameter automatically assigned to field
public sealed class PatientRepository(IHealthPassportDbContext context) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
}

// ❌ OLD STYLE: Explicit constructor and field
public sealed class PatientRepository : IPatientRepository
{
    private readonly IHealthPassportDbContext _context;

    public PatientRepository(IHealthPassportDbContext context) => _context = context;

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
}
```

### Collection Expressions (C# 12+)

```csharp
// ✅ COLLECTION EXPRESSION: Concise, flexible
public async Task CreateBatchAsync(IEnumerable<Patient> patients)
{
    var patientsArray = [..patients];
    var patientsList = [..patients];
    var spread = [..existing, ..new_];
}

// ❌ OLD STYLE: Verbose
public async Task CreateBatchAsync(IEnumerable<Patient> patients)
{
    var patientsArray = patients.ToArray();
    var patientsList = patients.ToList();
}
```

### Init-Only Properties

```csharp
// ✅ INIT-ONLY: Can be set in constructor or initializer, then immutable
public sealed class PatientRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string Email { get; init; } = "";
}

var request = new PatientRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com"
};
// request.FirstName = "Jane";  // Compiler error

// ❌ MUTABLE: Can be changed unexpectedly
public sealed class PatientRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
```

### Required Members (C# 11+)

```csharp
// ✅ REQUIRED: Compiler ensures properties are set
public sealed class PatientResponse
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

var response = new PatientResponse { Id = id, FirstName = "John", LastName = "Doe" };
// var incomplete = new PatientResponse();  // Compiler error

// ❌ WITHOUT REQUIRED: Easy to forget properties
public sealed class PatientResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
}
```

## Async & Task Patterns

### Async All the Way

```csharp
// ✅ FULLY ASYNC: No blocking calls
public sealed class PatientService(IPatientRepository repository)
{
    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct);

    public async Task<IEnumerable<Patient>> SearchAsync(string query, CancellationToken ct) =>
        await repository.SearchAsync(query, ct);
}

// ❌ BLOCKING: Sync over async (deadlock risk in UI contexts)
public Patient? Get(Guid id) =>
    repository.GetByIdAsync(id, CancellationToken.None).Result;  // DEADLOCK RISK!

// ❌ MIXING: Async method without async body
public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
{
    return repository.GetByIdAsync(id, ct).Result;  // Wrong!
}
```

### Task vs. ValueTask

```csharp
// ✅ TASK: Standard async method
public async Task<Patient?> GetFromDatabaseAsync(Guid id, CancellationToken ct) =>
    await _repository.GetByIdAsync(id, ct);

// ✅ VALUETASK: Hot path that's often synchronous (cache hit, validation pass)
public ValueTask<Patient?> GetWithCacheAsync(Guid id, CancellationToken ct)
{
    if (_cache.TryGetValue(id, out var patient))
        return new ValueTask<Patient?>(patient);

    return new ValueTask<Patient?>(GetFromDatabaseAsync(id, ct));
}

// ❌ AVOID: ValueTask when allocation is unavoidable (defeats purpose)
public async ValueTask<Patient> GetAsync(Guid id, CancellationToken ct) =>
    await _repository.GetByIdAsync(id, ct)!;  // Always allocates if async
```

### CancellationToken Propagation

```csharp
// ✅ PROPAGATE TOKENS: Respect caller's cancellation
public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken ct)
{
    var patients = new List<Patient>();

    foreach (var batchId in GetBatchIds())
    {
        var batch = await _repository.GetBatchAsync(batchId, ct);
        patients.AddRange(batch);
    }

    return patients;
}

// ❌ IGNORE CANCELLATION: Creates "fire-and-forget" operations
public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken ct)
{
    var patients = new List<Patient>();

    foreach (var batchId in GetBatchIds())
    {
        var batch = await _repository.GetBatchAsync(batchId, CancellationToken.None);
        patients.AddRange(batch);
    }

    return patients;
}
```

## Error Handling & Result<T> Pattern

### Use Result<T> for Domain Errors, Exceptions for Bugs

```csharp
// ✅ RESULT<T> FOR DOMAIN ERRORS
public sealed class CreatePatientCommandHandler(IRepository repository)
    : ICommandHandler<CreatePatientCommand, Result<PatientResponse>>
{
    public async Task<Result<PatientResponse>> Handle(CreatePatientCommand cmd, CancellationToken ct)
    {
        // Validation is a domain concern, not exceptional
        if (string.IsNullOrWhiteSpace(cmd.Email))
            return Result<PatientResponse>.Failure(Error.Validation("Email required"));

        var existing = await repository.GetByEmailAsync(cmd.Email, ct);
        if (existing is not null)
            return Result<PatientResponse>.Failure(Error.Conflict("Email already registered"));

        var patient = new Patient { Id = Guid.NewGuid(), Email = cmd.Email };
        await repository.AddAsync(patient, ct);

        return Result<PatientResponse>.Success(patient.ToResponse());
    }
}

// ✅ EXCEPTION FOR BUGS (missing dependencies, runtime failures)
public sealed class PatientRepository(IHealthPassportDbContext context) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id.ToString(), nameof(id));

        try
        {
            return await context.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        catch (SqlException ex)
        {
            // Database failure is exceptional
            throw new DataAccessException("Failed to query patients", ex);
        }
    }
}

// ❌ THROWING FOR DOMAIN ERRORS: Lose information, harder to handle
if (string.IsNullOrWhiteSpace(cmd.Email))
    throw new ValidationException("Email required");  // Now caller must catch
```

### Accumulating Validation Errors

```csharp
// ✅ COLLECT ALL ERRORS: Help caller fix multiple issues at once
public Result<PatientResponse> ValidatePatientCommand(CreatePatientCommand cmd)
{
    var errors = new List<Error>();

    if (string.IsNullOrWhiteSpace(cmd.FirstName))
        errors.Add(Error.Validation("FirstName is required"));

    if (string.IsNullOrWhiteSpace(cmd.LastName))
        errors.Add(Error.Validation("LastName is required"));

    if (string.IsNullOrWhiteSpace(cmd.Email) || !cmd.Email.Contains("@"))
        errors.Add(Error.Validation("Email is required and must be valid"));

    return errors.Count > 0
        ? Result<PatientResponse>.Failure(errors.ToArray())
        : Result<PatientResponse>.Success(null!);  // Placeholder
}

// ❌ FAIL ON FIRST ERROR: Caller must fix one issue at a time
if (string.IsNullOrWhiteSpace(cmd.FirstName))
    return Result<PatientResponse>.Failure(Error.Validation("FirstName is required"));
if (string.IsNullOrWhiteSpace(cmd.LastName))
    return Result<PatientResponse>.Failure(Error.Validation("LastName is required"));
```

## LINQ & Extension Methods

### Query vs. Method Syntax

```csharp
// ✅ QUERY SYNTAX: Readable for complex transformations
var activePatients =
    from p in patients
    where p.IsActive
    join a in appointments on p.Id equals a.PatientId
    group a by p.Id into g
    where g.Count() > 0
    select new { PatientId = g.Key, AppointmentCount = g.Count() };

// ✅ METHOD SYNTAX: Concise for simple operations
var activePatients = patients
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.CreatedAt)
    .Take(10);

// ❌ OVERCOMPLICATED: Too much chaining without intermediate variables
var result = patients
    .Where(p => p.IsActive)
    .Join(appointments, p => p.Id, a => a.PatientId, (p, a) => new { p, a })
    .GroupBy(x => x.p.Id)
    .Select(g => new { PatientId = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .Take(10)
    .ToList();

// ✅ CLEARER: Break into named steps
var activePatients = patients.Where(p => p.IsActive).ToList();
var withAppointments = activePatients
    .Join(appointments, p => p.Id, a => a.PatientId, (p, a) => new { p, a });
var grouped = withAppointments.GroupBy(x => x.p.Id);
var result = grouped
    .Select(g => new { PatientId = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .Take(10)
    .ToList();
```

### Lazy vs. Eager Evaluation

```csharp
// ✅ LAZY: IEnumerable defers execution
IEnumerable<Patient> GetActive(IEnumerable<Patient> patients) =>
    patients.Where(p => p.IsActive);  // No execution yet

var active = GetActive(patients);
// Execution happens here
foreach (var p in active) { /* ... */ }

// ✅ EAGER: ToList() for required evaluation
List<Patient> GetActive(IEnumerable<Patient> patients) =>
    patients.Where(p => p.IsActive).ToList();  // Executes immediately

// ❌ ACCIDENTALLY LAZY: Forgot to materialize
public IEnumerable<Patient> GetActivePatients()
{
    return _patients.Where(p => p.IsActive);  // Returns query, not results
}
// Caller expects data but gets query that might execute later (or change)
```

## Nullable Reference Types & Null Safety

### Proper Null Handling

```csharp
// ✅ GUARD CLAUSES: Check early, fail fast
public async Task<Result<PatientResponse>> GetAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return Result<PatientResponse>.Failure(Error.Validation("Id required"));

    var patient = await _repository.GetByIdAsync(id, ct);
    return patient is null
        ? Result<PatientResponse>.Failure(Error.NotFound())
        : Result<PatientResponse>.Success(patient.ToResponse());
}

// ✅ NULL COALESCING: Provide fallback
public string GetPatientName(Patient? patient) =>
    patient?.FirstName ?? "Unknown";

// ❌ UNSAFE: Null reference exception risk
public string GetPatientName(Patient? patient)
{
    return patient.FirstName;  // Compiler warning (if NRT enabled)
}

// ❌ DEFENSIVE: Over-checking
if (patient is not null && patient.FirstName is not null && patient.FirstName.Length > 0)
{
    // Use patient.FirstName
}

// ✅ CLEAN: Type tells the story
public sealed record Patient(
    Guid Id,
    string FirstName,  // Non-nullable; always present
    string LastName,
    string? MiddleName = null);  // Nullable; optional
```

## Performance Considerations

### Identify Hot Paths

```csharp
// ✅ MEASURE FIRST: Only optimize when profiling shows it matters
[Fact]
public void GetPatients_Performance()
{
    var sw = Stopwatch.StartNew();

    for (int i = 0; i < 1000; i++)
    {
        GetPatients(departmentId);
    }

    sw.Stop();
    sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));  // 1ms per call
}

// ✅ USE SPAN FOR PARSING/PROCESSING
public bool TryParsePatientId(ReadOnlySpan<char> input, out Guid id)
{
    return Guid.TryParse(input, out id);
}

// ❌ UNNECESSARY ALLOCATION: String conversion before parsing
public bool TryParsePatientId(ReadOnlySpan<char> input, out Guid id)
{
    return Guid.TryParse(input.ToString(), out id);  // Allocates string
}
```

## See Also

- `.github/agents/architect.md` – Component design patterns
- `.github/agents/blazor-expert.md` – Blazor best practices
- `skills/dotnet-modern-development/SKILL.md` – .NET SDK and language guidance
- `skills/coding-standards/SKILL.md` – Engineering principles
