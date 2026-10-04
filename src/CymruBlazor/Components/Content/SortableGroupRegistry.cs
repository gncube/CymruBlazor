namespace CymruBlazor.Components.Content;

/// <summary>
/// What a <see cref="CySortableList{TItem}"/> exposes to other lists of the same group, so one list can hand an item to another.
/// </summary>
internal interface ISortableList
{
    string ListId { get; }

    string ListLabel { get; }

    string? ListGroup { get; }

    int Count { get; }

    Type ItemType { get; }

    Task ReceiveAsync(ISortableList source, object item, int oldIndex, int newIndex);
}

/// <summary>
/// Tracks the sortable lists of one circuit/session, by group. Scoped (see <c>AddCymruBlazor()</c>), never static,
/// so users of a Blazor Server app cannot see each other's lists. Resolved optionally: without it a list still
/// reorders within itself.
/// </summary>
internal sealed class SortableGroupRegistry
{
    private readonly Lock _gate = new();
    private readonly List<ISortableList> _lists = [];

    public void Register(ISortableList list)
    {
        lock (_gate)
        {
            if (!_lists.Contains(list))
            {
                _lists.Add(list);
            }
        }
    }

    public void Unregister(ISortableList list)
    {
        lock (_gate)
        {
            _lists.Remove(list);
        }
    }

    /// <summary>The other lists in <paramref name="list"/>'s group that hold the same item type, in registration order.</summary>
    public IReadOnlyList<ISortableList> Peers(ISortableList list)
    {
        if (string.IsNullOrEmpty(list.ListGroup))
        {
            return [];
        }

        lock (_gate)
        {
            return _lists
                .Where(l => !ReferenceEquals(l, list)
                            && l.ItemType == list.ItemType
                            && string.Equals(l.ListGroup, list.ListGroup, StringComparison.Ordinal))
                .ToList();
        }
    }

    public ISortableList? Find(ISortableList from, string listId) =>
        Peers(from).FirstOrDefault(l => string.Equals(l.ListId, listId, StringComparison.Ordinal));
}
