using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Components.Data;

/// <summary>
/// A data table with optional sorting, row selection and paging, composed from <see cref="CyTable"/> (which is unchanged):
/// the same captioned, keyboard-reachable scroll region and real <c>&lt;table&gt;</c> markup, with the header and body
/// written for you from <see cref="Columns"/>. Everything beyond a plain table is opt-in and off by default.
/// </summary>
/// <remarks>
/// <para>
/// <b>Data.</b> Pass <see cref="Items"/> (sorted and paged in memory) <em>or</em> <see cref="ItemsProvider"/> (asked for one
/// sorted page at a time; it receives a <see cref="CyDataTableRequest"/> and returns a <see cref="CyDataTableResult{TItem}"/>).
/// A provider is called again when the sort, page, page size or the provider itself changes; call
/// <see cref="RefreshAsync"/> to force it. A slower, older answer never replaces a newer one, and an exception shows an
/// error row with a Retry button.
/// </para>
/// <para>
/// <b>Sorting.</b> A sortable heading is a button inside its <c>&lt;th&gt;</c>; only the sorted column has
/// <c>aria-sort</c>. The direction is also shown by an icon, never by colour alone, and each change is announced
/// ("Sorted by Name, ascending"). Sorting returns to page 1.
/// </para>
/// <para>
/// <b>Selection.</b> <see cref="SelectionMode"/> adds a checkbox (or radio button) per row, named with
/// <see cref="RowLabel"/>. The header checkbox selects or clears the rows <em>on the current page</em>. Set
/// <see cref="KeySelector"/> (required with a provider) so a selection survives paging and reloads. <c>aria-selected</c>
/// is not used: it is not valid on a row of a plain table; the checked control and a visible count carry the state.
/// </para>
/// <para>
/// <b>Keyboard.</b> This is a table, not an ARIA grid: Tab visits the scroll region, each sort button, then each row's
/// selection control and any focusable content in its cells, then the pagination. There is no arrow-key cell navigation.
/// </para>
/// <para>
/// <b>Expandable rows.</b> Provide <see cref="DetailTemplate"/> to render master-detail disclosure rows. Each row receives an
/// accessible expand/collapse button with <c>aria-expanded</c> and <c>aria-controls</c>. Use <see cref="AutoCollapseDetails"/>
/// to ensure only one row is open at a time, or bind <see cref="ExpandedItems"/> / listen to <see cref="OnRowExpandChanged"/>.
/// </para>
/// <para>
/// <b>Not included</b> (use <c>QuickGrid</c> or build on this): virtualisation, inline editing, column resizing,
/// reordering or hiding, stacked rows on small screens, grouping, a filter UI and export. After a page
/// change focus moves to the "Showing 11 to 20 of 134" line, because the pagination replaces the pressed button.
/// </para>
/// </remarks>
/// <typeparam name="TItem">The row type.</typeparam>
public partial class CyDataTable<TItem> : CyComponentBase, IDisposable
{
    private static readonly CyDataTableText DefaultText = new();

    private readonly Dictionary<object, TItem> _selected = [];
    private readonly Dictionary<object, TItem> _expanded = [];
    private IReadOnlyList<TItem> _view = [];
    private CancellationTokenSource? _cts;
    private ElementReference _range;
    private int _total;
    private int _loadVersion;
    private bool _loading;
    private bool _loadError;
    private bool _focusRange;
    private bool _disposed;
    private string _status = string.Empty;

    // Working state; seeded from, and echoed to, the two-way parameters.
    private string? _sortKey;
    private bool _sortDescending;
    private int _page = 1;
    private int _pageSize;

    // The parameter values last seen, so an unchanged parameter never overwrites state the user changed.
    private bool _paramsSeen;
    private string? _seenSortKey;
    private bool _seenSortDescending;
    private int _seenPage = 1;
    private int _seenPageSize;
    private IReadOnlyList<TItem>? _seenSelected;
    private IReadOnlyList<TItem>? _seenExpanded;

