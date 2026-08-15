namespace CymruBlazor.Enums;

/// <summary>
/// Specifies the background treatment of a <c>CyHeroBanner</c>.
/// </summary>
public enum HeroBackground
{
    /// <summary>
    /// NHS Wales primary blue gradient.
    /// </summary>
    Primary = 0,

    /// <summary>
    /// The darker navy accent (see <c>--cymru-color-accent</c>).
    /// </summary>
    Accent = 1,

    /// <summary>
    /// No background treatment - inherits the page background.
    /// </summary>
    Plain = 2
}
