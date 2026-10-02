namespace CymruBlazor.Enums;

/// <summary>
/// Which side of a <c>CyButton</c>'s text its icon is drawn on. Logical
/// (reading-direction) rather than left/right, so it mirrors correctly in
/// right-to-left layouts.
/// </summary>
/// <remarks>
/// Deliberately a new type: the older <c>IconPosition</c> enum is obsolete and
/// scheduled for removal in 2.0.0 (see <c>docs/MIGRATION-2.0.md</c>).
/// </remarks>
public enum ButtonIconPlacement
{
    /// <summary>Before the text (the start edge).</summary>
    Start = 0,

    /// <summary>After the text (the end edge).</summary>
    End = 1,
}
