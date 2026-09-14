namespace StarterApp.Layout;

using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

public partial class AppSidebar : ComponentBase
{
    private CySidebar? _sidebar;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string ActiveHref { get; set; } = "/";

    public async Task ToggleAsync()
    {
        if (_sidebar is not null)
        {
            await _sidebar.ToggleAsync();
        }
    }

    protected override void OnInitialized()
    {
        SyncSubmenuExpandedState();
    }

    protected override void OnParametersSet()
    {
        SyncSubmenuExpandedState();
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
                if (item.HasSubmenu && item.SubItems.Count > 0)
                {
                    if (item.SubItems.Any(subItem => string.Equals(NormalizeHref(subItem.Href), normalizedActiveHref, StringComparison.OrdinalIgnoreCase)))
                    {
                        item.IsExpanded = true;
                    }
                }
            }
        }
    }

    private static string NormalizeHref(string href) =>
        href.TrimEnd('/').ToLowerInvariant();

    private void ToggleSubmenu(NavItemModel item)
    {
        if (item.HasSubmenu)
        {
            item.IsExpanded = !item.IsExpanded;
        }
    }

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

    public sealed class NavItemModel
    {
        public NavItemModel(
            string label,
            string href,
            string iconName,
            string? badgeText = null,
            bool hasSubmenu = false,
            bool isExpanded = false,
            IReadOnlyList<NavItemModel>? subItems = null)
        {
            Label = label;
            Href = href;
            IconName = iconName;
            BadgeText = badgeText;
            HasSubmenu = hasSubmenu;
            IsExpanded = isExpanded;
            SubItems = subItems ?? [];
        }

        public string Label { get; }
        public string Href { get; }
        public string IconName { get; }
        public string? BadgeText { get; }
        public bool HasSubmenu { get; }
        public bool IsExpanded { get; set; }
        public IReadOnlyList<NavItemModel> SubItems { get; }
    }
}
