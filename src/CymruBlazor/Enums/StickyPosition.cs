namespace CymruBlazor.Enums;

/// <summary>
/// Where a bar sticks while its scroll container (or the page) scrolls.
/// </summary>
public enum StickyPosition
{
    /// <summary>Not sticky; the bar scrolls with the content.</summary>
    None = 0,

    /// <summary>Sticks to the top edge.</summary>
    Top = 1,

    /// <summary>Sticks to the bottom edge.</summary>
    Bottom = 2
}
