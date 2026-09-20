using CymruBlazor.Enums;

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

    public required string NavOverview { get; init; }

    public required string OpenDialog { get; init; }

    public required string DialogTitle { get; init; }

    public required string DialogDescription { get; init; }

    public required string DialogBody { get; init; }

    public required string DialogConfirm { get; init; }

    public required string ShowNotification { get; init; }

    public required string NotificationText { get; init; }
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
    /// <summary>English: identical to CymruBlazor's built-in defaults.</summary>
    public static LibraryStrings EnglishLibrary { get; } = new()
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
        SidebarResize = "Resize sidebar"
    };

    /// <summary>Illustrative Welsh (Cymraeg). Not translator-reviewed.</summary>
    public static LibraryStrings WelshLibrary { get; } = new()
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
        SidebarResize = "Newid maint y bar ochr"
    };

    public static SampleContent EnglishContent { get; } = new()
    {
        AlertTitle = "Important",
        AlertBody = "This message uses translated accessible labels.",
        CrumbHome = "Home",
        CrumbAppointments = "Appointments",
        SampleNavigationLabel = "Sample navigation",
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
        NavOverview = "Trosolwg",
        OpenDialog = "Agor y ddeialog",
        DialogTitle = "Manylion yr apwyntiad",
        DialogDescription = "Gwiriwch y manylion cyn parhau.",
        DialogBody = "Pwyswch Escape i gau'r ddeialog.",
        DialogConfirm = "Iawn",
        ShowNotification = "Dangos hysbysiad",
        NotificationText = "Cadwyd eich newidiadau."
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
        new("CySidebar", "ResizeLabel", s => s.SidebarResize)
    ];

    /// <summary>Raised after <see cref="Language"/> changes. Layouts subscribe to re-render.</summary>
    public event Action? Changed;

    public AppLanguage Language { get; private set; } = AppLanguage.English;

    /// <summary>The library strings for <see cref="Language"/>.</summary>
    public LibraryStrings Library => Language == AppLanguage.Welsh ? WelshLibrary : EnglishLibrary;

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
}
