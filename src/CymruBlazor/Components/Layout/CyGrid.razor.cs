using Microsoft.AspNetCore.Components;

using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a CSS Grid layout.
/// </summary>
public partial class CyGrid : LayoutComponentBase
{
    [Parameter]
    public GridColumns Columns { get; set; } = GridColumns.Auto;

    [Parameter]
    public GridGap Gap { get; set; } = GridGap.Medium;

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public bool AutoRows { get; set; }

    protected override string ComponentClass => "cy-grid";

    protected override string CssClass =>
        CreateLayoutCss()

            .AddClass($"cy-grid--cols-{GetColumnClass()}")

            .AddClass($"cy-gap-{Gap.ToString().ToLowerInvariant()}")

            .AddClass("cy-grid--dense",
                Dense)

            .AddClass("cy-grid--auto-rows",
                AutoRows)

            .Build();

    private string GetColumnClass() =>
        Columns switch
        {
            GridColumns.Auto => "auto",
            GridColumns.One => "1",
            GridColumns.Two => "2",
            GridColumns.Three => "3",
            GridColumns.Four => "4",
            GridColumns.Five => "5",
            GridColumns.Six => "6",
            GridColumns.Twelve => "12",
            _ => "auto"
        };
}
