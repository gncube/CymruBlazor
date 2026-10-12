using System.Text.Json;

namespace CymruBlazor.Testing.Axe;

/// <summary>
/// Finds and reads the CSS that ships in the CymruBlazor package (it is a static web asset): <c>css/cymrublazor.css</c>
/// for the design tokens and shared styles, and the scoped component bundle for the per-component styles.
/// </summary>
internal static class CymruLibraryCss
{
    internal const string DirectoryVariable = "CYMRUBLAZOR_CSS_DIR";

    private const string MainFile = "css/cymrublazor.css";
    private static readonly string[] ScopedFiles = ["CymruBlazor.bundle.scp.css", "CymruBlazor.styles.css"];

    /// <summary>Finds the stylesheets and returns their text, main stylesheet first.</summary>
    /// <param name="directoryOverride">A static web assets folder to use, or null to search.</param>
    /// <param name="libraryAssemblyPath">The path of CymruBlazor.dll, or empty if it has none.</param>
    /// <param name="baseDirectory">The test output folder.</param>
    internal static string Load(string? directoryOverride, string libraryAssemblyPath, string baseDirectory)
    {
        var explicitDirectory = !string.IsNullOrWhiteSpace(directoryOverride)
            ? directoryOverride
            : Environment.GetEnvironmentVariable(DirectoryVariable);

        if (!string.IsNullOrWhiteSpace(explicitDirectory))
        {
            if (!File.Exists(Path.Combine(explicitDirectory, MainFile)))
            {
                throw new FileNotFoundException(
                    $"Could not find '{MainFile}' in '{explicitDirectory}', which was given as the CymruBlazor static web assets folder.");
            }

            return Read(explicitDirectory, baseDirectory);
        }

        var searched = new List<string>();

        foreach (var directory in Candidates(libraryAssemblyPath, baseDirectory))
        {
            searched.Add(directory);

            if (File.Exists(Path.Combine(directory, MainFile)))
            {
                return Read(directory, baseDirectory);
            }
        }

        throw new FileNotFoundException(
            $"Could not find the CymruBlazor stylesheet ('{MainFile}'). Searched: "
            + (searched.Count == 0 ? "(nowhere)" : string.Join("; ", searched))
            + $". Restore the CymruBlazor package, or set {DirectoryVariable} (or override CssDirectory) "
            + "to the package's staticwebassets folder.");
    }

    private static IEnumerable<string> Candidates(string libraryAssemblyPath, string baseDirectory)
    {
        // NuGet layout: <package>/lib/<tfm>/CymruBlazor.dll beside <package>/staticwebassets/.
        var assemblyDirectory = string.IsNullOrEmpty(libraryAssemblyPath) ? null : Path.GetDirectoryName(libraryAssemblyPath);

        if (assemblyDirectory is not null)
        {
            yield return Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "staticwebassets"));
        }

        // A web project's manifest lists the content root of every library it uses, including project references.
        foreach (var root in ManifestContentRoots(baseDirectory))
        {
            yield return root;
        }
    }

    private static List<string> ManifestContentRoots(string baseDirectory)
    {
        var roots = new List<string>();
        string[] manifests;

        try
        {
            manifests = Directory.GetFiles(baseDirectory, "*.staticwebassets.runtime.json");
        }
        catch (IOException)
        {
            return roots;
        }
        catch (UnauthorizedAccessException)
        {
            return roots;
        }

        foreach (var manifest in manifests)
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manifest));

                if (document.RootElement.TryGetProperty("ContentRoots", out var contentRoots)
                    && contentRoots.ValueKind == JsonValueKind.Array)
                {
                    roots.AddRange(contentRoots.EnumerateArray()
                        .Where(root => root.ValueKind == JsonValueKind.String)
                        .Select(root => root.GetString()!));
                }
            }
            catch (JsonException)
            {
                // Not a manifest we understand: ignore it.
            }
            catch (IOException)
            {
                // Unreadable: ignore it.
            }
        }

        return roots;
    }

    private static string Read(string directory, string baseDirectory)
    {
        var css = File.ReadAllText(Path.Combine(directory, MainFile));

        foreach (var name in ScopedFiles)
        {
            var path = Path.Combine(directory, name);

            if (File.Exists(path))
            {
                return css + Environment.NewLine + File.ReadAllText(path);
            }
        }

        // In a project-reference build the scoped bundle is a build output beside the app, not in the asset folder.
        try
        {
            var built = Directory.EnumerateFiles(baseDirectory, "CymruBlazor.styles.css", SearchOption.AllDirectories).FirstOrDefault();

            if (built is not null)
            {
                return css + Environment.NewLine + File.ReadAllText(built);
            }
        }
        catch (IOException)
        {
            // No scoped bundle: the tokens and shared styles still apply.
        }
        catch (UnauthorizedAccessException)
        {
            // As above.
        }

        return css;
    }
}
