using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using StarterApp.Layout.Models;

namespace StarterApp.Layout;

public sealed partial class MobileNavMenu : ComponentBase, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public MobileNavVariant Variant { get; set; } = MobileNavVariant.ActionCenter;

    [Parameter]
    public string? ActiveHref { get; set; }

    [Parameter]
    public int? MessagesBadgeCount { get; set; }

    [Parameter]
    public int? DiaryBadgeCount { get; set; }

    [Parameter]
    public EventCallback OnMoreClick { get; set; }

    [Parameter]
    public EventCallback OnActionClick { get; set; }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += HandleLocationChanged;
        ActiveHref ??= NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        ActiveHref = NavigationManager.ToBaseRelativePath(e.Location);
        InvokeAsync(StateHasChanged);
    }

    private IReadOnlyList<MobileNavItemModel> GetNavigationItems() =>
        Variant switch
        {
            MobileNavVariant.ActionCenter =>
            [
                new("Home", "home", Href: "/"),
                new("Patients", ResolveIconName("patients"), Href: "/patients"),
                new("Search", "search", IsAction: true, ActionKey: "search"),
                new("Diary", ResolveIconName("diary"), Href: "/diary", BadgeCount: DiaryBadgeCount),
                new("More", ResolveIconName("more"), IsAction: true, ActionKey: "more")
            ],
            _ =>
            [
                new("Home", "home", Href: "/"),
                new("Diary", ResolveIconName("diary"), Href: "/diary", BadgeCount: DiaryBadgeCount),
                new("Patients", ResolveIconName("patients"), Href: "/patients"),
                new("Messages", ResolveIconName("messages"), Href: "/messages", BadgeCount: MessagesBadgeCount),
                new("More", ResolveIconName("more"), IsAction: true, ActionKey: "more")
            ]
        };

    private static string ResolveIconName(string role) =>
        role switch
        {
            // Maps navigation targets to names that actually exist in
            // IconRegistry.AllNames - "users", "calendar-days",
            // "more-horizontal" and "message-square" all threw
            // ArgumentException at render time because CymruBlazor's
            // registry uses these names instead.
            "patients" => "patient",
            "diary" => "appointment",
            "more" => "more",
            "messages" => "message",
            _ => "info"
        };

    private Task HandleActionClickAsync() =>
        OnActionClick.HasDelegate ? OnActionClick.InvokeAsync() : Task.CompletedTask;

    private Task HandleMoreClickAsync() =>
        OnMoreClick.HasDelegate ? OnMoreClick.InvokeAsync() : Task.CompletedTask;

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
