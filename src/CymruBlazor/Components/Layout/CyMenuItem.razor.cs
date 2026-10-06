using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// One action inside a <see cref="CyMenu"/>. Activating it (click, <c>Enter</c> or <c>Space</c>)
/// closes the menu, returns focus to the trigger, then raises <see cref="OnClick"/>.
/// </summary>
/// <remarks>
/// A disabled item stays visible, is announced as <c>aria-disabled</c>, and is skipped by arrow-key navigation.
/// Use <see cref="Destructive"/> for removals; it changes only the colour, so the item's text must still say what it does.
/// </remarks>
public partial class CyMenuItem : CyComponentBase, IDisposable
{
    private ElementReference _button;

    /// <summary>The menu this item belongs to.</summary>
    [CascadingParameter]
    private CyMenu? Menu { get; set; }

    /// <summary>The item's text, used when <see cref="ChildContent"/> is not set.</summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>Rich item content in place of <see cref="Text"/>.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Optional leading icon name (see <c>IconRegistry</c>); decorative.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Disables the item.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Marks a destructive action such as "Remove" with the danger colour.</summary>
    [Parameter]
    public bool Destructive { get; set; }

    /// <summary>Raised when the item is activated, after the menu has closed.</summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-menu__item";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-menu__item--destructive", Destructive)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (Menu is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyMenuItem)} must be placed inside a {nameof(CyMenu)}.");
        }

        Menu.Register(this);
    }

    private Task ActivateAsync() => Menu is null ? Task.CompletedTask : Menu.ActivateAsync(this);

    internal void RefreshVisualState()
    {
        RefreshCssState();
        StateHasChanged();
    }

    internal async ValueTask FocusAsync()
    {
        try
        {
            await _button.FocusAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone, or the item was removed.
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Menu?.Unregister(this);
        GC.SuppressFinalize(this);
    }
}
