using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// <see cref="CyCombobox{TValue}"/> for choosing <em>several</em> values. The chosen values appear as a list of chips
/// above the input, each with a "Remove {name}" button; the list stays open after a choice so several can be made in a
/// row, and choosing an option that is already chosen removes it. Same WAI-ARIA 1.2 list-autocomplete pattern,
/// <see cref="CyOption{TValue}"/> items, async <see cref="ItemsProvider"/> and live-region announcements as the single
/// combobox; see <see cref="CyCombobox{TValue}"/> for the details they share.
/// </summary>
/// <remarks>
/// <para>
/// <b>Binding.</b> The bound value is an <c>IEnumerable&lt;TValue&gt;</c> (<c>@bind-Value</c>), in the order the user
/// chose. Each change assigns a <em>new</em> collection, so the property you bind must accept
/// <c>IEnumerable&lt;TValue&gt;</c>; for a <c>List&lt;T&gt;</c> or <c>HashSet&lt;T&gt;</c> property pass <c>Value</c> and
/// <c>ValueChanged</c> separately, as with <see cref="CyCheckboxGroup{TValue}"/>.
/// </para>
/// <para>
/// <b>Chip text.</b> A chip needs the option's text. For values you set from your model that are not among
/// <see cref="Items"/> (always the case with <see cref="ItemsProvider"/>), supply their options with
/// <see cref="SelectedOptions"/>; otherwise the chip shows the value's string form. Backspace does not remove the last
/// chip: an unannounced destructive key.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of each chosen value.</typeparam>
public partial class CyMultiCombobox<TValue> : CyFormFieldComponentBase<IEnumerable<TValue>>, ICyComboboxSelection<TValue>, IAsyncDisposable
{
    private static readonly CyComboboxText DefaultText = new();

    private readonly CyComboboxCore<TValue> _core;
    private readonly CyComboboxScript _script = new();
    private ElementReference _input;
    private List<CyOption<TValue>> _chips = [];
    private IReadOnlyList<CyOption<TValue>>? _lastItems;

    /// <summary>Creates the component.</summary>
    public CyMultiCombobox()
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
    /// Asked for the options matching the typed text. Receives a <see cref="CyComboboxRequest"/> and a token that is
    /// cancelled when the user types again.
    /// </summary>
    [Parameter]
    public Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<TValue>>>>? ItemsProvider { get; set; }

    /// <summary>Decides whether an option matches the typed text; defaults to a case-insensitive "contains". Only used with <see cref="Items"/>.</summary>
    [Parameter]
    public Func<CyOption<TValue>, string, bool>? Filter { get; set; }

    /// <summary>Milliseconds to wait after the last keystroke before calling <see cref="ItemsProvider"/>. Default 250.</summary>
    [Parameter]
    public int DebounceMilliseconds { get; set; } = 250;

    /// <summary>Characters needed before <see cref="ItemsProvider"/> is called. Defaults to 1 with a provider, 0 otherwise.</summary>
    [Parameter]
    public int? MinSearchLength { get; set; }

    /// <summary>The most options rendered at once (default 50).</summary>
    [Parameter]
    public int MaxResults { get; set; } = 50;

    /// <summary>Text shown in the empty input. Never a substitute for the label.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>When true the values can be read but not changed.</summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>Whether the list closes after each choice. Defaults to false so several can be chosen in a row.</summary>
    [Parameter]
    public bool CloseOnSelect { get; set; }

    /// <summary>The options (value and text) for values in <c>Value</c> that are not among <see cref="Items"/>.</summary>
    [Parameter]
    public IEnumerable<CyOption<TValue>>? SelectedOptions { get; set; }

    /// <summary>The tallest the list may grow before it scrolls: a plain length such as <c>16rem</c>.</summary>
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

    private string SelectedListLabel => string.Format(CultureInfo.CurrentCulture, EffectiveText.SelectedList, Label);

    private string RemoveLabel(CyOption<TValue> chip) =>
        string.Format(CultureInfo.CurrentCulture, EffectiveText.RemoveItem, chip.Text);

