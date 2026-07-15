using System.Globalization;
using System.Threading;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Default implementation of <see cref="IComponentIdGenerator"/>.
///
/// Generates deterministic, thread-safe identifiers suitable for
/// HTML element ids.
/// </summary>
public sealed class ComponentIdGenerator : IComponentIdGenerator
{
    private long _nextId;

   /// <inheritdoc />
    public string Create(string? prefix = null)
    {
        var id = Interlocked.Increment(ref _nextId);

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return $"cy-{id}";
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Sanitize(prefix)}-{id}");
    }

    private static string Sanitize(string prefix)
    {
        Span<char> buffer = stackalloc char[prefix.Length];

        var length = 0;

        foreach (var character in prefix)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length++] = char.ToLowerInvariant(character);
            }
            else if (character is '-' or '_')
            {
                buffer[length++] = character;
            }
        }

        return length == 0
            ? "cy"
            : new string(buffer[..length]);
    }
}
