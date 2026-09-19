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

#pragma warning disable BL0007, CS0618
    /// <summary>
    /// Legacy parameter indicating whether the sidebar is collapsed.
    /// </summary>
    [Obsolete("Use States and State instead.", error: false)]
    [Parameter]
    public bool Collapsed { get; set; }

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
    public SidebarCollapseMode CollapseMode { get; set; } = SidebarCollapseMode.Compact;
#pragma warning restore BL0007, CS0618

    /// <summary>
    /// Optional brand lockup rendered at the top of the sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    /// <summary>
    /// Optional brand content shown instead of <see cref="Brand"/> while the sidebar is in a rail
    /// state (<see cref="SidebarState.Compact"/> or <see cref="SidebarState.IconOnly"/>) - typically
    /// the icon/symbol version of the logo, since the full lockup does not fit a rail. When
    /// <see langword="null"/>, <see cref="Brand"/> is shown (or nothing, if
    /// <see cref="ShowBrandWhenCollapsed"/> is <see langword="false"/>).
    /// </summary>
    [Parameter]
    public RenderFragment? CollapsedBrand { get; set; }

    /// <summary>
    /// When <see langword="false"/> and no <see cref="CollapsedBrand"/> is supplied, no brand is
    /// rendered in the rail states, leaving just the size controls (use this when no icon-sized
    /// logo is available). Defaults to <see langword="true"/> so existing markup is unchanged.
    /// </summary>
    [Parameter]
    public bool ShowBrandWhenCollapsed { get; set; } = true;

    protected override string BaseCssClass => "cy-sidebar";

    /// <summary>
    /// Indicates whether the sidebar is currently in any non-expanded collapsed state.
    /// </summary>
    public bool EffectiveCollapsed => _state != SidebarState.Expanded;

    /// <summary>
    /// Resolves whether the top header row renders at all.
    /// </summary>
#pragma warning disable CS0618
    private bool ShowHeader =>
        MobileOpen
        || Brand is not null
        || CollapsedBrand is not null
        || CollapseMode != SidebarCollapseMode.NonCollapsible;
#pragma warning restore CS0618

    private bool IsRailState => _state is SidebarState.Compact or SidebarState.IconOnly;

    /// <summary>
    /// The brand shown in the header for the current state.
    /// </summary>
    private RenderFragment? HeaderBrand =>
        IsRailState
            ? CollapsedBrand ?? (ShowBrandWhenCollapsed ? Brand : null)
            : Brand;

    /// <summary>
    /// With three or more configured states a single cycling button is ambiguous, so the header shows
    /// directional chevrons instead (one to narrow, one to widen). Two-state sidebars keep the
    /// original single toggle.
    /// </summary>
#pragma warning disable CS0618
    private bool UseStepControls =>
        States.Count > 2 && !MobileOpen && CollapseMode != SidebarCollapseMode.NonCollapsible;
#pragma warning restore CS0618

    private int CurrentIndex
    {
        get
        {
            for (var i = 0; i < States.Count; i++)
            {
                if (States[i] == _state)
                {
                    return i;
                }
            }

            return 0;
        }
    }

    private bool CanNarrow => CurrentIndex < States.Count - 1;

    private bool CanWiden => CurrentIndex > 0;

    private string NarrowLabel => CanNarrow ? DescribeTarget(States[CurrentIndex + 1]) : "Narrow sidebar";

    private string WidenLabel => CanWiden ? DescribeTarget(States[CurrentIndex - 1]) : "Widen sidebar";

    // "Narrow" points towards the sidebar's own edge, "widen" points away from it.
    private string NarrowIconName => Position == SidebarPosition.Right ? "chevron-right" : "chevron-left";

    private string WidenIconName => Position == SidebarPosition.Right ? "chevron-left" : "chevron-right";

    private static string DescribeTarget(SidebarState target) => target switch
    {
        SidebarState.Expanded => "Expand sidebar",
        SidebarState.Compact => "Show compact sidebar",
        SidebarState.IconOnly => "Show icons only",
        SidebarState.Hidden => "Hide sidebar",
        _ => "Resize sidebar"
    };

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
#pragma warning disable CS0618
    public string CollapseModeAttribute => _state switch
    {
        SidebarState.Compact => "compact",
        SidebarState.IconOnly => "icon-only",
        SidebarState.Hidden => "hidden",
        _ => CollapseMode == SidebarCollapseMode.NonCollapsible
            ? "disabled"
            : CollapseMode switch
            {
                SidebarCollapseMode.IconOnly => "icon-only",
                SidebarCollapseMode.Hidden => "hidden",
                _ => "compact"
            }
    };
