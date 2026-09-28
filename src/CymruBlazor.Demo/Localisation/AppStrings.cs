using CymruBlazor.Enums;
using CymruBlazor.Localisation;

namespace CymruBlazor.Demo.Localisation;

/// <summary>
/// Every string CymruBlazor 1.3 lets you override, in one language. Each property
/// matches one <c>string?</c> parameter on a library component; see
/// <see cref="AppStrings.Catalogue"/> for the mapping.
/// </summary>
public sealed record LibraryStrings
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

    // Forms (v1.4.0)
    public required string DateDay { get; init; }

    public required string DateMonth { get; init; }

    public required string DateYear { get; init; }

    public required string DateDayRangeError { get; init; }

    public required string DateMonthRangeError { get; init; }

    public required string DateYearRangeError { get; init; }

    public required string DateInvalidCombination { get; init; }

    public required string CharacterRemainingFormat { get; init; }

    public required string CharactersRemainingFormat { get; init; }

    public required string CharacterOverLimitFormat { get; init; }

    public required string CharactersOverLimitFormat { get; init; }

    // Data (v1.5.0)
    public required string PaginationLabel { get; init; }

    public required string PaginationPrevious { get; init; }

    public required string PaginationNext { get; init; }

    public required string PageAriaLabelFormat { get; init; }

    public required string CurrentPageAriaLabelFormat { get; init; }

    // Feedback (v1.5.0)
    public required string SpinnerLabel { get; init; }
}

/// <summary>
/// The demo page's own copy. In a real app this is your content (resx, a CMS, ...);
/// it is separate from <see cref="LibraryStrings"/> to make that distinction visible.
/// </summary>
public sealed record SampleContent
{
    public required string AlertTitle { get; init; }

    public required string AlertBody { get; init; }

    public required string CrumbHome { get; init; }

    public required string CrumbAppointments { get; init; }

    public required string SampleNavigationLabel { get; init; }

    public required string SampleSidebarLabel { get; init; }

    public required string NavOverview { get; init; }

    public required string OpenDialog { get; init; }

    public required string DialogTitle { get; init; }

    public required string DialogDescription { get; init; }

    public required string DialogBody { get; init; }

    public required string DialogConfirm { get; init; }

    public required string ShowNotification { get; init; }

    public required string NotificationText { get; init; }
}

/// <summary>
/// Shell and navigation copy across English and Welsh.
/// </summary>
public sealed record NavStrings
{
    public required string GettingStarted { get; init; }
    public required string Foundations { get; init; }
    public required string Branding { get; init; }
    public required string Layout { get; init; }
    public required string Navigation { get; init; }
    public required string Forms { get; init; }
    public required string Content { get; init; }
    public required string Data { get; init; }
    public required string Feedback { get; init; }
    public required string Accessibility { get; init; }
    public required string Overview { get; init; }
    public required string Components { get; init; }
    public required string Docs { get; init; }
    public required string Search { get; init; }
    public required string SearchPlaceholder { get; init; }
    public required string SearchAriaLabel { get; init; }
    public required string SearchTitle { get; init; }
    public required string OpenNavigationMenu { get; init; }
    public required string CloseNavigationMenu { get; init; }
    public required string DocumentationNavigation { get; init; }
    public required string PrimaryNavigation { get; init; }
    public required string ToggleDarkMode { get; init; }
    public required string SwitchToLightMode { get; init; }
    public required string SwitchToDarkMode { get; init; }
    public required string Previous { get; init; }
    public required string Next { get; init; }
    public required string OnThisPage { get; init; }
    public required string Documentation { get; init; }
    public required string Community { get; init; }
    public required string Packages { get; init; }
    public required string NotFoundTitle { get; init; }
    public required string NotFoundBody { get; init; }
    public required string NotFoundHome { get; init; }
    public required string SearchResults { get; init; }
    public required string SearchEmptyFormat { get; init; }
    public required string SearchHint { get; init; }
    public required string PageNavigation { get; init; }
    public required string TableOfContents { get; init; }
    public required string Examples { get; init; }
    public required string ApiReference { get; init; }
    public required string Api { get; init; }
    public required string ComponentDocumentationTabs { get; init; }
}

