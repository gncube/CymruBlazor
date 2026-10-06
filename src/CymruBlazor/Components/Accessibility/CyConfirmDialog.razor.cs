using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// The host for <see cref="ICyConfirmService"/>: place one in your layout and every
/// <c>ConfirmAsync(...)</c> call is shown in a <see cref="CyDialog"/>.
/// </summary>
/// <remarks>
/// It adds no markup of its own while nothing is pending. The dialog is the library's native-<c>dialog</c> modal
/// (ADR-0001), so the page behind is inert, Tab stays inside, Escape closes it and focus returns to the control that
/// opened it. Escape, the close button and the cancel button all answer false; only the confirm button answers true.
/// </remarks>
public partial class CyConfirmDialog : ComponentBase, IDisposable
{
    private static readonly CyConfirmText s_defaultText = new();

    private IDisposable? _hostHandle;
    private CyConfirmRequest? _request;
    private bool _open;
    private bool _result;

    /// <summary>The default button text. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyConfirmText? Text { get; set; }

    private CyConfirmText Strings => Text ?? s_defaultText;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _hostHandle = Service.AttachHost();
        Service.Changed += OnServiceChanged;
        Sync();
    }

    private void OnServiceChanged() => _ = InvokeAsync(() =>
    {
        Sync();
        StateHasChanged();
    });

    private void Sync()
    {
        var next = Service.Current;

        if (!ReferenceEquals(next, _request))
        {
            _request = next;
            _open = next is not null;
            _result = false;
        }
    }

    private void Choose(bool result)
    {
        _result = result;
        _open = false;
    }

    private Task CompleteAsync(CyConfirmRequest request)
    {
        Service.Complete(request, _result);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        try
        {
            if (_hostHandle is not null)
            {
                _hostHandle.Dispose();
                _hostHandle = null;
            }
        }
        finally
        {
            Service.Changed -= OnServiceChanged;
            _request = null;
            _open = false;
            _result = false;
        }
    }
}
