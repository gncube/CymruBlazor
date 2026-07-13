---
title: Testing Standards
description: Test strategy, frameworks, and expectations for all code generation
applies_to: ["**/*.Tests.cs", "tests/**/*.cs"]
requires:
  - skills/coding-standards/SKILL.md
  - skills/dotnet-modern-development/SKILL.md
---

# Testing Standards

This skill establishes the testing philosophy and practices across the project. Tests are first-class code—they must be maintainable, clear, and reliable.

## Testing Philosophy

### Core Principle: Test Behavior, Not Implementation

Tests should verify that the code does what it's supposed to do, not HOW it does it. Tests should survive refactoring as long as behavior is preserved.

```csharp
// ❌ TESTS IMPLEMENTATION
[Fact]
public async Task Handle_ShouldQueryRepository_WhenCalled()
{
    // This test breaks if we refactor to use a different repository
    var mockRepository = new Mock<IRepository>();
    var handler = new GetPatientQueryHandler(mockRepository.Object);

    await handler.Handle(new GetPatientQuery(Guid.NewGuid()), CancellationToken.None);

    mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
}

// ✅ TESTS BEHAVIOR
[Fact]
public async Task Handle_WhenPatientExists_ReturnsPatientResponse()
{
    // Arrange
    var patientId = Guid.NewGuid();
    var patient = new Patient { Id = patientId, FirstName = "John", LastName = "Doe" };
    var mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(patient);
    var handler = new GetPatientQueryHandler(mockRepository.Object);

    // Act
    var result = await handler.Handle(new GetPatientQuery(patientId), CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(patientId);
    result.Value.FirstName.Should().Be("John");
}
```

### Test Coverage Standards

Aim for **high behavior coverage**, not high line coverage. 80%+ line coverage is a reasonable target for application code, but meaningless if tests are shallow.

- **New code:** Every public method/behavior must have at least one passing test
- **Bug fixes:** Add a test that reproduces the bug, then fix it
- **Refactoring:** Existing tests should continue to pass without modification

## Test Structure and Organization

### Folder Organization

```
tests/
├── Client.Tests/
│   ├── Features/
│   │   ├── Authentication/
│   │   │   ├── AuthenticationServiceTests.cs
│   │   │   └── LoginComponentTests.cs
│   │   ├── Dashboard/
│   │   │   └── DashboardPageTests.cs
│   │   └── Navigation/
│   │       └── NavigationAuthorizationServiceTests.cs
│   ├── Client.Tests.csproj
│   └── obj/, bin/
└── HealthPassport.API.Tests/
    ├── Features/
    │   ├── Patients/
    │   │   ├── GetPatientQueryHandlerTests.cs
    │   │   ├── CreatePatientCommandHandlerTests.cs
    │   │   └── PatientEndpointTests.cs
    │   └── Users/
    │       └── ...
    └── HealthPassport.API.Tests.csproj
```

### Naming Convention: `{ComponentUnderTest}Tests`

Test classes mirror production structure:

```csharp
// Production: HealthPassport.Application/Patients/Handlers/GetPatientQueryHandler.cs
// Test:       tests/HealthPassport.API.Tests/Features/Patients/GetPatientQueryHandlerTests.cs

public sealed class GetPatientQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenPatientExists_ReturnsSuccess()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        // ...
    }

    [Fact]
    public async Task Handle_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        // ...
    }
}
```

### Test Method Naming: `{Method}_{Scenario}_{Expected}`

Test names clearly describe the behavior being tested.

```csharp
public sealed class CreatePatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesPatientAndReturnsSuccess()
    {
        // Arrange
        var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsFail()
    {
        // Arrange
        var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "not-an-email" };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("email", StringComparison.OrdinalIgnoreCase));
    }
}
```

## Test Frameworks and Tools

### Primary Framework: xUnit

```csharp
// ✅ STANDARD TEST
[Fact]
public async Task SomeMethodAsync_WithCondition_ExpectedBehavior()
{
    // Arrange
    var sut = CreateSystemUnderTest();

    // Act
    var result = await sut.SomeMethodAsync();

    // Assert
    result.Should().NotBeNull();
}

// ✅ PARAMETERIZED TEST
[Theory]
[InlineData("john@example.com", true)]
[InlineData("invalid-email", false)]
[InlineData("", false)]
public void ValidateEmail_WithInput_ReturnsExpected(string email, bool expected)
{
    // Arrange
    var validator = new EmailValidator();

    // Act
    var result = validator.IsValid(email);

    // Assert
    result.Should().Be(expected);
}

// ❌ AVOID: NUnit or MSTest (not in this project)
[TestMethod]  // Wrong framework
public void TestSomething() { }

[Test]  // Wrong framework
public void TestSomething() { }
```

### Assertion Framework: Shouldly

