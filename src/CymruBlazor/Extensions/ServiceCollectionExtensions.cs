using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CymruBlazor.Accessibility;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Feedback;
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

        // Register the NuGet package version lookup used by CyFooter's
        // ShowVersion parameter. Resolves an already-registered
        // HttpClient (or none) via the service provider rather than
        // AddHttpClient/IHttpClientFactory, so this stays a lightweight
        // dependency for consuming apps that haven't opted into
        // Microsoft.Extensions.Http.
        services.AddScoped<IPackageVersionService>(
            sp => new NuGetPackageVersionService(sp.GetService<HttpClient>()));

        // Register focus management - used by FocusTrap, and transitively
        // by CyNavigation's mobile menu. Previously only ever registered
        // manually by consuming apps (e.g. the Demo app); any component
        // using FocusTrap would throw at resolution time without it.
        //
        // JsFocusManager (1.3.0) really moves, restores and contains focus via
        // the on-demand cymru-overlay.js module. Register your own
        // IFocusManager after AddCymruBlazor() to replace it (the last
        // registration wins). The old logging-only FocusManager remains as a
        // no-op implementation but is no longer registered.
        services.AddScoped<IFocusManager, JsFocusManager>();

        // Register the live-region registry used to forward Mediator
        // announcements to the actual, render-tree-attached CyLiveRegion
        // instance(s) - see ILiveRegionRegistry for why CyLiveRegion can't
        // safely implement INotificationHandler<> itself.
        services.AddScoped<ILiveRegionRegistry, LiveRegionRegistry>();

        // Register toast notifications and the Mediator handler that lets
        // ShowToastNotification be published through the pipeline below.
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IToastService, ToastService>();
        services.AddScoped<INotificationHandler<ShowToastNotification>>(
            sp => (ToastService)sp.GetRequiredService<IToastService>());

        // Register the source-generated Mediator context pipeline
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
