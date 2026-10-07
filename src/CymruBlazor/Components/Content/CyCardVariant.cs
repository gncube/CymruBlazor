namespace CymruBlazor.Components.Content;

/// <summary>Visual treatment of a <see cref="CyCard"/>.</summary>
public enum CyCardVariant
{
    /// <summary>Border plus the <see cref="CyCard.Elevation"/> shadow (unchanged from earlier releases).</summary>
    Raised,

    /// <summary>Border only; no shadow whatever <see cref="CyCard.Elevation"/> says.</summary>
    Outlined,

    /// <summary>A filled, borderless panel with no shadow (the border returns in forced-colours mode).</summary>
    Flat
}
