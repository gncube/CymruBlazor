using Bunit;
using CymruBlazor.Components.Button;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyButtonAccessibilityTests : AxeTestBase
{
    [Theory]
    [InlineData(ComponentColour.Primary)]
    [InlineData(ComponentColour.Secondary)]
    [InlineData(ComponentColour.Tertiary)]
    [InlineData(ComponentColour.Danger)]
    public async Task Should_Have_No_Violations_For_Each_Variant(ComponentColour variant)
    {
        // Act
        var result = await ScanComponentAsync<CyButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Save changes"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Disabled()
    {
        // Act
        var result = await ScanComponentAsync<CyButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .AddChildContent("Save changes"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Loading()
    {
        // Act
        var result = await ScanComponentAsync<CyButton>(parameters => parameters
            .Add(p => p.Loading, true)
            .AddChildContent("Save changes"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Rendered_As_A_Link()
    {
        // Act
        var result = await ScanComponentAsync<CyButton>(parameters => parameters
            .Add(p => p.Href, "/getting-started")
            .AddChildContent("Get started"));

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // This is exactly the class of bug that motivated building real axe
        // scans in the first place: a component can look fine in the
        // default light theme and still fail contrast once dark/high-
        // contrast tokens are active, and bUnit alone has no way to catch
        // that (it can assert a class is present, not what colour it
        // resolves to). See CHANGELOG 0.1.0-preview.8 for a real example
        // of exactly this - CyBrandLogo passed every existing test while
        // rendering illegible dark-on-dark text in a dark header.

        // Arrange
        var cut = Render<CyButton>(parameters => parameters
            .Add(p => p.Variant, ComponentColour.Primary)
            .AddChildContent("Save changes"));

        // Act
        var result = await ScanMarkupAsync(cut.Markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }

    // Dark and high-contrast scans (roadmap v1.2.1, Phase 5). Every state is
    // rendered into ONE markup per theme to keep CI time down.
    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Variants_And_States_In_Dark_And_High_Contrast_Themes(string theme)
    {
        // Arrange - every variant, plus disabled and loading. (The Theory
        // above only ever scanned the Primary variant.)
        ComponentColour[] variants =
            [ComponentColour.Primary, ComponentColour.Secondary, ComponentColour.Tertiary, ComponentColour.Danger];

        var markup = string.Join(
            Environment.NewLine,
            variants.Select(variant => Render<CyButton>(parameters => parameters
                .Add(p => p.Variant, variant)
                .AddChildContent("Save changes")).Markup)
            .Append(Render<CyButton>(parameters => parameters
                .Add(p => p.Disabled, true)
                .AddChildContent("Save changes")).Markup)
            .Append(Render<CyButton>(parameters => parameters
                .Add(p => p.Loading, true)
                .AddChildContent("Save changes")).Markup));

        // Act
        var result = await ScanMarkupAsync(markup, theme);

        // Assert
        result.Violations.ShouldBeEmpty();
    }
}
