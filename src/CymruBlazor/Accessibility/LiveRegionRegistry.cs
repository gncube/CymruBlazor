using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Accessibility;

namespace CymruBlazor.Accessibility;

/// <inheritdoc cref="ILiveRegionRegistry" />
public sealed class LiveRegionRegistry : ILiveRegionRegistry
{
    private readonly List<CyLiveRegion> _instances = [];
    private readonly Lock _sync = new();

    /// <inheritdoc />
    public void Register(CyLiveRegion liveRegion)
    {
        ArgumentNullException.ThrowIfNull(liveRegion, nameof(liveRegion));

        lock (_sync)
        {
            _instances.Add(liveRegion);
        }
    }

    /// <inheritdoc />
    public void Unregister(CyLiveRegion liveRegion)
    {
        ArgumentNullException.ThrowIfNull(liveRegion, nameof(liveRegion));

        lock (_sync)
        {
            _instances.Remove(liveRegion);
        }
    }

    /// <inheritdoc />
    public async ValueTask PublishAsync(LiveRegionAnnouncement notification, CancellationToken cancellationToken)
    {
        CyLiveRegion[] snapshot;

        lock (_sync)
        {
            snapshot = [.. _instances];
        }

        // Forward to every live, render-tree-attached instance. Each one
        // marshals the update onto its own renderer dispatcher internally.
        foreach (var instance in snapshot)
        {
            await instance.Handle(notification, cancellationToken);
        }
    }
}
