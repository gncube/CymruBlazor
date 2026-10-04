using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A placeholder for a region that has nothing to show yet - an empty designer
/// canvas, an empty list or a search with no results - that says why and offers
/// the next step.
/// </summary>
/// <remarks>
/// The description is rendered as plain text (never as markup), so it is safe to
/// bind to untrusted values. Use <see cref="ChildContent"/> or
/// <see cref="Actions"/> for anything richer. Pick <see cref="HeadingLevel"/>
/// to fit the page outline; the visual size does not change with it.
/// </remarks>
public partial class CyEmptyState : CyLayoutComponentBase
{
    /// <summary>The heading text. Required.</summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>Optional plain-text explanation shown under the heading.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Optional name of a built-in icon (see <c>IconRegistry</c>) shown above the heading.
    /// Decorative: it is hidden from assistive technology.
    /// Ignored when <see cref="IconContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Custom decorative content shown above the heading, in place of <see cref="Icon"/>.</summary>
    [Parameter]
    public RenderFragment? IconContent { get; set; }

    /// <summary>Primary and secondary actions (typically <c>CyButton</c>s) shown under the text.</summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>The heading level, 1 to 6. Defaults to 2.</summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-empty-state";

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                $"{nameof(CyEmptyState)}.{nameof(Title)} must not be empty: it names the empty region.");
        }

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyEmptyState)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }
    }
}