/// <summary>One library parameter, with how to read its value from a <see cref="LibraryStrings"/>.</summary>
public sealed record StringEntry(string Component, string Parameter, Func<LibraryStrings, string> Get);

/// <summary>
/// Holds the app's current language and the strings for it. Register it as a scoped
/// service and pass values into the library's override parameters:
/// <code>
/// &lt;CyAlert DismissAriaLabel="@T.Library.AlertDismiss" /&gt;
/// </code>
/// Components that appear once (the shell's toast container, navigation, sidebar) are
/// wired in the layout; repeated components are wrapped once. See the Localisation page.
/// </summary>
/// <remarks>
/// <b>The Welsh text below is illustrative.</b> It shows the mechanism and lets the tests
/// prove no English default leaks through. It has not been reviewed by a translator:
/// have production strings supplied and checked by your translation service before use.
/// </remarks>
public sealed class AppStrings
{
    /// <summary>
    /// English: for every field the library's own <see cref="ICyLocalizer"/> now covers,
    /// sourced directly from <see cref="CyLocalizer.English"/> (1.6.0) so there is exactly
    /// one place these literals live. <c>CyDateInput</c>/<c>CyTextArea</c> fields aren't part
    /// of <see cref="CyLocalizedStrings"/> (see <see cref="ICyLocalizer"/>'s remarks) and stay
    /// as Demo literals, identical to those components' own built-in defaults.
    /// </summary>
    public static LibraryStrings EnglishLibrary { get; } = new()
    {
        AlertDismiss = CyLocalizer.English.AlertDismiss,
        BreadcrumbLabel = CyLocalizer.English.BreadcrumbLabel,
        NavigationLabel = CyLocalizer.English.NavigationLabel,
        NavigationOpen = CyLocalizer.English.NavigationOpen,
        NavigationClose = CyLocalizer.English.NavigationClose,
        ToastRegion = CyLocalizer.English.ToastRegion,
        ToastDismiss = CyLocalizer.English.ToastDismiss,
        DialogClose = CyLocalizer.English.DialogClose,
        CodeLabel = CyLocalizer.English.CodeLabel,
        CopyLabel = CyLocalizer.English.CopyLabel,
        CopiedLabel = CyLocalizer.English.CopiedLabel,
        CopyFailedLabel = CyLocalizer.English.CopyFailedLabel,
        CopiedMessage = CyLocalizer.English.CopiedMessage,
        CopyFailedMessage = CyLocalizer.English.CopyFailedMessage,
        SidebarReveal = CyLocalizer.English.SidebarReveal,
        SidebarClose = CyLocalizer.English.SidebarClose,
        SidebarSizeGroup = CyLocalizer.English.SidebarSizeGroup,
        SidebarExpand = CyLocalizer.English.SidebarExpand,
        SidebarCollapse = CyLocalizer.English.SidebarCollapse,
        SidebarCompact = CyLocalizer.English.SidebarCompact,
        SidebarIconOnly = CyLocalizer.English.SidebarIconOnly,
        SidebarHide = CyLocalizer.English.SidebarHide,
        SidebarResize = CyLocalizer.English.SidebarResize,
        DateDay = "Day",
        DateMonth = "Month",
        DateYear = "Year",
        DateDayRangeError = "Day must be a number between 1 and 31.",
        DateMonthRangeError = "Month must be a number between 1 and 12.",
        DateYearRangeError = "Year must be a 4-digit number.",
        DateInvalidCombination = "Enter a real date - that day does not exist in that month.",
        CharacterRemainingFormat = "You have {0} character remaining",
        CharactersRemainingFormat = "You have {0} characters remaining",
        CharacterOverLimitFormat = "You have {0} character too many",
        CharactersOverLimitFormat = "You have {0} characters too many",
        PaginationLabel = CyLocalizer.English.PaginationLabel,
        PaginationPrevious = CyLocalizer.English.PaginationPrevious,
        PaginationNext = CyLocalizer.English.PaginationNext,
        PageAriaLabelFormat = CyLocalizer.English.PageAriaLabelFormat,
        CurrentPageAriaLabelFormat = CyLocalizer.English.CurrentPageAriaLabelFormat,
        SpinnerLabel = CyLocalizer.English.SpinnerLabel
    };