    // What the provider was last asked for.
    private bool _hasLoaded;
    private Delegate? _loadedProvider;
    private string? _loadedSortKey;
    private bool _loadedSortDescending;
    private int _loadedPage;
    private int _loadedPageSize;

    /// <summary>The table's caption: required, so every table has an accessible name.</summary>
    [Parameter, EditorRequired]
    public required string Caption { get; set; }

    /// <summary>Keeps the caption for assistive technology but hides it visually (see <see cref="CyTable"/>).</summary>
    [Parameter]
    public bool CaptionVisuallyHidden { get; set; }

    /// <summary>Lets cell text wrap instead of staying on one line (see <see cref="CyTable.Wrap"/>).</summary>
    [Parameter]
    public bool Wrap { get; set; }

    /// <summary>The columns, in order. Required and not empty.</summary>
    [Parameter, EditorRequired]
    public required IReadOnlyList<CyDataColumn<TItem>> Columns { get; set; }

    /// <summary>The rows, sorted and paged in memory. Use this or <see cref="ItemsProvider"/>.</summary>
    [Parameter]
    public IReadOnlyList<TItem>? Items { get; set; }

    /// <summary>Returns one sorted page of rows. Use this or <see cref="Items"/>.</summary>
    [Parameter]
    public Func<CyDataTableRequest, CancellationToken, Task<CyDataTableResult<TItem>>>? ItemsProvider { get; set; }

    /// <summary>The <see cref="CyDataColumn{TItem}.SortKey"/> (or heading) of the sorted column; <see langword="null"/> for none. Two-way bindable.</summary>
    [Parameter]
    public string? SortKey { get; set; }

    /// <summary>Raised when the user changes the sorted column.</summary>
    [Parameter]
    public EventCallback<string?> SortKeyChanged { get; set; }

    /// <summary>Whether the sort is descending. Two-way bindable.</summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>Raised when the user changes the sort direction.</summary>
    [Parameter]
    public EventCallback<bool> SortDescendingChanged { get; set; }

    /// <summary>Adds a third step to the sort cycle (ascending, descending, none). Default false.</summary>
    [Parameter]
    public bool AllowUnsorted { get; set; }

    /// <summary>Whether rows can be selected. Default none.</summary>
    [Parameter]
    public CyDataSelectionMode SelectionMode { get; set; }

    /// <summary>The selected rows. Two-way bindable; each change assigns a new list.</summary>
    [Parameter]
    public IReadOnlyList<TItem>? SelectedItems { get; set; }

    /// <summary>Raised with the new selection.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// A stable identity for a row (an id), so a selection survives paging and reloads. Defaults to the row object itself,
    /// which only works when the same instances come back each time. Required when <see cref="ItemsProvider"/> is used with selection.
    /// </summary>
    [Parameter]
    public Func<TItem, object>? KeySelector { get; set; }

    /// <summary>Names a row for its selection control ("Select {label}"). Required when <see cref="SelectionMode"/> is not none.</summary>
    [Parameter]
    public Func<TItem, string>? RowLabel { get; set; }

    /// <summary>Rows per page. 0 (the default) turns paging off.</summary>
    [Parameter]
    public int PageSize { get; set; }

    /// <summary>
    /// The page sizes offered in a labelled "Rows per page" select beside the pagination. <see langword="null"/> (the default)
    /// renders no select and leaves the markup unchanged. Needs <see cref="PageSize"/> to be above zero; the current
    /// <see cref="PageSize"/> is always offered, even when it is not in the list. Every value must be above zero.
    /// </summary>
    [Parameter]
    public IReadOnlyList<int>? PageSizeOptions { get; set; }

    /// <summary>Raised when the user picks another page size from <see cref="PageSizeOptions"/>. Makes <c>@bind-PageSize</c> work.</summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>Raised after the sort changes, once, with the new key and direction (in addition to the two-way sort parameters).</summary>
    [Parameter]
    public EventCallback<CyDataSortChange> OnSortChanged { get; set; }

