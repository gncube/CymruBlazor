using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Renders text using the NHS Wales typography scale (see
/// wwwroot/css/tokens/typography.css and base/typography.css).
///
/// This is a code-only component (no .razor markup file) because the
/// rendered HTML tag varies with <see cref="Variant"/> (h1-h6, p, span) -
/// Razor components can't parameterise their own root element's tag name,
/// so the render tree is built manually via <see cref="BuildRenderTree"/>.
/// </summary>
public sealed class CyTypography : CyLayoutComponentBase
{
    /// <summary>
    /// Gets or sets the visual/semantic variant. Defaults to
    /// <see cref="TypographyVariant.Body"/>.
    /// </summary>
    [Parameter]
    public TypographyVariant Variant { get; set; } = TypographyVariant.Body;

    /// <summary>
    /// Optionally overrides the rendered HTML tag while keeping
    /// <see cref="Variant"/>'s visual styling. Use this to keep heading
    /// levels sequential (e.g. visually an H2 but semantically an H3)
    /// rather than skipping levels purely for visual effect, which is a
    /// common WCAG 1.3.1 / 2.4.6 failure.
    /// </summary>
    [Parameter]
    public string? As { get; set; }

    protected override string BaseCssClass => "cy-typography";

    protected override string BuildCssClass()
    {
        var suffix = Variant.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-typography--{suffix}")
            .Build();
    }

    private string ResolveTagName()
    {
        if (!string.IsNullOrWhiteSpace(As))
        {
            return As;
        }

        return Variant switch
        {
            TypographyVariant.H1 => "h1",
            TypographyVariant.H2 => "h2",
            TypographyVariant.H3 => "h3",
            TypographyVariant.H4 => "h4",
            TypographyVariant.H5 => "h5",
            TypographyVariant.H6 => "h6",
            TypographyVariant.Caption => "span",
            TypographyVariant.Label => "span",
            _ => "p"
        };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var sequence = 0;

        builder.OpenElement(sequence++, ResolveTagName());
        builder.AddAttribute(sequence++, "id", Id);
        builder.AddAttribute(sequence++, "class", CssClass);
        builder.AddAttribute(sequence++, "style", CssStyle);

        if (AdditionalAttributes is not null)
        {
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
        }

        builder.AddContent(sequence++, ChildContent);
        builder.CloseElement();
    }
}
