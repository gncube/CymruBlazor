using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Contracts;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A warning callout component following the NHS Design System pattern.
/// Highlights critical advice, urgent clinical directions, or important caveats
/// with a prominent border, visually hidden alert prefix, and accessible alert role.
/// </summary>
public partial class CyWarningCallout : CyLayoutComponentBase, IHasColour
{
    private static readonly CyWarningCalloutText s_defaultText = new();

    /// <summary>
    /// Gets or sets the accessible ARIA role.
    /// Defaults to <c>"alert"</c> to fulfill accessible alert callout requirements.
    /// Can also be set to <c>"note"</c>, <c>"region"</c>, or <c>null</c>.
    /// </summary>
    [Parameter]
    public string? Role { get; set; } = "alert";

    /// <summary>
    /// Gets or sets the heading text for the callout.
    /// </summary>
    [Parameter]
    public string? Heading { get; set; }

    /// <summary>
    /// Alias for <see cref="Heading"/>.
    /// </summary>
    [Parameter]
    public string? Title
    {
        get => Heading;
        set => Heading = value;
    }

    /// <summary>
    /// Gets or sets an optional custom template for the heading content.
    /// Takes precedence over <see cref="Heading"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? HeadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets the heading level (1-6). Defaults to 3 (<c>&lt;h3&gt;</c>) per NHS Design System guidelines.
    /// Adjust to maintain sequential heading levels without skipping.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 3;

    /// <summary>
    /// Alias for <see cref="HeadingLevel"/>.
    /// </summary>
    [Parameter]
    public int TitleLevel
    {
        get => HeadingLevel;
        set => HeadingLevel = value;
    }

    /// <summary>
    /// Gets or sets custom visually hidden prefix read by screen readers before the heading.
    /// If omitted, falls back to <see cref="CyWarningCalloutText.VisuallyHiddenPrefix"/> ("Important: ").
    /// </summary>
    [Parameter]
    public string? VisuallyHiddenPrefix { get; set; }

    /// <summary>
    /// Gets or sets whether to render the visually hidden prefix for screen readers.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowVisuallyHiddenPrefix { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a heading should be rendered. Defaults to <see langword="true"/>.
    /// If set to <see langword="false"/>, no heading tag is rendered.
    /// </summary>
    [Parameter]
    public bool ShowHeading { get; set; } = true;

    /// <summary>
    /// Gets or sets the severity/color of the warning callout.
    /// Defaults to <see cref="ComponentColour.Warning"/>.
    /// </summary>
    [Parameter]
    public ComponentColour Severity { get; set; } = ComponentColour.Warning;

    /// <summary>
    /// Alias for <see cref="Severity"/>.
    /// </summary>
    [Parameter]
    public ComponentColour Colour
    {
        get => Severity;
        set => Severity = value;
    }

    /// <summary>
    /// Gets or sets the localized text configuration.
    /// </summary>
    [Parameter]
    public CyWarningCalloutText? Text { get; set; }

    /// <summary>
    /// Gets the effective localized phrases.
    /// </summary>
    protected CyWarningCalloutText EffectiveText => Text ?? s_defaultText;

    /// <summary>
    /// Gets the effective visually hidden prefix.
    /// </summary>
    protected string? EffectiveVisuallyHiddenPrefix => VisuallyHiddenPrefix ?? EffectiveText.VisuallyHiddenPrefix;

    /// <summary>
    /// Gets whether a heading should be rendered.
    /// </summary>
    protected bool HasHeading => ShowHeading && (Heading != null || HeadingTemplate != null || !string.IsNullOrWhiteSpace(EffectiveText.DefaultHeading));

    /// <inheritdoc />
    ComponentColour IHasColour.Colour => Severity;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-warning-callout";

    /// <inheritdoc />
    protected override string BuildCssClass()
    {
        var suffix = Severity switch
        {
            ComponentColour.Danger => "danger",
            ComponentColour.Info => "info",
            ComponentColour.Primary => "primary",
            ComponentColour.Success => "success",
            _ => "warning"
        };

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-warning-callout--{suffix}")
            .Build();
    }

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyWarningCallout)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }
    }
}
