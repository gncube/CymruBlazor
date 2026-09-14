namespace CymruBlazor.Components.Feedback;

using System.Collections.Concurrent;
using Mediator;

/// <summary>
/// Default implementation of <see cref="IToastService"/> and Mediator handler for <see cref="ShowToastNotification"/>.
/// </summary>
public sealed class ToastService : IToastService, INotificationHandler<ShowToastNotification>, IDisposable
{
    private const int MaxToasts = 5;
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private readonly object _syncLock = new();
    private readonly List<ToastNotification> _toasts = [];
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _dismissTokens = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastNotification> Toasts
    {
        get
        {
            lock (_syncLock)
            {
                return [.. _toasts];
            }
        }
    }

    public void Show(string message, ToastVariant variant = ToastVariant.Success, TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Toast message cannot be null, empty, or whitespace.", nameof(message));
        }

        var effectiveDuration = duration ?? DefaultDuration;

        if (effectiveDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be greater than or equal to TimeSpan.Zero.");
        }

        var notification = new ToastNotification(Guid.NewGuid(), message.Trim(), variant, effectiveDuration);

        lock (_syncLock)
        {
            while (_toasts.Count >= MaxToasts)
            {
                var oldest = _toasts[0];
                CancelDismissTimer(oldest.Id);
                _toasts.RemoveAt(0);
            }

            _toasts.Add(notification);
        }

        OnChange?.Invoke();

        if (effectiveDuration > TimeSpan.Zero)
        {
            ScheduleAutoDismiss(notification.Id, effectiveDuration);
        }
    }

    public void Remove(Guid id)
    {
        var removed = false;

        lock (_syncLock)
        {
            var index = _toasts.FindIndex(t => t.Id == id);
            if (index >= 0)
            {
                _toasts.RemoveAt(index);
                removed = true;
            }
        }

        CancelDismissTimer(id);

        if (removed)
        {
            OnChange?.Invoke();
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            foreach (var toast in _toasts)
            {
                CancelDismissTimer(toast.Id);
            }
            _toasts.Clear();
        }

        OnChange?.Invoke();
    }

    public ValueTask Handle(ShowToastNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        Show(notification.Message, notification.Variant, notification.Duration);
        return ValueTask.CompletedTask;
    }

    private void ScheduleAutoDismiss(Guid id, TimeSpan delay)
    {
        var cts = new CancellationTokenSource();
        _dismissTokens[id] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cts.Token);
                Remove(id);
            }
            catch (OperationCanceledException)
            {
                // Task cancellation expected on explicit dismissal or disposal
            }
        });
    }

    private void CancelDismissTimer(Guid id)
    {
        if (_dismissTokens.TryRemove(id, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            foreach (var cts in _dismissTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _dismissTokens.Clear();
            _toasts.Clear();
        }
    }
}
