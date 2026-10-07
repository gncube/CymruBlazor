using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A text box with a list of suggestions, for choosing <em>one</em> value from a long or searchable list (a coded term,
/// a country, a clinician). Implements the WAI-ARIA 1.2 combobox pattern with list autocomplete: focus stays in the
/// input, the active option is named by <c>aria-activedescendant</c>, and the number of results (or "No results found")
/// is announced through a polite live region. For several values use <see cref="CyMultiCombobox{TValue}"/>; for a short
/// fixed list use <see cref="CySelect{TValue}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Items.</b> Pass <see cref="Items"/> (filtered in memory) <em>or</em> <see cref="ItemsProvider"/> (asked for matches
/// as the user types, debounced; a slower earlier answer never replaces a newer one; an exception shows an error message
/// instead of crashing). Both use <see cref="CyOption{TValue}"/>, so <c>ToCyOptions(...)</c> works here too.
/// </para>
/// <para>
/// <b>Value.</b> Only an option from the list can be chosen; free text is never a value. To allow "nothing chosen" make
/// <typeparamref name="TValue"/> nullable (<c>string</c>, <c>int?</c>): the type's default value means no selection. With
/// <see cref="ItemsProvider"/> the component cannot look up the text of a value you set from your model, so supply it
/// with <see cref="SelectedText"/>.
/// </para>
/// <para>
/// <b>Keyboard.</b> <c>Down</c> opens the list (on the selected option, if any) and moves; <c>Up</c> moves, or opens on
/// the last option; <c>Alt+Down</c> opens without moving; <c>Enter</c> chooses the active option; <c>Escape</c> closes the
/// list, and pressed again clears the field; <c>Tab</c> leaves without choosing. Leaving the field with text that is not a
/// choice puts the chosen text back (or clears the field if you emptied it).
/// </para>
/// <para>
/// <b>Script.</b> A small on-demand module (<c>cymru-inputs.js</c>) stops <c>Enter</c> on an active option submitting the
/// surrounding form and <c>Escape</c> also closing an enclosing dialog, and scrolls the active option into view. Without
/// it the component still works, minus those. The list opens below the input and is positioned by CSS; an
/// <c>overflow: hidden</c> ancestor will clip it.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of the chosen value.</typeparam>
public partial class CyCombobox<TValue> : CyFormFieldComponentBase<TValue>, ICyComboboxSelection<TValue>, IAsyncDisposable
{
    private static readonly CyComboboxText DefaultText = new();

    private readonly CyComboboxCore<TValue> _core;
    private readonly CyComboboxScript _script = new();
    private ElementReference _input;
    private CyOption<TValue>? _selected;
    private bool _synced;
    private TValue _syncedValue = default!;
    private IReadOnlyList<CyOption<TValue>>? _lastItems;

    /// <summary>Creates the component.</summary>
    public CyCombobox()
    {
        _core = new CyComboboxCore<TValue>(this);
        _core.Changed += OnCoreChanged;
    }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>The options to choose from, filtered in memory as the user types. Use this or <see cref="ItemsProvider"/>.</summary>
    [Parameter]
    public IReadOnlyList<CyOption<TValue>>? Items { get; set; }

    /// <summary>
    /// Asked for the options matching the typed text, for searches too large to hold in memory. Receives a
    /// <see cref="CyComboboxRequest"/> and a token that is cancelled when the user types again.
    /// </summary>
    [Parameter]
    public Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<TValue>>>>? ItemsProvider { get; set; }

    /// <summary>
    /// Decides whether an option matches the typed text. Defaults to a case-insensitive "contains" on
    /// <see cref="CyOption{TValue}.Text"/>. Only used with <see cref="Items"/>.
    /// </summary>
    [Parameter]
    public Func<CyOption<TValue>, string, bool>? Filter { get; set; }

    /// <summary>Milliseconds to wait after the last keystroke before calling <see cref="ItemsProvider"/>. Default 250.</summary>
    [Parameter]
    public int DebounceMilliseconds { get; set; } = 250;

    /// <summary>
    /// How many characters must be typed before <see cref="ItemsProvider"/> is called. Defaults to 1 with a provider and
    /// 0 with <see cref="Items"/>.
    /// </summary>
    [Parameter]
    public int? MinSearchLength { get; set; }

    /// <summary>
    /// The most options rendered at once (default 50). When more match, the list says so and asks the user to keep typing;
    /// there is no virtualisation.
    /// </summary>
    [Parameter]
    public int MaxResults { get; set; } = 50;

    /// <summary>Text shown in the empty input. Never a substitute for <see cref="CyFormFieldComponentBase{TValue}.Label"/>.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>When true the value can be read but not changed, and the list never opens.</summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>Shows a button that clears the choice (default true). It only appears while something is chosen.</summary>
    [Parameter]
    public bool AllowClear { get; set; } = true;

    /// <summary>
    /// The text to show for the current <c>Value</c> when it is not among <see cref="Items"/> (always the case with
    /// <see cref="ItemsProvider"/> until the user chooses something).
    /// </summary>
    [Parameter]
    public string? SelectedText { get; set; }

    /// <summary>The tallest the list may grow before it scrolls: a plain length such as <c>16rem</c> (default 16rem).</summary>
    [Parameter]
    public string? MaxListHeight { get; set; }

    /// <summary>How values are compared. Defaults to <see cref="EqualityComparer{T}.Default"/>.</summary>
    [Parameter]
    public IEqualityComparer<TValue>? Comparer { get; set; }

    /// <summary>The phrases the component announces and labels; defaults to English.</summary>
    [Parameter]
    public CyComboboxText? Text { get; set; }

    private IEqualityComparer<TValue> EffectiveComparer => Comparer ?? EqualityComparer<TValue>.Default;

    private CyComboboxText EffectiveText => Text ?? DefaultText;

    private string ListId => $"{FieldId}-list";

    private string LabelId => $"{FieldId}-label";

    private string OptionId(int index) => CyComboboxCore<TValue>.OptionId(ListId, index);

    private string ListStyle => StyleBuilder.Empty.AddStyle("--cy-combobox-max-height", MaxListHeight).Build();

    private bool ShowClear => AllowClear && !Disabled && !ReadOnly && _selected is not null;

    private string ClearLabel => string.Format(System.Globalization.CultureInfo.CurrentCulture, EffectiveText.Clear, Label);

    private bool IsSelectedValue(TValue value) => _selected is not null && EffectiveComparer.Equals(_selected.Value, value);

    private static string OptionClass(bool selected, bool active, bool disabled) =>
        CssBuilder.Empty
            .AddClass("cy-combobox__option")
            .AddClass("cy-combobox__option--selected", selected)
            .AddClass("cy-combobox__option--active", active)
            .AddClass("cy-combobox__option--disabled", disabled)
            .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Items is not null && ItemsProvider is not null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyCombobox<TValue>)}: set {nameof(Items)} or {nameof(ItemsProvider)}, not both.");
        }

        CssLength.Require(MaxListHeight, nameof(CyCombobox<TValue>), nameof(MaxListHeight));

        _core.Items = Items;
        _core.Provider = ItemsProvider;
        _core.Filter = Filter;
        _core.DebounceMilliseconds = Math.Max(0, DebounceMilliseconds);
        _core.MinSearchLength = Math.Max(0, MinSearchLength ?? (ItemsProvider is null ? 0 : 1));
        _core.MaxResults = Math.Max(1, MaxResults);
        _core.Disabled = Disabled;
        _core.ReadOnly = ReadOnly;
        _core.CloseOnSelect = true;
        _core.Text = EffectiveText;

        if (!ReferenceEquals(_lastItems, Items))
        {
            _lastItems = Items;
            _core.RefreshItems();
        }

        SyncFromValue();
    }

    private static bool IsDefault(TValue value) =>
        EqualityComparer<TValue>.Default.Equals(value, default!) || value is string { Length: 0 };

    /// <summary>The value meaning "nothing chosen": null, or an empty string for a <c>string</c> (so a non-nullable model property stays valid).</summary>
    private static TValue EmptyValue => typeof(TValue) == typeof(string) ? (TValue)(object)string.Empty : default!;

    private void SyncFromValue()
    {
        if (_core.Typing)
        {
            return;
        }

        var value = CurrentValue!;
        var changed = !_synced || !EffectiveComparer.Equals(_syncedValue, value);
        var needsLookup = _selected is null && !IsDefault(value);

        if (!changed && !needsLookup)
        {
            return;
        }

        _synced = true;
        _syncedValue = value;

        var match = Items?.FirstOrDefault(option => EffectiveComparer.Equals(option.Value, value));

        if (match is not null)
        {
            _selected = match;
        }
        else if (_selected is not null && EffectiveComparer.Equals(_selected.Value, value))
        {
            // Keep the option the user chose.
        }
        else if (!IsDefault(value))
        {
            _selected = new CyOption<TValue>(value, SelectedText ?? FormatValueAsString(value) ?? string.Empty);
        }
        else
        {
            _selected = null;
        }

        _core.SetTextFromSelection(_selected?.Text ?? string.Empty);
    }

    private Task OnInputAsync(ChangeEventArgs args) => _core.HandleInputAsync(args.Value?.ToString());

    private static void OnListMouseDown()
    {
        // Registered only so the browser's default (moving focus out of the input) is prevented.
    }

    private async Task OnClearAsync()
    {
        await _core.ClearAsync();

        try
        {
            await _input.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone.
        }
    }

    private void OnCoreChanged() => _ = InvokeAsync(StateHasChanged);

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _script.AttachAsync(JSRuntime, _input);
        }
    }

    // ---- ICyComboboxSelection

    bool ICyComboboxSelection<TValue>.Multiple => false;

    bool ICyComboboxSelection<TValue>.IsSelected(TValue value) => IsSelectedValue(value);

    string ICyComboboxSelection<TValue>.DisplayText => _selected?.Text ?? string.Empty;

    bool ICyComboboxSelection<TValue>.HasSelection => _selected is not null;

    Task ICyComboboxSelection<TValue>.SelectAsync(CyOption<TValue> option)
    {
        _selected = option;
        _synced = true;
        _syncedValue = option.Value;
        CurrentValue = option.Value;
        return Task.CompletedTask;
    }

    Task ICyComboboxSelection<TValue>.RemoveAsync(CyOption<TValue> option) =>
        ((ICyComboboxSelection<TValue>)this).ClearAsync();

    Task ICyComboboxSelection<TValue>.ClearAsync()
    {
        _selected = null;
        _synced = true;
        _syncedValue = EmptyValue;
        CurrentValue = EmptyValue;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Unreachable: the combobox binds directly to the chosen option's value and never parses a string.
    /// </summary>
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotSupportedException(
            $"{nameof(CyCombobox<int>)} binds directly to the chosen option and does not parse a string representation.");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        _core.Dispose();
        await _script.DisposeAsync();

        // Blazor calls only DisposeAsync when both interfaces are present, so release InputBase's EditContext
        // subscription here.
        ((IDisposable)this).Dispose();
    }
}
