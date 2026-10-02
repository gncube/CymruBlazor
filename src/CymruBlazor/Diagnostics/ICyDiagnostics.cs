namespace CymruBlazor.Diagnostics;

/// <summary>
/// Reports recoverable component misuse according to the configured
/// <see cref="CyDiagnosticsMode"/>. Components resolve this optionally, so an
/// app (or test) that never registered it behaves as
/// <see cref="CyDiagnosticsMode.Strict"/>.
/// </summary>
public interface ICyDiagnostics
{
    /// <summary>Gets the active mode.</summary>
    CyDiagnosticsMode Mode { get; }

    /// <summary>Gets whether misuse should throw rather than degrade.</summary>
    bool IsStrict => Mode == CyDiagnosticsMode.Strict;

    /// <summary>
    /// Logs a warning. Each distinct <paramref name="code"/> +
    /// <paramref name="message"/> pair is logged once per process, so a
    /// component re-rendering every frame cannot flood the log.
    /// </summary>
    /// <param name="code">Stable identifier, e.g. <c>CY0001</c>.</param>
    /// <param name="message">Human-readable explanation, including the fix.</param>
    void Warn(string code, string message);
}
