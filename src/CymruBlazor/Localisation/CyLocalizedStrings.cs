namespace CymruBlazor.Localisation;

/// <summary>
/// Every string a CymruBlazor component can fall back to when its own
/// <c>string?</c> override parameter is left unset, for one language.
/// Property names match the corresponding component parameter 1:1 (see
/// <see cref="ICyLocalizer"/>'s remarks for the mapping) so consuming code
/// migrating from a hand-rolled catalogue - such as the Demo's
/// <c>LibraryStrings</c> - can do so field by field.
/// </summary>
/// <remarks>
/// This record only covers parameters that are genuinely optional
/// (<c>string?</c>, defaulting to a literal via <c>??</c>) - see
/// <see cref="ICyLocalizer"/>'s remarks for why a handful of localisable
/// parameters (e.g. <c>CyDateInput.DayLabel</c>, <c>CyTextArea</c>'s
/// character-count formats) are deliberately not part of this record.
/// </remarks>
public sealed record CyLocalizedStrings
{
    public required string AlertDismiss { get; init; }

    public required string BreadcrumbLabel { get; init; }

    public required string NavigationLabel { get; init; }

    public required string NavigationOpen { get; init; }

    public required string NavigationClose { get; init; }

    public required string ToastRegion { get; init; }

    public required string ToastDismiss { get; init; }

    public required string DialogClose { get; init; }

    public required string CodeLabel { get; init; }

    public required string CopyLabel { get; init; }

    public required string CopiedLabel { get; init; }

    public required string CopyFailedLabel { get; init; }

    public required string CopiedMessage { get; init; }

    public required string CopyFailedMessage { get; init; }

    public required string SidebarReveal { get; init; }

    public required string SidebarClose { get; init; }

    public required string SidebarSizeGroup { get; init; }

    public required string SidebarExpand { get; init; }

    public required string SidebarCollapse { get; init; }

    public required string SidebarCompact { get; init; }

    public required string SidebarIconOnly { get; init; }

    public required string SidebarHide { get; init; }

    public required string SidebarResize { get; init; }

    public required string PaginationLabel { get; init; }

    public required string PaginationPrevious { get; init; }

    public required string PaginationNext { get; init; }

    public required string PageAriaLabelFormat { get; init; }

    public required string CurrentPageAriaLabelFormat { get; init; }

    public required string SpinnerLabel { get; init; }
}
