using System.Diagnostics.CodeAnalysis;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Describes one completed move in a <see cref="CySortableList{TItem}"/>, raised by
/// <see cref="CySortableList{TItem}.OnReorder"/>.
/// </summary>
/// <remarks>
/// <see cref="CySortableList{TItem}"/> never changes your collection: apply the move to your own data in the
/// handler (remove the item at <see cref="OldIndex"/> from the source list, insert it at <see cref="NewIndex"/>
/// in the target list) and the list re-renders. For a move between lists, only the <em>receiving</em> list raises
/// the event, once; update both collections from it.
/// </remarks>
/// <typeparam name="TItem">The item type.</typeparam>
/// <param name="Item">The item that moved.</param>
/// <param name="OldIndex">Its index in the source list before the move.</param>
/// <param name="NewIndex">
/// Its index in the target list after the move. For a move within one list this is its final index once it has been
/// removed from <paramref name="OldIndex"/> and inserted.
/// </param>
/// <param name="SourceListId">The <c>Id</c> of the list the item came from.</param>
/// <param name="TargetListId">The <c>Id</c> of the list that received it (equal to the source for a reorder).</param>
/// <param name="Group">The shared <c>Group</c> of the lists, or <see langword="null"/> when the list has none.</param>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Follows the Blazor naming convention for the payload of an EventCallback " +
        "(MouseEventArgs, ChangeEventArgs). It is a record, not a System.EventArgs subclass, " +
        "so the .NET event-pattern naming rule does not apply.")]
public sealed record CyReorderEventArgs<TItem>(
    TItem Item,
    int OldIndex,
    int NewIndex,
    string SourceListId,
    string TargetListId,
    string? Group)
{
    /// <summary>True when the item moved from one list to another.</summary>
    public bool IsCrossList => !string.Equals(SourceListId, TargetListId, StringComparison.Ordinal);
}
