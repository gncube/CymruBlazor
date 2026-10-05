using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A row of mutually exclusive choices drawn as one joined control - a view switch such as List / Grid, or a
/// density choice - for settings that apply immediately (use <c>CyTabs</c> to switch panels, <c>CyRadioGroup</c>
/// for a form answer).
/// </summary>
/// <remarks>
/// It is a native radio group underneath, so the browser provides the keyboard model: Tab moves into and out of the
/// group, the arrow keys move and select, and screen readers announce "n of m". The group is named by
/// <see cref="Label"/> (<c>role="radiogroup"</c>). Selection is two-way bindable: <c>@bind-Value</c>. Each option is
/// a <see cref="CyOption{TValue}"/>; <see cref="CyOption{TValue}.Hint"/> is not shown.
/// </remarks>
/// <typeparam name="TValue">The type of the option values.</typeparam>
public partial class CySegmentedControl<TValue> : CyInteractiveComponentBase
{
    private List<CyOption<TValue>> _items = [];

    /// <summary>The accessible name of the group, for example "View". Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>The choices, in order.</summary>
    [Parameter]
    public IEnumerable<CyOption<TValue>>? Items { get; set; }

    /// <summary>The selected value. Two-way bindable.</summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>Raised when the user selects an option.</summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>The control size: <see cref="ComponentSize.Small"/> or <see cref="ComponentSize.Medium"/> (the default).</summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>Stretch the control to the full width, sharing it equally between options.</summary>
    [Parameter]
    public bool FullWidth { get; set; }

    /// <summary>The radio <c>name</c>. Defaults to a value derived from <see cref="CyComponentBase.Id"/>.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-segmented";

    private string GroupName => string.IsNullOrWhiteSpace(Name) ? $"{Id}-group" : Name;

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Size == ComponentSize.Small ? "cy-segmented--sm" : "cy-segmented--md")
            .AddClass("cy-segmented--full", FullWidth)
            .AddClass("cy-segmented--disabled", Disabled)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CySegmentedControl<TValue>)}.{nameof(Label)} must not be empty: it names the group.");
        }

        _items = Items is null ? [] : [.. Items];
    }

    private bool IsSelected(CyOption<TValue> item) =>
        EqualityComparer<TValue>.Default.Equals(item.Value, Value!);

    private static string OptionClass(bool selected, bool disabled) =>
        CssBuilder.Empty
            .AddClass("cy-segmented__option")
            .AddClass("cy-segmented__option--selected", selected)
            .AddClass("cy-segmented__option--disabled", disabled)
            .Build();

    private async Task SelectAsync(CyOption<TValue> item)
    {
        Value = item.Value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(item.Value);
        }
    }
}
