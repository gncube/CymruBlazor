using System.Collections.Concurrent;
using System.Diagnostics;
using Mediator;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// Default implementation of <see cref="IToastService"/> and Mediator handler for <see cref="ShowToastNotification"/>.
/// </summary>
public sealed class ToastService : IToastService, INotificationHandler<ShowToastNotification>, IDisposable
{
    private const int MaxToasts = 5;
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private readonly object _syncLock = new();
    private readonly List<ToastNotification> _toasts = [];
    private static readonly TimeSpan MinimumResume = TimeSpan.FromSeconds(1);

    private readonly ConcurrentDictionary<Guid, DismissTimer> _dismissTokens = new();
    private readonly TimeProvider _timeProvider;

    public ToastService() : this(TimeProvider.System)
    {
    }

    public ToastService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

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

    /// <inheritdoc />
    public void PauseAutoDismiss(Guid id)
    {
        if (!_dismissTokens.TryGetValue(id, out var timer))
        {
            return;
        }

        lock (timer)
        {
            if (timer.Timer is null)
            {
                return; // already paused
            }

            var elapsed = _timeProvider.GetElapsedTime(timer.StartedAt);
            timer.Remaining = timer.Remaining > elapsed ? timer.Remaining - elapsed : TimeSpan.Zero;
            CancelTimer(timer);
        }
    }

    /// <inheritdoc />
    public void ResumeAutoDismiss(Guid id)
    {
        if (!_dismissTokens.TryGetValue(id, out var timer))
        {
            return;
        }

        lock (timer)
        {
            if (timer.Timer is not null)
            {
                return; // not paused
            }

            StartTimer(id, timer, timer.Remaining < MinimumResume ? MinimumResume : timer.Remaining);
        }
    }

    private void ScheduleAutoDismiss(Guid id, TimeSpan delay)
    {
        var timer = new DismissTimer();
        _dismissTokens[id] = timer;

        lock (timer)
        {
            StartTimer(id, timer, delay);
        }
    }

    private void StartTimer(Guid id, DismissTimer timer, TimeSpan delay)
    {
        timer.Remaining = delay;
        timer.StartedAt = _timeProvider.GetTimestamp();
        timer.Timer = _timeProvider.CreateTimer(state => Remove((Guid)state!), id, delay, Timeout.InfiniteTimeSpan);
    }

    private static void CancelTimer(DismissTimer timer)
    {
        var t = timer.Timer;
        timer.Timer = null;
        t?.Dispose();
    }

    private void CancelDismissTimer(Guid id)
    {
        if (_dismissTokens.TryRemove(id, out var timer))
        {
            lock (timer)
            {
                CancelTimer(timer);
            }
        }
    }

    private sealed class DismissTimer
    {
        public ITimer? Timer { get; set; }

        public TimeSpan Remaining { get; set; }

        public long StartedAt { get; set; }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            foreach (var timer in _dismissTokens.Values)
            {
                lock (timer)
                {
                    CancelTimer(timer);
                }
            }
            _dismissTokens.Clear();
            _toasts.Clear();
        }
    }
}
