using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Content;

/// <summary>
/// The grip that picks up an item in a <see cref="CySortableList{TItem}"/>. It is a real button, at least 32x32
/// CSS px (WCAG 2.5.8 asks for 24x24), with <c>aria-pressed</c> showing whether the item is picked up.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CySortableList{TItem}"/> renders one per item; use this component directly only when building your
/// own list. Pointer dragging is wired by the list's script through the <c>data-cy-drag-handle</c> attribute.
/// </para>
/// <para>
/// <see cref="OnActivate"/> fires for keyboard and assistive-technology activation (<c>Space</c>/<c>Enter</c>, a
/// screen reader's "click") but <em>not</em> for a real mouse or touch click, which is the start of a drag, never
/// a pick-up. Every drag therefore has a keyboard route here and a button route in the list (WCAG 2.5.7).
/// </para>
/// </remarks>
public partial class CyDragHandle : CyComponentBase
{
    /// <summary>The accessible name, for example "Reorder Disease status". Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>Whether the item is currently picked up (<c>aria-pressed</c>).</summary>
    [Parameter]
    public bool Picked { get; set; }

    /// <summary>Disables the handle.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Id of the element holding keyboard instructions (<c>aria-describedby</c>).</summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>Raised when the handle is activated from the keyboard or by assistive technology.</summary>
    [Parameter]
    public EventCallback OnActivate { get; set; }

    /// <summary>Raised for every key pressed while the handle has focus.</summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>Raised when focus leaves the handle.</summary>
    [Parameter]
    public EventCallback OnBlur { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-drag-handle";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-drag-handle--picked", Picked)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyDragHandle)}.{nameof(Label)} must not be empty: an icon-only button needs an accessible name.");
        }
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        // Detail is 0 for a click synthesised by the keyboard or assistive technology, and >= 1 for a pointer.
        if (args.Detail == 0 && OnActivate.HasDelegate)
        {
            await OnActivate.InvokeAsync();
        }
    }

    private Task HandleKeyDownAsync(KeyboardEventArgs args) =>
        OnKeyDown.HasDelegate ? OnKeyDown.InvokeAsync(args) : Task.CompletedTask;

    private Task HandleFocusOutAsync() =>
        OnBlur.HasDelegate ? OnBlur.InvokeAsync() : Task.CompletedTask;
}
