using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A list whose items can be reordered - within the list, or between lists that share a <see cref="Group"/> - by
/// dragging, by keyboard, or with buttons. The list shows and announces the move; <em>you</em> own the collection and
/// apply the change in <see cref="OnReorder"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every drag has an alternative (WCAG 2.5.7).</b> Besides dragging the grip (<see cref="CyDragHandle"/>), each
/// item has <em>Move up</em> and <em>Move down</em> buttons and, when other lists share its group, a <em>Move to</em>
/// menu. Set <see cref="ShowMoveActions"/> to false only if you provide an equivalent of your own.
/// </para>
/// <para>
/// <b>Keyboard.</b> Focus an item's grip, then <c>Space</c> picks it up; <c>ArrowUp</c>/<c>ArrowDown</c> move it one place,
/// <c>Home</c>/<c>End</c> to the ends; <c>Space</c> or <c>Enter</c> drops (raising <see cref="OnReorder"/>); <c>Escape</c> or moving
/// focus away cancels. Moving between lists by keyboard uses the <em>Move to</em> menu (it appends to the end of the other list).
/// </para>
/// <para>
/// <b>Announcements.</b> Pick-up, each move, drop and cancel are announced through <see cref="ILiveRegionRegistry"/>, so
/// place a <see cref="Accessibility.CyLiveRegion"/> in your layout. If no registry is registered, the list falls back to its own
/// hidden live region. All phrases are replaceable through <see cref="Text"/>.
/// </para>
/// <para>
/// <b>Pointer.</b> A small on-demand script (<c>sortable-list.js</c>, no dependency, Pointer Events with pointer capture) drags the
/// grip with mouse, touch or pen, shows a drop indicator and honours <c>prefers-reduced-motion</c>. Without it, buttons and keyboard
/// still work. Set <see cref="HandleOnly"/> to false to drag from anywhere on an item except its controls; this blocks touch scrolling
/// over items, so prefer the grip on touch devices.
/// </para>
/// <para>
/// Give each list a stable <c>Id</c> when you need to tell lists apart in <see cref="CyReorderEventArgs{TItem}"/>. The item template is
/// a render fragment, never markup text, so item content is escaped as usual.
/// </para>
/// </remarks>
/// <typeparam name="TItem">The item type.</typeparam>
public partial class CySortableList<TItem> : CyComponentBase, ISortableList, IAsyncDisposable
{
    private readonly List<object> _keys = [];
    private List<TItem> _items = [];
    private ElementReference _root;
    private IJSObjectReference? _module;
    private DotNetObjectReference<CySortableList<TItem>>? _selfReference;
    private int _token;
    private SortableGroupRegistry? _registry;
    private ILiveRegionRegistry? _liveRegions;
    private bool _hasLiveRegionRegistry;
    private bool _registeredWithGroup;
    private string? _registeredGroup;
    private string _localMessage = string.Empty;
    private ScriptOptions? _lastScriptOptions;
    private (int Index, string Selector)? _focusRequest;

