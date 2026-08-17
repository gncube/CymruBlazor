using Microsoft.AspNetCore.Components;
using CymruBlazor.Icons;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Renders an icon from <see cref="IconRegistry"/> (sourced from Lucide
/// Icons - see that type for provenance details). Grid: 24x24, 2px
/// stroke, round linecap/linejoin, matching the design system.
///
/// Icons are decorative by default (paired with visible text) and
/// hidden from assistive technology accordingly. Set <see cref="Label"/>
/// only when the icon is the *sole* conveyor of meaning (e.g. an
/// icon-only button with no visible text) - if there's already visible
/// text next to the icon, leave Label unset to avoid the label being
/// announced twice.
/// </summary>
public partial class CyIcon : CyLayoutComponentBase
{
    /// <summary>
    /// The icon name, e.g. "search", "patient", "critical". See
    /// <see cref="IconRegistry.AllNames"/> for the full list.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Width/height in pixels. Defaults to 24, the design system's
    /// native grid size - scaling significantly beyond that may make the
    /// 2px stroke look disproportionately thin.
    /// </summary>
    [Parameter]
    public int Size { get; set; } = 24;

    /// <summary>
    /// Accessible label. Leave unset for decorative icons (the common
    /// case - see the type-level remarks).
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    protected override string BaseCssClass => "cy-icon";

    private string IconMarkup => IconRegistry.GetMarkup(Name);

    private string? AriaRole => string.IsNullOrWhiteSpace(Label) ? null : "img";

    private string? AriaHidden => string.IsNullOrWhiteSpace(Label) ? "true" : null;

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (!IconRegistry.Exists(Name))
        {
            throw new ArgumentException(
                $"Unknown icon name '{Name}'. See {nameof(IconRegistry)}.{nameof(IconRegistry.AllNames)} for the full list of available icons.",
                nameof(Name));
        }
    }
}
