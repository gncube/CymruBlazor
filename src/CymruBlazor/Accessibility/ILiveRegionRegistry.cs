using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Accessibility;

namespace CymruBlazor.Accessibility;

/// <summary>
/// Tracks the currently rendered <see cref="CyLiveRegion"/> component
/// instance(s) so that accessibility announcements published through the
/// Mediator pipeline can be forwarded to the real, render-tree-attached
/// component rather than a separate instance resolved directly from the
/// DI container.
///
/// This indirection exists because <c>Mediator</c>'s source generator
/// discovers <see cref="Mediator.INotificationHandler{TNotification}"/>
/// implementations and resolves them from the DI container. A Razor
/// component can never safely implement that interface itself: DI would
/// construct an entirely separate instance to handle the notification,
/// one that was never attached to the render tree and therefore has no
/// render handle assigned, causing an
/// <see cref="InvalidOperationException"/> the moment it tries to call
/// <c>InvokeAsync</c>/<c>StateHasChanged</c>. Routing through a plain,
/// non-component registry service avoids that entirely.
/// </summary>
public interface ILiveRegionRegistry
{
    /// <summary>
    /// Registers a live <see cref="CyLiveRegion"/> instance to receive
    /// forwarded announcements. Call from <c>OnInitialized</c>.
    /// </summary>
    void Register(CyLiveRegion liveRegion);

    /// <summary>
    /// Removes a previously registered instance. Call from <c>Dispose</c>.
    /// </summary>
    void Unregister(CyLiveRegion liveRegion);

    /// <summary>
    /// Forwards an announcement to every currently registered, live
    /// <see cref="CyLiveRegion"/> instance.
    /// </summary>
    ValueTask PublishAsync(LiveRegionAnnouncement notification, CancellationToken cancellationToken);
}