```csharp
// ✅ SHOULDLY (fluent, readable)
result.IsSuccess.Should().BeTrue();
patient.FirstName.Should().Be("John");
patients.Should().HaveCount(3);
patients.Should().AllSatisfy(p => p.IsActive);
exception.Should().BeOfType<ArgumentException>();
action.Should().Throw<InvalidOperationException>();

// ❌ AVOID: Manual assertions (unclear)
Assert.IsTrue(result.IsSuccess);
Assert.AreEqual("John", patient.FirstName);
Assert.AreEqual(3, patients.Count);

// ❌ AVOID: Fluent Assertions (not in this project)
result.IsSuccess.Should().BeTrue();  // Use Shouldly instead
```

### Mocking Framework: Moq

```csharp
// ✅ MOCK EXTERNAL DEPENDENCIES
[Fact]
public async Task Handle_WhenRepositoryReturnsNull_ReturnsNotFound()
{
    // Arrange
    var query = new GetPatientQuery(Guid.NewGuid());
    var mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(r => r.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Patient?)null);
    var handler = new GetPatientQueryHandler(mockRepository.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().Contain(e => e.Code == "NotFound");
}

// ✅ VERIFY CALLS
[Fact]
public async Task Handle_CallsRepository_WithCorrectId()
{
    // Arrange
    var patientId = Guid.NewGuid();
    var query = new GetPatientQuery(patientId);
    var mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Patient { Id = patientId });
    var handler = new GetPatientQueryHandler(mockRepository.Object);

    // Act
    await handler.Handle(query, CancellationToken.None);

    // Assert
    mockRepository.Verify(
        r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
        Times.Once);
}

// ❌ AVOID: Mocking everything (defeats the purpose)
var mockLogger = new Mock<ILogger>();  // Why mock a logger in handler tests?
var mockDateTime = new Mock<IDateTimeProvider>();  // Inject real time provider or use a stub
```

### Approval Testing for Complex Objects

Use Approval Tests for complex response objects and generated output.

```csharp
// ✅ APPROVAL TEST
[Fact]
public async Task GeneratePatientReport_WithPatientData_GeneratesCorrectReport()
{
    // Arrange
    var patient = new Patient { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
    var reportService = new PatientReportService();

    // Act
    var report = await reportService.GenerateAsync(patient);

    // Assert
    Approvals.Verify(report);  // Creates PatientReportServiceTests.GeneratePatientReport_WithPatientData_GeneratesCorrectReport.approved.txt
}
```

## Arrange-Act-Assert Pattern

Every test follows AAA structure:

```csharp
[Fact]
public async Task Handle_WithValidCommand_SavesPatientAndReturnsSuccess()
{
    // ARRANGE: Set up test data and dependencies
    var command = new CreatePatientCommand
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com"
    };
    var mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    mockUnitOfWork
        .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);  // 1 row saved
    var handler = new CreatePatientCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

    // ACT: Execute the behavior being tested
    var result = await handler.Handle(command, CancellationToken.None);

    // ASSERT: Verify the expected outcome
    result.IsSuccess.Should().BeTrue();
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
    mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

## Test Quality Principles

### One Assertion Focus Per Test (Not One Assertion Total)

A test can have multiple assertions, but they should all verify the same behavior. If you're testing multiple independent behaviors, write separate tests.

```csharp
// ❌ MULTIPLE INDEPENDENT BEHAVIORS
[Fact]
public async Task Handle_WithValidCommand_WorksCorrectly()
{
    var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var handler = CreateHandler();

    var result = await handler.Handle(command, CancellationToken.None);

    // These are independent assertions—failure in one hides others
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().NotBe(Guid.Empty);
    result.Value.FirstName.Should().Be("John");
    result.Value.Email.Should().Be("john@example.com");
    result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}

