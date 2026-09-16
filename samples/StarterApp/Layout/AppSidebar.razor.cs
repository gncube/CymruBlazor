using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Components.Layout;

namespace StarterApp.Layout;

public partial class AppSidebar : ComponentBase
{
    private const string StorageKey = "cymru_sidebar_expanded_menus";

    private CySidebar? _sidebar;
    private readonly HashSet<string> _expandedItemKeys = new(StringComparer.OrdinalIgnoreCase);
    private bool _hasLoadedFromStorage;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public bool MobileOpen { get; set; }

    [Parameter]
    public EventCallback<bool> MobileOpenChanged { get; set; }

    [Parameter]
    public EventCallback OnNavigate { get; set; }

    [Parameter]
    public string ActiveHref { get; set; } = "/";

    public async Task ToggleAsync()
    {
        if (_sidebar is not null)
        {
            await _sidebar.ToggleAsync();
        }
    }

    public async Task ToggleMobileAsync()
    {
        MobileOpen = !MobileOpen;
        await MobileOpenChanged.InvokeAsync(MobileOpen);
        StateHasChanged();
    }

    public async Task CloseMobileAsync()
    {
        if (MobileOpen)
        {
            MobileOpen = false;
            await MobileOpenChanged.InvokeAsync(false);
            StateHasChanged();
        }
    }

    private async Task HandleCloseMobileAsync()
    {
        await CloseMobileAsync();
    }

    private async Task HandleNavClickAsync()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync();
        }

        if (MobileOpen)
        {
            await CloseMobileAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadPersistedStateAsync();
            SyncSubmenuExpandedState();
            _hasLoadedFromStorage = true;
            StateHasChanged();
        }
    }

    protected override void OnParametersSet()
    {
        if (_hasLoadedFromStorage)
        {
            SyncSubmenuExpandedState();
        }
    }

    private void SyncSubmenuExpandedState()
    {
        if (string.IsNullOrWhiteSpace(ActiveHref))
        {
            return;
        }

        var normalizedActiveHref = NormalizeHref(ActiveHref);

        foreach (var section in _menuSections)
        {
            foreach (var item in section.Items)
            {
                if (!item.HasSubmenu || item.SubItems.Count == 0)
                {
                    continue;
                }

                var hasActiveChild = item.SubItems.Any(subItem =>
                    string.Equals(NormalizeHref(subItem.Href), normalizedActiveHref, StringComparison.OrdinalIgnoreCase));

                if (hasActiveChild || _expandedItemKeys.Contains(item.Key))
                {
                    item.IsExpanded = true;
                    _expandedItemKeys.Add(item.Key);
                }
            }
        }
    }

    private async Task ToggleSubmenuAsync(NavItemModel item)
    {
        if (!item.HasSubmenu)
        {
            return;
        }

        item.IsExpanded = !item.IsExpanded;

        if (item.IsExpanded)
        {
            _expandedItemKeys.Add(item.Key);
        }
        else
        {
            _expandedItemKeys.Remove(item.Key);
        }

        await PersistStateAsync();
    }

    private async Task LoadPersistedStateAsync()
    {
        try
        {
            var storedJson = await JsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(storedJson))
            {
                var keys = JsonSerializer.Deserialize<List<string>>(storedJson);
                if (keys is not null)
                {
                    foreach (var key in keys)
                    {
                        _expandedItemKeys.Add(key);
                    }
                }
            }
        }
        catch (JSException)
        {
            // Fallback gracefully when localStorage is inaccessible (e.g. strict privacy mode)
        }
    }

    private async Task PersistStateAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_expandedItemKeys);
            await JsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (JSException)
        {
            // Suppress non-critical storage quota or privacy restrictions
        }
    }

    private static string NormalizeHref(string href) =>
        href.TrimEnd('/').ToLowerInvariant();

    private readonly IReadOnlyList<NavSectionModel> _menuSections =
    [
        new("HOME",
        [
            new("Dashboard", "/", "dashboard")
        ]),
        new("PATIENTS",
        [
            new("Patient search", "/patients/search", "search"),
            new("Referrals", "/patients/referrals", "sort", BadgeText: "20", HasSubmenu: true,
                SubItems:
                [
                    new("Active Referrals", "/patients/referrals/active", "file-text"),
                    new("Archived Referrals", "/patients/referrals/archived", "archive")
                ]),
            new("Appointments", "/patients/appointments", "appointment", BadgeText: "20", HasSubmenu: true,
                SubItems:
                [
                    new("Today", "/patients/appointments/today", "appointment"),
                    new("Upcoming", "/patients/appointments/upcoming", "appointment")
                ]),
            new("Watchlists", "/patients/watchlists", "clipboard-list", HasSubmenu: true,
                SubItems:
                [
                    new("General Watchlist", "/patients/watchlists/general", "clipboard-list"),
                    new("Urgent Watchlist", "/patients/watchlists/urgent", "alert")
                ])
        ]),
        new("CLINICAL",
        [
            new("Specialists", "/clinical/specialists", "specialist", HasSubmenu: true,
                SubItems:
                [
                    new("Cardiology", "/clinical/specialists/cardiology", "heart-pulse"),
                    new("Orthopaedics", "/clinical/specialists/orthopaedics", "clinician")
                ]),
            new("Tests", "/clinical/tests", "flask", HasSubmenu: true,
                SubItems:
                [
                    new("Lab Results", "/clinical/tests/labs", "flask"),
                    new("Radiology", "/clinical/tests/radiology", "scan")
                ])
        ]),
        new("NURSING",
        [
            new("Adults", "/nursing/adults", "patient", HasSubmenu: true,
                SubItems:
                [
                    new("Wards", "/nursing/adults/wards", "ward"),
                    new("Handover", "/nursing/adults/handover", "ward-round")
                ]),
            new("Paediatrics", "/nursing/paediatrics", "carer", HasSubmenu: true,
                SubItems:
                [
                    new("PICU", "/nursing/paediatrics/picu", "critical"),
                    new("Day Unit", "/nursing/paediatrics/day-unit", "department")
                ])
        ]),
        new("URGENT & EMERGENCY",
        [
            new("Nursing", "/urgent/nursing", "bed", HasSubmenu: true,
                SubItems:
                [
                    new("Triage", "/urgent/nursing/triage", "urgent"),
                    new("Assessment", "/urgent/nursing/assessment", "activity")
                ]),
            new("Urgent & emergency", "/urgent/emergency", "critical", HasSubmenu: true,
                SubItems:
                [
                    new("Ambulance Intake", "/urgent/emergency/ambulance", "ambulance"),
                    new("Resus", "/urgent/emergency/resus", "critical")
                ])
        ])
    ];

    private static readonly IReadOnlyList<NavItemModel> FooterLinks =
    [
        new("Settings", "/settings/account", "settings"),
        new("Log Out", "/logout", "logout")
    ];

    public sealed record NavSectionModel(
        string Heading,
        IReadOnlyList<NavItemModel> Items);

    public sealed class NavItemModel(
        string label,
        string href,
        string iconName,
        string? BadgeText = null,
        bool HasSubmenu = false,
        bool isExpanded = false,
        IReadOnlyList<NavItemModel>? SubItems = null)
    {
        public string Label { get; } = label;
        public string Href { get; } = href;
        public string IconName { get; } = iconName;
        public string? BadgeText { get; } = BadgeText;
        public bool HasSubmenu { get; } = HasSubmenu;
        public bool IsExpanded { get; set; } = isExpanded;
        public IReadOnlyList<NavItemModel> SubItems { get; } = SubItems ?? [];
        public string Key => $"{Label}:{Href}";
    }
}
