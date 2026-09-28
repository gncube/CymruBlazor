using CymruBlazor.Enums;

namespace CymruBlazor.Localisation;

/// <summary>
/// Default <see cref="ICyLocalizer"/>. Scoped (registered by <c>AddCymruBlazor()</c>)
/// to match Blazor's per-circuit/per-session lifetime, the same as <c>IThemeService</c>.
/// </summary>
/// <remarks>
/// <b>The Welsh (Cymraeg) strings below are the same illustrative text CymruBlazor has
/// shipped since 1.3.0's step-1 pattern and the 1.5.x Demo catalogue - they are carried
/// over here as a starting point, not newly translator-reviewed.</b> They demonstrate the
/// mechanism and are covered by <c>CyLocalizerTests</c>. Replace <see cref="Welsh"/> (and,
/// if your own content also needs it, your app's own catalogue - see the Demo's
/// <c>AppStrings</c>) with text supplied and checked by your translation service before
/// using this in a live bilingual service.
/// </remarks>
public sealed class CyLocalizer : ICyLocalizer
{
    /// <summary>English: identical to every component's own built-in literal default.</summary>
    public static CyLocalizedStrings English { get; } = new()
    {
        AlertDismiss = "Dismiss",
        BreadcrumbLabel = "Breadcrumb",
        NavigationLabel = "Main",
        NavigationOpen = "Open menu",
        NavigationClose = "Close menu",
        ToastRegion = "Notifications",
        ToastDismiss = "Close notification",
        DialogClose = "Close",
        CodeLabel = "Code",
        CopyLabel = "Copy",
        CopiedLabel = "Copied",
        CopyFailedLabel = "Copy failed",
        CopiedMessage = "Code copied to clipboard.",
        CopyFailedMessage = "Copying to clipboard failed.",
        SidebarReveal = "Reveal sidebar",
        SidebarClose = "Close sidebar",
        SidebarSizeGroup = "Sidebar size",
        SidebarExpand = "Expand sidebar",
        SidebarCollapse = "Collapse sidebar",
        SidebarCompact = "Show compact sidebar",
        SidebarIconOnly = "Show icons only",
        SidebarHide = "Hide sidebar",
        SidebarResize = "Resize sidebar",
        PaginationLabel = "Pagination",
        PaginationPrevious = "Previous",
        PaginationNext = "Next",
        PageAriaLabelFormat = "Page {0}",
        CurrentPageAriaLabelFormat = "Current page, page {0}",
        SpinnerLabel = "Loading"
    };

    /// <summary>Cymraeg. See the type-level remarks: carried over from the 1.3.0/1.5.x
    /// illustrative text pending a translator-reviewed replacement.</summary>
    public static CyLocalizedStrings Welsh { get; } = new()
    {
        AlertDismiss = "Diystyru",
        BreadcrumbLabel = "Briwsion bara",
        NavigationLabel = "Prif ddewislen",
        NavigationOpen = "Agor y ddewislen",
        NavigationClose = "Cau'r ddewislen",
        ToastRegion = "Hysbysiadau",
        ToastDismiss = "Cau'r hysbysiad",
        DialogClose = "Cau",
        CodeLabel = "Cod",
        CopyLabel = "Copïo",
        CopiedLabel = "Copïwyd",
        CopyFailedLabel = "Methwyd copïo",
        CopiedMessage = "Copïwyd y cod i'r clipfwrdd.",
        CopyFailedMessage = "Methwyd copïo i'r clipfwrdd.",
        SidebarReveal = "Dangos y bar ochr",
        SidebarClose = "Cau'r bar ochr",
        SidebarSizeGroup = "Maint y bar ochr",
        SidebarExpand = "Ehangu'r bar ochr",
        SidebarCollapse = "Cwympo'r bar ochr",
        SidebarCompact = "Dangos bar ochr cryno",
        SidebarIconOnly = "Dangos eiconau'n unig",
        SidebarHide = "Cuddio'r bar ochr",
        SidebarResize = "Newid maint y bar ochr",
        PaginationLabel = "Tudalennu",
        PaginationPrevious = "Blaenorol",
        PaginationNext = "Nesaf",
        PageAriaLabelFormat = "Tudalen {0}",
        CurrentPageAriaLabelFormat = "Tudalen {0}, tudalen gyfredol",
        SpinnerLabel = "Wrthi'n llwytho"
    };

    /// <inheritdoc />
    public event Action? LanguageChanged;

    /// <inheritdoc />
    public AppLanguage Language { get; private set; } = AppLanguage.English;

    /// <inheritdoc />
    public CyLocalizedStrings Strings => Language == AppLanguage.Welsh ? Welsh : English;

    /// <inheritdoc />
    public string LangTag => Language == AppLanguage.Welsh ? "cy" : "en";

    /// <inheritdoc />
    public void SetLanguage(AppLanguage language)
    {
        if (language == Language)
        {
            return;
        }

        Language = language;
        LanguageChanged?.Invoke();
    }
}
