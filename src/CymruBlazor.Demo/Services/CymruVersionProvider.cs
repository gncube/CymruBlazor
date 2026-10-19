using System.Reflection;
using CymruBlazor.Components.Branding;

namespace CymruBlazor.Demo.Services;

/// <summary>
/// Authoritative provider for the resolved CymruBlazor library version.
/// Inspects assembly informational version attributes to dynamically resolve
/// package versions including prerelease tags, avoiding manual hardcoded constants.
/// </summary>
public static class CymruVersionProvider
{
    private static readonly Lazy<string?> LazyDefaultVersion = new(() => ResolveVersion(typeof(CyBrandLogo).Assembly));

    /// <summary>
    /// Gets the formatted version string for the active CymruBlazor assembly (e.g. "v2.0.0", "v1.4.0-preview.1"),
    /// or <see langword="null"/> if metadata could not be resolved.
    /// </summary>
    public static string? Version => LazyDefaultVersion.Value;

    /// <summary>
    /// Resolves and formats the version from the specified assembly's metadata.
    /// </summary>
    public static string? ResolveVersion(Assembly? assembly)
    {
        if (assembly is null)
        {
            return null;
        }

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var formatted = FormatVersion(informationalVersion);
        if (!string.IsNullOrWhiteSpace(formatted))
        {
            return formatted;
        }

        var assemblyVersion = assembly.GetName().Version;
        if (assemblyVersion != null && assemblyVersion != new Version(0, 0, 0, 0))
        {
            return FormatVersion(assemblyVersion.ToString(3));
        }

        return null;
    }

    /// <summary>
    /// Normalizes and formats a raw version string.
    /// Trims build metadata after '+', preserves prerelease suffixes, and prefixes 'v' exactly once.
    /// Returns <see langword="null"/> if the raw version is empty or invalid.
    /// </summary>
    public static string? FormatVersion(string? rawVersion)
    {
        if (string.IsNullOrWhiteSpace(rawVersion))
        {
            return null;
        }

        var trimmed = rawVersion.Trim();

        // Strip commit hash / build metadata after '+' (e.g., "2.0.0+773830a..." -> "2.0.0")
        var plusIndex = trimmed.IndexOf('+');
        if (plusIndex >= 0)
        {
            trimmed = trimmed[..plusIndex];
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        // Prefix with 'v' exactly once
        if (trimmed.StartsWith('v') || trimmed.StartsWith('V'))
        {
            return $"v{trimmed[1..]}";
        }

        return $"v{trimmed}";
    }
}
