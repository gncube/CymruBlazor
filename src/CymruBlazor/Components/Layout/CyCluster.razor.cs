using Microsoft.AspNetCore.Components;

using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Arranges child content horizontally with automatic wrapping.
/// </summary>
public partial class CyCluster : LayoutComponentBase
{
    /// <summary>
    /// Gets or sets the spacing between child elements.
    /// </summary>
    [Parameter]
    public ComponentSize Gap { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Gets or sets the cross-axis alignment.
    /// </summary>
    [Parameter]
    public AlignItems AlignItems { get; set; } = AlignItems.Center;

    /// <summary>
    /// Gets or sets the main-axis alignment.
    /// </summary>
    [Parameter]
    public JustifyContent JustifyContent { get; set; }

    /// <summary>
    /// Gets or sets whether items should wrap.
    /// </summary>
    [Parameter]
    public bool Wrap { get; set; } = true;

    protected override string ComponentClass => "cy-cluster";

    protected override string CssClass =>
        CreateLayoutCss()

            .AddClass("cy-cluster--wrap", Wrap)

            .AddClass($"cy-gap-{Gap.ToString().ToLowerInvariant()}")

            .AddClass($"cy-align-{AlignItems.ToString().ToLowerInvariant()}")

            .AddClass($"cy-justify-{JustifyContent.ToString().ToLowerInvariant()}")

            .Build();
}
