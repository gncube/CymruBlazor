using Bunit;
using CymruBlazor.Components.Data;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyPaginationAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_On_A_Middle_Page()
    {
        // Act
        var result = await ScanComponentAsync<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 5)
            .Add(p => p.TotalPages, 10));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_On_The_First_Page()
    {
        // Act
        var result = await ScanComponentAsync<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 10));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange
        var markup = string.Join(
            Environment.NewLine,
            Render<CyPagination>(parameters => parameters
                .Add(p => p.CurrentPage, 1)
                .Add(p => p.TotalPages, 10)
                .Add(p => p.AriaLabel, "Pagination (first page)")).Markup,
            Render<CyPagination>(parameters => parameters
                .Add(p => p.CurrentPage, 5)
                .Add(p => p.TotalPages, 10)
                .Add(p => p.AriaLabel, "Pagination (middle page)")).Markup);

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
