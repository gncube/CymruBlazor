using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A set of tabs, each showing one child <see cref="CyTabPanel"/> at a
/// time. Implements the WAI-ARIA Tabs pattern with automatic activation:
/// Left/Right (and Home/End) move focus between tab buttons using a
/// roving <c>tabindex</c>, and moving focus also selects the tab,
/// matching the simple/common tabs interaction model.
///
/// <see cref="CyTabPanel"/> children register themselves with this
/// component via a cascaded reference, so the tab strip can be rendered
/// once, ahead of - and independently from - whichever single panel is
/// currently visible.
/// </summary>
public partial class CyTabs : CyLayoutComponentBase
{
    private readonly List<CyTabPanel> _panels = [];
    private readonly Dictionary<string, ElementReference> _tabRefs = [];

    /// <summary>
    /// The <see cref="CyTabPanel.TabId"/> of the currently active tab.
    /// Two-way bindable via <c>@bind-ActiveTabId</c>. When left unset,
    /// the first registered panel becomes active automatically.
    /// </summary>
    [Parameter]
    public string? ActiveTabId { get; set; }

    [Parameter]
    public EventCallback<string> ActiveTabIdChanged { get; set; }

    /// <summary>
    /// The accessible name for the tab list (<c>aria-label</c>). Should
    /// describe what the tabs collectively switch between, e.g.
    /// "Component documentation".
    /// </summary>
    [Parameter, EditorRequired]
    public required string TabListAriaLabel { get; set; }

    protected override string BaseCssClass => "cy-tabs";

    internal bool IsActive(CyTabPanel panel) => panel.TabId == ActiveTabId;

    internal string TabButtonId(CyTabPanel panel) => $"{Id}-tab-{panel.TabId}";

    internal string PanelId(CyTabPanel panel) => $"{Id}-panel-{panel.TabId}";

    internal string TabButtonClass(CyTabPanel panel) =>
        CssBuilder.Empty
            .AddClass("cy-tabs__tab")
            .AddClass("cy-tabs__tab--active", IsActive(panel))
            .Build();

    internal void RegisterPanel(CyTabPanel panel)
    {
        if (_panels.Contains(panel))
        {
            return;
        }

        _panels.Add(panel);

        if (ActiveTabId is null)
        {
            ActiveTabId = panel.TabId;
        }

        StateHasChanged();
    }

    internal void UnregisterPanel(CyTabPanel panel)
    {
        _panels.Remove(panel);
        _tabRefs.Remove(panel.TabId);

        if (ActiveTabId == panel.TabId)
        {
            ActiveTabId = _panels.Count > 0 ? _panels[0].TabId : null;
        }

        StateHasChanged();
    }

    internal async Task ActivateAsync(string tabId)
    {
        if (ActiveTabId == tabId)
        {
            return;
        }

        ActiveTabId = tabId;

        if (ActiveTabIdChanged.HasDelegate)
        {
            await ActiveTabIdChanged.InvokeAsync(tabId);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Moves focus (and, per the automatic-activation model, selection)
    /// between tab buttons. Disabled tabs are skipped. As with
    /// <see cref="CyAccordion.MoveFocusAsync"/>, Home/End's
    /// browser-default scroll behaviour is not suppressed, to avoid
    /// adding a JS interop layer purely for conditional preventDefault.
    /// </summary>
    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (_panels.Count == 0 || ActiveTabId is null)
        {
            return;
        }

        var currentIndex = _panels.FindIndex(p => p.TabId == ActiveTabId);

        if (currentIndex < 0)
        {
            return;
        }

        var targetIndex = args.Key switch
        {
            "ArrowRight" => NextEnabledIndex(currentIndex, +1),
            "ArrowLeft" => NextEnabledIndex(currentIndex, -1),
            "Home" => FirstEnabledIndex(),
            "End" => LastEnabledIndex(),
            _ => (int?)null
        };

        if (targetIndex is null || targetIndex == currentIndex)
        {
            return;
        }

        var targetPanel = _panels[targetIndex.Value];

        await ActivateAsync(targetPanel.TabId);

        if (_tabRefs.TryGetValue(targetPanel.TabId, out var elementRef))
        {
            await elementRef.FocusAsync();
        }
    }

    private int NextEnabledIndex(int fromIndex, int step)
    {
        var index = fromIndex;

        for (var attempt = 0; attempt < _panels.Count; attempt++)
        {
            index = (index + step + _panels.Count) % _panels.Count;

            if (!_panels[index].Disabled)
            {
                return index;
            }
        }

        return fromIndex;
    }

    private int FirstEnabledIndex()
    {
        for (var i = 0; i < _panels.Count; i++)
        {
            if (!_panels[i].Disabled)
            {
                return i;
            }
        }

        return 0;
    }

    private int LastEnabledIndex()
    {
        for (var i = _panels.Count - 1; i >= 0; i--)
        {
            if (!_panels[i].Disabled)
            {
                return i;
            }
        }

        return _panels.Count - 1;
    }
}