    /// <summary>
    /// Illustrative Welsh (Cymraeg). For fields the library's <see cref="ICyLocalizer"/>
    /// now covers, sourced from <see cref="CyLocalizer.Welsh"/> so the Demo and the library
    /// agree - <b>this is still the same placeholder text carried over from 1.3.0/1.5.x,
    /// not translator-reviewed</b> (see this class's remarks and <see cref="CyLocalizer"/>'s).
    /// The remaining (<c>CyDateInput</c>/<c>CyTextArea</c>) fields are unchanged Demo literals,
    /// equally unreviewed, pending the same translator handoff.
    /// </summary>
    public static LibraryStrings WelshLibrary { get; } = new()
    {
        AlertDismiss = CyLocalizer.Welsh.AlertDismiss,
        BreadcrumbLabel = CyLocalizer.Welsh.BreadcrumbLabel,
        NavigationLabel = CyLocalizer.Welsh.NavigationLabel,
        NavigationOpen = CyLocalizer.Welsh.NavigationOpen,
        NavigationClose = CyLocalizer.Welsh.NavigationClose,
        ToastRegion = CyLocalizer.Welsh.ToastRegion,
        ToastDismiss = CyLocalizer.Welsh.ToastDismiss,
        DialogClose = CyLocalizer.Welsh.DialogClose,
        CodeLabel = CyLocalizer.Welsh.CodeLabel,
        CopyLabel = CyLocalizer.Welsh.CopyLabel,
        CopiedLabel = CyLocalizer.Welsh.CopiedLabel,
        CopyFailedLabel = CyLocalizer.Welsh.CopyFailedLabel,
        CopiedMessage = CyLocalizer.Welsh.CopiedMessage,
        CopyFailedMessage = CyLocalizer.Welsh.CopyFailedMessage,
        SidebarReveal = CyLocalizer.Welsh.SidebarReveal,
        SidebarClose = CyLocalizer.Welsh.SidebarClose,
        SidebarSizeGroup = CyLocalizer.Welsh.SidebarSizeGroup,
        SidebarExpand = CyLocalizer.Welsh.SidebarExpand,
        SidebarCollapse = CyLocalizer.Welsh.SidebarCollapse,
        SidebarCompact = CyLocalizer.Welsh.SidebarCompact,
        SidebarIconOnly = CyLocalizer.Welsh.SidebarIconOnly,
        SidebarHide = CyLocalizer.Welsh.SidebarHide,
        SidebarResize = CyLocalizer.Welsh.SidebarResize,
        DateDay = "Diwrnod",
        DateMonth = "Mis",
        DateYear = "Blwyddyn",
        DateDayRangeError = "Rhaid i'r diwrnod fod yn rif rhwng 1 a 31.",
        DateMonthRangeError = "Rhaid i'r mis fod yn rif rhwng 1 a 12.",
        DateYearRangeError = "Rhaid i'r flwyddyn fod yn rif 4 digid.",
        DateInvalidCombination = "Rhowch ddyddiad go iawn - nid yw'r diwrnod hwnnw'n bodoli yn y mis hwnnw.",
        CharacterRemainingFormat = "Mae gennych {0} nod ar ôl",
        CharactersRemainingFormat = "Mae gennych {0} o nodau ar ôl",
        CharacterOverLimitFormat = "Mae gennych {0} nod gormod",
        CharactersOverLimitFormat = "Mae gennych {0} o nodau gormod",
        PaginationLabel = CyLocalizer.Welsh.PaginationLabel,
        PaginationPrevious = CyLocalizer.Welsh.PaginationPrevious,
        PaginationNext = CyLocalizer.Welsh.PaginationNext,
        PageAriaLabelFormat = CyLocalizer.Welsh.PageAriaLabelFormat,
        CurrentPageAriaLabelFormat = CyLocalizer.Welsh.CurrentPageAriaLabelFormat,
        SpinnerLabel = CyLocalizer.Welsh.SpinnerLabel
    };

