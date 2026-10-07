using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A vertically spaced block of a page. With a <see cref="Heading"/> it renders a labelled
/// <c>&lt;section&gt;</c> (a landmark named by its heading); without one it is a plain <c>&lt;div&gt;</c>,
/// because an unnamed section carries no meaning.
/// </summary>
public partial class CyPageSection : CyLayoutComponentBase
{
    /// <summary>Optional section heading.</summary>
    [Parameter]
    public string? Heading { get; set; }

    /// <summary>
    /// Heading level (1 to 6) of <see cref="Heading"/>. Defaults to 2, which suits a page whose title is an
    /// <c>h1</c>. The visual size does not change with the level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    private bool HasHeading => !string.IsNullOrWhiteSpace(Heading);

    private string HeadingTag => $"h{HeadingLevel}";

    private string HeadingId => $"{Id}-heading";

    protected override string BaseCssClass => "cy-page-section";

    protected override void ValidateParameters()
    {
        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyPageSection)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }
    }
}
