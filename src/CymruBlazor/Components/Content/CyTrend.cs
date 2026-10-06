namespace CymruBlazor.Components.Content;

/// <summary>The direction of change shown on a <see cref="CyStatCard"/>.</summary>
public enum CyTrend
{
    /// <summary>No trend is shown.</summary>
    None = 0,

    /// <summary>The value went up.</summary>
    Up = 1,

    /// <summary>The value went down.</summary>
    Down = 2,

    /// <summary>The value did not change.</summary>
    Flat = 3
}
