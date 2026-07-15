using CymruBlazor.Components.Core;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Core;

public sealed class ComponentIdGeneratorTests
{
    [Fact]
    public void Create_WithNullPrefix_ReturnsDefaultPrefixWithUniqueId()
    {
        // Arrange
        IComponentIdGenerator generator = new ComponentIdGenerator();

        // Act
        var firstId = generator.Create(null);
        var secondId = generator.Create(null);

        // Assert
        firstId.ShouldStartWith("cy-");
        secondId.ShouldStartWith("cy-");
        firstId.ShouldNotBe(secondId);
    }

    [Fact]
    public void Create_WithValidPrefix_ReturnsSanitizedPrefixWithId()
    {
        // Arrange
        IComponentIdGenerator generator = new ComponentIdGenerator();

        // Act
        var result = generator.Create("Test_Prefix!123");

        // Assert
        result.ShouldStartWith("test_prefix123-");
    }
}
