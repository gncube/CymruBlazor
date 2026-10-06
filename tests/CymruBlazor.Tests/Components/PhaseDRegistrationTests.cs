using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Anything a component resolves from DI must be registered by <c>AddCymruBlazor()</c>; a missing registration
/// fails silently or only at runtime (the Phase C sortable-list registry did exactly that).
/// </summary>
public sealed class PhaseDRegistrationTests
{
    [Fact]
    public void AddCymruBlazor_Registers_The_Confirm_Service_Once_Per_Scope()
    {
        var services = new ServiceCollection();
        services.AddCymruBlazor();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var asInterface = scope.ServiceProvider.GetRequiredService<ICyConfirmService>();
        var asConcrete = scope.ServiceProvider.GetRequiredService<CyConfirmService>();

        asInterface.ShouldBeSameAs(asConcrete);
    }

    [Fact]
    public void AddCymruBlazor_Gives_Each_Scope_Its_Own_Confirm_Service()
    {
        var services = new ServiceCollection();
        services.AddCymruBlazor();
        using var provider = services.BuildServiceProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        first.ServiceProvider.GetRequiredService<ICyConfirmService>()
            .ShouldNotBeSameAs(second.ServiceProvider.GetRequiredService<ICyConfirmService>());
    }
}
