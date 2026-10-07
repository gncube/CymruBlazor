using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Data;

/// <summary>Whether, and how, rows of a <see cref="CyDataTable{TItem}"/> can be selected.</summary>
public enum CyDataSelectionMode
{
    /// <summary>No selection controls.</summary>
    None,

    /// <summary>One row at a time (a radio button per row).</summary>
#pragma warning disable CA1720 // "Single" describes the mode; it is not a reference to System.Single.
    Single,
#pragma warning restore CA1720

    /// <summary>Any number of rows (a checkbox per row, and "select all on this page" in the header).</summary>
    Multiple
}

/// <summary>Horizontal alignment of a column. Right-align (<see cref="End"/>) numbers.</summary>
public enum CyColumnAlign
{
    /// <summary>The start edge (left in English and Welsh). Default.</summary>
    Start,

    /// <summary>The end edge (right in English and Welsh).</summary>
    End,

    /// <summary>Centred.</summary>
    Center
}

/// <summary>What a <see cref="CyDataTable{TItem}"/> asks its <c>ItemsProvider</c> for.</summary>
/// <param name="Page">The page to return, starting at 1.</param>
/// <param name="PageSize">Rows per page; 0 when paging is off (return everything).</param>
/// <param name="SortKey">The key of the column to sort by, or <see langword="null"/> for the data's natural order.</param>
/// <param name="SortDescending">Whether the sort is descending.</param>
public sealed record CyDataTableRequest(int Page, int PageSize, string? SortKey, bool SortDescending);

/// <summary>What an <c>ItemsProvider</c> returns.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="Items">The rows of the requested page, already sorted.</param>
/// <param name="TotalCount">The number of rows on all pages.</param>
public sealed record CyDataTableResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);

/// <summary>One column of a <see cref="CyDataTable{TItem}"/>.</summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed class CyDataColumn<TItem>
{
    /// <summary>The column heading. Required: it is the column header cell's text and the sort button's name.</summary>
    public required string Header { get; init; }

    /// <summary>Renders the cell. Takes precedence over <see cref="Value"/>.</summary>
    public RenderFragment<TItem>? Cell { get; init; }

    /// <summary>
    /// Selects the cell's value, shown as encoded text, and (when <see cref="Compare"/> is not set) the value rows are sorted by.
    /// Sorting uses the default comparer, so the values must be comparable (strings, numbers, dates).
    /// </summary>
    public Func<TItem, object?>? Value { get; init; }

    /// <summary>Whether the heading is a button that sorts by this column.</summary>
    public bool Sortable { get; init; }

    /// <summary>
    /// Identifies the column in <c>SortKey</c> and in <see cref="CyDataTableRequest"/>. Defaults to <see cref="Header"/>; set it
    /// when the sort key your server expects differs from the heading.
    /// </summary>
    public string? SortKey { get; init; }

    /// <summary>Compares two rows, for sorting in memory. Overrides sorting by <see cref="Value"/>.</summary>
    public Comparison<TItem>? Compare { get; init; }

    /// <summary>Horizontal alignment of the heading and cells.</summary>
    public CyColumnAlign Align { get; init; }

    /// <summary>Lets this column's text wrap, even when the table's <c>Wrap</c> is off.</summary>
    public bool Wrap { get; init; }

    /// <summary>Single line with an ellipsis (the full text is the cell's <c>title</c> when <see cref="Value"/> is used).</summary>
    public bool Truncate { get; init; }

    /// <summary>A plain length such as <c>12rem</c> or <c>20%</c>.</summary>
    public string? Width { get; init; }

    /// <summary>Makes this column's cells row headers (<c>&lt;th scope="row"&gt;</c>). Use it for the column that names the row.</summary>
    public bool RowHeader { get; init; }

    /// <summary>Hides the heading text visually but keeps it for assistive technology (for example an icon column).</summary>
    public bool HeaderVisuallyHidden { get; init; }

    internal string Key => string.IsNullOrWhiteSpace(SortKey) ? Header : SortKey;
}
