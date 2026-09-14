using Microsoft.AspNetCore.Components;
using CymruBlazor.Enums;
using CymruBlazor.Accessibility;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// An accessibility component that dynamically announces content changes to screen readers using ARIA live regions.
///
/// Deliberately does <em>not</em> implement Mediator's
/// <c>INotificationHandler&lt;LiveRegionAnnouncement&gt;</c> itself - doing
/// so would cause Mediator's DI-based dispatch to resolve a separate,
/// disconnected instance of this component from the container (never
/// attached to the render tree, so it has no render handle) instead of
/// calling this live instance. Instead it registers itself with
/// <see cref="ILiveRegionRegistry"/>, which the real handler
/// (<see cref="LiveRegionAnnouncementHandler"/>) uses to forward
/// announcements to whichever instance(s) are actually rendered.
/// </summary>
public partial class CyLiveRegion : CyComponentBase
{
    private ElementReference _elementRef;
    private string _activeMessage = string.Empty;

    [Inject]
    private ILiveRegionRegistry Registry { get; set; } = default!;

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
            .AddClass("cy-visually-hidden")
            .AddClass("u-sr-only")
            .Build();

    /// <inheritdoc />
    protected override void OnInitialized() => Registry.Register(this);

    /// <summary>
    /// Handles incoming accessibility announcements, forwarded via
    /// <see cref="ILiveRegionRegistry"/> from the Mediator pipeline,
    /// safely within the UI thread context.
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
        Registry.Unregister(this);
        GC.SuppressFinalize(this);
    }
}
