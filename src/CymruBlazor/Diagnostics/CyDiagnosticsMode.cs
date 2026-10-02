namespace CymruBlazor.Diagnostics;

/// <summary>
/// How CymruBlazor reacts when a component is misused in a way it can
/// recover from - for example an unknown <c>CyIcon</c> name.
/// </summary>
public enum CyDiagnosticsMode
{
    /// <summary>
    /// Fail fast: throw, exactly as CymruBlazor has always done. Right for
    /// Development, where a typo should be loud. This is the default.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Degrade gracefully: log a warning and render a safe fallback so one
    /// mistake cannot take down a whole page. Right for Production.
    /// </summary>
    Lenient = 1,
}
