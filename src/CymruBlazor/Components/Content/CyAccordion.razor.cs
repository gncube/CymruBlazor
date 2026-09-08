using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A vertically stacked set of expand/collapse sections, each rendered by
/// a child <see cref="CyAccordionItem"/>. Implements the WAI-ARIA
/// Accordion pattern: each item is a button with <c>aria-expanded</c>
/// controlling a region, and Up/Down/Home/End move focus between the
/// section headers.
///
/// Unlike <c>CyTabs</c>, items render themselves independently rather
/// than the parent extracting a shared header strip - each item's header
/// and (when expanded) panel sit together in document order, which is
/// the correct accordion structure.
/// </summary>
public partial class CyAccordion : CyLayoutComponentBase
{
    private readonly HashSet<string> _expandedItemIds = [];
    private readonly List<string> _itemOrder = [];
    private readonly Dictionary<string, ElementReference> _triggerRefs = [];

    /// <summary>
    /// When <see langword="false"/> (the default), expanding an item
    /// collapses any other currently-expanded item. When
    /// <see langword="true"/>, any number of items may be expanded
    /// simultaneously.
    /// </summary>
    [Parameter]
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// The <see cref="CyAccordionItem.ItemId"/> of the item that should
    /// start expanded. Ignored if no item with that id is registered.
    /// </summary>
    [Parameter]
    public string? DefaultExpandedItemId { get; set; }

    protected override string BaseCssClass => "cy-accordion";

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (!string.IsNullOrWhiteSpace(DefaultExpandedItemId))
        {
            _expandedItemIds.Add(DefaultExpandedItemId);
        }
    }

    internal bool IsExpanded(string itemId) => _expandedItemIds.Contains(itemId);

    internal void Toggle(string itemId)
    {
        if (_expandedItemIds.Contains(itemId))
        {
            _expandedItemIds.Remove(itemId);
        }
        else
        {
            if (!AllowMultiple)
            {
                _expandedItemIds.Clear();
            }

            _expandedItemIds.Add(itemId);
        }

        StateHasChanged();
    }

    internal void RegisterTrigger(string itemId, ElementReference elementRef)
    {
        if (!_itemOrder.Contains(itemId))
        {
            _itemOrder.Add(itemId);
        }

        _triggerRefs[itemId] = elementRef;
    }

    internal void UnregisterTrigger(string itemId)
    {
        _itemOrder.Remove(itemId);
        _triggerRefs.Remove(itemId);
    }

    /// <summary>
    /// Moves focus between section header buttons per the WAI-ARIA
    /// Accordion keyboard pattern. Home/End are not prevented from their
    /// browser-default scroll-to-top/bottom behaviour, since Blazor
    /// cannot conditionally call preventDefault per key without adding a
    /// JS interop layer purely for this - a page-scroll side effect
    /// alongside the intended focus move is judged an acceptable trade
    /// against introducing JavaScript for it.
    /// </summary>
    internal async Task MoveFocusAsync(string currentItemId, KeyboardEventArgs args)
    {
        if (_itemOrder.Count == 0)
        {
            return;
        }

        var currentIndex = _itemOrder.IndexOf(currentItemId);

        if (currentIndex < 0)
        {
            return;
        }

        var targetIndex = args.Key switch
        {
            "ArrowDown" => (currentIndex + 1) % _itemOrder.Count,
            "ArrowUp" => (currentIndex - 1 + _itemOrder.Count) % _itemOrder.Count,
            "Home" => 0,
            "End" => _itemOrder.Count - 1,
            _ => (int?)null
        };

        if (targetIndex is null)
        {
            return;
        }

        var targetItemId = _itemOrder[targetIndex.Value];

        if (_triggerRefs.TryGetValue(targetItemId, out var elementRef))
        {
            await elementRef.FocusAsync();
        }
    }
}
