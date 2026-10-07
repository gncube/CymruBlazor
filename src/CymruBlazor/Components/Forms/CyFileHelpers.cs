using System.Globalization;
using System.Text.RegularExpressions;

namespace CymruBlazor.Components.Forms;

/// <summary>Human-readable file sizes ("512 KB", "1.5 MB").</summary>
internal static class CyFileSize
{
    internal static string Format(long bytes, string bytesUnit = "bytes")
    {
        if (bytes < 1024)
        {
            return string.Create(CultureInfo.CurrentCulture, $"{bytes} {bytesUnit}");
        }

        string[] units = ["KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = -1;

        do
        {
            value /= 1024;
            unit++;
        }
        while (value >= 1024 && unit < units.Length - 1);

        var text = value >= 100 ? value.ToString("0", CultureInfo.CurrentCulture) : value.ToString("0.#", CultureInfo.CurrentCulture);
        return $"{text} {units[unit]}";
    }
}

/// <summary>
/// Parses and applies the <c>Accept</c> list (<c>.pdf, image/png, image/*</c>). This is a usability check on values the
/// browser reports, not enforcement.
/// </summary>
internal static partial class CyFileAccept
{
    [GeneratedRegex(
        @"^(\.[a-z0-9][a-z0-9+._-]*|[a-z0-9][a-z0-9!#$&^_.+-]*/(?:[a-z0-9][a-z0-9!#$&^_.+-]*|\*))$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();

    /// <summary>Parses <paramref name="accept"/>; returns false (with the offending token) when it is malformed.</summary>
    internal static bool TryParse(string? accept, out IReadOnlyList<string> tokens, out string? invalid)
    {
        var parsed = new List<string>();
        tokens = parsed;
        invalid = null;

        if (string.IsNullOrWhiteSpace(accept))
        {
            return true;
        }

        foreach (var raw in accept.Split(','))
        {
            var token = raw.Trim();

            if (!TokenPattern().IsMatch(token))
            {
                invalid = token;
                return false;
            }

            parsed.Add(token.ToLowerInvariant());
        }

        return true;
    }

    /// <summary>True when no restriction is set, or the file name's extension or reported content type is listed.</summary>
    internal static bool Matches(IReadOnlyList<string> tokens, string fileName, string? contentType)
    {
        if (tokens.Count == 0)
        {
            return true;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var type = (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();

        foreach (var token in tokens)
        {
            if (token[0] == '.')
            {
                if (extension == token)
                {
                    return true;
                }
            }
            else if (token.EndsWith("/*", StringComparison.Ordinal))
            {
                if (type.StartsWith(token[..^1], StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if (type == token)
            {
                return true;
            }
        }

        return false;
    }
}
