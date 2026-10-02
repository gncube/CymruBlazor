using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Data;

/// <summary>
/// A styled semantic table. <see cref="CyTable"/> does not implement
/// sorting, filtering or virtualisation - per the roadmap decision (D5),
/// that is a product of its own; document styling
/// <c>Microsoft.AspNetCore.Components.QuickGrid</c> instead for that case.
/// This component's job is narrower: a real <c>&lt;table&gt;</c> with a
/// required <see cref="Caption"/>, and a keyboard-accessible scroll
/// container so wide tables do not silently overflow.
///
/// Write the table's own <c>&lt;thead&gt;</c>/<c>&lt;tbody&gt;</c> markup
/// as <see cref="CyLayoutComponentBase.ChildContent"/> - including
/// <c>scope="col"</c> on column headers and <c>scope="row"</c> on any row
/// header cells, since those live on individual <c>&lt;th&gt;</c>
/// elements this component does not itself render.
/// </summary>
public partial class CyTable : CyLayoutComponentBase
{
    /// <summary>
    /// The table's caption - required, so every table has an accessible
    /// name and a plain-text summary of what it contains. Set
    /// <see cref="CaptionVisuallyHidden"/> when the surrounding content
    /// (e.g. a preceding heading) already makes this redundant to sighted
    /// users.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Caption { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the caption is still present in the
    /// accessibility tree and announced by screen readers, but visually
    /// hidden - use when a preceding visible heading already names the
    /// table for sighted users.
    /// </summary>
    [Parameter]
    public bool CaptionVisuallyHidden { get; set; }

    /// <summary>
    /// When <see langword="true"/> (the default), wraps the table in a
    /// horizontally scrollable, keyboard-focusable region
    /// (<c>role="region"</c>, <c>tabindex="0"</c>, named by
    /// <see cref="Caption"/>) so a table wider than its container can
    /// still be reached without a mouse, instead of silently overflowing
    /// the page. Set to <see langword="false"/> only when the table is
    /// already known to always fit its container.
    /// </summary>
    [Parameter]
    public bool ScrollContainer { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, cell text wraps instead of staying on one
    /// line, so a long description no longer forces a horizontal scrollbar.
    /// Defaults to <see langword="false"/> (single-line cells, as in every
    /// earlier 1.x release) so existing tables don't reflow; wrapping is
    /// expected to become the default in 2.0.0. Individual cells can opt in or
    /// out with the <c>cy-table__cell--wrap</c> / <c>cy-table__cell--nowrap</c>
    /// classes, or truncate with <c>cy-table__cell--truncate</c> (give the cell
    /// a <c>title</c> so the full text stays available).
    /// </summary>
    [Parameter]
    public bool Wrap { get; set; }

    protected override string BaseCssClass => "cy-table";

    private string ScrollContainerId => $"{Id}-scroll";

    private string CaptionId => $"{Id}-caption";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-table--wrap", Wrap)
            .Build();

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Caption))
        {
            throw new InvalidOperationException(
                $"{nameof(CyTable)}.{nameof(Caption)} must not be empty - every table needs an accessible name.");
        }
    }
}
