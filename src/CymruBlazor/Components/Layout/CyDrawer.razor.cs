using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Contracts;
using CymruBlazor.Enums;
using CymruBlazor.Localisation;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A panel that slides in from the left or right edge: an inspector, an "add question" panel or a filter
/// pane that keeps the page behind it in view. A <see cref="CyDialog"/> is centred and always blocks the page;
/// a drawer is for editing in context.
/// </summary>
/// <remarks>
/// <para>
/// <b>Modal (the default).</b> Built on the native <c>&lt;dialog&gt;</c> element opened with
/// <c>showModal()</c>, for the reasons in ADR-0001: the browser makes the page behind inert, contains
/// <c>Tab</c>, closes on <c>Escape</c> and draws the drawer in the top layer. The shared overlay module
/// (<c>cymru-overlay.js</c>) reports closes back to .NET and returns focus to the control that opened it.
/// The backdrop dims the page; <see cref="CloseOnBackdropClick"/> lets a click on it close the drawer.
/// </para>
/// <para>
/// <b>Non-modal (<see cref="Modal"/> = false).</b> Renders an <c>&lt;aside&gt;</c> beside the content with no
/// backdrop and no focus containment, so the page stays usable. Focus moves into the drawer when it opens,
/// <c>Escape</c> (while focus is inside) closes it, and focus returns to the opener. Changing
/// <see cref="Modal"/> while the drawer is open takes effect the next time it opens.
/// </para>
/// <para>
/// Named by <see cref="Title"/> (<c>aria-labelledby</c>). The body is only rendered while open, so closed drawers
/// cost nothing and cannot duplicate ids. Open state is bindable (<c>@bind-Open</c>); set it from a button's
/// click handler so focus can return to that button.
/// </para>
/// </remarks>
public partial class CyDrawer : CyLayoutComponentBase, IHasSize, IAsyncDisposable
{
    private ElementReference _dialog;
    private DotNetObjectReference<CyDrawer>? _selfReference;
    private IJSObjectReference? _module;
    private int _token;
    private bool _closedRaised;
    private bool _focusEntered;
    private bool _lastDismissible = true;
    private bool _lastCloseOnBackdropClick;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IFocusManager FocusManager { get; set; } = default!;

    /// <summary>Whether the drawer is open. Two-way bindable.</summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>Raised when <see cref="Open"/> changes because the user closed the drawer.</summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Raised once each time the drawer has closed, whatever the cause (close button, <c>Escape</c>,
    /// backdrop click, or the parent setting <see cref="Open"/> to false).
    /// </summary>
    [Parameter]
    public EventCallback OnClosed { get; set; }

    /// <summary>The drawer's heading and accessible name. Required.</summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>The heading level of <see cref="Title"/>, 1 to 6. Defaults to 2.</summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    /// <summary>The edge the drawer slides in from. Defaults to <see cref="DrawerSide.Right"/>.</summary>
    [Parameter]
    public DrawerSide Side { get; set; } = DrawerSide.Right;

    /// <summary>
    /// When true (the default) the drawer is modal: backdrop, inert page, contained focus. When false it is
    /// a non-modal side panel. See the class remarks.
    /// </summary>
    [Parameter]
    public bool Modal { get; set; } = true;

    /// <summary>Optional actions shown in a footer (typically <c>CyButton</c>s).</summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// When true (the default) the drawer shows a close button and closes on <c>Escape</c>. When false, only
    /// your own actions can close it.
    /// </summary>
    [Parameter]
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// For a modal drawer, whether clicking the backdrop closes it (only if <see cref="Dismissible"/>).
    /// Defaults to false so an accidental click cannot discard work.
    /// </summary>
    [Parameter]
    public bool CloseOnBackdropClick { get; set; }

    /// <summary>The drawer width: Small, Medium (the default) or Large. Extra sizes clamp to the nearest.</summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>Accessible name of the close button. Defaults to the English "Close".</summary>
    [Parameter]
    public string? CloseLabel { get; set; }

    /// <summary>The cascaded localiser (ADR-0002), if any. See <see cref="CloseLabel"/>.</summary>
    [CascadingParameter]
    public ICyLocalizer? Localizer { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-drawer";

    private string TitleId => $"{Id}-title";

    /// <inheritdoc />
    protected override string BuildCssClass()
    {
        var sizeSuffix = Size switch
        {
            ComponentSize.ExtraSmall or ComponentSize.Small => "sm",
            ComponentSize.ExtraLarge or ComponentSize.Large => "lg",
            _ => "md"
        };

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Side == DrawerSide.Left ? "cy-drawer--left" : "cy-drawer--right")
            .AddClass($"cy-drawer--{sizeSuffix}")
            .AddClass(Modal ? "cy-drawer--modal" : "cy-drawer--inline")
            .AddClass(Class)
            .Build();
    }

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                $"{nameof(CyDrawer)}.{nameof(Title)} must not be empty: it names the drawer.");
        }

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyDrawer)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (Modal)
            {
                await SyncModalAsync();
            }
            else
            {
                await SyncInlineAsync();
            }
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // Browser or circuit is gone; nothing to show or release.
        }
    }

    private async Task SyncModalAsync()
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

    private async Task SyncInlineAsync()
    {
        if (Open && !_focusEntered)
        {
            _focusEntered = true;
            _closedRaised = false;
            await FocusManager.FocusAsync(Id, new FocusOptions(true, true));
        }
        else if (!Open && _focusEntered)
        {
            _focusEntered = false;
            await FocusManager.RestoreFocusAsync();
            await RaiseClosedAsync();
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

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && Dismissible)
        {
            await CloseFromUiAsync();
        }
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

        // Modal: OnClosed is raised once the native dialog confirms the close. Inline: after focus returns.
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

    /// <inheritdoc />
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
