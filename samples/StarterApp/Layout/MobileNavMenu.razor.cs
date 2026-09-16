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
                new("Patients", "user", Href: "/patients"),
                new("Search", "search", IsAction: true, ActionKey: "search"),
                new("Diary", "calendar", Href: "/diary", BadgeCount: DiaryBadgeCount),
                new("More", "dots-horizontal", IsAction: true, ActionKey: "more")
            ],
            _ =>
            [
                new("Home", "home", Href: "/"),
                new("Diary", "calendar", Href: "/diary", BadgeCount: DiaryBadgeCount),
                new("Patients", "user", Href: "/patients"),
                new("Messages", "message-square", Href: "/messages", BadgeCount: MessagesBadgeCount),
                new("More", "dots-horizontal", IsAction: true, ActionKey: "more")
            ]
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
