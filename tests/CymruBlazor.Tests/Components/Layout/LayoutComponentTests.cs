using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class LayoutComponentTests : TestContextBase
{
    [Fact]
    public async Task CySidebar_ToggleAsync_InvertsCollapsedState()
    {
        // Arrange
        var eventFired = false;
        var cut = Render<CySidebar>(parameters => parameters
            .Add(p => p.State, SidebarState.Expanded)
            .Add(p => p.StateChanged, val => eventFired = val == SidebarState.Compact));

        // Act - Safely synchronize state updates using the Blazor Dispatcher
        await cut.InvokeAsync(async () => await cut.Instance.ToggleAsync());

        // Assert
        cut.Instance.State.ShouldBe(SidebarState.Compact);
        cut.Instance.EffectiveCollapsed.ShouldBeTrue();
        eventFired.ShouldBeTrue();
    }
}
