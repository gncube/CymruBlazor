using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A theme-aware user navigation and profile component with unauthenticated action trigger
/// and authenticated user avatar pill with dropdown menu, implementing the WAI-ARIA menu button pattern.
/// </summary>
public partial class CyUserNav : CyLayoutComponentBase, IAsyncDisposable
{
    private ElementReference _root;
    private ElementReference _trigger;
    private ElementReference _menu;
    private DotNetObjectReference<CyUserNav>? _selfReference;
    private IJSObjectReference? _module;
    private int _token;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether the user is currently authenticated.
    /// When <see langword="false"/>, an unauthenticated action link or button is rendered.
    /// When <see langword="true"/>, the authenticated user pill with avatar, name, and menu is rendered.
    /// </summary>
    [Parameter]
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the display name of the authenticated user.
    /// </summary>
    [Parameter]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the name of the authenticated user (alias for <see cref="DisplayName"/>).
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address or username shown in the dropdown profile header card.
    /// </summary>
    [Parameter]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's avatar image.
    /// </summary>
    [Parameter]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the presence or activity status of the user. Defaults to <see cref="AvatarStatus.None"/>.
    /// </summary>
    [Parameter]
    public AvatarStatus Status { get; set; } = AvatarStatus.None;

    /// <summary>
    /// Gets or sets the tint or colour variant for the user avatar. Defaults to <see cref="ComponentColour.Unspecified"/>.
    /// </summary>
    [Parameter]
    public ComponentColour AvatarColour { get; set; } = ComponentColour.Unspecified;

    /// <summary>
    /// Gets or sets the visible label for the unauthenticated state trigger. Defaults to "Log in".
    /// </summary>
    [Parameter]
    public string LoginText { get; set; } = "Log in";

    /// <summary>
    /// Gets or sets the target URL for the unauthenticated state link. When null, a button is rendered.
    /// </summary>
    [Parameter]
    public string? LoginHref { get; set; }

    /// <summary>
    /// Gets or sets the event callback invoked when the unauthenticated login trigger is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnLogin { get; set; }

    /// <summary>
    /// Gets or sets navigation items or links rendered inside the dropdown menu.
    /// </summary>
    [Parameter]
    public RenderFragment? MenuItems { get; set; }

