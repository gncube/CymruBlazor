namespace CymruBlazor.Services;

/// <summary>
/// Resolves the latest published version of a NuGet package.
///
/// Used by <see cref="CymruBlazor.Components.Layout.CyFooter"/> to display
/// the currently published package version without requiring the
/// consuming application to hard-code or manually update it.
/// </summary>
public interface IPackageVersionService
{
    /// <summary>
    /// Gets the latest published version for the given NuGet package id.
    /// </summary>
    /// <param name="packageId">
    /// The NuGet package id (case-insensitive), e.g. <c>"CymruBlazor"</c>.
    /// </param>
    /// <param name="includePrerelease">
    /// When <see langword="true"/>, prerelease versions (e.g.
    /// <c>1.0.0-preview.3</c>) are eligible to be returned as the latest
    /// version. When <see langword="false"/>, only stable versions are
    /// considered.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// The latest version string (e.g. <c>"1.2.0"</c>), or
    /// <see langword="null"/> when the version could not be resolved -
    /// for example, no <see cref="HttpClient"/> is available, the
    /// package doesn't exist, or the request failed. This method never
    /// throws for network/availability failures; callers should treat a
    /// <see langword="null"/> result as "unknown" and degrade gracefully.
    /// </returns>
    Task<string?> GetLatestVersionAsync(
        string packageId,
        bool includePrerelease = false,
        CancellationToken cancellationToken = default);
}
