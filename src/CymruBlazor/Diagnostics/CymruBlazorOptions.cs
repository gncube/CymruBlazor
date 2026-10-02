namespace CymruBlazor.Diagnostics;

/// <summary>
/// Library-wide options, configured through
/// <c>services.AddCymruBlazor(options =&gt; ...)</c>.
/// </summary>
public sealed class CymruBlazorOptions
{
    /// <summary>
    /// How recoverable misuse is handled. Defaults to
    /// <see cref="CyDiagnosticsMode.Strict"/> so behaviour is unchanged for
    /// apps that do nothing.
    /// </summary>
    /// <remarks>
    /// The package ships as a Release build, so it cannot know whether the
    /// host is in Development. Tie it to your own environment check:
    /// <code>
    /// builder.Services.AddCymruBlazor(o =&gt;
    ///     o.Diagnostics = builder.HostEnvironment.IsDevelopment()
    ///         ? CyDiagnosticsMode.Strict
    ///         : CyDiagnosticsMode.Lenient);
    /// </code>
    /// </remarks>
    public CyDiagnosticsMode Diagnostics { get; set; } = CyDiagnosticsMode.Strict;
}
