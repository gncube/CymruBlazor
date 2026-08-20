using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Content;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Components.Theming;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Branding;

/// <summary>
/// Renders a CymruBlazor brand logo.
///
/// The component supports two rendering modes:
/// <list type="bullet">
/// <item>
/// When <see cref="LogoPath"/> is supplied, an image-based logo is rendered.
/// </item>
/// <item>
/// When <see cref="LogoPath"/> is not supplied, the built-in SVG mark and
/// wordmark lockup is rendered.
/// </item>
/// </list>
///
/// Image-based logos can provide separate light and dark theme assets.
/// Theme selection is performed entirely through CSS using the ambient
/// <c>data-theme</c> attribute. No JavaScript interop or theme-service
/// dependency is required by this component.
///
/// The component follows the whole-element link convention used by other
/// CymruBlazor components. Set <see cref="Href"/> to render the logo as
/// a single link target.
/// </summary>
public partial class CyBrandLogo : CyLayoutComponentBase
{
    /// <summary>
    /// Which parts of the built-in lockup to render.
    ///
    /// This parameter applies when <see cref="LogoPath"/> is not supplied.
    /// Defaults to <see cref="BrandLogoVariant.Full"/>.
    /// </summary>
    [Parameter]
    public BrandLogoVariant Variant { get; set; } = BrandLogoVariant.Full;

    /// <summary>
    /// Overall sizing of the logo from the shared component size scale.
    ///
    /// For the built-in logo this controls the SVG mark and wordmark.
    /// For external assets this controls the maximum logo height while
    /// preserving the supplied asset's intrinsic aspect ratio.
    ///
    /// Defaults to <see cref="ComponentSize.Medium"/>.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Path or URL to the logo asset used by the light theme.
    ///
    /// When supplied, the image-based logo takes precedence over the
    /// built-in SVG mark and wordmark.
    ///
    /// When <see cref="DarkLogoPath"/> is not supplied, this asset is used
    /// for both light and dark themes.
    /// </summary>
    [Parameter]
    public string? LogoPath { get; set; }

    /// <summary>
    /// Path or URL to the logo asset used by the dark theme.
    ///
    /// When omitted, <see cref="LogoPath"/> is used for both themes.
    /// This parameter has no effect when <see cref="LogoPath"/> is null.
    /// </summary>
    [Parameter]
    public string? DarkLogoPath { get; set; }

    /// <summary>
    /// The organisation or product name rendered by the built-in wordmark.
    ///
    /// This parameter applies when <see cref="LogoPath"/> is not supplied.
    /// Defaults to <c>CymruBlazor</c>.
    /// </summary>
    [Parameter]
    public string Text { get; set; } = "CymruBlazor";

    /// <summary>
    /// When set, the entire logo renders as a single anchor element.
    /// Typically this points to the application's home route.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Accessible label applied to the logo.
    ///
    /// When omitted, the label defaults to "<see cref="Text"/> home" for
    /// linked logos and "<see cref="Text"/>" for non-linked logos.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    protected override string BaseCssClass => "cy-brand-logo";

    private bool ShowMark =>
        Variant is BrandLogoVariant.Full
            or BrandLogoVariant.Mark
            or BrandLogoVariant.Stacked;

    private bool ShowWordmark =>
        Variant is BrandLogoVariant.Full
            or BrandLogoVariant.Wordmark
            or BrandLogoVariant.Stacked;

    private string ComputedAriaLabel =>
        AriaLabel ?? (Href is not null ? $"{Text} home" : Text);

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
}