    public static SampleContent EnglishContent { get; } = new()
    {
        AlertTitle = "Important",
        AlertBody = "This message uses translated accessible labels.",
        CrumbHome = "Home",
        CrumbAppointments = "Appointments",
        SampleNavigationLabel = "Sample navigation",
        SampleSidebarLabel = "Sample sidebar",
        NavOverview = "Overview",
        OpenDialog = "Open dialog",
        DialogTitle = "Appointment details",
        DialogDescription = "Check the details before you continue.",
        DialogBody = "Press Escape to close this dialog.",
        DialogConfirm = "OK",
        ShowNotification = "Show a notification",
        NotificationText = "Your changes were saved."
    };

    public static SampleContent WelshContent { get; } = new()
    {
        AlertTitle = "Pwysig",
        AlertBody = "Mae'r neges hon yn defnyddio labeli hygyrch wedi'u cyfieithu.",
        CrumbHome = "Hafan",
        CrumbAppointments = "Apwyntiadau",
        SampleNavigationLabel = "Llywio enghreifftiol",
        SampleSidebarLabel = "Bar ochr enghreifftiol",
        NavOverview = "Trosolwg",
        OpenDialog = "Agor y ddeialog",
        DialogTitle = "Manylion yr apwyntiad",
        DialogDescription = "Gwiriwch y manylion cyn parhau.",
        DialogBody = "Pwyswch Escape i gau'r ddeialog.",
        DialogConfirm = "Iawn",
        ShowNotification = "Dangos hysbysiad",
        NotificationText = "Cadwyd eich newidiadau."
    };

    public static NavStrings EnglishNav { get; } = new()
    {
        GettingStarted = "Getting Started",
        Foundations = "Foundations",
        Branding = "Branding",
        Layout = "Layout",
        Navigation = "Navigation",
        Forms = "Forms",
        Content = "Content",
        Data = "Data",
        Feedback = "Feedback",
        Accessibility = "Accessibility",
        Overview = "Overview",
        Components = "Components",
        Docs = "Docs",
        Search = "Search...",
        SearchPlaceholder = "Search components and docs...",
        SearchAriaLabel = "Search documentation",
        SearchTitle = "Search (Ctrl+K)",
        OpenNavigationMenu = "Open navigation menu",
        CloseNavigationMenu = "Close navigation menu",
        DocumentationNavigation = "Documentation navigation",
        PrimaryNavigation = "Primary",
        ToggleDarkMode = "Toggle dark mode",
        SwitchToLightMode = "Switch to light mode",
        SwitchToDarkMode = "Switch to dark mode",
        Previous = "Previous",
        Next = "Next",
        OnThisPage = "On this page",
        Documentation = "Documentation",
        Community = "Community",
        Packages = "Packages",
        NotFoundTitle = "Page not found",
        NotFoundBody = "There is no page at this address. Check the URL or use the navigation to find what you are looking for.",
        NotFoundHome = "← Back to home",
        SearchResults = "Search results",
        SearchEmptyFormat = "No pages match \"{0}\".",
        SearchHint = "Type to search across every component and doc page.",
        PageNavigation = "Page navigation",
        TableOfContents = "Table of contents",
        Examples = "Examples",
        ApiReference = "API Reference",
        Api = "API",
        ComponentDocumentationTabs = "Component Documentation Tabs"
    };

