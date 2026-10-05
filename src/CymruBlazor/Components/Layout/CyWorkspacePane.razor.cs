using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// One pane of a <see cref="CyWorkspace"/>: a labelled <c>section</c> with a heading, optional header actions
/// and a body that scrolls on its own. Can be made collapsible.
/// </summary>
/// <remarks>
/// <para>
/// A collapsible pane shows a toggle button (<c>aria-expanded</c>, <c>aria-controls</c>) in its header; when
/// collapsed the body is <c>hidden</c> but stays in the DOM, so state inside it (a half-typed value, a scroll
/// position) survives. <see cref="Collapsed"/> is bindable.
/// </para>
/// <para>
/// <see cref="Width"/> and <see cref="MinWidth"/> take a plain CSS length (<c>16rem</c>, <c>320px</c>, <c>30%</c>).
/// They are validated and written as custom properties, so arbitrary CSS cannot be injected.
/// </para>
/// </remarks>
public partial class CyWorkspacePane : CyLayoutComponentBase
{
    private static readonly Regex Length =
        new(@"^\d+(\.\d+)?(px|rem|em|ch|%|vw)$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>The pane's heading and accessible name. Required.</summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>The heading level of <see cref="Title"/>, 1 to 6. Defaults to 2.</summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    /// <summary>Optional controls shown at the end of the header. Hidden while the pane is collapsed.</summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>When true the pane takes the space the other panes leave. Usually exactly one pane (the canvas) does.</summary>
    [Parameter]
    public bool Fill { get; set; }

    /// <summary>
    /// The preferred width as a CSS length such as <c>18rem</c>. Ignored when <see cref="Fill"/> is true.
    /// Defaults to <c>20rem</c>.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// The width the pane never shrinks below, as a CSS length. Defaults to <c>14rem</c> for a fixed pane and
    /// <c>20rem</c> for a <see cref="Fill"/> pane.
    /// </summary>
    [Parameter]
    public string? MinWidth { get; set; }

    /// <summary>Shows a toggle button that collapses the pane to a narrow rail.</summary>
    [Parameter]
    public bool Collapsible { get; set; }

    /// <summary>Whether the pane is collapsed. Two-way bindable; ignored unless <see cref="Collapsible"/>.</summary>
    [Parameter]
    public bool Collapsed { get; set; }

    /// <summary>Raised when <see cref="Collapsed"/> changes because the user pressed the toggle.</summary>
    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>Format of the toggle's accessible name while expanded; <c>{0}</c> is the title. Defaults to "Collapse {0}".</summary>
    [Parameter]
    public string? CollapseLabelFormat { get; set; }

    /// <summary>Format of the toggle's accessible name while collapsed; <c>{0}</c> is the title. Defaults to "Expand {0}".</summary>
    [Parameter]
    public string? ExpandLabelFormat { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-workspace-pane";

    private string TitleId => $"{Id}-title";

    private string BodyId => $"{Id}-body";

    private bool IsCollapsed => Collapsible && Collapsed;

    private string ToggleLabel => string.Format(
        CultureInfo.CurrentCulture,
        IsCollapsed ? ExpandLabelFormat ?? "Expand {0}" : CollapseLabelFormat ?? "Collapse {0}",
        Title);

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-workspace-pane--fill", Fill)
            .AddClass("cy-workspace-pane--collapsed", IsCollapsed)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override string BuildCssStyle() =>
        StyleBuilder.Empty
            .AddStyle("--cy-pane-width", Fill ? null : Width)
            .AddStyle("--cy-pane-min-width", MinWidth)
            .AddStyle(Style)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                $"{nameof(CyWorkspacePane)}.{nameof(Title)} must not be empty: it names the pane.");
        }

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyWorkspacePane)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }

        RequireLength(Width, nameof(Width));
        RequireLength(MinWidth, nameof(MinWidth));
    }

    private static void RequireLength(string? value, string parameter)
    {
        if (value is not null && !Length.IsMatch(value))
        {
            throw new InvalidOperationException(
                $"{nameof(CyWorkspacePane)}.{parameter} must be a CSS length such as '16rem', '320px' or '30%'. Received '{value}'.");
        }
    }

    private async Task ToggleAsync()
    {
        Collapsed = !Collapsed;

        if (CollapsedChanged.HasDelegate)
        {
            await CollapsedChanged.InvokeAsync(Collapsed);
        }
    }
}
