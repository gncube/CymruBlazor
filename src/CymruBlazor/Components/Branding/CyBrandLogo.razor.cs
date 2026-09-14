using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Components.Theming;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Branding;

/// <summary>
/// Renders an NHS Wales / DHCW brand logo with theme-reactive asset resolution
/// or built-in SVG mark and wordmark lockup.
/// </summary>
public partial class CyBrandLogo : CyLayoutComponentBase
{
    /// <summary>
    /// Gets or sets the display or theme mode variant.
    /// Defaults to <see cref="BrandLogoVariant.Auto"/>.
    /// </summary>
    [Parameter]
    public BrandLogoVariant Variant { get; set; } = BrandLogoVariant.Auto;

    /// <summary>
    /// Gets or sets whether to render only the symbol/icon rather than the full logo lockup.
    /// </summary>
    [Parameter]
    public bool SymbolOnly { get; set; }

    /// <summary>
    /// Overall sizing of the logo from the shared component size scale.
    /// Defaults to <see cref="ComponentSize.Medium"/>.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Optional explicit path or URL to the light theme logo asset.
    /// Overrides standard DHCW light logo paths.
    /// </summary>
    [Parameter]
    public string? LogoPath { get; set; }

    /// <summary>
    /// Optional explicit path or URL to the dark theme logo asset.
    /// Overrides standard DHCW dark logo paths.
    /// </summary>
    [Parameter]
    public string? DarkLogoPath { get; set; }

    /// <summary>
    /// The organisation or product name rendered by the built-in wordmark.
    /// Defaults to <c>NHS Wales</c>.
    /// </summary>
    [Parameter]
    public string Text { get; set; } = "NHS Wales";

    /// <summary>
    /// Destination route or URL. When non-null, renders the logo within an anchor element.
    /// Defaults to root home route <c>"/"</c>.
    /// </summary>
    [Parameter]
    public string? Href { get; set; } = "/";

    /// <summary>
    /// Accessible label applied to the logo anchor or container element.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    protected override string BaseCssClass => "cy-brand-logo";

    private bool UseImageAssets =>
        Variant is BrandLogoVariant.Auto or BrandLogoVariant.Light or BrandLogoVariant.Dark
        || LogoPath is not null
        || DarkLogoPath is not null;

    private bool ShowMark =>
        Variant is BrandLogoVariant.Full
            or BrandLogoVariant.Mark
            or BrandLogoVariant.Stacked;

    private bool ShowWordmark =>
        Variant is BrandLogoVariant.Full
            or BrandLogoVariant.Wordmark
            or BrandLogoVariant.Stacked;

    private string ComputedAriaLabel =>
        AriaLabel ?? (Href is not null ? $"{Text} Home" : Text);

    private string LogoPathResolved
    {
        get
        {
            var useDark = Variant switch
            {
                BrandLogoVariant.Dark => true,
                BrandLogoVariant.Light => false,
                _ => ThemeService.IsDark
            };

            if (useDark && DarkLogoPath is not null)
            {
                return DarkLogoPath;
            }

            if (!useDark && LogoPath is not null)
            {
                return LogoPath;
            }

            return (useDark, SymbolOnly) switch
            {
                (true, true) => "images/icon-dhcw-dark.svg",
                (true, false) => "images/logo-dhcw-dark.svg",
                (false, true) => "images/icon-dhcw-light.svg",
                (false, false) => "images/logo-dhcw-light.svg"
            };
        }
    }

    private string ComputedLinkClass =>
        CssBuilder.Empty
            .AddClass("inline-flex items-center gap-2 rounded-sm focus:outline-none focus-visible:ring-2 focus-visible:ring-sr-focus-ring")
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();

    private string ComputedContainerClass =>
        CssBuilder.Empty
            .AddClass("inline-flex items-center")
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();

    private string ComputedImageClass =>
        CssBuilder.Empty
            .AddClass(SymbolOnly ? "h-8 w-auto" : "h-15 w-auto")
            .AddClass("transition-opacity duration-150")
            .Build();

    protected override void OnInitialized()
    {
        ThemeService.OnChange += HandleThemeChanged;
    }

    protected override string BuildCssClass()
    {
        var variantSuffix = Variant.ToString().ToLowerInvariant();
        var sizeSuffix = Size.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-brand-logo--{variantSuffix}")
            .AddClass($"cy-brand-logo--{sizeSuffix}")
            .Build();
    }

    private void HandleThemeChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ThemeService.OnChange -= HandleThemeChanged;
    }
}
