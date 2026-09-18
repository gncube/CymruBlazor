using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Provides a collapsible, responsive sidebar layout component supporting multi-state cycling,
/// legacy two-state collapse modes, and mobile off-canvas drawer presentation.
/// </summary>
public partial class CySidebar : CyLayoutComponentBase
{
    private SidebarState _state = SidebarState.Expanded;
    private bool _legacyCollapsed;
    private SidebarCollapseMode _legacyCollapseMode = SidebarCollapseMode.Compact;

    /// <summary>
    /// Gets or sets the list of states through which the sidebar cycles.
    /// Defaults to <see cref="SidebarState.Expanded"/> and <see cref="SidebarState.Compact"/>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<SidebarState> States { get; set; } = [SidebarState.Expanded, SidebarState.Compact];

    /// <summary>
    /// Gets or sets the active display state of the sidebar.
    /// </summary>
    [Parameter]
    public SidebarState State
    {
        get => _state;
        set => _state = value;
    }

    /// <summary>
    /// Event callback invoked when <see cref="State"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<SidebarState> StateChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the responsive mobile off-canvas drawer is open.
    /// </summary>
    [Parameter]
    public bool MobileOpen { get; set; }

    /// <summary>
    /// Event callback invoked when <see cref="MobileOpen"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> MobileOpenChanged { get; set; }

    /// <summary>
    /// Gets or sets the CSS media query breakpoint below which mobile drawer layout rules apply.
    /// Defaults to <c>47.99rem</c>.
    /// </summary>
    [Parameter]
    public string MobileBreakpoint { get; set; } = "47.99rem";

    /// <summary>
    /// Gets or sets whether the dismissible backdrop overlay is rendered when <see cref="MobileOpen"/> is true.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowMobileBackdrop { get; set; } = true;

    /// <summary>
    /// Gets or sets the position of the sidebar relative to the page content.
    /// </summary>
    [Parameter]
    public SidebarPosition Position { get; set; } = SidebarPosition.Left;

    /// <summary>
    /// Gets or sets the width of the sidebar when in the <see cref="SidebarState.Expanded"/> state.
    /// </summary>
    [Parameter]
    public SidebarWidth Width { get; set; } = SidebarWidth.Medium;

    /// <summary>
    /// Legacy parameter indicating whether the sidebar is collapsed.
    /// </summary>
    [Obsolete("Use States and State instead.", error: false)]
    [Parameter]
    public bool Collapsed
    {
        get => EffectiveCollapsed;
        set => _legacyCollapsed = value;
    }

    /// <summary>
    /// Legacy event callback invoked when <see cref="Collapsed"/> changes.
    /// </summary>
    [Obsolete("Use States and State instead.", error: false)]
    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>
    /// Legacy parameter configuring the appearance mode used when <see cref="Collapsed"/> is true.
    /// </summary>
    [Obsolete("Use States and State instead.", error: false)]
    [Parameter]
    public SidebarCollapseMode CollapseMode
    {
        get => _legacyCollapseMode;
        set => _legacyCollapseMode = value;
    }

    /// <summary>
    /// Optional brand lockup rendered at the top of the sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    protected override string BaseCssClass => "cy-sidebar";

    /// <summary>
    /// Indicates whether the sidebar is currently in any non-expanded collapsed state.
    /// </summary>
    public bool EffectiveCollapsed => _state != SidebarState.Expanded;

    /// <summary>
    /// Resolves whether the top header row renders at all.
    /// </summary>
    private bool ShowHeader =>
        MobileOpen || Brand is not null || _legacyCollapseMode != SidebarCollapseMode.Disabled;

    /// <summary>
    /// The chevron icon used by the collapse/expand toggle button.
    /// </summary>
    private string ToggleIconName => (Position, EffectiveCollapsed) switch
    {
        (SidebarPosition.Right, false) => "chevron-right",
        (SidebarPosition.Right, true) => "chevron-left",
        (_, false) => "chevron-left",
        (_, true) => "chevron-right"
    };

    /// <summary>
    /// The chevron icon used by the external reveal handle button when the sidebar is hidden.
    /// </summary>
    private string RevealIconName => Position == SidebarPosition.Right
        ? "chevron-left"
        : "chevron-right";

    /// <summary>
    /// Legacy HTML attribute value for styling compatibility.
    /// </summary>
    public string CollapseModeAttribute => _state switch
    {
        SidebarState.Compact => "compact",
        SidebarState.IconOnly => "icon-only",
        SidebarState.Hidden => "hidden",
        _ => _legacyCollapseMode == SidebarCollapseMode.Disabled
            ? "disabled"
            : _legacyCollapseMode switch
            {
                SidebarCollapseMode.IconOnly => "icon-only",
                SidebarCollapseMode.Hidden => "hidden",
                _ => "compact"
            }
    };

    /// <inheritdoc />
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        SynchronizeCompatibilityParameters(parameters);

