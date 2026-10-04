namespace CymruBlazor.Components.Layout;

/// <summary>
/// A multi-pane layout for tool-style pages that should use the full width of the screen - for example a
/// questionnaire designer with a palette, a canvas and an inspector side by side. Put
/// <see cref="CyWorkspacePane"/>s inside it.
/// </summary>
/// <remarks>
/// <para>
/// Panes sit side by side in source order. One pane normally fills the remaining space
/// (<see cref="CyWorkspacePane.Fill"/>); the others keep a preferred width and never shrink below their
/// <see cref="CyWorkspacePane.MinWidth"/>. Collapsible panes shrink to a narrow rail that keeps the toggle
/// button. Below roughly 48rem (768px) the panes stack vertically in source order, so put the pane that matters
/// most on a small screen first.
/// </para>
/// <para>
/// The workspace has no landmark role of its own: each pane is a labelled <c>section</c>. It is as tall as its
/// content unless you give it a height (for example <c>Style="--cy-workspace-height: 70vh"</c>), in which case each
/// pane scrolls on its own. Resizing by dragging is not part of 1.9.0.
/// </para>
/// </remarks>
public partial class CyWorkspace : CyLayoutComponentBase
{
    /// <inheritdoc />
    protected override string BaseCssClass => "cy-workspace";
}