// ✅ FOCUSED TESTS
[Fact]
public async Task Handle_WithValidCommand_ReturnsSuccess()
{
    var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var handler = CreateHandler();

    var result = await handler.Handle(command, CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
}

[Fact]
public async Task Handle_WithValidCommand_GeneratesNewId()
{
    var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var handler = CreateHandler();

    var result = await handler.Handle(command, CancellationToken.None);

    result.Value.Id.Should().NotBe(Guid.Empty);
}

[Fact]
public async Task Handle_WithValidCommand_PreservesNameAndEmail()
{
    var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var handler = CreateHandler();

    var result = await handler.Handle(command, CancellationToken.None);

    result.Value.FirstName.Should().Be("John");
    result.Value.LastName.Should().Be("Doe");
    result.Value.Email.Should().Be("john@example.com");
}
```

### Fast, Deterministic, Isolated Tests

✅ **Fast:** Unit tests should complete in milliseconds. Integration tests in seconds.

❌ **Slow:** Tests with `Thread.Sleep()`, database operations without cleanup, or external API calls.

```csharp
// ❌ SLOW AND FRAGILE
[Fact]
public async Task Handle_WithValidCommand_WorksCorrectly()
{
    // Wait for async operations
    Thread.Sleep(100);

    // Uses real database
    using var context = new HealthPassportDbContext();
    var repository = new PatientRepository(context);
    var handler = new CreatePatientCommandHandler(repository, new UnitOfWork(context));

    var result = await handler.Handle(
        new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
        CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    // Database not cleaned up; subsequent tests are affected
}

// ✅ FAST AND ISOLATED
[Fact]
public async Task Handle_WithValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var mockRepository = new Mock<IRepository>();
    mockRepository.Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    var handler = new CreatePatientCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    // No shared state; tests run independently
}
```

### Avoid Branching in Tests

Tests should not have if/else, loops, or conditional logic. Each test is a single path.

```csharp
// ❌ BRANCHING IN TESTS
[Theory]
[InlineData("john@example.com", true)]
[InlineData("invalid-email", false)]
public void ValidateEmail_WithInput_ReturnsExpected(string email, bool expected)
{
    var validator = new EmailValidator();

    var result = validator.IsValid(email);

    if (expected)
    {
        result.Should().BeTrue();
    }
    else
    {
        result.Should().BeFalse();
    }
}

// ✅ SEPARATE TESTS
[Fact]
public void ValidateEmail_WithValidEmail_ReturnsTrue()
{
    var validator = new EmailValidator();

    var result = validator.IsValid("john@example.com");

    result.Should().BeTrue();
}

[Fact]
public void ValidateEmail_WithInvalidEmail_ReturnsFalse()
{
    var validator = new EmailValidator();

    var result = validator.IsValid("invalid-email");

    result.Should().BeFalse();
}
```

## Testing Different Layers

### Unit Tests (Handler/Service Tests)

Test a single class in isolation with mocked dependencies.

```csharp
[Fact]
public async Task Handle_CallsRepository_AndReturnsResult()
{
    var mockRepository = new Mock<IRepository>();
    mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Patient { Id = Guid.NewGuid(), FirstName = "John" });

    var handler = new GetPatientQueryHandler(mockRepository.Object);
    var query = new GetPatientQuery(Guid.NewGuid());

    var result = await handler.Handle(query, CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
}
```

### Integration Tests (Endpoint Tests)

Test the full request/response pipeline without external services.

```csharp
[Fact]
public async Task GetPatient_WithValidId_Returns200AndPatientData()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();
    var patientId = Guid.NewGuid();

    // Act
    var response = await client.GetAsync($"/api/patients/{patientId}");

    // Assert
    response.StatusCode.Should().Be(200);
    var content = await response.Content.ReadAsAsync<PatientResponse>();
    content.Id.Should().Be(patientId);
}
```

### Performance/Load Tests

Use when performance is a requirement.

```csharp
[Fact]
public async Task GetPatient_WithThousandCalls_CompletesInReasonableTime()
{
    var handler = CreateHandler();
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < 1000; i++)
    {
        await handler.Handle(new GetPatientQuery(Guid.NewGuid()), CancellationToken.None);
    }

    stopwatch.Stop();
    stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));  // 5ms per call average
}
```

## Test Helpers and Fixtures

### Build Test Data Cleanly

```csharp
// ✅ BUILDER PATTERN for complex objects
public sealed class PatientBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john@example.com";

    public PatientBuilder WithId(Guid id) { _id = id; return this; }
    public PatientBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
    public PatientBuilder WithEmail(string email) { _email = email; return this; }

    public Patient Build() => new()
    {
        Id = _id,
        FirstName = _firstName,
        LastName = _lastName,
        Email = _email
    };
}

[Fact]
public async Task Handle_WithPatient_ReturnsPatientResponse()
{
    var patient = new PatientBuilder().WithFirstName("Alice").Build();
    // Much clearer than: new Patient { Id = Guid.NewGuid(), FirstName = "Alice", ... }
}

// ✅ FACTORY METHODS
public sealed class HandlerTestFixture
{
    public IRepository Repository { get; } = new Mock<IRepository>().Object;
    public IUnitOfWork UnitOfWork { get; } = new Mock<IUnitOfWork>().Object;

    public GetPatientQueryHandler CreateGetPatientQueryHandler() =>
        new(Repository);

    public CreatePatientCommandHandler CreateCreatePatientCommandHandler() =>
        new(Repository, UnitOfWork);
}
```

### Use xUnit ClassFixture for Shared Setup

```csharp
public sealed class PatientHandlerTests : IClassFixture<HandlerTestFixture>
{
    private readonly HandlerTestFixture _fixture;

    public PatientHandlerTests(HandlerTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        var handler = _fixture.CreateCreatePatientCommandHandler();
        var command = new CreatePatientCommand { FirstName = "John", LastName = "Doe", Email = "john@example.com" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
```

## Test Coverage Expectations

- **New code:** Tests must be added when new production code is generated
- **Bug fixes:** A test that reproduces the bug is required
- **Refactoring:** Existing tests should continue to pass
- **Line coverage:** Aim for 80%+ for application code
- **Behavior coverage:** More important than line coverage

## See Also

- `skills/coding-standards/SKILL.md` – General engineering standards
- `skills/dotnet-modern-development/SKILL.md` – Modern .NET practices
- `tests/Client.Tests/Client.Tests.csproj` – Existing test project
