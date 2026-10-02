using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CymruBlazor.Diagnostics;

/// <summary>
/// Default <see cref="ICyDiagnostics"/>. Registered by
/// <c>AddCymruBlazor()</c>.
/// </summary>
public sealed class CyDiagnostics : ICyDiagnostics
{
    // Bound the de-duplication set so a pathological caller (a different
    // message per render) cannot grow it without limit.
    private const int MaxRemembered = 1000;

    private static readonly Action<ILogger, string, string, Exception?> LogWarning =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(1, "CymruBlazorWarning"), "{Code}: {Message}");

    private readonly ConcurrentDictionary<string, byte> _seen = new(StringComparer.Ordinal);
    private readonly ILogger? _logger;

    /// <summary>
    /// The instance components fall back to when none is registered:
    /// <see cref="CyDiagnosticsMode.Strict"/>, no logger.
    /// </summary>
    internal static ICyDiagnostics Default { get; } = new CyDiagnostics(new CymruBlazorOptions());

    /// <summary>Creates the service.</summary>
    public CyDiagnostics(CymruBlazorOptions options, ILogger<CyDiagnostics>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        Mode = options.Diagnostics;
        _logger = logger;
    }

    /// <inheritdoc />
    public CyDiagnosticsMode Mode { get; }

    /// <inheritdoc />
    public void Warn(string code, string message)
    {
        if (_logger is null)
        {
            return;
        }

        var key = string.Concat(code, "|", message);
        if (_seen.Count >= MaxRemembered || !_seen.TryAdd(key, 0))
        {
            return;
        }

        LogWarning(_logger, code, message, null);
    }
}