    public static NavStrings WelshNav { get; } = new()
    {
        GettingStarted = "Dechrau Arni",
        Foundations = "Seiliau",
        Branding = "Brandio",
        Layout = "Cynllun",
        Navigation = "Llywio",
        Forms = "Ffurflenni",
        Content = "Cynnwys",
        Data = "Data",
        Feedback = "Adborth",
        Accessibility = "Hygyrchedd",
        Overview = "Trosolwg",
        Components = "Cydrannau",
        Docs = "Dogfennau",
        Search = "Chwilio...",
        SearchPlaceholder = "Chwilio cydrannau a dogfennau...",
        SearchAriaLabel = "Chwilio'r ddogfennaeth",
        SearchTitle = "Chwilio (Ctrl+K)",
        OpenNavigationMenu = "Agor y ddewislen lywio",
        CloseNavigationMenu = "Cau'r ddewislen lywio",
        DocumentationNavigation = "Llywio'r ddogfennaeth",
        PrimaryNavigation = "Prif ddewislen",
        ToggleDarkMode = "Toglo modd tywyll",
        SwitchToLightMode = "Newid i fodd golau",
        SwitchToDarkMode = "Newid i fodd tywyll",
        Previous = "Blaenorol",
        Next = "Nesaf",
        OnThisPage = "Ar y dudalen hon",
        Documentation = "Dogfennaeth",
        Community = "Cymuned",
        Packages = "Pecynnau",
        NotFoundTitle = "Tudalen heb ei chanfod",
        NotFoundBody = "Nid oes tudalen yn y cyfeiriad hwn. Gwiriwch yr URL neu ddefnyddiwch y llywio i ddod o hyd i'r hyn rydych chi'n chwilio amdano.",
        NotFoundHome = "← Yn ôl i'r hafan",
        SearchResults = "Canlyniadau chwilio",
        SearchEmptyFormat = "Dim tudalennau'n cyfateb i \"{0}\".",
        SearchHint = "Teipiwch i chwilio ar draws pob cydran a thudalen ddogfennaeth.",
        PageNavigation = "Llywio tudalen",
        TableOfContents = "Tabl cynnwys",
        Examples = "Enghreifftiau",
        ApiReference = "Cyfeirnod API",
        Api = "Cyfeirnod API",
        ComponentDocumentationTabs = "Tabiau Dogfennaeth Cydran"
    };