    private bool IsSelectedValue(TValue value) => _chips.Any(chip => EffectiveComparer.Equals(chip.Value, value));

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
                $"{nameof(CyMultiCombobox<TValue>)}: set {nameof(Items)} or {nameof(ItemsProvider)}, not both.");
        }

        CssLength.Require(MaxListHeight, nameof(CyMultiCombobox<TValue>), nameof(MaxListHeight));

        _core.Items = Items;
        _core.Provider = ItemsProvider;
        _core.Filter = Filter;
        _core.DebounceMilliseconds = Math.Max(0, DebounceMilliseconds);
        _core.MinSearchLength = Math.Max(0, MinSearchLength ?? (ItemsProvider is null ? 0 : 1));
        _core.MaxResults = Math.Max(1, MaxResults);
        _core.Disabled = Disabled;
        _core.ReadOnly = ReadOnly;
        _core.CloseOnSelect = CloseOnSelect;
        _core.Text = EffectiveText;

        if (!ReferenceEquals(_lastItems, Items))
        {
            _lastItems = Items;
            _core.RefreshItems();
        }

        SyncFromValue();
    }

    private void SyncFromValue()
    {
        var values = (CurrentValue ?? []).ToList();

        if (values.Count == _chips.Count
            && values.Zip(_chips).All(pair => EffectiveComparer.Equals(pair.First, pair.Second.Value)))
        {
            return;
        }

        var chips = new List<CyOption<TValue>>(values.Count);

        foreach (var value in values)
        {
            chips.Add(Resolve(value));
        }

        _chips = chips;
    }

    private CyOption<TValue> Resolve(TValue value) =>
        _chips.FirstOrDefault(chip => EffectiveComparer.Equals(chip.Value, value))
        ?? SelectedOptions?.FirstOrDefault(option => EffectiveComparer.Equals(option.Value, value))
        ?? Items?.FirstOrDefault(option => EffectiveComparer.Equals(option.Value, value))
        ?? new CyOption<TValue>(value, Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty);

    private Task OnInputAsync(ChangeEventArgs args) => _core.HandleInputAsync(args.Value?.ToString());

    private static void OnListMouseDown()
    {
        // Registered only so the browser's default (moving focus out of the input) is prevented.
    }

    private async Task OnRemoveChipAsync(CyOption<TValue> chip)
    {
        await _core.RemoveAsync(chip);

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

    private void Publish() => CurrentValue = _chips.Select(chip => chip.Value).ToList();

    // ---- ICyComboboxSelection

    bool ICyComboboxSelection<TValue>.Multiple => true;

    bool ICyComboboxSelection<TValue>.IsSelected(TValue value) => IsSelectedValue(value);

    string ICyComboboxSelection<TValue>.DisplayText => string.Empty;

    bool ICyComboboxSelection<TValue>.HasSelection => _chips.Count > 0;

    Task ICyComboboxSelection<TValue>.SelectAsync(CyOption<TValue> option)
    {
        if (!IsSelectedValue(option.Value))
        {
            _chips = [.. _chips, option];
            Publish();
        }

        return Task.CompletedTask;
    }

    Task ICyComboboxSelection<TValue>.RemoveAsync(CyOption<TValue> option)
    {
        var remaining = _chips.Where(chip => !EffectiveComparer.Equals(chip.Value, option.Value)).ToList();

        if (remaining.Count != _chips.Count)
        {
            _chips = remaining;
            Publish();
        }

        return Task.CompletedTask;
    }

    Task ICyComboboxSelection<TValue>.ClearAsync()
    {
        if (_chips.Count > 0)
        {
            _chips = [];
            Publish();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Unreachable: the combobox binds directly to its collection and never parses a string.
    /// </summary>
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out IEnumerable<TValue> result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotSupportedException(
            $"{nameof(CyMultiCombobox<int>)} binds directly to its collection and does not parse a string representation.");
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
