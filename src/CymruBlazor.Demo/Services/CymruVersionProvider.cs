using System.Reflection;
using CymruBlazor.Components.Branding;

namespace CymruBlazor.Demo.Services;

/// <summary>
/// Authoritative provider for the resolved CymruBlazor library version.
/// Inspects central package metadata (Directory.Packages.props) and assembly attributes
/// to dynamically resolve package versions, avoiding manual hardcoded constants.
/// </summary>
public static class CymruVersionProvider
{
    /// <summary>
    /// Current release version of the CymruBlazor library used as fallback
    /// when running from a build without Git tags (such as shallow CI clones).
    /// </summary>
    public const string FallbackVersion = "v2.3.0";

    private static readonly Lazy<string> LazyDefaultVersion = new(() => ResolveVersion(typeof(CyBrandLogo).Assembly) ?? FallbackVersion);

    /// <summary>
    /// Gets the formatted version string for the active CymruBlazor assembly (e.g. "v2.3.0", "v2.2.0"),
    /// or <see cref="FallbackVersion"/> if metadata could not be resolved.
    /// </summary>
    public static string Version => LazyDefaultVersion.Value;

    /// <summary>
    /// Resolves and formats the version from the specified assembly's metadata.
    /// Prioritizes central package alignment metadata (<c>CymruBlazorPackageVersion</c>) from <c>Directory.Packages.props</c>,
    /// before checking assembly informational or version attributes.
    /// </summary>
    public static string? ResolveVersion(Assembly? assembly)
    {
        if (assembly is null)
        {
            return null;
        }

        // 1. Authoritative: Central Package Version from Directory.Packages.props stamped as AssemblyMetadata
        var packageVersionMeta = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, "CymruBlazorPackageVersion", StringComparison.OrdinalIgnoreCase))?
            .Value;

        var formattedPackageVersion = FormatVersion(packageVersionMeta);
        if (!string.IsNullOrWhiteSpace(formattedPackageVersion) && !IsUninitializedVersion(formattedPackageVersion))
        {
            return formattedPackageVersion;
        }

        // Also check demo/entry assembly if the specified assembly does not carry the attribute
        var entryPackageVersionMeta = typeof(CymruVersionProvider).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, "CymruBlazorPackageVersion", StringComparison.OrdinalIgnoreCase))?
            .Value;

        var formattedEntryVersion = FormatVersion(entryPackageVersionMeta);
        if (!string.IsNullOrWhiteSpace(formattedEntryVersion) && !IsUninitializedVersion(formattedEntryVersion))
        {
            return formattedEntryVersion;
        }

        // 2. AssemblyInformationalVersion (e.g. from tag release)
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var formatted = FormatVersion(informationalVersion);
        if (!string.IsNullOrWhiteSpace(formatted) && !IsUninitializedVersion(formatted))
        {
            return formatted;
        }

        // 3. AssemblyVersion
        var assemblyVersion = assembly.GetName().Version;
        if (assemblyVersion != null && assemblyVersion != new Version(0, 0, 0, 0))
        {
            var formattedAssemblyVersion = FormatVersion(assemblyVersion.ToString(3));
            if (!string.IsNullOrWhiteSpace(formattedAssemblyVersion) && !IsUninitializedVersion(formattedAssemblyVersion))
            {
                return formattedAssemblyVersion;
            }
        }

        return FallbackVersion;
    }

    /// <summary>
    /// Determines whether the formatted version string represents an uninitialized or fallback 0.0.0 version.
    /// </summary>
    public static bool IsUninitializedVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return true;
        }

        var trimmed = version.Trim().TrimStart('v', 'V');
        return trimmed.StartsWith("0.0.0", StringComparison.OrdinalIgnoreCase);
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
