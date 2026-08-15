using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A visually-hidden-until-focused link to the main content region,
/// satisfying WCAG 2.4.1 (Bypass Blocks). Should be the first focusable
/// element on the page - place it before <see cref="CyNavigation"/> in
/// markup, not after.
/// </summary>
public partial class CySkipLink : CyLayoutComponentBase
{
    /// <summary>
    /// The id of the element to jump to. Defaults to "main-content",
    /// matching the convention already used by
    /// <c>CymruBlazor.Demo</c>'s <c>MainLayout.razor</c>.
    /// </summary>
    [Parameter]
    public string TargetId { get; set; } = "main-content";

    protected override string BaseCssClass => "u-sr-only-focusable cy-skip-link";

    private static string DefaultText => "Skip to main content";
}
