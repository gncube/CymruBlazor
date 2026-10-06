using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A menu button: a trigger that opens a list of actions (<see cref="CyMenuItem"/>),
/// for example the "more" overflow menu at the end of a table or questionnaire row.
/// Implements the WAI-ARIA Authoring Practices menu-button pattern.
/// </summary>
/// <remarks>
/// <para>
/// <b>Keyboard.</b> On the trigger, <c>Enter</c>, <c>Space</c> and <c>ArrowDown</c> open the menu and
/// focus the first item; <c>ArrowUp</c> opens it on the last item. In the open menu, <c>ArrowDown</c> and
/// <c>ArrowUp</c> move (and wrap), <c>Home</c>/<c>End</c> jump to the ends, <c>Enter</c>/<c>Space</c>
/// activate, <c>Escape</c> closes and returns focus to the trigger, and <c>Tab</c> closes. Only one item is
/// in the tab sequence at a time (roving <c>tabindex</c>). Pressing outside the menu closes it. Type-ahead is
/// not implemented.
/// </para>
/// <para>
/// <b>Name.</b> <see cref="Label"/> is required and names the trigger when it shows only an icon; if
/// <see cref="Text"/> is set, the visible text names it instead. The trigger is 24x24 CSS px or larger
/// (WCAG 2.5.8). The menu popup is positioned by CSS (<see cref="AlignEnd"/>); it is not rendered in the top
/// layer, so avoid placing it inside an <c>overflow: hidden</c> ancestor.
/// </para>
/// <para>
/// The popup (and so every <see cref="CyMenuItem"/>) is only rendered while open. A small on-demand script
/// (<c>cymru-editing.js</c>) stops the arrow keys scrolling the page and detects presses outside; without it
/// the menu still works from the keyboard and pointer.
/// </para>
/// </remarks>
public partial class CyMenu : CyLayoutComponentBase, IAsyncDisposable
{
    private enum FocusRequest { None, First, Last, Trigger }

    private readonly List<CyMenuItem> _items = [];
    private ElementReference _root;
    private ElementReference _trigger;
    private DotNetObjectReference<CyMenu>? _selfReference;
    private IJSObjectReference? _module;
    private int _token;
    private CyMenuItem? _activeItem;
    private FocusRequest _pendingFocus;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>The accessible name of the trigger button. Required, because the trigger is often icon-only.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>Optional visible text on the trigger. When set it names the trigger instead of <see cref="Label"/>.</summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>Name of the trigger's icon (see <c>IconRegistry</c>). Defaults to <c>more</c>; set to null for a text-only trigger.</summary>
    [Parameter]
    public string? Icon { get; set; } = "more";

    /// <summary>Whether the menu is open. Two-way bindable.</summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>Raised when <see cref="Open"/> changes because of user interaction.</summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>Disables the trigger.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>When true (the default) the popup's right edge lines up with the trigger's; otherwise the left edges line up.</summary>
    [Parameter]
    public bool AlignEnd { get; set; } = true;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-menu";

    private string TriggerId => $"{Id}-trigger";

    private string ListId => $"{Id}-list";

    private bool HasVisibleText => !string.IsNullOrWhiteSpace(Text);

    private string TriggerCssClass =>
        CssBuilder.Empty
            .AddClass("cy-menu__trigger")
            .AddClass("cy-menu__trigger--icon-only", !HasVisibleText)
            .Build();

