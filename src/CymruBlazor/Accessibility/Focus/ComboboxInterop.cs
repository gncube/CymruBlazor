using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Location of the on-demand ES module behind <c>CyCombobox</c> and <c>CyMultiCombobox</c>
/// (<c>wwwroot/js/cymru-inputs.js</c>). Same pattern as <see cref="EditingInterop"/>: no script tag is needed
/// by consumers, and the module is only fetched when a combobox renders.
/// </summary>
internal static class ComboboxInterop
{
    internal const string ModulePath = "./_content/CymruBlazor/js/cymru-inputs.js";

    internal static ValueTask<IJSObjectReference> ImportAsync(IJSRuntime js, CancellationToken cancellationToken = default)
        => js.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);
}

/// <summary>
/// Owns the module reference and attachment token of one combobox. Every failure is swallowed (prerendering,
/// a disconnected circuit): the combobox still works without the script, minus the key defaults it prevents.
/// </summary>
internal sealed class CyComboboxScript : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private int _token;

    internal async Task AttachAsync(IJSRuntime js, ElementReference input)
    {
        if (_token != 0)
        {
            return;
        }

        try
        {
            _module ??= await ComboboxInterop.ImportAsync(js);
            _token = await _module.InvokeAsync<int>("attachCombobox", input);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Works without the script.
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module is not null)
            {
                if (_token != 0)
                {
                    await _module.InvokeVoidAsync("detachCombobox", _token);
                    _token = 0;
                }

                await _module.DisposeAsync();
                _module = null;
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or ObjectDisposedException or InvalidOperationException)
        {
            // Browser or circuit is already gone.
        }
    }
}
