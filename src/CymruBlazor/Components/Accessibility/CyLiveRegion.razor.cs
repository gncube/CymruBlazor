using Microsoft.AspNetCore.Components;
using Mediator;
using CymruBlazor.Enums;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// An accessibility component that dynamically announces content changes to screen readers using ARIA live regions.
/// </summary>
public partial class CyLiveRegion : CyComponentBase, INotificationHandler<LiveRegionAnnouncement>
{
    private ElementReference _elementRef;
    private string _activeMessage = string.Empty;

    [Inject]
    private IMediator MediatorInstance { get; set; } = default!;

    [Parameter]
    public LiveRegionPoliteness Politeness { get; set; } = LiveRegionPoliteness.Polite;

    [Parameter]
    public bool AriaAtomic { get; set; } = true;

    [Parameter]
    public string AriaRelevant { get; set; } = "additions text";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string BaseCssClass => "cy-live-region";

    protected string PolitenessString => Politeness switch
    {
        LiveRegionPoliteness.Assertive => "assertive",
        LiveRegionPoliteness.Off => "off",
        _ => "polite"
    };

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass("sr-only")
            .Build();

    /// <summary>
    /// Handles incoming accessibility announcements from the Mediator pipeline safely within the UI thread context.
    /// </summary>
    public async ValueTask Handle(LiveRegionAnnouncement notification, CancellationToken cancellationToken)
    {
        if (_activeMessage == notification.Message)
        {
            _activeMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
        }

        _activeMessage = notification.Message;
        Politeness = notification.Politeness;

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        // Lifetime managed transparently by the container framework
        GC.SuppressFinalize(this);
    }
}