    // Keyboard pick-up state. `_pickIndex` is the item's index in Items; `_previewPosition` where it would land.
    private int? _pickIndex;
    private object? _pickKey;
    private int _previewPosition;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    /// <summary>The items, in display order. The list never modifies this collection.</summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>Renders one item's content (the grip and move buttons are added around it). Required.</summary>
    [Parameter, EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = default!;

    /// <summary>Returns a value that uniquely and stably identifies an item (an id, not an index). Required.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, object> KeySelector { get; set; } = default!;

    /// <summary>The list's accessible name, for example "Questions in Section 1". Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>
    /// Lists with the same group name accept each other's items. Leave null for a list that only reorders itself.
    /// Items must be the same type.
    /// </summary>
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Raised once for each completed move: a drop, a button press, a menu move or a pointer drag. See
    /// <see cref="CyReorderEventArgs{TItem}"/>; apply the change to your data here.
    /// </summary>
    [Parameter]
    public EventCallback<CyReorderEventArgs<TItem>> OnReorder { get; set; }

    /// <summary>The name used in labels and announcements for an item. Defaults to <c>ToString()</c>.</summary>
    [Parameter]
    public Func<TItem, string>? ItemLabelSelector { get; set; }

    /// <summary>Disables dragging, picking up and the move buttons.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>When true (the default) a pointer drag starts only from the grip. When false, from anywhere on an item except its controls.</summary>
    [Parameter]
    public bool HandleOnly { get; set; } = true;

    /// <summary>Shows Move up, Move down and (for grouped lists) Move to controls on every item. Defaults to true; see the class remarks before turning it off.</summary>
    [Parameter]
    public bool ShowMoveActions { get; set; } = true;

    /// <summary>Content shown, and a drop target for other lists, while <see cref="Items"/> is empty.</summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>Replaces the English phrases used for labels and announcements.</summary>
    [Parameter]
    public CySortableListText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-sortable";

    private CySortableListText Strings => Text ?? DefaultText;

    private static readonly CySortableListText DefaultText = new();

    private string InstructionsId => $"{Id}-instructions";

    // ISortableList
    string ISortableList.ListId => Id;

    string ISortableList.ListLabel => Label;

    string? ISortableList.ListGroup => Group;

    int ISortableList.Count => _items.Count;

    Type ISortableList.ItemType => typeof(TItem);

    // Built on every render (not in OnParametersSet) because a keyboard pick-up changes it without new parameters.
    private string ListCssClass =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-sortable--disabled", Disabled)
            .AddClass("cy-sortable--picked", _pickIndex is not null)
            .AddClass("cy-sortable--handle-only", HandleOnly)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (ItemTemplate is null)
        {
            throw new InvalidOperationException($"{nameof(CySortableList<TItem>)}.{nameof(ItemTemplate)} is required.");
        }

        if (KeySelector is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CySortableList<TItem>)}.{nameof(KeySelector)} is required: it must return a stable, unique key per item.");
        }

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CySortableList<TItem>)}.{nameof(Label)} must not be empty: it names the list.");
        }
    }

    /// <inheritdoc />
    protected override void OnParametersValidated()
    {
        _items = Items is null ? [] : [.. Items];
        _keys.Clear();

        var seen = new HashSet<object>();

        foreach (var item in _items)
        {
            var key = KeySelector(item)
                ?? throw new InvalidOperationException(
                    $"{nameof(CySortableList<TItem>)}.{nameof(KeySelector)} returned null. Keys must be non-null.");

            if (!seen.Add(key))
            {
                throw new InvalidOperationException(
                    $"{nameof(CySortableList<TItem>)}.{nameof(KeySelector)} returned the duplicate key '{key}'. Keys must be unique within a list.");
            }

            _keys.Add(key);
        }

        // The data changed under a pick-up (e.g. the parent reloaded): abandon it rather than move the wrong item.
        if (_pickIndex is { } pick && (pick >= _keys.Count || !Equals(_keys[pick], _pickKey)))
        {
            ClearPick();
        }

        _liveRegions ??= Services.GetService(typeof(ILiveRegionRegistry)) as ILiveRegionRegistry;
        _hasLiveRegionRegistry = _liveRegions is not null;
        _registry ??= Services.GetService(typeof(SortableGroupRegistry)) as SortableGroupRegistry;

        if (_registry is not null && (!_registeredWithGroup || _registeredGroup != Group))
        {
            _registry.Register(this);
            _registeredWithGroup = true;
            _registeredGroup = Group;
        }
    }

    private IReadOnlyList<ISortableList> Peers() => _registry?.Peers(this) ?? [];

    private string NameOf(TItem item)
    {
        var name = ItemLabelSelector?.Invoke(item) ?? item?.ToString();
        return string.IsNullOrWhiteSpace(name) ? Strings.ItemFallback : name;
    }

    private static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    private bool IsPicked(int index) => _pickIndex == index;

    private string ItemCssClass(int index) =>
        CssBuilder.Empty
            .AddClass("cy-sortable__item")
            .AddClass("cy-sortable__item--picked", IsPicked(index))
            .Build();

    /// <summary>While a keyboard pick-up is active the DOM order is left alone and CSS <c>order</c> previews the move.</summary>
    private string? ItemStyle(int index)
    {
        if (_pickIndex is not { } pick)
        {
            return null;
        }

        return $"order:{PreviewPositionOf(index, pick)}";
    }

    private int PreviewPositionOf(int index, int pick)
    {
        if (index == pick)
        {
            return _previewPosition;
        }

        // Items between the original and preview position shift by one.
        var shifted = index;
        if (index > pick)
        {
            shifted--;
        }

        return shifted >= _previewPosition ? shifted + 1 : shifted;
    }

    // ------------------------------------------------------------ keyboard

    private async Task ActivateHandleAsync(int index)
    {
        if (Disabled)
        {
            return;
        }

        if (_pickIndex is null)
        {
            _pickIndex = index;
            _pickKey = _keys[index];
            _previewPosition = index;
            await AnnounceAsync(Format(Strings.PickedUp, NameOf(_items[index]), index + 1, _items.Count));
        }
        else if (_pickIndex == index)
        {
            await DropAsync();
        }
    }

    private async Task HandleKeyDownAsync(int index, KeyboardEventArgs args)
    {
        if (_pickIndex != index)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowUp":
                await PreviewAsync(_previewPosition - 1);
                break;
            case "ArrowDown":
                await PreviewAsync(_previewPosition + 1);
                break;
            case "Home":
                await PreviewAsync(0);
                break;
            case "End":
                await PreviewAsync(_items.Count - 1);
                break;
            case "Escape":
                await CancelAsync(announce: true);
                break;
        }
    }

    private async Task HandleBlurAsync(int index)
    {
        if (_pickIndex == index)
        {
            await CancelAsync(announce: true);
        }
    }

    private async Task PreviewAsync(int position)
    {
        if (_pickIndex is not { } pick)
        {
            return;
        }

        var clamped = Math.Clamp(position, 0, _items.Count - 1);

        if (clamped == _previewPosition)
        {
            return;
        }

        _previewPosition = clamped;
        await AnnounceAsync(Format(Strings.Moved, NameOf(_items[pick]), clamped + 1, _items.Count));
    }

    private async Task DropAsync()
    {
        if (_pickIndex is not { } from)
        {
            return;
        }

        var to = _previewPosition;
        var item = _items[from];
        ClearPick();

        if (to == from)
        {
            _focusRequest = (from, "[data-cy-drag-handle]");
            await AnnounceAsync(Format(Strings.Dropped, NameOf(item), to + 1, _items.Count));
            return;
        }

        _focusRequest = (to, "[data-cy-drag-handle]");
        await ReceiveAsync(this, item!, from, to, Strings.Dropped);
    }

    private async Task CancelAsync(bool announce)
    {
        if (_pickIndex is not { } pick)
        {
            return;
        }

        var item = _items[pick];
        ClearPick();
        _focusRequest = (pick, "[data-cy-drag-handle]");

        if (announce)
        {
            await AnnounceAsync(Format(Strings.Cancelled, NameOf(item), pick + 1, _items.Count));
        }
    }

    private void ClearPick()
    {
        _pickIndex = null;
        _pickKey = null;
        _previewPosition = 0;
    }

    // ------------------------------------------------------------- buttons and menu

    private async Task MoveByAsync(int index, int step)
    {
        var to = index + step;

        if (Disabled || to < 0 || to >= _items.Count)
        {
            return;
        }

        var item = _items[index];
        var button = step < 0 ? "up" : "down";
        var atEdge = step < 0 ? to == 0 : to == _items.Count - 1;

        // Keep focus on the button that was pressed; if it becomes disabled at the edge, use its sibling.
        _focusRequest = (to, atEdge ? $"[data-cy-action='{(step < 0 ? "down" : "up")}']" : $"[data-cy-action='{button}']");

        await ReceiveAsync(this, item!, index, to);
    }

    private async Task MoveToListAsync(int index, ISortableList target)
    {
        if (Disabled || index < 0 || index >= _items.Count)
        {
            return;
        }

        await target.ReceiveAsync(this, _items[index]!, index, target.Count);
    }

    // ------------------------------------------------------------- committing a move

    /// <inheritdoc />
    async Task ISortableList.ReceiveAsync(ISortableList source, object item, int oldIndex, int newIndex) =>
        await ReceiveAsync(source, item, oldIndex, newIndex);

    private async Task ReceiveAsync(ISortableList source, object item, int oldIndex, int newIndex, string? sameListFormat = null)
    {
        if (item is not TItem typed)
        {
            return;
        }

        var cross = !ReferenceEquals(source, this);
        var name = NameOf(typed);

        // The receiving list reports a cross-list move and takes focus for its new item.
        if (cross)
        {
            _focusRequest = (newIndex, "[data-cy-drag-handle]");
        }

        var args = new CyReorderEventArgs<TItem>(typed, oldIndex, newIndex, source.ListId, Id, Group);

        if (OnReorder.HasDelegate)
        {
            await OnReorder.InvokeAsync(args);
        }

        // `_items` still holds the pre-move data: the parent has not re-rendered us yet.
        var message = cross
            ? Format(Strings.MovedToList, name, Label, newIndex + 1, _items.Count + 1)
            : Format(sameListFormat ?? Strings.Moved, name, newIndex + 1, _items.Count);

        await AnnounceAsync(message);
    }

    /// <summary>Called by the script when a pointer drag finishes. Not for application code.</summary>
    [JSInvokable]
    public async Task OnPointerDrop(string targetListId, int oldIndex, int newIndex)
    {
        await InvokeAsync(async () =>
        {
            if (Disabled || oldIndex < 0 || oldIndex >= _items.Count)
            {
                return;
            }

            ISortableList? target = string.Equals(targetListId, Id, StringComparison.Ordinal)
                ? this
                : _registry?.Find(this, targetListId);

            if (target is null)
            {
                return;
            }

            await target.ReceiveAsync(this, _items[oldIndex]!, oldIndex, newIndex);
            StateHasChanged();
        });
    }

    // ------------------------------------------------------------- announcements

    private async Task AnnounceAsync(string message)
    {
        if (_liveRegions is not null)
        {
            await _liveRegions.PublishAsync(
                new LiveRegionAnnouncement(message, LiveRegionPoliteness.Polite),
                CancellationToken.None);
        }
        else
        {
            _localMessage = message;
        }
    }

    // ------------------------------------------------------------- script

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            await SyncScriptAsync();

            if (_focusRequest is { } request && _module is not null)
            {
                _focusRequest = null;
                await _module.InvokeAsync<bool>("focusIn", _root, request.Index, request.Selector);
            }
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // Browser or circuit is gone, or the script is unavailable: buttons and keyboard still work.
        }
    }

    private async Task SyncScriptAsync()
    {
        try
        {
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", SortableInterop.ModulePath);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            return;
        }

        var options = new ScriptOptions(Group, HandleOnly, Disabled);

        if (_token == 0)
        {
            _selfReference ??= DotNetObjectReference.Create(this);
            _token = await _module.InvokeAsync<int>("register", _root, _selfReference, options);
            _lastScriptOptions = options;
        }
        else if (_lastScriptOptions != options)
        {
            await _module.InvokeVoidAsync("update", _token, options);
            _lastScriptOptions = options;
        }
    }

    private sealed record ScriptOptions(string? Group, bool HandleOnly, bool Disabled);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        _registry?.Unregister(this);

        try
        {
            if (_module is not null)
            {
                if (_token != 0)
                {
                    await _module.InvokeVoidAsync("dispose", _token);
                    _token = 0;
                }

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
