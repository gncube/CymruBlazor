using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A page-level heading region: title, optional subtitle, optional
/// breadcrumb trail above it, and optional right-aligned actions (e.g. an
/// "Edit" button). Internally composes <see cref="CyStack"/> and
/// <c>CyTypography</c> rather than introducing new layout primitives.
/// </summary>
public partial class CyPageHeader : CyLayoutComponentBase
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Typically a <see cref="CyBreadcrumb"/>, rendered above the title.
    /// </summary>
    [Parameter]
    public RenderFragment? Breadcrumb { get; set; }

    /// <summary>
    /// Right-aligned content alongside the title (typically one or more
    /// buttons).
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Short text above the title (a section or record type such as "Referral"). Optional.
    /// </summary>
    [Parameter]
    public string? Eyebrow { get; set; }

    /// <summary>
    /// Status content shown beside the title, typically one or more <c>CyBadge</c>. Status must also be
    /// readable as text; colour alone is not enough.
    /// </summary>
    [Parameter]
    public RenderFragment? Badges { get; set; }

    /// <summary>
    /// Heading level (1 to 6) of the title. Defaults to 1; use a lower level only when the page already has
    /// its own <c>h1</c>. The visual size does not change with the level.
    /// </summary>
    [Parameter]
    public int TitleLevel { get; set; } = 1;

    private string? TitleTag => TitleLevel == 1 ? null : $"h{TitleLevel}";

    protected override void ValidateParameters()
    {
        if (TitleLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyPageHeader)}.{nameof(TitleLevel)} must be between 1 and 6. Received '{TitleLevel}'.");
        }
    }

    protected override string BaseCssClass => "cy-page-header";
}
