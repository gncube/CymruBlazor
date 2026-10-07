namespace CymruBlazor.Components.Layout;

/// <summary>Maximum content width of a <see cref="CyPage"/>, mapped to the <c>--cymru-page-max-width-*</c> tokens.</summary>
public enum CyPageWidth
{
    /// <summary>Reading and form pages.</summary>
    Narrow,

    /// <summary>Most pages (the default).</summary>
    Default,

    /// <summary>Dashboards and wide tables.</summary>
    Wide,

    /// <summary>No maximum width, for tool-style pages that should use the whole screen.</summary>
    Full
}
