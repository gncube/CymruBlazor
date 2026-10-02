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
    /// The icon name, e.g. "search", "patient", "waiting-list". See
    /// <see cref="IconRegistry.AllNames"/> for the complete, registry-backed list -
    /// there is no fixed enum, since the registry is the source of truth and grows
    /// independently of this component.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Width/height in pixels. Defaults to 24, the design system's
    /// native grid size.
    /// </summary>
    [Parameter]
    public int Size { get; set; } = 24;

    /// <summary>
    /// Stroke width of the icon lines in pixels. Defaults to 2 (matching Lucide and Welsh NHS standard).
    /// </summary>
    [Parameter]
    public double StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Stroke colour of the icon. Defaults to <see langword="null"/> (which resolves to <c>currentColor</c>).
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// Accessible label. Leave unset for decorative icons (the common
    /// case - see the type-level remarks).
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    protected override string BaseCssClass => "cy-icon";

    /// <summary>The name of the neutral placeholder rendered for an unknown icon in lenient mode.</summary>
    internal const string FallbackIconName = "unknown";

    private string IconMarkup =>
        IconRegistry.GetMarkup(IconRegistry.Exists(Name) ? Name : FallbackIconName);

    private string? AriaRole => string.IsNullOrWhiteSpace(Label) ? null : "img";

    private string? AriaHidden => string.IsNullOrWhiteSpace(Label) ? "true" : null;

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (IconRegistry.Exists(Name))
        {
            return;
        }

        var message =
            $"Unknown icon name '{Name}'. See {nameof(IconRegistry)}.{nameof(IconRegistry.AllNames)} for the full list of available icons, " +
            $"or add your own with {nameof(IconRegistry)}.{nameof(IconRegistry.Register)}.";

        if (Diagnostics.IsStrict)
        {
            throw new ArgumentException(message, nameof(Name));
        }

        // Lenient (Production): one bad icon name must not take down the page.
        Diagnostics.Warn("CY0001", message);
    }
}