    /// <summary>The current page, starting at 1. Two-way bindable.</summary>
    [Parameter]
    public int Page { get; set; } = 1;

    /// <summary>Raised when the user changes page.</summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    /// <summary>Shows the loading state (skeleton rows, <c>aria-busy</c>). A provider's own loading is shown automatically.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>Replaces the default "No results" message.</summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>Content above the table: your filters and bulk actions. The component has no filter UI of its own.</summary>
    [Parameter]
    public RenderFragment? Toolbar { get; set; }

    /// <summary>Renders a last column of per-row actions (ordinary buttons or links), under a visually hidden "Actions" heading.</summary>
    [Parameter]
    public RenderFragment<TItem>? RowActions { get; set; }

    /// <summary>
    /// Limits the height of the scrolling region to a plain length such as <c>24rem</c> and keeps the header row in view.
    /// The region stays keyboard-focusable (see <see cref="CyTable"/>).
    /// </summary>
    [Parameter]
    public string? MaxHeight { get; set; }

    /// <summary>Template rendered in an expanded row below the main row for master-detail views.</summary>
    [Parameter]
    public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>When true, expanding any row automatically collapses all other expanded rows. Default false.</summary>
    [Parameter]
    public bool AutoCollapseDetails { get; set; }

    /// <summary>The currently expanded rows. Two-way bindable; each change assigns a new list.</summary>
    [Parameter]
    public IReadOnlyList<TItem>? ExpandedItems { get; set; }

    /// <summary>Raised when the list of expanded items changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> ExpandedItemsChanged { get; set; }

    /// <summary>Raised when a row is expanded or collapsed.</summary>
    [Parameter]
    public EventCallback<(TItem Item, bool Expanded)> OnRowExpandChanged { get; set; }

    /// <summary>The phrases the table shows and announces; defaults to English.</summary>
    [Parameter]
    public CyDataTableText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-data-table";

    private CyDataTableText EffectiveText => Text ?? DefaultText;

    private bool Busy => Loading || _loading;

    private bool ShowSkeleton => Busy && _view.Count == 0;

    private int SkeletonRows => _pageSize > 0 ? Math.Min(_pageSize, 5) : 5;

    private int SelectedCount => _selected.Count;

    private string RadioName => $"{Id}-select";

    private int TotalPages => _pageSize > 0 ? Math.Max(1, (int)Math.Ceiling(_total / (double)_pageSize)) : 1;

    private bool ShowPageSizes => _pageSize > 0 && PageSizeOptions is { Count: > 0 };

    private IEnumerable<CyOption<int>> PageSizeChoices =>
        (PageSizeOptions ?? Array.Empty<int>())
            .Append(_pageSize)
            .Distinct()
            .Order()
            .Select(size => new CyOption<int>(size, size.ToString(CultureInfo.CurrentCulture)));

    private int ColumnSpan =>
        Columns.Count
        + (SelectionMode != CyDataSelectionMode.None ? 1 : 0)
        + (DetailTemplate is not null ? 1 : 0)
        + (RowActions is not null ? 1 : 0);

    private bool AllPageSelected => _view.Count > 0 && _view.All(IsSelected);

    private string PaginationLabel => Format(EffectiveText.PaginationLabel, Caption);

