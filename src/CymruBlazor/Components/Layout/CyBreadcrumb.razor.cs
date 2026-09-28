using Microsoft.AspNetCore.Components;
using CymruBlazor.Localisation;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A breadcrumb trail. Contains <see cref="CyBreadcrumbItem"/> children.
/// </summary>
public partial class CyBreadcrumb : CyLayoutComponentBase
{
    /// <summary>The cascaded step-2 localiser (ADR-0002), if any. See <see cref="AriaLabel"/>.</summary>
    [CascadingParameter]
    public ICyLocalizer? Localizer { get; set; }

    /// <summary>
    /// Accessible name of the breadcrumb landmark. Defaults to the English
    /// "Breadcrumb"; supply a translation for bilingual services.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    protected override string BaseCssClass => "cy-breadcrumb";
}
