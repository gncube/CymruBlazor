namespace CymruBlazor.Components.Content;

/// <summary>
/// Every phrase <see cref="CySortableList{TItem}"/> speaks or labels, as format strings, so a Welsh (or any other)
/// service can replace them. <c>{0}</c> is always the item (or list) name; the other placeholders are listed per property.
/// </summary>
/// <remarks>
/// Pass an instance to <see cref="CySortableList{TItem}.Text"/>: <c>Text="@(new CySortableListText { MoveUp = "Symud {0} i fyny" })"</c>.
/// Unset properties keep their English defaults.
/// </remarks>
public sealed record CySortableListText
{
    /// <summary>Announced on pick-up. {0} item, {1} position, {2} count.</summary>
    public string PickedUp { get; init; } = "Picked up {0}, position {1} of {2}.";

    /// <summary>Announced when the item shifts. {0} item, {1} position, {2} count.</summary>
    public string Moved { get; init; } = "Moved {0} to position {1} of {2}.";

    /// <summary>Announced on drop. {0} item, {1} position, {2} count.</summary>
    public string Dropped { get; init; } = "Dropped {0} at position {1} of {2}.";

    /// <summary>Announced on cancel. {0} item, {1} original position, {2} count.</summary>
    public string Cancelled { get; init; } = "Reorder cancelled. {0} is back at position {1} of {2}.";

    /// <summary>Announced after a move to another list. {0} item, {1} list, {2} position, {3} count.</summary>
    public string MovedToList { get; init; } = "Moved {0} to {1}, position {2} of {3}.";

    /// <summary>Accessible name of the drag handle. {0} item.</summary>
    public string DragHandle { get; init; } = "Reorder {0}";

    /// <summary>Accessible name of the Move up button. {0} item.</summary>
    public string MoveUp { get; init; } = "Move {0} up";

    /// <summary>Accessible name of the Move down button. {0} item.</summary>
    public string MoveDown { get; init; } = "Move {0} down";

    /// <summary>Accessible name of the Move to menu. {0} item.</summary>
    public string MoveTo { get; init; } = "Move {0} to another list";

    /// <summary>Keyboard instructions, read as the handle's description.</summary>
    public string Instructions { get; init; } =
        "Press Space to pick up, then the up and down arrow keys to move, Space to drop and Escape to cancel.";

    /// <summary>Name used for an item whose label is empty.</summary>
    public string ItemFallback { get; init; } = "item";
}
