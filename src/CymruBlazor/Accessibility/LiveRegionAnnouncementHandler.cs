using Mediator;
using CymruBlazor.Accessibility.Notifications;

namespace CymruBlazor.Accessibility;

/// <summary>
/// The actual <see cref="INotificationHandler{TNotification}"/> resolved
/// by Mediator's DI-based dispatch for <see cref="LiveRegionAnnouncement"/>.
///
/// This is a plain, non-component class deliberately: it simply forwards
/// to <see cref="ILiveRegionRegistry"/>, which relays the announcement to
/// whichever <see cref="Components.Accessibility.CyLiveRegion"/>
/// component(s) are actually live in the render tree. See
/// <see cref="ILiveRegionRegistry"/> for why the component itself must
/// not implement this interface directly.
/// </summary>
public sealed class LiveRegionAnnouncementHandler(ILiveRegionRegistry registry) : INotificationHandler<LiveRegionAnnouncement>
{
    /// <inheritdoc />
    public ValueTask Handle(LiveRegionAnnouncement notification, CancellationToken cancellationToken) =>
        registry.PublishAsync(notification, cancellationToken);
}
