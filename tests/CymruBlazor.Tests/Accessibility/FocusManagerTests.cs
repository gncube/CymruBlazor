using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

using Xunit;

using CymruBlazor.Accessibility.Focus;

namespace CymruBlazor.Tests.Accessibility;

public class FocusManagerTests
{
    private readonly FocusManager _manager =
        new(NullLogger<FocusManager>.Instance);

    [Fact]
    public async Task Focus_By_Id_Returns_Success()
    {
        // Arrange
        const string elementId = "test-element";

        // Act
        var result = await _manager.FocusAsync(elementId);

        // Assert
        result.Success.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public async Task Focus_By_Target_Returns_Success()
    {
        // Act
        var result = await _manager.FocusAsync(FocusTarget.First);

        // Assert
        result.Success.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public async Task RestoreFocus_Returns_Success()
    {
        // Act
        var result = await _manager.RestoreFocusAsync();

        // Assert
        result.Success.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public async Task Empty_Id_Throws()
    {
        // Act
        var action = () => _manager.FocusAsync(string.Empty);

        // Assert
        await Should.ThrowAsync<ArgumentException>(action);
    }

    [Fact]
    public async Task Null_Id_Throws()
    {
        // Act
        var action = () => _manager.FocusAsync(null!);

        // Assert
        await Should.ThrowAsync<ArgumentNullException>(action);
    }
}
