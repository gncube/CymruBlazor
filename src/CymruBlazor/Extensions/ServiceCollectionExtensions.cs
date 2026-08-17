using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Services;
using CymruBlazor.Themes;

namespace CymruBlazor.Extensions;

/// <summary>
/// Provides extension methods for registering CymruBlazor infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all framework core dependencies, id generators, theming, and Mediator pipeline infrastructure.
    /// </summary>
    public static IServiceCollection AddCymruBlazor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        // Register core unique ID generator utilities safely for client lifecycle scopes
        services.AddScoped<IComponentIdGenerator, ComponentIdGenerator>();

        // Register theme management. Scoped to match Blazor's per-circuit/
        // per-session lifetime; ThemeService picks up IJSRuntime from DI
        // automatically where one is available (WASM and interactive
        // Server render modes both register it).
        services.AddScoped<IThemeService, ThemeService>();

        // Register focus management - used by FocusTrap, and transitively
        // by CyNavigation's mobile menu. Previously only ever registered
        // manually by consuming apps (e.g. the Demo app); any component
        // using FocusTrap would throw at resolution time without it.
        services.AddScoped<IFocusManager, FocusManager>();

        // Register the source-generated Mediator context pipeline
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
