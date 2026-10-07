namespace CymruBlazor.Components.Content;

/// <summary>Inner spacing of a <see cref="CyCard"/>.</summary>
public enum CyCardPadding
{
    /// <summary>The standard card spacing (unchanged from earlier releases).</summary>
    Default,

    /// <summary>Tighter spacing for dense layouts.</summary>
    Compact,

    /// <summary>No inner spacing, for content that fills the card edge to edge (an image, a table).</summary>
    None
}
