using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A single tab's content, paired with the tab button <see cref="CyTabs"/>
/// renders for it. Must be used inside a <see cref="CyTabs"/> - it reads
/// active/disabled state from, and registers itself with, the cascaded
/// parent, which is what allows <see cref="CyTabs"/> to render the tab
/// strip once, ahead of whichever single panel is currently visible.
/// </summary>
public partial class CyTabPanel : CyLayoutComponentBase, IDisposable
{
    private bool _isRegistered;

    [CascadingParameter]
    internal CyTabs? Parent { get; set; }

    /// <summary>
    /// A stable identifier for this tab within its parent
    /// <see cref="CyTabs"/>, used for activation, keyboard focus order,
    /// and generating the paired tab button/panel DOM ids.
    /// </summary>
    [Parameter, EditorRequired]
    public required string TabId { get; set; }

    /// <summary>
    /// The tab button's label text.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the tab button is rendered disabled
    /// and skipped by keyboard navigation.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    protected override string BaseCssClass => "cy-tabs__panel";

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Parent is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyTabPanel)} must be used inside a {nameof(CyTabs)}.");
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Parent is not null)
        {
            Parent.RegisterPanel(this);
            _isRegistered = true;
        }
    }

    public void Dispose()
    {
        if (_isRegistered)
        {
            Parent?.UnregisterPanel(this);
        }

        GC.SuppressFinalize(this);
    }
}
