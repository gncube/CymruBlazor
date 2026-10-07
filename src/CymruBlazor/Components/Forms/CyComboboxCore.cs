using System.Globalization;
using Microsoft.AspNetCore.Components.Web;

namespace CymruBlazor.Components.Forms;

/// <summary>What the combobox engine needs from the component that owns the selection.</summary>
internal interface ICyComboboxSelection<TValue>
{
    /// <summary>True for <see cref="CyMultiCombobox{TValue}"/>.</summary>
    bool Multiple { get; }

    /// <summary>Whether <paramref name="value"/> is currently selected.</summary>
    bool IsSelected(TValue value);

    /// <summary>Single: the text of the selected option, or empty. Multiple: always empty.</summary>
    string DisplayText { get; }

    /// <summary>Whether anything is selected.</summary>
    bool HasSelection { get; }

    /// <summary>Adds (multiple) or replaces (single) the selection.</summary>
    Task SelectAsync(CyOption<TValue> option);

    /// <summary>Removes one option from the selection (multiple only).</summary>
    Task RemoveAsync(CyOption<TValue> option);

    /// <summary>Clears the whole selection.</summary>
    Task ClearAsync();
}

/// <summary>
/// The behaviour shared by <see cref="CyCombobox{TValue}"/> and <see cref="CyMultiCombobox{TValue}"/>: filtering,
/// debounced async search with stale-response discard, the active option, and the live-region status text. Holds no
/// markup; the two components render it. Not thread-safe: call from the component's synchronisation context.
/// </summary>
internal sealed class CyComboboxCore<TValue> : IDisposable
{
    private enum Pending { None, Selected, First, Last }

    private readonly ICyComboboxSelection<TValue> _selection;
    private CancellationTokenSource? _cts;
    private int _searchVersion;
    private Pending _pending;
    private string _lastQuery = string.Empty;

    internal CyComboboxCore(ICyComboboxSelection<TValue> selection) => _selection = selection;

    // ---- settings, assigned by the component on every parameter set

    internal IReadOnlyList<CyOption<TValue>>? Items { get; set; }

    internal Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<TValue>>>>? Provider { get; set; }

    internal Func<CyOption<TValue>, string, bool>? Filter { get; set; }

    internal int DebounceMilliseconds { get; set; } = 250;

    internal int MinSearchLength { get; set; }

    internal int MaxResults { get; set; } = 50;

    internal bool Disabled { get; set; }

    internal bool ReadOnly { get; set; }

    internal bool CloseOnSelect { get; set; } = true;

    internal CyComboboxText Text { get; set; } = new();

    // ---- state

    internal bool Expanded { get; private set; }

    internal int ActiveIndex { get; private set; } = -1;

    internal string InputText { get; set; } = string.Empty;

    /// <summary>True while the text in the input was typed by the user rather than set from the selection.</summary>
    internal bool Typing { get; private set; }

    internal bool Loading { get; private set; }

    internal bool LoadFailed { get; private set; }

    internal string StatusText { get; private set; } = string.Empty;

    /// <summary>The visible message shown in the popup instead of (or above) the options, or empty.</summary>
    internal string Message { get; private set; } = string.Empty;

    internal IReadOnlyList<CyOption<TValue>> Results { get; private set; } = [];

    internal int TotalMatches { get; private set; }

    internal event Action? Changed;

    internal string? ActiveDescendantId(string listId) =>
        Expanded && ActiveIndex >= 0 && ActiveIndex < Results.Count ? OptionId(listId, ActiveIndex) : null;

    internal static string OptionId(string listId, int index) => $"{listId}-option-{index}";

    internal bool PopupVisible => Expanded && (Results.Count > 0 || Message.Length > 0);

    // ---- text

    /// <summary>Sets the input text from the selection (used when the value changes from outside).</summary>
    internal void SetTextFromSelection(string text)
    {
        if (!Typing)
        {
            InputText = text;
        }
    }

    // ---- input events

