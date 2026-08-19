using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Theming;

/// <summary>
/// Registers the services CymruBlazor components depend on.
/// </summary>
public static class CymruBlazorServiceCollectionExtensions
{
    /// <summary>
    /// Adds CymruBlazor's services (currently <see cref="ThemeService"/>) to the container.
    /// Call once from the consuming application's composition root.
    /// </summary>
    public static IServiceCollection AddCymruBlazor(this IServiceCollection services)
    {
        services.AddScoped<ThemeService>();
        return services;
    }
}
