using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A page-level heading region: title, optional subtitle, optional
/// breadcrumb trail above it, and optional right-aligned actions (e.g. an
/// "Edit" button). Internally composes <see cref="CyStack"/> and
/// <c>CyTypography</c> rather than introducing new layout primitives.
/// </summary>
public partial class CyPageHeader : CyLayoutComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Typically a <see cref="CyBreadcrumb"/>, rendered above the title.
    /// </summary>
    [Parameter]
    public RenderFragment? Breadcrumb { get; set; }

    /// <summary>
    /// Right-aligned content alongside the title (typically one or more
    /// buttons).
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    protected override string BaseCssClass => "cy-page-header";
}
