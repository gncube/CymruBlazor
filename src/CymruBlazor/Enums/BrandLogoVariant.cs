namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the display variant or theme mode for brand logos.
/// </summary>
public enum BrandLogoVariant
{
    /// <summary>
    /// Automatically determines the asset or styling based on ambient theme settings.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Forces light theme logo styling or assets.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Forces dark theme logo styling or assets.
    /// </summary>
    Dark = 2,

    /// <summary>
    /// Full logo lockup including mark and wordmark.
    /// </summary>
    Full = 3,

    /// <summary>
    /// Standalone mark / icon symbol only.
    /// </summary>
    Mark = 4,

    /// <summary>
    /// Text wordmark only.
    /// </summary>
    Wordmark = 5,

    /// <summary>
    /// Stacked lockup layout.
    /// </summary>
    Stacked = 6
}
