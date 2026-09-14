
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

namespace StarterApp.Layout;
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

    private static readonly IReadOnlyList<NavSectionModel> MenuSections =
    [
        new("Home",
        [
            new("Dashboard", "/", "dashboard")
        ]),
        new("Patients",
        [
            new("Patient search", "/patients/search", "search"),
            new("Referrals", "/patients/referrals", "sort", BadgeText: "20", HasSubmenu: true),
            new("Appointments", "/patients/appointments", "appointment", BadgeText: "20", HasSubmenu: true),
            new("Watchlists", "/patients/watchlists", "clipboard-list", HasSubmenu: true)
        ]),
        new("Clinical",
        [
            new("Specialists", "/clinical/specialists", "specialist", HasSubmenu: true),
            new("Tests", "/clinical/tests", "flask", HasSubmenu: true)
        ]),
        new("Nursing",
        [
            new("Adults", "/nursing/adults", "patient", HasSubmenu: true),
            new("Paediatrics", "/nursing/paediatrics", "carer", HasSubmenu: true)
        ]),
        new("Urgent & Emergency",
        [
            new("Nursing", "/urgent/nursing", "bed", HasSubmenu: true),
            new("Urgent & emergency", "/urgent/emergency", "critical", HasSubmenu: true)
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

    public sealed record NavItemModel(
        string Label,
        string Href,
        string IconName,
        string? BadgeText = null,
        bool HasSubmenu = false);
}
