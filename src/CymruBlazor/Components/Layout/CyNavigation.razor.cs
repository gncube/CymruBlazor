using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Top-level site navigation with a responsive mobile toggle. Contains
/// <see cref="CyNavigationItem"/> children.
///
/// At desktop widths (min-width: 64rem, matching
/// <c>tokens/breakpoints.css</c>'s existing desktop breakpoint) the menu
/// is always visible via CSS and the mobile toggle button is hidden - the
/// open/closed state below only affects the collapsed mobile view.
/// </summary>
public partial class CyNavigation : CyLayoutComponentBase
{
    private bool _isMobileMenuOpen;

    /// <summary>
    /// Logo/wordmark slot, rendered at the start of the navigation bar.
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    /// <summary>
    /// Accessible name of the navigation landmark. Defaults to the English "Main".
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Text (visually hidden) of the mobile toggle while the menu is closed.
    /// Defaults to the English "Open menu".
    /// </summary>
    [Parameter]
    public string? OpenMenuLabel { get; set; }

    /// <summary>
    /// Text (visually hidden) of the mobile toggle while the menu is open.
    /// Defaults to the English "Close menu".
    /// </summary>
    [Parameter]
    public string? CloseMenuLabel { get; set; }

    protected override string BaseCssClass => "cy-navigation";

    private bool IsMobileMenuOpen => _isMobileMenuOpen;

    private string MenuId => $"{Id}-menu";

    private string MenuCssClass =>
        CssBuilder.Empty
            .AddClass("cy-navigation__menu")
            .AddClass("cy-navigation__menu--open", _isMobileMenuOpen)
            .Build();

    private void ToggleMobileMenu()
    {
        _isMobileMenuOpen = !_isMobileMenuOpen;
    }
}
