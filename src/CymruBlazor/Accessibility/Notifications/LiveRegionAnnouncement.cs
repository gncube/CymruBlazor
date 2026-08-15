using Mediator;
using CymruBlazor.Enums;

namespace CymruBlazor.Accessibility.Notifications;

/// <summary>
/// Immutable value object representing an accessibility screen-reader announcement request.
/// </summary>
public sealed record LiveRegionAnnouncement(
    string Message,
    LiveRegionPoliteness Politeness = LiveRegionPoliteness.Polite) : INotification;
