using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Data;

/// <summary>
/// Page navigation for a set of results too large to show at once - e.g.
/// pages of a <see cref="CyTable"/>. Renders "Previous"/"Next" controls
/// plus a truncated run of page numbers (with an ellipsis for large page
/// counts), following the same boundary/sibling truncation model as
/// GOV.UK's and Material UI's pagination components.
///
/// Deliberately does not implement a data grid (roadmap decision D5) -
/// this component only renders and raises page-change events; the caller
/// owns fetching/slicing the underlying data for the new page.
/// </summary>
public partial class CyPagination : CyComponentBase
{
    /// <summary>
    /// The current page, 1-based. Two-way bindable via
    /// <c>@bind-CurrentPage</c>.
    /// </summary>
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    /// <summary>
    /// The total number of pages. When 0 or 1, <see cref="CyPagination"/>
    /// renders nothing - there is nothing to page between.
    /// </summary>
    [Parameter]
    public int TotalPages { get; set; }

    /// <summary>
    /// How many page numbers are always shown at the very start and end
    /// of the sequence (e.g. 1 shows page 1 and the last page).
    /// </summary>
    [Parameter]
    public int BoundaryCount { get; set; } = 1;

    /// <summary>
    /// How many page numbers are shown immediately either side of the
    /// current page.
    /// </summary>
    [Parameter]
    public int SiblingCount { get; set; } = 1;

    /// <summary>
    /// Accessible name of the pagination landmark. Defaults to the
    /// English "Pagination"; supply a translation (e.g. Welsh) for
    /// bilingual services.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Label for the "previous page" control. Defaults to the English
    /// "Previous".
    /// </summary>
    [Parameter]
    public string? PreviousLabel { get; set; }

    /// <summary>
    /// Label for the "next page" control. Defaults to the English
    /// "Next".
    /// </summary>
    [Parameter]
    public string? NextLabel { get; set; }

    /// <summary>
    /// Composite format string (containing <c>{0}</c>) used to build each
    /// non-current page button's accessible name, e.g. "Page 4". Defaults
    /// to the English "Page {0}".
    /// </summary>
    [Parameter]
    public string? PageAriaLabelFormat { get; set; }

    /// <summary>
    /// Composite format string (containing <c>{0}</c>) used to build the
    /// current page's accessible name, e.g. "Current page, page 4".
    /// Defaults to the English "Current page, page {0}".
    /// </summary>
    [Parameter]
    public string? CurrentPageAriaLabelFormat { get; set; }

    protected override string BaseCssClass => "cy-pagination";

    private IReadOnlyList<int?> PageItems =>
        ComputePageItems(CurrentPage, TotalPages, SiblingCount, BoundaryCount);

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (TotalPages < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyPagination)}.{nameof(TotalPages)} must not be negative.");
        }

        if (SiblingCount < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyPagination)}.{nameof(SiblingCount)} must not be negative.");
        }

        if (BoundaryCount < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyPagination)}.{nameof(BoundaryCount)} must not be negative.");
        }

        if (TotalPages > 0 && (CurrentPage < 1 || CurrentPage > TotalPages))
        {
            throw new InvalidOperationException(
                $"{nameof(CyPagination)}.{nameof(CurrentPage)} ('{CurrentPage}') must be between 1 " +
                $"and {nameof(TotalPages)} ('{TotalPages}').");
        }
    }

    private string FormatPageLabel(int page) =>
        string.Format(
            CultureInfo.CurrentCulture,
            string.IsNullOrWhiteSpace(PageAriaLabelFormat) ? "Page {0}" : PageAriaLabelFormat,
            page);

    private string FormatCurrentPageLabel(int page) =>
        string.Format(
            CultureInfo.CurrentCulture,
            string.IsNullOrWhiteSpace(CurrentPageAriaLabelFormat) ? "Current page, page {0}" : CurrentPageAriaLabelFormat,
            page);

    private async Task GoToAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        CurrentPage = page;

        if (CurrentPageChanged.HasDelegate)
        {
            await CurrentPageChanged.InvokeAsync(page);
        }
    }

    /// <summary>
    /// Computes the run of page numbers to render, with <see langword="null"/>
    /// marking an ellipsis. Ported from the boundary/sibling truncation
    /// algorithm used by GOV.UK's and Material UI's pagination components -
    /// deliberately padding the visible range near either end (rather than
    /// showing the minimum possible set) so the control's width does not
    /// jump sharply as the user pages through.
    /// </summary>
    internal static IReadOnlyList<int?> ComputePageItems(
        int currentPage,
        int totalPages,
        int siblingCount,
        int boundaryCount)
    {
        if (totalPages <= 0)
        {
            return [];
        }

        var startPages = Range(1, Math.Min(boundaryCount, totalPages));
        var endPages = Range(Math.Max(totalPages - boundaryCount + 1, boundaryCount + 1), totalPages);

        var siblingsStart = Math.Max(
            Math.Min(currentPage - siblingCount, totalPages - boundaryCount - (siblingCount * 2) - 1),
            boundaryCount + 2);

        var siblingsEnd = Math.Min(
            Math.Max(currentPage + siblingCount, boundaryCount + (siblingCount * 2) + 2),
            endPages.Count > 0 ? endPages[0] - 2 : totalPages - 1);

        var items = new List<int?>();

        if (boundaryCount > 0)
        {
            items.AddRange(startPages.Select(p => (int?)p));
        }

        if (siblingsStart > boundaryCount + 2)
        {
            items.Add(null);
        }
        else if (boundaryCount + 1 < totalPages - boundaryCount)
        {
            items.Add(boundaryCount + 1);
        }

        items.AddRange(Range(siblingsStart, siblingsEnd).Select(p => (int?)p));

        if (siblingsEnd < totalPages - boundaryCount - 1)
        {
            items.Add(null);
        }
        else if (totalPages - boundaryCount > boundaryCount)
        {
            items.Add(totalPages - boundaryCount);
        }

        if (boundaryCount > 0)
        {
            items.AddRange(endPages.Select(p => (int?)p));
        }

        return items;
    }

    private static List<int> Range(int start, int end) =>
        end < start ? [] : Enumerable.Range(start, end - start + 1).ToList();
}
