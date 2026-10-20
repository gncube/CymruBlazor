using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyAvatarAccessibilityTests : AxeTestBase
{
    [Fact]
    public async Task Should_Have_No_Violations_For_Default_Avatar()
    {
        var result = await ScanComponentAsync<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe"));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Decorative()
    {
        var result = await ScanComponentAsync<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Decorative, true));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(AvatarStatus.Online)]
    [InlineData(AvatarStatus.Busy)]
    [InlineData(AvatarStatus.Away)]
    [InlineData(AvatarStatus.Offline)]
    public async Task Should_Have_No_Violations_For_Status_Variants(AvatarStatus status)
    {
        var result = await ScanComponentAsync<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Status, status));

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ComponentSize.Small)]
    [InlineData(ComponentSize.Medium)]
    [InlineData(ComponentSize.Large)]
    [InlineData(ComponentSize.ExtraLarge)]
    public async Task Should_Have_No_Violations_For_Sizes(ComponentSize size)
    {
        var result = await ScanComponentAsync<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Size, size));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Interactive()
    {
        var result = await ScanComponentAsync<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.OnClick, _ => { }));

        result.Violations.ShouldBeEmpty();
    }
}
