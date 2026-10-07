namespace CymruBlazor.Components.Data;

/// <summary>
/// Every phrase <see cref="CyDataTable{TItem}"/> shows, labels or announces, as format strings, so a Welsh (or any other)
/// service can replace them. Unset properties keep their English defaults.
/// </summary>
/// <remarks>Pass an instance to <c>Text</c>: <c>Text="@(new CyDataTableText { NoResults = "Dim canlyniadau" })"</c>.</remarks>
public sealed record CyDataTableText
{
    /// <summary>Announcement after sorting. {0} column heading, {1} the direction word.</summary>
    public string SortedBy { get; init; } = "Sorted by {0}, {1}.";

    /// <summary>The direction word for an ascending sort.</summary>
    public string Ascending { get; init; } = "ascending";

    /// <summary>The direction word for a descending sort.</summary>
    public string Descending { get; init; } = "descending";

    /// <summary>Announcement when sorting is removed.</summary>
    public string SortCleared { get; init; } = "Sorting removed.";

    /// <summary>Hidden heading of the selection column.</summary>
    public string SelectColumn { get; init; } = "Select";

    /// <summary>Accessible name of a row's selection control. {0} the row's label.</summary>
    public string SelectRow { get; init; } = "Select {0}";

    /// <summary>Accessible name of the header checkbox when not every row on the page is selected.</summary>
    public string SelectAllRows { get; init; } = "Select all rows on this page";

    /// <summary>Accessible name of the header checkbox when every row on the page is selected.</summary>
    public string ClearPageRows { get; init; } = "Deselect all rows on this page";

    /// <summary>Visible and announced selection count. {0} the count.</summary>
    public string SelectedCount { get; init; } = "{0} selected";

    /// <summary>The range line under the table. {0} first row, {1} last row, {2} total.</summary>
    public string ShowingRange { get; init; } = "Showing {0} to {1} of {2}";

    /// <summary>Announcement after changing page. {0} page, {1} number of pages.</summary>
    public string PageChanged { get; init; } = "Page {0} of {1}.";

    /// <summary>Accessible name of the pagination. {0} the table's caption.</summary>
    public string PaginationLabel { get; init; } = "Pagination for {0}";

    /// <summary>Shown when there are no rows.</summary>
    public string NoResults { get; init; } = "No results";

    /// <summary>Announcement while data loads.</summary>
    public string Loading { get; init; } = "Loading";

    /// <summary>Shown when the provider failed.</summary>
    public string LoadError { get; init; } = "Could not load the data.";

    /// <summary>The retry button after a failure.</summary>
    public string Retry { get; init; } = "Retry";

    /// <summary>Hidden heading of the row-actions column.</summary>
    public string ActionsHeader { get; init; } = "Actions";
}
