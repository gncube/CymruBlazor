using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Layout;
using CymruBlazor.Contracts;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// A modal dialog built on the native <c>&lt;dialog&gt;</c> element.
/// </summary>
/// <remarks>
/// <para>
/// Opening calls <c>showModal()</c>, so the browser itself provides what a
/// hand-rolled modal has to imitate: the rest of the page becomes inert,
/// <c>Tab</c> stays inside the dialog, <c>Escape</c> closes it and it is drawn
/// in the top layer above everything (z-index is irrelevant). A small,
/// on-demand JavaScript module (<c>cymru-overlay.js</c>, no script tag needed)
/// opens the dialog, reports closes back to .NET and returns focus to the
/// element that opened it.
/// </para>
/// <para>
/// The dialog is named by <see cref="Title"/> (<c>aria-labelledby</c>) and
/// described by <see cref="Description"/> (<c>aria-describedby</c>). Body content is
/// only rendered while the dialog is open. Put <c>autofocus</c> on the control
/// that should receive focus; otherwise the browser focuses the first focusable
/// element (the close button when <see cref="Dismissible"/>).
/// </para>
/// <para>
/// Open state is two-way bindable: <c>@bind-Open="_open"</c>. Set it to
/// <see langword="true"/> from a button's click handler so focus can return to
/// that button.
/// </para>
/// </remarks>
public partial class CyDialog : CyLayoutComponentBase, IHasSize, IAsyncDisposable
{
    private ElementReference _dialog;
    private DotNetObjectReference<CyDialog>? _selfReference;
    private IJSObjectReference? _module;
    private int _token;
    private bool _closedRaised;
    private bool _lastDismissible = true;
    private bool _lastCloseOnBackdropClick;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>Whether the dialog is open. Two-way bindable.</summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>Raised when <see cref="Open"/> changes because the user closed the dialog.</summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Raised once each time the dialog has closed, whatever the cause (close button,
    /// <c>Escape</c>, backdrop click, a footer action or the parent setting <see cref="Open"/> to false).
    /// </summary>
    [Parameter]
    public EventCallback OnClosed { get; set; }

    /// <summary>The dialog's heading and accessible name.</summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>Optional plain-text description shown under the heading and used as the accessible description.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Optional actions (typically <c>CyButton</c>s) shown in the footer.</summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// When true (the default) the dialog shows a close button and closes on <c>Escape</c>. When false, the
    /// dialog can only be closed by your own actions (for example a footer button setting <see cref="Open"/>
    /// to false). Note that browsers may still close a dialog on a repeated <c>Escape</c> press.
    /// </summary>
    [Parameter]
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// When true, clicking the backdrop closes the dialog (only if <see cref="Dismissible"/>). Defaults to false
    /// so an accidental click cannot discard work.
    /// </summary>
    [Parameter]
    public bool CloseOnBackdropClick { get; set; }

    /// <summary>
    /// The dialog width: <see cref="ComponentSize.Small"/>, <see cref="ComponentSize.Medium"/> (the default,
    /// also used for <see cref="ComponentSize.Unspecified"/>) or <see cref="ComponentSize.Large"/>.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>Accessible name of the close button. Defaults to the English "Close".</summary>
    [Parameter]
    public string? CloseLabel { get; set; }

    protected override string BaseCssClass => "cy-dialog";

    private string TitleId => $"{Id}-title";

    private string DescriptionId => $"{Id}-description";

    private bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    protected override string BuildCssClass()
    {
        var sizeSuffix = Size switch
        {
            ComponentSize.Small => "sm",
            ComponentSize.Large => "lg",
            _ => "md"
        };

        return Components.Core.CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-dialog--{sizeSuffix}")
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Size is ComponentSize.ExtraSmall or ComponentSize.ExtraLarge)
        {
            throw new InvalidOperationException(
                $"{nameof(CyDialog)}.{nameof(Size)} must be Small, Medium or Large. Received '{Size}'.");
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException($"{nameof(CyDialog)}.{nameof(Title)} must not be empty: it names the dialog.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (Open && _token == 0)
            {
                await ShowAsync();
            }
            else if (!Open && _token != 0)
            {
                await ReleaseAsync();
                await RaiseClosedAsync();
            }
            else if (Open && _token != 0 && (_lastDismissible != Dismissible || _lastCloseOnBackdropClick != CloseOnBackdropClick))
            {
                await PushOptionsAsync();
            }
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // Browser or circuit is gone; nothing to show or release.
        }
    }

    /// <summary>Called by the overlay module when the native dialog has closed. Not for application code.</summary>
    [JSInvokable]
    public async Task OnNativeClosed()
    {
        if (!Open)
        {
            return;
        }

        Open = false;

        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(false);
        }

        await RaiseClosedAsync();

        StateHasChanged();
    }

    private async Task CloseFromUiAsync()
    {
        if (!Open)
        {
            return;
        }

        Open = false;

        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(false);
        }

        await RaiseClosedAsync();
    }

    private async Task RaiseClosedAsync()
    {
        if (_closedRaised)
        {
            return;
        }

        _closedRaised = true;

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }
    }

    private async Task ShowAsync()
    {
        _module ??= await OverlayInterop.ImportAsync(JSRuntime);
        _selfReference ??= DotNetObjectReference.Create(this);
        _closedRaised = false;

        _token = await _module.InvokeAsync<int>(
            "showDialog",
            _dialog,
            _selfReference,
            new { dismissible = Dismissible, closeOnBackdropClick = CloseOnBackdropClick });

        _lastDismissible = Dismissible;
        _lastCloseOnBackdropClick = CloseOnBackdropClick;
    }

    private async Task PushOptionsAsync()
    {
        _lastDismissible = Dismissible;
        _lastCloseOnBackdropClick = CloseOnBackdropClick;

        if (_module is not null)
        {
            await _module.InvokeVoidAsync(
                "updateDialog",
                _token,
                new { dismissible = Dismissible, closeOnBackdropClick = CloseOnBackdropClick });
        }
    }

    private async Task ReleaseAsync()
    {
        var token = _token;
        _token = 0;

        if (_module is not null && token != 0)
        {
            await _module.InvokeVoidAsync("disposeDialog", token);
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        try
        {
            await ReleaseAsync();

            if (_module is not null)
            {
                await _module.DisposeAsync();
                _module = null;
            }
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // Browser or circuit is already gone.
        }

        _selfReference?.Dispose();
        _selfReference = null;
    }
}
