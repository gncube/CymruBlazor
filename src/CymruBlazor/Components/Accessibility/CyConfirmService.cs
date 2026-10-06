namespace CymruBlazor.Components.Accessibility;

/// <summary>One pending question. Internal: the host and the service exchange it.</summary>
internal sealed class CyConfirmRequest(CyConfirmOptions options)
{
    private readonly TaskCompletionSource<bool> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public CyConfirmOptions Options { get; } = options;

    public Task<bool> Task => _completion.Task;

    public CancellationTokenRegistration Registration { get; set; }

    public bool TryComplete(bool result) => _completion.TrySetResult(result);
}

/// <summary>
/// Default <see cref="ICyConfirmService"/>. Registered scoped by <c>AddCymruBlazor()</c>; it is the object the
/// <see cref="CyConfirmDialog"/> host and the callers share.
/// </summary>
public sealed class CyConfirmService : ICyConfirmService
{
    private readonly object _gate = new();
    private readonly List<CyConfirmRequest> _queue = [];
    private int _hosts;

    internal event Action? Changed;

    /// <summary>The request being shown, or null.</summary>
    internal CyConfirmRequest? Current
    {
        get
        {
            lock (_gate)
            {
                return _queue.Count > 0 ? _queue[0] : null;
            }
        }
    }

    /// <inheritdoc />
    public Task<bool> ConfirmAsync(CyConfirmOptions options, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"ConfirmAsync start: title={options.Title}, hostCount={_hosts}, queue={_queue.Count}");
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            throw new ArgumentException("A confirmation needs a title: it names the dialog.", nameof(options));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(false);
        }

        var request = new CyConfirmRequest(options);

        lock (_gate)
        {
            if (_hosts == 0)
            {
                throw new InvalidOperationException(
                    $"No {nameof(CyConfirmDialog)} is on the page. Add <{nameof(CyConfirmDialog)} /> once to your layout " +
                    $"(next to the toast container) before calling {nameof(ICyConfirmService)}.{nameof(ConfirmAsync)}.");
            }

            _queue.Add(request);
        }

        if (cancellationToken.CanBeCanceled)
        {
            request.Registration = cancellationToken.Register(() => Complete(request, false));
        }

        Changed?.Invoke();

        return request.Task;
    }

    /// <summary>Resolves a request. Safe to call more than once; only the first call counts.</summary>
    internal void Complete(CyConfirmRequest request, bool result)
    {
        Console.WriteLine($"Complete request: result={result}, queueBefore={_queue.Count}");
        if (!request.TryComplete(result))
        {
            Console.WriteLine($"Complete ignored because already completed");
            return;
        }

        lock (_gate)
        {
            _queue.Remove(request);
        }

        request.Registration.Dispose();
        Changed?.Invoke();
    }

    /// <summary>Called by each mounted host. Disposing the returned handle resolves anything still pending as false.</summary>
    internal IDisposable AttachHost()
    {
        lock (_gate)
        {
            _hosts++;
        }

        return new HostHandle(this);
    }

    private void DetachHost()
    {
        List<CyConfirmRequest> orphaned = [];

        lock (_gate)
        {
            if (_hosts > 0)
            {
                _hosts--;
            }

            if (_hosts == 0)
            {
                orphaned.AddRange(_queue);
                _queue.Clear();
            }
        }

        foreach (var request in orphaned)
        {
            Complete(request, false);
        }
    }

    private sealed class HostHandle(CyConfirmService owner) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                owner.DetachHost();
            }
        }
    }
}
