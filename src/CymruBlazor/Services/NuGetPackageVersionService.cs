using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CymruBlazor.Services;

/// <summary>
/// Default <see cref="IPackageVersionService"/> implementation, backed by
/// the public NuGet flat-container index
/// (<c>https://api.nuget.org/v3-flatcontainer/{id}/index.json</c>).
///
/// This endpoint is unauthenticated, CORS-enabled, and requires no API
/// key, so it can be called directly from Blazor WebAssembly as well as
/// from server-rendered Blazor Web Apps.
///
/// Like <see cref="ThemeService"/>'s optional <c>IJSRuntime</c>
/// dependency, the <see cref="HttpClient"/> dependency here is optional:
/// when none is available (e.g. a consuming app hasn't registered one,
/// or in unit tests), <see cref="GetLatestVersionAsync"/> simply returns
/// <see langword="null"/> rather than throwing, so
/// <see cref="Components.Layout.CyFooter"/> can always render safely.
/// </summary>
public sealed class NuGetPackageVersionService : IPackageVersionService
{
    private const string FlatContainerBaseUrl = "https://api.nuget.org/v3-flatcontainer/";

    private readonly HttpClient? _httpClient;

    public NuGetPackageVersionService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<string?> GetLatestVersionAsync(
        string packageId,
        bool includePrerelease = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        if (_httpClient is null)
        {
            return null;
        }

        try
        {
            var requestUrl = $"{FlatContainerBaseUrl}{packageId.ToLowerInvariant()}/index.json";

            var index = await _httpClient.GetFromJsonAsync(
                requestUrl,
                NuGetJsonContext.Default.NuGetFlatContainerIndex,
                cancellationToken).ConfigureAwait(false);

            var versions = index?.Versions;

            if (versions is null || versions.Count == 0)
            {
                return null;
            }

            if (!includePrerelease)
            {
                var stableVersions = versions
                    .Where(v => !v.Contains('-', StringComparison.Ordinal))
                    .ToList();

                if (stableVersions.Count > 0)
                {
                    return stableVersions[^1];
                }

                // No stable release exists yet - fall through and
                // return the latest prerelease so the caller still gets
                // a usable value instead of null.
            }

            return versions[^1];
        }
        catch (Exception ex) when (
            ex is HttpRequestException
            or JsonException
            or NotSupportedException
            or TaskCanceledException)
        {
            // Network/availability failures degrade to "unknown" rather
            // than surfacing to the UI - version display is
            // best-effort, never critical-path.
            return null;
        }
    }

}

/// <summary>
/// Minimal DTO matching the NuGet v3 flat-container package index
/// response shape, e.g. <c>{ "versions": ["1.0.0", "1.0.1"] }</c>.
/// </summary>
internal sealed class NuGetFlatContainerIndex
{
    [JsonPropertyName("versions")]
    public List<string>? Versions { get; set; }
}

/// <summary>
/// Source-generated JSON context for trimming/AOT-safe deserialization
/// of the NuGet flat-container index response.
/// </summary>
[JsonSerializable(typeof(NuGetFlatContainerIndex))]
internal sealed partial class NuGetJsonContext : JsonSerializerContext
{
}
