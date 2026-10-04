using Microsoft.JSInterop;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Loads the on-demand ES module behind <c>CyMenu</c> and <c>CyToolbar</c>
/// (<c>wwwroot/js/cymru-editing.js</c>). Same pattern as <see cref="OverlayInterop"/>:
/// no script tag is needed by consumers.
/// </summary>
internal static class EditingInterop
{
    internal const string ModulePath = "./_content/CymruBlazor/js/cymru-editing.js";

    internal static ValueTask<IJSObjectReference> ImportAsync(IJSRuntime js, CancellationToken cancellationToken = default)
        => js.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);
}