    /// <summary>
    /// Gets or sets custom content for the logout / sign-out section at the bottom of the dropdown menu.
    /// </summary>
    [Parameter]
    public RenderFragment? LogoutContent { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the default logout button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnLogout { get; set; }

    /// <summary>
    /// Gets or sets the label for the default logout button. Defaults to "Log out".
    /// </summary>
    [Parameter]
    public string LogoutText { get; set; } = "Log out";

    /// <summary>
    /// Gets or sets custom content for the profile header card inside the dropdown menu.
    /// </summary>
    [Parameter]
    public RenderFragment? ProfileHeader { get; set; }

    /// <summary>
    /// Gets or sets whether the dropdown menu is open. Supports two-way binding (<c>@bind-Open</c>).
    /// </summary>
    /// <summary>
    /// Gets or sets localised phrases for the component. When null, English defaults are used.
    /// </summary>
    [Parameter]
    public CyUserNavText? Text { get; set; }

    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// Event raised when <see cref="Open"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets or sets the accessible label for the authenticated menu trigger button.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-user-nav";

    private string TriggerId => $"{Id}-trigger";

    private string MenuId => $"{Id}-menu";

    private string? EffectiveName =>
        !string.IsNullOrWhiteSpace(DisplayName)
            ? DisplayName
            : !string.IsNullOrWhiteSpace(Name)
                ? Name
                : Email;

    private static readonly CyUserNavText DefaultText = new();

    private CyUserNavText EffectiveText => Text ?? DefaultText;

    private string EffectiveLoginText => !string.IsNullOrWhiteSpace(LoginText) && LoginText != "Log in" ? LoginText : EffectiveText.Login;

    private string EffectiveLogoutText => !string.IsNullOrWhiteSpace(LogoutText) && LogoutText != "Log out" ? LogoutText : EffectiveText.Logout;

    private string? TriggerAriaLabel =>
        !string.IsNullOrWhiteSpace(AriaLabel)
            ? AriaLabel
            : (string.IsNullOrWhiteSpace(EffectiveName) ? EffectiveText.UserMenu : null);

    private RenderFragment? EffectiveMenuItems => MenuItems ?? ChildContent;

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-user-nav--authenticated", IsAuthenticated)
            .AddClass("cy-user-nav--unauthenticated", !IsAuthenticated)
            .AddClass("cy-user-nav--open", Open)
            .AddClass(Class)
            .Build();

    /// <summary>
    /// Returns the localized display text for the given status.
    /// </summary>
    public string StatusLabel(AvatarStatus status) => status switch
    {
        AvatarStatus.Online => EffectiveText.StatusOnline,
        AvatarStatus.Busy => EffectiveText.StatusBusy,
        AvatarStatus.Away => EffectiveText.StatusAway,
        AvatarStatus.Offline => EffectiveText.StatusOffline,
        _ => string.Empty
    };

    private async Task ToggleMenuAsync()
    {
        await SetOpenAsync(!Open, returnFocusToTrigger: false);
    }

    private async Task SetOpenAsync(bool open, bool returnFocusToTrigger = false)
    {
        if (Open == open)
        {
            return;
        }

        Open = open;
        RefreshCssState();

        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(open);
        }

        if (!open && returnFocusToTrigger)
        {
            try
            {
                await _trigger.FocusAsync();
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
            {
            }
        }

        StateHasChanged();
    }

    private async Task HandleTriggerKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Enter":
            case " ":
            case "ArrowDown":
            case "ArrowUp":
                if (!Open)
                {
                    await SetOpenAsync(true);
                }
                break;
            case "Escape" when Open:
                await SetOpenAsync(false, returnFocusToTrigger: true);
                break;
        }
    }

    private async Task HandleMenuKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Escape":
                await SetOpenAsync(false, returnFocusToTrigger: true);
                break;
            case "Tab":
                await SetOpenAsync(false, returnFocusToTrigger: false);
                break;
        }
    }

    private async Task HandleLoginClickAsync(MouseEventArgs args)
    {
        if (OnLogin.HasDelegate)
        {
            await OnLogin.InvokeAsync(args);
        }
    }

    private async Task HandleLogoutClickAsync(MouseEventArgs args)
    {
        await SetOpenAsync(false, returnFocusToTrigger: true);

        if (OnLogout.HasDelegate)
        {
            await OnLogout.InvokeAsync();
        }
    }

    /// <summary>
    /// Called by the JavaScript editing interop when a pointer down occurs outside the component root.
    /// </summary>
    [JSInvokable]
    public async Task OnOutsidePointer()
    {
        if (Open)
        {
            await InvokeAsync(() => SetOpenAsync(false, returnFocusToTrigger: false));
        }
    }

    /// <inheritdoc />
    private bool _wasAuthenticated;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsAuthenticated && (_token == 0 || !_wasAuthenticated))
        {
            await AttachAsync();
        }
        else if (!IsAuthenticated && _token != 0)
        {
            await DetachAsync();
        }

        _wasAuthenticated = IsAuthenticated;
    }

    private async Task DetachAsync()
    {
        if (_token != 0 && _module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("detachMenu", _token);
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or ObjectDisposedException or InvalidOperationException)
            {
            }
            _token = 0;
        }
    }

    private async Task AttachAsync()
    {
        if (_token != 0)
        {
            return;
        }

        try
        {
            _module ??= await EditingInterop.ImportAsync(JSRuntime);
            _selfReference ??= DotNetObjectReference.Create(this);
            _token = await _module.InvokeAsync<int>("attachMenu", _root, _selfReference);
        }
        catch (Exception)
        {
            // Operates gracefully without JavaScript
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        try
        {
            if (_module is not null)
            {
                await DetachAsync();

                await _module.DisposeAsync();
                _module = null;
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or ObjectDisposedException or InvalidOperationException)
        {
        }

        _selfReference?.Dispose();
        _selfReference = null;
    }
}