        await base.SetParametersAsync(ParameterView.Empty);
    }

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("cy-sidebar--left", Position == SidebarPosition.Left)
            .AddClass("cy-sidebar--right", Position == SidebarPosition.Right)
            .AddClass("cy-sidebar--sm", Width == SidebarWidth.Small)
            .AddClass("cy-sidebar--md", Width == SidebarWidth.Medium)
            .AddClass("cy-sidebar--lg", Width == SidebarWidth.Large)
            .AddClass("cy-sidebar--compact", _state == SidebarState.Compact)
            .AddClass("cy-sidebar--icon-only", _state == SidebarState.IconOnly)
            .AddClass("cy-sidebar--collapsed", _state == SidebarState.Hidden)
            .AddClass("cy-sidebar--mobile-open", MobileOpen)
            .Build();

    /// <inheritdoc />
    protected override string? BuildCssStyle()
    {
        var customBreakpoint = !string.IsNullOrWhiteSpace(MobileBreakpoint)
            ? $"--cy-sidebar-mobile-breakpoint: {MobileBreakpoint}"
            : null;

        if (string.IsNullOrWhiteSpace(Style))
        {
            return customBreakpoint;
        }

        if (string.IsNullOrWhiteSpace(customBreakpoint))
        {
            return Style;
        }

        var trimmedStyle = Style.TrimEnd(';', ' ');
        return $"{trimmedStyle}; {customBreakpoint};";
    }

    /// <summary>
    /// Closes the mobile off-canvas drawer and dispatches state change events.
    /// </summary>
    public async Task CloseMobileDrawerAsync()
    {
        if (!MobileOpen)
        {
            return;
        }

        MobileOpen = false;

        if (MobileOpenChanged.HasDelegate)
        {
            await MobileOpenChanged.InvokeAsync(false);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Cycles forward to the next state configured in <see cref="States"/>, wrapping around to the first entry.
    /// </summary>
    public async Task CycleNextAsync()
    {
        if (States.Count == 0 || _legacyCollapseMode == SidebarCollapseMode.Disabled)
        {
            return;
        }

        var currentIndex = -1;
        for (var i = 0; i < States.Count; i++)
        {
            if (States[i] == _state)
            {
                currentIndex = i;
                break;
            }
        }

        var nextIndex = currentIndex >= 0 ? (currentIndex + 1) % States.Count : 0;
        await ApplyStateChangeAsync(States[nextIndex]);
    }

    /// <summary>
    /// Sets the active sidebar state to the primary expanded state (<c>States[0]</c>).
    /// </summary>
    public async Task ExpandAsync()
    {
        if (States.Count == 0)
        {
            return;
        }

        await ApplyStateChangeAsync(States[0]);
    }

    /// <summary>
    /// Sets the active sidebar state to the specified <paramref name="state"/> if present in <see cref="States"/>.
    /// </summary>
    /// <param name="state">The target sidebar state.</param>
    public async Task SetStateAsync(SidebarState state)
    {
        if (!States.Contains(state))
        {
            return;
        }

        await ApplyStateChangeAsync(state);
    }

    /// <summary>
    /// Programmatically toggles the collapsed state of the sidebar.
    /// </summary>
    public async Task ToggleAsync()
    {
        if (_legacyCollapseMode == SidebarCollapseMode.Disabled)
        {
            return;
        }

        if (States.Count > 1)
        {
            await CycleNextAsync();
            return;
        }

        var targetState = _state == SidebarState.Expanded
            ? ResolveCollapsedStateFromLegacyMode(_legacyCollapseMode)
            : SidebarState.Expanded;

        await ApplyStateChangeAsync(targetState);
    }

    private async Task ApplyStateChangeAsync(SidebarState newState)
    {
        _state = newState;
        _legacyCollapsed = EffectiveCollapsed;
        _legacyCollapseMode = MapStateToCollapseMode(newState);

        if (StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(_state);
        }

        if (CollapsedChanged.HasDelegate)
        {
            await CollapsedChanged.InvokeAsync(_legacyCollapsed);
        }

        StateHasChanged();
    }

    private void SynchronizeCompatibilityParameters(ParameterView parameters)
    {
        var hasState = parameters.TryGetValue<SidebarState>(nameof(State), out _);
        var hasCollapsed = parameters.TryGetValue<bool>(nameof(Collapsed), out _);
        var hasCollapseMode = parameters.TryGetValue<SidebarCollapseMode>(nameof(CollapseMode), out _);

        if (hasState)
        {
            _legacyCollapsed = EffectiveCollapsed;
            _legacyCollapseMode = MapStateToCollapseMode(_state);
            return;
        }

        if (hasCollapsed || hasCollapseMode)
        {
            if (_legacyCollapseMode == SidebarCollapseMode.Disabled)
            {
                _state = SidebarState.Expanded;
                _legacyCollapsed = false;
                return;
            }

            _state = _legacyCollapsed
                ? ResolveCollapsedStateFromLegacyMode(_legacyCollapseMode)
                : SidebarState.Expanded;

            _legacyCollapsed = EffectiveCollapsed;
        }
    }

    private static SidebarState ResolveCollapsedStateFromLegacyMode(SidebarCollapseMode mode) => mode switch
    {
        SidebarCollapseMode.IconOnly => SidebarState.IconOnly,
        SidebarCollapseMode.Hidden => SidebarState.Hidden,
        _ => SidebarState.Compact
    };

    private static SidebarCollapseMode MapStateToCollapseMode(SidebarState state) => state switch
    {
        SidebarState.IconOnly => SidebarCollapseMode.IconOnly,
        SidebarState.Hidden => SidebarCollapseMode.Hidden,
        SidebarState.Compact => SidebarCollapseMode.Compact,
        SidebarState.Expanded => SidebarCollapseMode.Compact,
        _ => SidebarCollapseMode.Compact
    };
}
