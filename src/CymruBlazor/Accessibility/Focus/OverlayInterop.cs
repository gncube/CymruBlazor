using Microsoft.JSInterop;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Loads the library's on-demand overlay ES module
/// (<c>wwwroot/js/cymru-overlay.js</c>). The browser evaluates the module once,
/// so every importer shares the same module state (trap stack, focus history).
/// </summary>
internal static class OverlayInterop
{
    internal const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    internal static ValueTask<IJSObjectReference> ImportAsync(IJSRuntime js, CancellationToken cancellationToken = default)
        => js.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);

    /// <summary>True for the exceptions thrown when the browser or circuit is already gone.</summary>
    internal static bool IsTeardown(Exception exception)
        => exception is JSDisconnectedException or ObjectDisposedException or TaskCanceledException
            or OperationCanceledException or InvalidOperationException;
}
