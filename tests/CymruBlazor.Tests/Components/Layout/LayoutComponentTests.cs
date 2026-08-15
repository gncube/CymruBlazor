using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class LayoutComponentTests : TestContextBase
{
    [Fact]
    public async Task CySidebar_ToggleAsync_InvertsCollapsedState()
    {
        // Arrange
        var eventFired = false;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.Collapsed, false)
            .Add(p => p.CollapsedChanged, val => eventFired = val));

        // Act - Safely synchronize state updates using the Blazor Dispatcher
        await cut.InvokeAsync(async () => await cut.Instance.ToggleAsync());

        // Assert
        cut.Instance.Collapsed.ShouldBeTrue();
        eventFired.ShouldBeTrue();
    }
}
