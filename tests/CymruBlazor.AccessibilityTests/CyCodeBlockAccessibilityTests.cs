using Bunit;
using CymruBlazor.Components.Content;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyCodeBlockAccessibilityTests : AxeTestBase
{
    public CyCodeBlockAccessibilityTests()
    {
        Services.AddSingleton(new Mock<IMediator>().Object);
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_Copy_Button()
    {
        // Act
        var result = await ScanComponentAsync<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "dotnet build")
            .Add(p => p.Language, "bash"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_Without_Copy_Button()
    {
        // Act
        var result = await ScanComponentAsync<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "dotnet build")
            .Add(p => p.ShowCopyButton, false));

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
