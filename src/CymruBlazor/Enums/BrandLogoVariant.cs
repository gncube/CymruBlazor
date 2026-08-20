namespace CymruBlazor.Enums;

/// <summary>
/// Specifies which parts of the <c>CyBrandLogo</c> lockup are rendered.
/// </summary>
public enum BrandLogoVariant
{
    /// <summary>
    /// Icon mark followed by the wordmark, side by side. The default -
    /// suitable for a top navigation bar with enough horizontal space.
    /// </summary>
    Full = 0,

    /// <summary>
    /// Icon mark only, no text. Suitable for a collapsed/rail sidebar or
    /// a small-viewport header where horizontal space is constrained.
    /// </summary>
    Mark = 1,

    /// <summary>
    /// Wordmark text only, no icon mark.
    /// </summary>
    Wordmark = 2,

    /// <summary>
    /// Icon mark above the wordmark. Suitable for a vertical sidebar
    /// header or a centred splash/login screen.
    /// </summary>
    Stacked = 3
}
