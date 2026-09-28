using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Components.Localisation;
using CymruBlazor.Enums;
using CymruBlazor.Localisation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Localisation;

/// <summary>
/// <see cref="CyLocalizationProvider"/> cascades the registered <see cref="ICyLocalizer"/>,
/// and a component under it falls back to the localiser's strings - but only when its own
/// override parameter is left unset, and only when actually wrapped by the provider.
/// </summary>
public sealed class CyLocalizationProviderTests : TestContextBase
{
    public CyLocalizationProviderTests()
    {
        Services.AddScoped<ICyLocalizer, CyLocalizer>();
    }

    [Fact]
    public void Renders_ChildContent()
    {
        var cut = Render<CyLocalizationProvider>(p => p.AddChildContent("<p>Hello</p>"));

        cut.Markup.ShouldContain("Hello");
    }

    [Fact]
    public void Cascades_Localizer_To_A_Nested_Component_Without_An_Override()
    {
        var cut = Render<CyLocalizationProvider>(p => p.AddChildContent<CyAlert>(a => a
            .Add(x => x.Dismissible, true)));

        var localizer = Services.GetRequiredService<ICyLocalizer>();
        localizer.SetLanguage(AppLanguage.Welsh);
        cut.Render();

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe(CyLocalizer.Welsh.AlertDismiss);
    }

    [Fact]
    public void Explicit_Override_Still_Wins_Over_The_Cascaded_Localizer()
    {
        var localizer = Services.GetRequiredService<ICyLocalizer>();
        localizer.SetLanguage(AppLanguage.Welsh);

        var cut = Render<CyLocalizationProvider>(p => p.AddChildContent<CyAlert>(a => a
            .Add(x => x.Dismissible, true)
            .Add(x => x.DismissAriaLabel, "Custom override")));

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe("Custom override");
    }

    [Fact]
    public void Rerenders_When_The_Localizer_Language_Changes()
    {
        var localizer = Services.GetRequiredService<ICyLocalizer>();

        var cut = Render<CyLocalizationProvider>(p => p.AddChildContent<CyAlert>(a => a
            .Add(x => x.Dismissible, true)));

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe(CyLocalizer.English.AlertDismiss);

        localizer.SetLanguage(AppLanguage.Welsh);

        cut.WaitForAssertion(() =>
            cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe(CyLocalizer.Welsh.AlertDismiss));
    }

    [Fact]
    public void A_Component_Not_Wrapped_In_The_Provider_Ignores_The_Registered_Localizer()
    {
        // ICyLocalizer is registered, but never cascaded here - CyAlert must still fall
        // back to its own English literal, exactly as it did before 1.6.0.
        var localizer = Services.GetRequiredService<ICyLocalizer>();
        localizer.SetLanguage(AppLanguage.Welsh);

        var cut = Render<CyAlert>(p => p.Add(x => x.Dismissible, true));

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe("Dismiss");
    }
}