#pragma warning restore CS0618

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
    protected override string BuildCssStyle()
    {
        var customBreakpoint = !string.IsNullOrWhiteSpace(MobileBreakpoint)
            ? $"--cy-sidebar-mobile-breakpoint: {MobileBreakpoint}"
            : null;

        if (string.IsNullOrWhiteSpace(Style))
        {
            return customBreakpoint ?? string.Empty;
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
#pragma warning disable CS0618
        if (States.Count == 0 || CollapseMode == SidebarCollapseMode.NonCollapsible)
        {
            return;
        }
#pragma warning restore CS0618

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
    /// Moves one step towards the end of <see cref="States"/> (e.g. Expanded to Compact), without wrapping.
    /// </summary>
    public async Task NarrowAsync()
    {
#pragma warning disable CS0618
        if (CollapseMode == SidebarCollapseMode.NonCollapsible || !CanNarrow)
        {
            return;
        }
#pragma warning restore CS0618

        await ApplyStateChangeAsync(States[CurrentIndex + 1]);
    }

    /// <summary>
    /// Moves one step towards the start of <see cref="States"/> (e.g. IconOnly to Compact), without wrapping.
    /// </summary>
    public async Task WidenAsync()
    {
        if (!CanWiden)
        {
            return;
        }

        await ApplyStateChangeAsync(States[CurrentIndex - 1]);
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
#pragma warning disable CS0618
        if (CollapseMode == SidebarCollapseMode.NonCollapsible)
        {
            return;
        }
#pragma warning restore CS0618

        if (States.Count > 1)
        {
            await CycleNextAsync();
            return;
        }

#pragma warning disable CS0618
        var targetState = _state == SidebarState.Expanded
            ? ResolveCollapsedStateFromLegacyMode(CollapseMode)
            : SidebarState.Expanded;
#pragma warning restore CS0618

        await ApplyStateChangeAsync(targetState);
    }

    private async Task ApplyStateChangeAsync(SidebarState newState)
    {
        _state = newState;

#pragma warning disable CS0618
        Collapsed = EffectiveCollapsed;
        CollapseMode = MapStateToCollapseMode(newState);

        if (StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(_state);
        }

        if (CollapsedChanged.HasDelegate)
        {
            await CollapsedChanged.InvokeAsync(Collapsed);
        }
#pragma warning restore CS0618

        StateHasChanged();
    }

    private void SynchronizeCompatibilityParameters(ParameterView parameters)
    {
        var hasState = parameters.TryGetValue<SidebarState>(nameof(State), out _);

#pragma warning disable CS0618
        var hasCollapsed = parameters.TryGetValue<bool>(nameof(Collapsed), out _);
        var hasCollapseMode = parameters.TryGetValue<SidebarCollapseMode>(nameof(CollapseMode), out _);

        if (hasState)
        {
            Collapsed = EffectiveCollapsed;
            CollapseMode = MapStateToCollapseMode(_state);
            return;
        }

        if (hasCollapsed || hasCollapseMode)
        {
            if (CollapseMode == SidebarCollapseMode.NonCollapsible)
            {
                _state = SidebarState.Expanded;
                Collapsed = false;
                return;
            }

            _state = Collapsed
                ? ResolveCollapsedStateFromLegacyMode(CollapseMode)
                : SidebarState.Expanded;

            Collapsed = EffectiveCollapsed;
        }
#pragma warning restore CS0618
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
