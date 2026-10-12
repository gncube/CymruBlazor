using CymruBlazor.Diagnostics;

namespace CymruBlazor.Diagnostics;

/// <summary>
/// Reports <c>CY0005</c>: a form field with no accessible name. Shared by the form fields so the rule and
/// the wording live in one place. It only ever <b>warns</b> (never throws, in either
/// <see cref="CyDiagnosticsMode"/>), so a page that rendered before 1.13.0 keeps rendering.
/// </summary>
internal static class CyAccessibleNameCheck
{
    /// <summary>The diagnostic code.</summary>
    internal const string Code = "CY0005";

    /// <summary>
    /// Warns when <paramref name="label"/> is blank and <paramref name="attributes"/> carries neither a
    /// non-blank <c>aria-label</c> nor <c>aria-labelledby</c>.
    /// </summary>
    internal static void Check(
        ICyDiagnostics diagnostics,
        Type componentType,
        string? label,
        IReadOnlyDictionary<string, object>? attributes)
    {
        if (!string.IsNullOrWhiteSpace(label)
            || HasValue(attributes, "aria-label")
            || HasValue(attributes, "aria-labelledby"))
        {
            return;
        }

        var name = componentType.Name;
        var tick = name.IndexOf('`', StringComparison.Ordinal);
        if (tick >= 0)
        {
            name = name[..tick];
        }

        diagnostics.Warn(
            Code,
            $"{name} has no accessible name. Set Label (every CymruBlazor field needs one), " +
            "or supply aria-label or aria-labelledby when the label is shown elsewhere.");
    }

    private static bool HasValue(IReadOnlyDictionary<string, object>? attributes, string key) =>
        attributes is not null
        && attributes.TryGetValue(key, out var value)
        && !string.IsNullOrWhiteSpace(value?.ToString());
}
