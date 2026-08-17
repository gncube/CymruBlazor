using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Site footer - navy band (matching the NHS Wales/DHCW header+footer
/// colour pairing), optional link groups, optional copyright line.
///
/// Structurally promotes the pattern already used by
/// <c>CymruBlazor.Demo</c>'s <c>DemoFooter.razor</c> into the shipped
/// library - that Demo-only component can be simplified to configure
/// this one instead of maintaining its own markup/CSS, as a follow-up.
/// </summary>
public partial class CyFooter : CyLayoutComponentBase
{
    /// <summary>
    /// Optional copyright line, rendered below the link groups.
    /// </summary>
    [Parameter]
    public string? Copyright { get; set; }

    protected override string BaseCssClass => "cy-footer";
}
