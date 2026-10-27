using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Contracts;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// An inset text callout component following the NHS Design System pattern.
/// Used to emphasize important contextual information, instructions, or caveats
/// with a distinctive thick accent border.
/// </summary>
public partial class CyInsetText : CyLayoutComponentBase, IHasColour
{
    private static readonly CyInsetTextText s_defaultText = new();

    /// <summary>
    /// Gets or sets the accessible ARIA role for the inset text.
    /// Defaults to <c>"note"</c> for supplementary or parenthetic guidance.
    /// Can be overridden or set to <c>null</c>.
    /// </summary>
    [Parameter]
    public string? Role { get; set; } = "note";

    /// <summary>
    /// Gets or sets optional semantic coloring for the callout border.
    /// Defaults to <see cref="ComponentColour.Unspecified"/>, rendering the standard NHS inset border.
    /// </summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Unspecified;

    /// <summary>
    /// Gets or sets custom visually hidden text announced by screen readers before the child content.
    /// If omitted, falls back to <see cref="CyInsetTextText.VisuallyHiddenText"/> ("Information: ").
    /// </summary>
    [Parameter]
    public string? VisuallyHiddenText { get; set; }

    /// <summary>
    /// Gets or sets whether to render the visually hidden prefix for screen readers.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowVisuallyHiddenText { get; set; } = true;

    /// <summary>
    /// Gets or sets the localized text configuration.
    /// </summary>
    [Parameter]
    public CyInsetTextText? Text { get; set; }

    /// <summary>
    /// Gets the effective localized phrases.
    /// </summary>
    protected CyInsetTextText EffectiveText => Text ?? s_defaultText;

    /// <summary>
    /// Gets the effective visually hidden text prefix.
    /// </summary>
    protected string? EffectiveVisuallyHiddenText => VisuallyHiddenText ?? EffectiveText.VisuallyHiddenText;

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Colour;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-inset-text";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-inset-text--{Colour.ToString().ToLowerInvariant()}", Colour != ComponentColour.Unspecified)
            .Build();
}
