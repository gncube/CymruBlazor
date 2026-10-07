using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Guard (TASK-074, closed in Phase F): every library-owned service that any CymruBlazor component injects must
/// resolve from a provider built by <c>AddCymruBlazor()</c> alone. Services owned by the framework or the host
/// (<c>NavigationManager</c>, <c>IJSRuntime</c>, <c>IMediator</c>) are out of scope: only types in the
/// <c>CymruBlazor</c> namespace are checked.
/// </summary>
public sealed class AddCymruBlazorGuardTests
{
    private static IEnumerable<Type> ComponentTypes() =>
        typeof(CyCard).Assembly.GetTypes()
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition);

    private static IEnumerable<(Type Component, Type Service)> InjectedLibraryServices()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        foreach (var component in ComponentTypes())
        {
            for (var type = component; type is not null && type != typeof(object); type = type.BaseType)
            {
                foreach (var property in type.GetProperties(flags))
                {
                    if (property.GetCustomAttribute<InjectAttribute>() is null)
                    {
                        continue;
                    }

                    var service = property.PropertyType;
                    if (service.Namespace is not null && service.Namespace.StartsWith("CymruBlazor", StringComparison.Ordinal))
                    {
                        yield return (component, service);
                    }
                }
            }
        }
    }

    /// <summary>
    /// True when the service is registered. A registered service may need something only the host provides
    /// (<c>IJSRuntime</c>, <c>NavigationManager</c>, <c>IMediator</c>); the container then throws "Unable to
    /// resolve service for type" naming that host type. That still proves the library registration exists, so it
    /// counts as registered. A missing library type (or a missing library dependency) does not.
    /// </summary>
    private static bool IsRegistered(IServiceProvider provider, Type service)
    {
        try
        {
            return provider.GetService(service) is not null;
        }
        catch (InvalidOperationException ex) when (NeedsOnlyHostServices(ex.Message))
        {
            return true;
        }
    }

    private static bool NeedsOnlyHostServices(string message) =>
        message.Contains("Unable to resolve service for type '", StringComparison.Ordinal) &&
        !message.Contains("Unable to resolve service for type 'CymruBlazor", StringComparison.Ordinal);

    [Fact]
    public void The_Guard_Finds_Injected_Library_Services()
    {
        // If reflection finds nothing the guard below proves nothing.
        InjectedLibraryServices().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Every_Library_Service_A_Component_Injects_Resolves_From_AddCymruBlazor()
    {
        var services = new ServiceCollection();
        services.AddCymruBlazor();

        // Async disposal: ThemeService only implements IAsyncDisposable, so a synchronous Dispose throws.
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var unresolved = InjectedLibraryServices()
            .Where(pair => !IsRegistered(scope.ServiceProvider, pair.Service))
            .Select(pair => $"{pair.Component.Name} injects {pair.Service.FullName}")
            .Distinct()
            .ToList();

        unresolved.ShouldBeEmpty();
    }
}
