using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Branding;

/// <summary>
/// Renders an NHS Wales brand logo with CSS-driven theme resolution
/// or built-in SVG mark and wordmark lockup.
/// </summary>
public partial class CyBrandLogo : CyLayoutComponentBase
{
    /// <summary>
    /// Gets or sets the display variant or theme mode.
    /// Defaults to <see cref="BrandLogoVariant.Full"/>.
    /// </summary>
    [Parameter]
    public BrandLogoVariant Variant { get; set; } = BrandLogoVariant.Full;

    /// <summary>
    /// When true, renders only the mark/symbol icon rather than the full lockup.
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
    /// Path or URL to the logo asset used by the light theme.
    /// </summary>
    [Parameter]
    public string? LogoPath { get; set; }

    /// <summary>
    /// Path or URL to the logo asset used by the dark theme.
    /// </summary>
    [Parameter]
    public string? DarkLogoPath { get; set; }

    /// <summary>
    /// The organisation or product name rendered by the built-in wordmark.
    /// Defaults to <c>CymruBlazor</c>.
    /// </summary>
    [Parameter]
    public string Text { get; set; } = "CymruBlazor";

    /// <summary>
    /// When set, the entire logo renders as an anchor element.
    /// Defaults to <c>"/"</c>.
    /// </summary>
    [Parameter]
    public string? Href { get; set; } = "/";

    /// <summary>
    /// Accessible label applied to the logo.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    protected override string BaseCssClass => "cy-brand-logo";

    private bool ShowMark =>
        SymbolOnly || Variant is BrandLogoVariant.Full or BrandLogoVariant.Mark or BrandLogoVariant.Stacked;

    private bool ShowWordmark =>
        !SymbolOnly && Variant is BrandLogoVariant.Full or BrandLogoVariant.Wordmark or BrandLogoVariant.Stacked;

    private string ComputedAriaLabel =>
        AriaLabel ?? (Href is not null ? $"{Text} Home" : Text);

    private string? EffectiveLogoPath =>
        LogoPath ?? (Variant is BrandLogoVariant.Auto or BrandLogoVariant.Light or BrandLogoVariant.Dark
            ? (SymbolOnly ? "images/icon-dhcw-light.svg" : "images/logo-dhcw-light.svg")
            : null);

    private string? EffectiveDarkLogoPath =>
        DarkLogoPath ?? (Variant is BrandLogoVariant.Auto or BrandLogoVariant.Light or BrandLogoVariant.Dark
            ? (SymbolOnly ? "images/icon-dhcw-dark.svg" : "images/logo-dhcw-dark.svg")
            : null);

    private string ComputedLinkClass =>
        CssBuilder.Empty
            .AddClass("inline-flex items-center gap-2 rounded-sm focus:outline-none focus-visible:ring-2 focus-visible:ring-sr-focus-ring")
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-brand-logo--{Variant.ToString().ToLowerInvariant()}")
            .AddClass($"cy-brand-logo--{Size.ToString().ToLowerInvariant()}")
            .Build();

    private string ComputedContainerClass =>
        CssBuilder.Empty
            .AddClass("inline-flex items-center")
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-brand-logo--{Variant.ToString().ToLowerInvariant()}")
            .AddClass($"cy-brand-logo--{Size.ToString().ToLowerInvariant()}")
            .Build();

    private string ComputedImageClass(string themeModifierClass) =>
        CssBuilder.Empty
            .AddClass(themeModifierClass)
            .AddClass(SymbolOnly ? "h-8 w-auto" : "h-15 w-auto")
            .AddClass("transition-opacity duration-150")
            .Build();

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-brand-logo--{Variant.ToString().ToLowerInvariant()}")
            .AddClass($"cy-brand-logo--{Size.ToString().ToLowerInvariant()}")
            .Build();
}