    private string RangeText
    {
        get
        {
            var first = ((_page - 1) * _pageSize) + 1;
            var last = Math.Min(_total, _page * _pageSize);
            return Format(EffectiveText.ShowingRange, first, last, _total);
        }
    }

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-data-table--sticky", !string.IsNullOrWhiteSpace(MaxHeight))
            .AddClass("cy-data-table--busy", Loading)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override string BuildCssStyle() =>
        StyleBuilder.Empty
            .AddStyle("--cy-data-table-max-height", MaxHeight)
            .AddStyle(Style)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Caption))
        {
            throw new InvalidOperationException(
                $"{nameof(CyDataTable<TItem>)}.{nameof(Caption)} must not be empty - every table needs an accessible name.");
        }

        if (Columns is null || Columns.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(CyDataTable<TItem>)}.{nameof(Columns)} must contain at least one column.");
        }

        if (Items is not null && ItemsProvider is not null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyDataTable<TItem>)}: set {nameof(Items)} or {nameof(ItemsProvider)}, not both.");
        }

        if (PageSize < 0)
        {
            throw new InvalidOperationException($"{nameof(CyDataTable<TItem>)}.{nameof(PageSize)} must not be negative.");
        }

        if (PageSizeOptions is not null && PageSizeOptions.Any(size => size <= 0))
        {
            throw new InvalidOperationException($"{nameof(CyDataTable<TItem>)}.{nameof(PageSizeOptions)} must contain only values above zero.");
        }

        CssLength.Require(MaxHeight, nameof(CyDataTable<TItem>), nameof(MaxHeight));

        if (SelectionMode != CyDataSelectionMode.None && RowLabel is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyDataTable<TItem>)}.{nameof(RowLabel)} is required when {nameof(SelectionMode)} is not None: every selection control needs a name.");
        }

        if (SelectionMode != CyDataSelectionMode.None && ItemsProvider is not null && KeySelector is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyDataTable<TItem>)}.{nameof(KeySelector)} is required when selection is combined with {nameof(ItemsProvider)}, or a selection cannot survive paging.");
        }

        foreach (var column in Columns)
        {
            CssLength.Require(column.Width, nameof(CyDataTable<TItem>), $"{nameof(CyDataColumn<TItem>.Width)} of column '{column.Header}'");

            if (column.Sortable && ItemsProvider is null && column.Value is null && column.Compare is null)
            {
                throw new InvalidOperationException(
                    $"Column '{column.Header}' is sortable but has neither a Value nor a Compare to sort in memory.");
            }
        }
    }

    // ---- loading

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        SyncState();

        if (ItemsProvider is null)
        {
            Recompute();
            return;
        }

        var changed = !_hasLoaded
            || !Equals(_loadedProvider, ItemsProvider)
            || _loadedSortKey != _sortKey
            || _loadedSortDescending != _sortDescending
            || _loadedPage != _page
            || _loadedPageSize != _pageSize;

        if (changed)
        {
            await LoadAsync();
        }
    }

    private void SyncState()
    {
        if (!_paramsSeen || SortKey != _seenSortKey || SortDescending != _seenSortDescending)
        {
            _sortKey = SortKey;
            _sortDescending = SortDescending;
            _seenSortKey = SortKey;
            _seenSortDescending = SortDescending;
        }

        if (!_paramsSeen || PageSize != _seenPageSize)
        {
            _pageSize = PageSize;
            _seenPageSize = PageSize;
        }

        if (!_paramsSeen || Page != _seenPage)
        {
            _page = Math.Max(1, Page);
            _seenPage = Page;
        }

        _paramsSeen = true;

        if (SelectedItems is not null && !ReferenceEquals(SelectedItems, _seenSelected))
        {
            _seenSelected = SelectedItems;
            _selected.Clear();

            foreach (var item in SelectedItems)
            {
                _selected[KeyOf(item)] = item;
            }
        }

        if (ExpandedItems is not null && !ReferenceEquals(ExpandedItems, _seenExpanded))
        {
            _seenExpanded = ExpandedItems;
            _expanded.Clear();

            foreach (var item in ExpandedItems)
            {
                _expanded[KeyOf(item)] = item;
            }
        }
    }

    private CyDataColumn<TItem>? SortColumn =>
        _sortKey is null ? null : Columns.FirstOrDefault(column => column.Sortable && column.Key == _sortKey);

    private void Recompute()
    {
        IEnumerable<TItem> rows = Items ?? Array.Empty<TItem>();

        if (SortColumn is { } column)
        {
            var comparer = column.Compare is { } compare
                ? Comparer<TItem>.Create(compare)
                : Comparer<TItem>.Create((a, b) => Comparer<object?>.Default.Compare(column.Value!(a), column.Value!(b)));

            rows = _sortDescending
                ? rows.OrderByDescending(row => row, comparer)
                : rows.OrderBy(row => row, comparer);
        }

        var list = rows as IReadOnlyList<TItem> ?? rows.ToList();
        _total = list.Count;

        if (_pageSize > 0)
        {
            _page = Math.Clamp(_page, 1, TotalPages);
            list = list.Skip((_page - 1) * _pageSize).Take(_pageSize).ToList();
        }

        _view = list;
    }

    private async Task LoadAsync()
    {
        CancelLoad();
        var version = ++_loadVersion;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _hasLoaded = true;
        _loadedProvider = ItemsProvider;
        _loadedSortKey = _sortKey;
        _loadedSortDescending = _sortDescending;
        _loadedPage = _page;
        _loadedPageSize = _pageSize;

        _loading = true;
        _loadError = false;
        _status = EffectiveText.Loading;

        try
        {
            var result = await ItemsProvider!(new CyDataTableRequest(_page, _pageSize, _sortKey, _sortDescending), token);

            if (version != _loadVersion || token.IsCancellationRequested)
            {
                return;
            }

            _view = result?.Items ?? [];
            _total = result?.TotalCount ?? 0;
            _status = string.Empty;
        }
        catch (OperationCanceledException)
        {
            return;
        }
#pragma warning disable CA1031 // A consumer's provider may throw anything; show an error row, never crash the circuit.
        catch (Exception)
#pragma warning restore CA1031
        {
            if (version != _loadVersion)
            {
                return;
            }

            _view = [];
            _total = 0;
            _loadError = true;
            _status = EffectiveText.LoadError;
        }
        finally
        {
            if (version == _loadVersion)
            {
                _loading = false;
            }
        }
    }

    private void CancelLoad()
    {
        _loadVersion++;

        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    /// <summary>Loads the rows again (with a provider) or recomputes them (with <see cref="Items"/>), keeping sort, page and selection.</summary>
    public async Task RefreshAsync()
    {
        await Reapply();

        if (!_disposed)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task Reapply()
    {
        if (ItemsProvider is not null)
        {
            await LoadAsync();
        }
        else
        {
            Recompute();
        }
    }

    // ---- sorting

    private string? AriaSort(CyDataColumn<TItem> column) =>
        _sortKey is not null && column.Sortable && column.Key == _sortKey
            ? (_sortDescending ? "descending" : "ascending")
            : null;

    private string SortIcon(CyDataColumn<TItem> column) => AriaSort(column) switch
    {
        "ascending" => "arrow-up",
        "descending" => "arrow-down",
        _ => "sort"
    };

    private async Task SortAsync(CyDataColumn<TItem> column)
    {
        var key = column.Key;

        if (_sortKey == key)
        {
            if (!_sortDescending)
            {
                _sortDescending = true;
            }
            else if (AllowUnsorted)
            {
                _sortKey = null;
                _sortDescending = false;
            }
            else
            {
                _sortDescending = false;
            }
        }
        else
        {
            _sortKey = key;
            _sortDescending = false;
        }

        var pageChanged = _page != 1;
        _page = 1;

        _status = _sortKey is null
            ? EffectiveText.SortCleared
            : Format(EffectiveText.SortedBy, column.Header, _sortDescending ? EffectiveText.Descending : EffectiveText.Ascending);

        await Reapply();

        if (SortKeyChanged.HasDelegate)
        {
            await SortKeyChanged.InvokeAsync(_sortKey);
        }

        if (SortDescendingChanged.HasDelegate)
        {
            await SortDescendingChanged.InvokeAsync(_sortDescending);
        }

        if (pageChanged && PageChanged.HasDelegate)
        {
            await PageChanged.InvokeAsync(_page);
        }

        if (OnSortChanged.HasDelegate)
        {
            await OnSortChanged.InvokeAsync(new CyDataSortChange(_sortKey, _sortDescending));
        }
    }

    private async Task OnPageSizeChangedAsync(int size)
    {
        if (size <= 0 || size == _pageSize)
        {
            return;
        }

        var pageChanged = _page != 1;
        _pageSize = size;
        _page = 1;
        _focusRange = true;

        await Reapply();

        _status = Format(EffectiveText.RowsPerPageChanged, _pageSize, TotalPages);

        if (PageSizeChanged.HasDelegate)
        {
            await PageSizeChanged.InvokeAsync(_pageSize);
        }

        if (pageChanged && PageChanged.HasDelegate)
        {
            await PageChanged.InvokeAsync(_page);
        }
    }

    // ---- paging

    private async Task OnPageChangedAsync(int page)
    {
        _page = page;
        _focusRange = true;
        await Reapply();

        _status = Format(EffectiveText.PageChanged, _page, TotalPages);

        if (PageChanged.HasDelegate)
        {
            await PageChanged.InvokeAsync(_page);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_focusRange || Busy)
        {
            return;
        }

        _focusRange = false;

        try
        {
            await _range.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone, or the line is not rendered (no rows).
        }
    }

    // ---- selection

    private object KeyOf(TItem item) => KeySelector?.Invoke(item) ?? item!;

    private bool IsSelected(TItem item) => _selected.ContainsKey(KeyOf(item));

    private string SelectRowLabel(TItem item) => Format(EffectiveText.SelectRow, RowLabel!(item));

    private async Task ToggleAsync(TItem item, bool on)
    {
        if (on)
        {
            _selected[KeyOf(item)] = item;
        }
        else
        {
            _selected.Remove(KeyOf(item));
        }

        await PublishSelectionAsync();
    }

    private async Task SelectSingleAsync(TItem item)
    {
        _selected.Clear();
        _selected[KeyOf(item)] = item;
        await PublishSelectionAsync();
    }

    private async Task TogglePageAsync(bool on)
    {
        foreach (var item in _view)
        {
            if (on)
            {
                _selected[KeyOf(item)] = item;
            }
            else
            {
                _selected.Remove(KeyOf(item));
            }
        }

        await PublishSelectionAsync();
    }

    private async Task PublishSelectionAsync()
    {
        var list = _selected.Values.ToList();
        _seenSelected = list;
        _status = Format(EffectiveText.SelectedCount, list.Count);

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(list);
        }
    }

    // ---- cells

    private static string HeaderClass(CyDataColumn<TItem> column) =>
        CssBuilder.Empty
            .AddClass("cy-data-table__header")
            .AddClass(AlignClass(column.Align))
            .Build();

    private static string CellClass(CyDataColumn<TItem> column) =>
        CssBuilder.Empty
            .AddClass(AlignClass(column.Align))
            .AddClass("cy-table__cell--wrap", column.Wrap)
            .AddClass("cy-table__cell--truncate", column.Truncate)
            .Build();

    private static string? AlignClass(CyColumnAlign align) => align switch
    {
        CyColumnAlign.End => "cy-data-table__cell--end",
        CyColumnAlign.Center => "cy-data-table__cell--center",
        _ => null
    };

    private static string? WidthStyle(CyDataColumn<TItem> column) =>
        string.IsNullOrWhiteSpace(column.Width) ? null : StyleBuilder.Empty.AddStyle("inline-size", column.Width).Build();

    private static string TextOf(CyDataColumn<TItem> column, TItem item) =>
        Convert.ToString(column.Value?.Invoke(item), CultureInfo.CurrentCulture) ?? string.Empty;

    private static string? CellTitle(CyDataColumn<TItem> column, TItem item) =>
        column.Truncate && column.Cell is null ? TextOf(column, item) : null;

    private static RenderFragment CellContent(CyDataColumn<TItem> column, TItem item) =>
        column.Cell is { } cell
            ? cell(item)
            : builder => builder.AddContent(0, TextOf(column, item));

    private static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    // ---- expandable rows

    /// <summary>Checks whether a given row is currently expanded.</summary>
    public bool IsExpanded(TItem item) => _expanded.ContainsKey(KeyOf(item));

    /// <summary>Toggles the expansion state of a row.</summary>
    public async Task ToggleRowDetailsAsync(TItem item)
    {
        if (IsExpanded(item))
        {
            await CollapseRowDetailsAsync(item);
        }
        else
        {
            await ExpandRowDetailsAsync(item);
        }
    }

    /// <summary>Expands a row's details.</summary>
    public async Task ExpandRowDetailsAsync(TItem item)
    {
        var key = KeyOf(item);
        if (_expanded.ContainsKey(key))
        {
            return;
        }

        if (AutoCollapseDetails)
        {
            _expanded.Clear();
        }

        _expanded[key] = item;
        await PublishExpandedAsync(item, true);
    }

    /// <summary>Collapses a row's details.</summary>
    public async Task CollapseRowDetailsAsync(TItem item)
    {
        var key = KeyOf(item);
        if (!_expanded.Remove(key))
        {
            return;
        }

        await PublishExpandedAsync(item, false);
    }

    /// <summary>Collapses all expanded rows.</summary>
    public async Task CollapseAllDetailsAsync()
    {
        if (_expanded.Count == 0)
        {
            return;
        }

        _expanded.Clear();
        var list = _expanded.Values.ToList();
        _seenExpanded = list;

        if (ExpandedItemsChanged.HasDelegate)
        {
            await ExpandedItemsChanged.InvokeAsync(list);
        }
    }

    private async Task PublishExpandedAsync(TItem item, bool isExpanded)
    {
        var list = _expanded.Values.ToList();
        _seenExpanded = list;

        if (ExpandedItemsChanged.HasDelegate)
        {
            await ExpandedItemsChanged.InvokeAsync(list);
        }

        if (OnRowExpandChanged.HasDelegate)
        {
            await OnRowExpandChanged.InvokeAsync((item, isExpanded));
        }
    }

    private string ExpandRowLabel(TItem item, bool isExpanded)
    {
        var label = RowLabel?.Invoke(item);
        if (string.IsNullOrWhiteSpace(label))
        {
            var firstVal = Columns.Count > 0 && Columns[0].Value is not null
                ? Convert.ToString(Columns[0].Value!(item), CultureInfo.CurrentCulture)
                : null;

            label = !string.IsNullOrWhiteSpace(firstVal) ? firstVal : "row";
        }

        return Format(isExpanded ? EffectiveText.CollapseRow : EffectiveText.ExpandRow, label);
    }

    private int IndexOf(TItem item)
    {
        for (var i = 0; i < _view.Count; i++)
        {
            if (EqualityComparer<TItem>.Default.Equals(_view[i], item))
            {
                return i;
            }
        }
        return -1;
    }

    private string RowDetailId(TItem item)
    {
        var key = KeySelector?.Invoke(item)?.ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            var index = IndexOf(item);
            key = index >= 0 ? index.ToString(CultureInfo.InvariantCulture) : Math.Abs(item?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
        }

        return $"{Id}-detail-{SanitizeId(key)}";
    }

    private string RowTriggerId(TItem item)
    {
        var key = KeySelector?.Invoke(item)?.ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            var index = IndexOf(item);
            key = index >= 0 ? index.ToString(CultureInfo.InvariantCulture) : Math.Abs(item?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
        }

        return $"{Id}-expand-{SanitizeId(key)}";
    }

    private static string SanitizeId(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9_\-]", "-");

    private static string RowClass(bool selected, bool expanded) =>
        CssBuilder.Empty
            .AddClass("cy-data-table__row")
            .AddClass("cy-data-table__row--selected", selected)
            .AddClass("cy-data-table__row--expanded", expanded)
            .Build();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CancelLoad();
        GC.SuppressFinalize(this);
    }
}
