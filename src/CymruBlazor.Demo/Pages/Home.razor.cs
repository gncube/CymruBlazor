using CymruBlazor.Demo.Shared;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Demo.Pages;

/// <summary>
/// Landing page for the CymruBlazor Demo application.
/// </summary>
public partial class Home : ComponentBase
{
    private sealed record Feature(string Icon, string IconBackground, string Title, string Description);

    private static readonly IReadOnlyList<Feature> _features =
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

    private static readonly IReadOnlyList<DemoQuickStart.Step> _quickStartSteps =
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
}