    internal async Task HandleInputAsync(string? text)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        InputText = text ?? string.Empty;
        Typing = true;
        Expanded = true;
        ActiveIndex = -1;
        _pending = Pending.None;
        await SearchAsync(InputText, debounce: true);
    }

    internal async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                if (!Expanded)
                {
                    await OpenAsync(args.AltKey ? Pending.None : Pending.Selected);
                }
                else if (!args.AltKey)
                {
                    Move(+1);
                }

                break;

            case "ArrowUp":
                if (!Expanded)
                {
                    await OpenAsync(Pending.Last);
                }
                else
                {
                    Move(-1);
                }

                break;

            case "Enter":
                if (Expanded && ActiveIndex >= 0 && ActiveIndex < Results.Count)
                {
                    await SelectAsync(Results[ActiveIndex]);
                }

                break;

            case "Escape":
                if (Expanded)
                {
                    Close();
                }
                else if (InputText.Length > 0 || _selection.HasSelection)
                {
                    await EscapeClearAsync();
                }

                break;

            case "Tab":
                // Leave without selecting; blur does the tidying.
                Close();
                break;
        }

        RaiseChanged();
    }

    internal async Task HandleBlurAsync()
    {
        Close();

        if (Typing)
        {
            Typing = false;

            if (_selection.Multiple)
            {
                InputText = string.Empty;
            }
            else if (InputText.Trim().Length == 0)
            {
                await _selection.ClearAsync();
                InputText = string.Empty;
            }
            else
            {
                InputText = _selection.DisplayText;
            }
        }

        RaiseChanged();
    }

    internal async Task ClearAsync()
    {
        await _selection.ClearAsync();
        InputText = string.Empty;
        Typing = false;
        Close();
        RaiseChanged();
    }

    internal async Task SelectAsync(CyOption<TValue> option)
    {
        if (option.Disabled || Disabled || ReadOnly)
        {
            return;
        }

        // In a multiple combobox, choosing an option that is already selected deselects it.
        if (_selection.Multiple && _selection.IsSelected(option.Value))
        {
            await RemoveAsync(option);
            return;
        }

        await _selection.SelectAsync(option);
        Typing = false;

        if (_selection.Multiple)
        {
            InputText = string.Empty;
            ActiveIndex = -1;

            if (CloseOnSelect)
            {
                Close();
            }
            else
            {
                await SearchAsync(string.Empty, debounce: false);
            }

            StatusText = Format(Text.Selected, option.Text);
        }
        else
        {
            InputText = option.Text;
            Close();
        }

        RaiseChanged();
    }

    internal async Task RemoveAsync(CyOption<TValue> option)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        await _selection.RemoveAsync(option);
        StatusText = Format(Text.Removed, option.Text);
        RaiseChanged();
    }

    // ---- opening, closing, moving

    internal void Close()
    {
        Expanded = false;
        ActiveIndex = -1;
        Loading = false;
        Message = string.Empty;
        StatusText = string.Empty;
        _pending = Pending.None;
        CancelSearch();
    }

    private async Task OpenAsync(Pending pending)
    {
        Expanded = true;
        _pending = pending;
        ActiveIndex = -1;

        // Text that is just the current selection is not a search: show every option.
        await SearchAsync(Typing ? InputText : string.Empty, debounce: false);
    }

    private void Move(int delta)
    {
        var index = ActiveIndex + delta;

        while (index >= 0 && index < Results.Count && Results[index].Disabled)
        {
            index += delta;
        }

        if (index >= Results.Count)
        {
            return;
        }

        ActiveIndex = index < 0 ? -1 : index;
    }

    private async Task EscapeClearAsync()
    {
        InputText = string.Empty;
        Typing = false;

        if (!_selection.Multiple)
        {
            await _selection.ClearAsync();
        }
    }

    // ---- searching

    private void CancelSearch()
    {
        _searchVersion++;

        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    internal async Task SearchAsync(string query, bool debounce)
    {
        _lastQuery = query;
        CancelSearch();
        var version = _searchVersion;

        if (Provider is null)
        {
            Apply(Filtered(query));
            RaiseChanged();
            return;
        }

        if (query.Length < MinSearchLength)
        {
            Loading = false;
            LoadFailed = false;
            Results = [];
            TotalMatches = 0;
            Message = Format(Text.MinLength, MinSearchLength);
            StatusText = Message;
            RaiseChanged();
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Loading = true;
        LoadFailed = false;
        Message = Text.Loading;
        StatusText = Text.Loading;
        RaiseChanged();

        try
        {
            if (debounce && DebounceMilliseconds > 0)
            {
                await Task.Delay(DebounceMilliseconds, token);
            }

            var found = await Provider(new CyComboboxRequest(query), token);

            if (version != _searchVersion || token.IsCancellationRequested)
            {
                return;
            }

            Loading = false;
            Apply(found ?? []);
        }
        catch (OperationCanceledException)
        {
            return;
        }
#pragma warning disable CA1031 // A consumer's provider may throw anything; the combobox must show an error, not crash the circuit.
        catch (Exception)
#pragma warning restore CA1031
        {
            if (version != _searchVersion)
            {
                return;
            }

            Loading = false;
            LoadFailed = true;
            Results = [];
            TotalMatches = 0;
            Message = Text.LoadError;
            StatusText = Text.LoadError;
        }

        RaiseChanged();
    }

    private IReadOnlyList<CyOption<TValue>> Filtered(string query)
    {
        var all = Items ?? [];

        if (query.Length == 0)
        {
            return all;
        }

        return all.Where(option => Filter is null ? DefaultFilter(option, query) : Filter(option, query)).ToList();
    }

    private static bool DefaultFilter(CyOption<TValue> option, string query) =>
        option.Text.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    private void Apply(IReadOnlyList<CyOption<TValue>> matches)
    {
        TotalMatches = matches.Count;
        Results = matches.Count > MaxResults ? matches.Take(MaxResults).ToList() : matches;
        LoadFailed = false;

        if (TotalMatches == 0)
        {
            Message = Text.NoResults;
            StatusText = Text.NoResults;
        }
        else
        {
            Message = TotalMatches > Results.Count ? Format(Text.Capped, Results.Count, TotalMatches) : string.Empty;
            StatusText = TotalMatches > Results.Count
                ? Message
                : TotalMatches == 1 ? Text.OneResult : Format(Text.ResultsAvailable, TotalMatches);
        }

        ActiveIndex = ResolvePending();
        _pending = Pending.None;
    }

    private int ResolvePending()
    {
        switch (_pending)
        {
            case Pending.First:
                return FirstEnabled();

            case Pending.Last:
                for (var i = Results.Count - 1; i >= 0; i--)
                {
                    if (!Results[i].Disabled)
                    {
                        return i;
                    }
                }

                return -1;

            case Pending.Selected:
                for (var i = 0; i < Results.Count; i++)
                {
                    if (!Results[i].Disabled && _selection.IsSelected(Results[i].Value))
                    {
                        return i;
                    }
                }

                return FirstEnabled();

            default:
                return -1;
        }
    }

    private int FirstEnabled()
    {
        for (var i = 0; i < Results.Count; i++)
        {
            if (!Results[i].Disabled)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Re-runs the current search after the consumer's <c>Items</c> changed.</summary>
    internal void RefreshItems()
    {
        if (Provider is null && Expanded)
        {
            Apply(Filtered(_lastQuery));
        }
    }

    private void RaiseChanged() => Changed?.Invoke();

    private static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    public void Dispose()
    {
        CancelSearch();
        Changed = null;
    }
}
