using CymruBlazor.Demo.Localisation;
using CymruBlazor.Demo.SharedComponents;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Demo.Pages;

/// <summary>
/// Landing page for the CymruBlazor Demo application.
/// </summary>
public partial class Home : ComponentBase
{
    private sealed record Feature(string Icon, string IconBackground, string Title, string Description);

    private static readonly IReadOnlyList<Feature> _englishFeatures =
    [
        new("success", "#0d9488", "WCAG 2.2 AA",
            "Accessible composability built to a high standard across Welsh NHS digital services."),
        new("language", "#dc2626", "Welsh language",
            "Native language support and toggling primitives for Welsh and English content."),
        new("grid-2x2", "#7c3aed", "Design Tokens",
            "Tokenised palettes, spacing, and typography aligned to Welsh NHS branding guidelines."),
        new("link", "#2563eb", "Composable",
            "Small, single-purpose components that connect together instead of one large monolith."),
        new("activity", "#d97706", "Mediator pipeline",
            "Cross-cutting concerns like validation and notifications flow through a shared mediator pipeline."),
        new("check", "#16a34a", "Tested",
            "Automatically tested with bUnit and accessibility checks across every component.")
    ];

    /// <summary>Illustrative Welsh; not translator-reviewed - see the v1.6.0 CHANGELOG entry.</summary>
    private static readonly IReadOnlyList<Feature> _welshFeatures =
    [
        new("success", "#0d9488", "WCAG 2.2 AA",
            "Cyfansoddadwyedd hygyrch wedi'i adeiladu i safon uchel ar draws gwasanaethau digidol GIG Cymru."),
        new("language", "#dc2626", "Iaith Gymraeg",
            "Cefnogaeth iaith frodorol a chyntefigau toglo ar gyfer cynnwys Cymraeg a Saesneg."),
        new("grid-2x2", "#7c3aed", "Tocynnau Dylunio",
            "Paletau, bylchau a theipograffeg wedi'u tocynnu, wedi'u halinio â chanllawiau brandio GIG Cymru."),
        new("link", "#2563eb", "Cyfansoddadwy",
            "Cydrannau bach, un-pwrpas sy'n cysylltu â'i gilydd yn hytrach nag un bloc mawr."),
        new("activity", "#d97706", "Piblinell cyfryngwr",
            "Mae pryderon ar draws y bwrdd fel dilysu a hysbysiadau'n llifo drwy biblinell cyfryngwr a rennir."),
        new("check", "#16a34a", "Wedi'i brofi",
            "Wedi'i brofi'n awtomatig gyda bUnit a gwiriadau hygyrchedd ar draws pob cydran.")
    ];

    private IReadOnlyList<Feature> Features => Strings.Language == AppLanguage.Welsh ? _welshFeatures : _englishFeatures;

    private static readonly IReadOnlyList<DemoQuickStart.Step> _englishQuickStartSteps =
    [
        new(
            "Install the package",
            "dotnet add package CymruBlazor"),
        new(
            "Register the services",
            "builder.Services.AddCymruBlazor();",
            "csharp"),
        new(
            "Use a component",
            "<CyButton Variant=\"ComponentColour.Primary\">Save changes</CyButton>",
            "razor")
    ];

    /// <summary>Illustrative Welsh; not translator-reviewed - see the v1.6.0 CHANGELOG entry.</summary>
    private static readonly IReadOnlyList<DemoQuickStart.Step> _welshQuickStartSteps =
    [
        new(
            "Gosod y pecyn",
            "dotnet add package CymruBlazor"),
        new(
            "Cofrestru'r gwasanaethau",
            "builder.Services.AddCymruBlazor();",
            "csharp"),
        new(
            "Defnyddio cydran",
            "<CyButton Variant=\"ComponentColour.Primary\">Save changes</CyButton>",
            "razor")
    ];

    private IReadOnlyList<DemoQuickStart.Step> QuickStartSteps =>
        Strings.Language == AppLanguage.Welsh ? _welshQuickStartSteps : _englishQuickStartSteps;
}
