using CymruBlazor.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A single expand/collapse section within a <see cref="CyAccordion"/>.
/// Must be used inside a <see cref="CyAccordion"/> - it reads expanded
/// state from, and reports toggles/focus moves to, the cascaded parent.
/// </summary>
public partial class CyAccordionItem : CyLayoutComponentBase, IHasDisabledState, IDisposable
{
    private ElementReference _triggerRef;
    private bool _isRegistered;

    [CascadingParameter]
    internal CyAccordion? Parent { get; set; }

    /// <summary>
    /// A stable identifier for this item within its parent
    /// <see cref="CyAccordion"/>, used to track expanded/collapsed state
    /// and keyboard focus order. Distinct from the inherited HTML
    /// <see cref="CyComponentBase.Id"/>, which is only ever the DOM id.
    /// </summary>
    [Parameter, EditorRequired]
    public required string ItemId { get; set; }

    /// <summary>
    /// The section header text.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    protected override string BaseCssClass => "cy-accordion-item";

    private bool IsExpanded => Parent?.IsExpanded(ItemId) ?? false;

    private string HeaderId => $"{Id}-header";

    private string PanelId => $"{Id}-panel";

    /// <summary>
    /// The heading level (1-6) announced for this item's title, via <c>aria-level</c>. Defaults to 3.
    /// Set it to one more than the nearest preceding heading so assistive technology gets an unbroken outline
    /// (the visual style does not change).
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 3;

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (HeadingLevel is < 1 or > 6)
        {
            throw new InvalidOperationException(
                $"{nameof(CyAccordionItem)}.{nameof(HeadingLevel)} must be between 1 and 6. Received '{HeadingLevel}'.");
        }

        if (Parent is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CyAccordionItem)} must be used inside a {nameof(CyAccordion)}.");
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        // Registered (and re-registered) on every render rather than only
        // firstRender, since the ElementReference underlying the same
        // logical button can change across renders.
        Parent?.RegisterTrigger(ItemId, _triggerRef);

        _isRegistered = true;
    }

    private Task ToggleAsync()
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        Parent?.Toggle(ItemId);

        return Task.CompletedTask;
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Parent is null)
        {
            return;
        }

        await Parent.MoveFocusAsync(ItemId, args);
    }

    public void Dispose()
    {
        if (_isRegistered)
        {
            Parent?.UnregisterTrigger(ItemId);
        }

        GC.SuppressFinalize(this);
    }
}
