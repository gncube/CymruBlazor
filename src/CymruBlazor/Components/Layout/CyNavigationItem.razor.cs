using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A single entry in a <see cref="CyNavigation"/> menu. Active-route
/// highlighting is delegated to the framework's own
/// <see cref="Microsoft.AspNetCore.Components.Routing.NavLink"/> rather
/// than reimplemented - it already correctly handles comparing against
/// the current URI.
/// </summary>
public partial class CyNavigationItem : CyLayoutComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    public string Href { get; set; } = string.Empty;

    protected override string BaseCssClass => "cy-navigation__item";
}