    private string ListCssClass =>
        CssBuilder.Empty
            .AddClass("cy-menu__list")
            .AddClass(AlignEnd ? "cy-menu__list--end" : "cy-menu__list--start")
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyMenu)}.{nameof(Label)} must not be empty: it names the menu button.");
        }
    }

    internal bool IsActive(CyMenuItem item) => ReferenceEquals(_activeItem, item);

    internal void Register(CyMenuItem item)
    {
        _items.Add(item);
        _activeItem ??= item.Disabled ? null : item;
    }

    internal void Unregister(CyMenuItem item)
    {
        _items.Remove(item);

        if (ReferenceEquals(_activeItem, item))
        {
            _activeItem = null;
        }
    }

    internal async Task ActivateAsync(CyMenuItem item)
    {
        if (item.Disabled)
        {
            return;
        }

        await SetOpenAsync(false, FocusRequest.Trigger);

        if (item.OnClick.HasDelegate)
        {
            await item.OnClick.InvokeAsync();
        }
    }

    /// <summary>Called by the script when the pointer is pressed outside the menu. Not for application code.</summary>
    [JSInvokable]
    public async Task OnOutsidePointer()
    {
        if (Open)
        {
            await InvokeAsync(() => SetOpenAsync(false, FocusRequest.None));
        }
    }

    private Task ToggleAsync() =>
        Open ? SetOpenAsync(false, FocusRequest.Trigger) : SetOpenAsync(true, FocusRequest.First);

    private async Task SetOpenAsync(bool open, FocusRequest focus)
    {
        if (Open == open && focus != FocusRequest.Trigger)
        {
            return;
        }

        var changed = Open != open;
        Open = open;
        _pendingFocus = open ? focus : focus == FocusRequest.Trigger ? FocusRequest.Trigger : FocusRequest.None;

        if (!open)
        {
            _activeItem = null;
        }
        else if (_activeItem is null)
        {
            _activeItem = _items.FirstOrDefault(i => !i.Disabled);
        }

        if (changed && OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(open);
        }

        StateHasChanged();
    }

    private async Task HandleTriggerKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "ArrowDown":
                if (Open)
                {
                    await FocusItemAsync(_items.FirstOrDefault(i => !i.Disabled));
                }
                else
                {
                    await SetOpenAsync(true, FocusRequest.First);
                }

                break;
            case "ArrowUp":
                if (Open)
                {
                    await FocusItemAsync(_items.LastOrDefault(i => !i.Disabled));
                }
                else
                {
                    await SetOpenAsync(true, FocusRequest.Last);
                }

                break;
            case "Escape" when Open:
                await SetOpenAsync(false, FocusRequest.Trigger);
                break;
        }
    }

    private async Task HandleListKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "ArrowDown":
                await MoveAsync(+1);
                break;
            case "ArrowUp":
                await MoveAsync(-1);
                break;
            case "Home":
                await FocusItemAsync(_items.FirstOrDefault(i => !i.Disabled));
                break;
            case "End":
                await FocusItemAsync(_items.LastOrDefault(i => !i.Disabled));
                break;
            case "Escape":
                await SetOpenAsync(false, FocusRequest.Trigger);
                break;
            case "Tab":
                await SetOpenAsync(false, FocusRequest.None);
                break;
        }
    }

    private async Task MoveAsync(int step)
    {
        var enabled = _items.Where(i => !i.Disabled).ToList();

        if (enabled.Count == 0)
        {
            return;
        }

        var current = _activeItem is null ? -1 : enabled.IndexOf(_activeItem);
        var next = current < 0
            ? (step > 0 ? 0 : enabled.Count - 1)
            : (current + step + enabled.Count) % enabled.Count;

        await FocusItemAsync(enabled[next]);
    }

    private async Task FocusItemAsync(CyMenuItem? item)
    {
        if (item is null)
        {
            return;
        }

        _activeItem = item;

        foreach (var menuItem in _items)
        {
            menuItem.RefreshVisualState();
        }

        await item.FocusAsync();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await AttachAsync();
        }

        var focus = _pendingFocus;
        _pendingFocus = FocusRequest.None;

        switch (focus)
        {
            case FocusRequest.Trigger:
                await TryFocusAsync(_trigger);
                break;
            case FocusRequest.First when Open:
                await FocusItemAsync(_items.FirstOrDefault(i => !i.Disabled));
                break;
            case FocusRequest.Last when Open:
                await FocusItemAsync(_items.LastOrDefault(i => !i.Disabled));
                break;
        }
    }

    private static async Task TryFocusAsync(ElementReference element)
    {
        try
        {
            await element.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone, or the element was removed.
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
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // The menu still works without the script: keyboard and pointer are handled in .NET.
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
                if (_token != 0)
                {
                    await _module.InvokeVoidAsync("detachMenu", _token);
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

        _selfReference?.Dispose();
        _selfReference = null;
    }
}