    /// <summary>Every library override parameter this release adds, for the catalogue table and the tests.</summary>
    public static IReadOnlyList<StringEntry> Catalogue { get; } =
    [
        new("CyAlert", "DismissAriaLabel", s => s.AlertDismiss),
        new("CyBreadcrumb", "AriaLabel", s => s.BreadcrumbLabel),
        new("CyNavigation", "AriaLabel", s => s.NavigationLabel),
        new("CyNavigation", "OpenMenuLabel", s => s.NavigationOpen),
        new("CyNavigation", "CloseMenuLabel", s => s.NavigationClose),
        new("CyToastContainer", "AriaLabel", s => s.ToastRegion),
        new("CyToastContainer", "DismissAriaLabel", s => s.ToastDismiss),
        new("CyDialog", "CloseLabel", s => s.DialogClose),
        new("CyCodeBlock", "CodeLabel", s => s.CodeLabel),
        new("CyCodeBlock", "CopyLabel", s => s.CopyLabel),
        new("CyCodeBlock", "CopiedLabel", s => s.CopiedLabel),
        new("CyCodeBlock", "CopyFailedLabel", s => s.CopyFailedLabel),
        new("CyCodeBlock", "CopiedMessage", s => s.CopiedMessage),
        new("CyCodeBlock", "CopyFailedMessage", s => s.CopyFailedMessage),
        new("CySidebar", "RevealLabel", s => s.SidebarReveal),
        new("CySidebar", "CloseLabel", s => s.SidebarClose),
        new("CySidebar", "SizeGroupLabel", s => s.SidebarSizeGroup),
        new("CySidebar", "ExpandLabel", s => s.SidebarExpand),
        new("CySidebar", "CollapseLabel", s => s.SidebarCollapse),
        new("CySidebar", "CompactLabel", s => s.SidebarCompact),
        new("CySidebar", "IconOnlyLabel", s => s.SidebarIconOnly),
        new("CySidebar", "HideLabel", s => s.SidebarHide),
        new("CySidebar", "ResizeLabel", s => s.SidebarResize),
        new("CyDateInput", "DayLabel", s => s.DateDay),
        new("CyDateInput", "MonthLabel", s => s.DateMonth),
        new("CyDateInput", "YearLabel", s => s.DateYear),
        new("CyDateInput", "DayRangeErrorMessage", s => s.DateDayRangeError),
        new("CyDateInput", "MonthRangeErrorMessage", s => s.DateMonthRangeError),
        new("CyDateInput", "YearRangeErrorMessage", s => s.DateYearRangeError),
        new("CyDateInput", "InvalidDateErrorMessage", s => s.DateInvalidCombination),
        new("CyTextArea", "CharacterRemainingFormat", s => s.CharacterRemainingFormat),
        new("CyTextArea", "CharactersRemainingFormat", s => s.CharactersRemainingFormat),
        new("CyTextArea", "CharacterOverLimitFormat", s => s.CharacterOverLimitFormat),
        new("CyTextArea", "CharactersOverLimitFormat", s => s.CharactersOverLimitFormat),
        new("CyPagination", "AriaLabel", s => s.PaginationLabel),
        new("CyPagination", "PreviousLabel", s => s.PaginationPrevious),
        new("CyPagination", "NextLabel", s => s.PaginationNext),
        new("CyPagination", "PageAriaLabelFormat", s => s.PageAriaLabelFormat),
        new("CyPagination", "CurrentPageAriaLabelFormat", s => s.CurrentPageAriaLabelFormat),
        new("CySpinner", "Label", s => s.SpinnerLabel)
    ];

    /// <summary>Raised after <see cref="Language"/> changes. Layouts subscribe to re-render.</summary>
    public event Action? Changed;

    public AppLanguage Language { get; private set; } = AppLanguage.English;

    /// <summary>The library strings for <see cref="Language"/>.</summary>
    public LibraryStrings Library => Language == AppLanguage.Welsh ? WelshLibrary : EnglishLibrary;

    /// <summary>The shell navigation strings for <see cref="Language"/>.</summary>
    public NavStrings Nav => Language == AppLanguage.Welsh ? WelshNav : EnglishNav;

    /// <summary>The demo page's own copy for <see cref="Language"/>.</summary>
    public SampleContent Content => Language == AppLanguage.Welsh ? WelshContent : EnglishContent;

    /// <summary>BCP 47 tag for <c>lang</c> attributes (WCAG 3.1.2 Language of Parts).</summary>
    public string LangTag => Language == AppLanguage.Welsh ? "cy" : "en";

    public void SetLanguage(AppLanguage language)
    {
        if (language == Language)
        {
            return;
        }

        Language = language;
        Changed?.Invoke();
    }

    /// <summary>Translates a top-level category name according to the active language.</summary>
    public string GetCategory(string category) => category switch
    {
        "Getting Started" => Nav.GettingStarted,
        "Foundations" => Nav.Foundations,
        "Branding" => Nav.Branding,
        "Layout" => Nav.Layout,
        "Navigation" => Nav.Navigation,
        "Forms" => Nav.Forms,
        "Content" => Nav.Content,
        "Data" => Nav.Data,
        "Feedback" => Nav.Feedback,
        "Accessibility" => Nav.Accessibility,
        _ => category
    };
}
