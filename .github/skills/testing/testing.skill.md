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

... (rest preserved)
