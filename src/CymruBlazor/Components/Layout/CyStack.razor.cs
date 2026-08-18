using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a flexbox stack layout.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "'Stack' is the established name for this flexbox layout primitive " +
        "in UI component libraries (MudBlazor, Fluent UI Blazor). It is not intended to " +
        "resemble System.Collections.Stack, so the collection-naming convention does not apply.")]
public partial class CyStack : CyLayoutComponentBase
{
    [Parameter]
    public Orientation Orientation { get; set; }

    [Parameter]
    public ComponentSize Gap { get; set; } = ComponentSize.Medium;

    [Parameter]
    public AlignItems AlignItems { get; set; }

    [Parameter]
    public JustifyContent JustifyContent { get; set; }

    [Parameter]
    public bool Wrap { get; set; }

    protected override string BaseCssClass => "cy-stack";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-stack--horizontal", Orientation == Orientation.Horizontal)
            .AddClass("cy-stack--vertical", Orientation == Orientation.Vertical)
            .AddClass("cy-stack--wrap", Wrap)
            .AddClass($"cy-gap-{Gap.ToString().ToLowerInvariant()}")
            .AddClass($"cy-align-{AlignItems.ToString().ToLowerInvariant()}")
            .AddClass($"cy-justify-{JustifyContent.ToString().ToLowerInvariant()}")
            .Build();
}
