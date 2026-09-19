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

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_With_Language_Label_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - CyCodeBlock hard-codes some colours, so expect real findings here first
        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "dotnet build")
            .Add(p => p.Language, "bash"));

        // Act
        var result = await ScanMarkupAsync(cut.Markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
