using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A single entry in a <see cref="CyBreadcrumb"/> trail. Renders as a
/// link when <see cref="Href"/> is set, or as plain text with
/// <c>aria-current="page"</c> when it isn't - use the latter for the
/// current page, which is the last item and should not link to itself.
/// </summary>
public partial class CyBreadcrumbItem : CyLayoutComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The link target. Leave unset for the current/last item in the
    /// trail.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    protected override string BaseCssClass => "cy-breadcrumb__item";
}
