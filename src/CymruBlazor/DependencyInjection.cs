using CymruBlazor.Components.Feedback;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor;

/// <summary>
/// Extension methods for registering CymruBlazor services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers CymruBlazor services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCymruBlazor(this IServiceCollection services)
    {
        services.AddScoped<IToastService, ToastService>();
        services.AddScoped<INotificationHandler<ShowToastNotification>>(sp =>
            (ToastService)sp.GetRequiredService<IToastService>());

        return services;
    }
}
