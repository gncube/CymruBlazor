using AngleSharp.Dom;
using Bunit;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Feedback;
using CymruBlazor.Demo.Localisation;
using CymruBlazor.Demo.Pages.Components.Foundations;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// Renders the real <c>/foundations/localisation</c> demo page. The headline test: with Welsh selected,
/// not one of the library's English defaults appears in the page's preview - as an accessible name, a
/// tooltip, or visible text.
/// </summary>
public sealed class LocalisationPageTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly AppStrings _strings = new();
    private readonly ToastService _toasts = new();

    public LocalisationPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule(ModulePath);
        module.Mode = JSRuntimeMode.Loose;
        module.Setup<int>("showDialog", _ => true).SetResult(1);

        Services.AddSingleton(new Mock<IMediator>().Object);
        Services.AddSingleton(_strings);
        Services.AddSingleton<IToastService>(_toasts);
    }

    private static HashSet<string> EnglishDefaults() =>
        typeof(LibraryStrings).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => (string)p.GetValue(AppStrings.EnglishLibrary)!)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>Every accessible name, tooltip and leaf text node inside the preview.</summary>
    private static HashSet<string> RenderedStrings(IElement region)
    {
        var strings = new HashSet<string>(StringComparer.Ordinal);

        foreach (var element in region.QuerySelectorAll("*"))
        {
            foreach (var attribute in new[] { "aria-label", "title", "placeholder", "alt" })
            {
                var value = element.GetAttribute(attribute);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    strings.Add(value.Trim());
                }
            }

            if (element.Children.Length == 0 && !string.IsNullOrWhiteSpace(element.TextContent))
            {
                strings.Add(element.TextContent.Trim());
            }
        }

        return strings;
    }

    private IRenderedComponent<LocalisationPage> RenderPage(AppLanguage language)
    {
        _strings.SetLanguage(language);
        return Render<LocalisationPage>();
    }

    [Fact]
    public void In_Welsh_No_English_Library_Default_Reaches_The_Preview()
    {
        var cut = RenderPage(AppLanguage.Welsh);

        var rendered = RenderedStrings(cut.Find("#localisation-preview"));
        var leaked = rendered.Intersect(EnglishDefaults()).ToList();

        leaked.ShouldBeEmpty($"English defaults leaked into the Welsh preview: {string.Join(", ", leaked)}");
    }

    [Fact]
    public void In_Welsh_The_Translated_Strings_Are_Present()
    {
        var cut = RenderPage(AppLanguage.Welsh);
        var w = AppStrings.WelshLibrary;

        var rendered = RenderedStrings(cut.Find("#localisation-preview"));

        rendered.ShouldContain(w.AlertDismiss);
        rendered.ShouldContain(w.BreadcrumbLabel);
        rendered.ShouldContain(w.CopyLabel);
        rendered.ShouldContain(w.SidebarSizeGroup);
        rendered.ShouldContain(w.SidebarExpand);
        rendered.ShouldContain(w.SidebarIconOnly);
    }

    [Fact]
    public void In_English_The_Preview_Shows_The_Library_Defaults()
    {
        var cut = RenderPage(AppLanguage.English);
        var e = AppStrings.EnglishLibrary;

        var rendered = RenderedStrings(cut.Find("#localisation-preview"));

        rendered.ShouldContain(e.AlertDismiss);
        rendered.ShouldContain(e.BreadcrumbLabel);
        rendered.ShouldContain(e.CopyLabel);
        rendered.ShouldContain(e.SidebarSizeGroup);
    }

    [Fact]
    public void The_Preview_Is_Marked_With_The_Language_Of_Its_Text()
    {
        RenderPage(AppLanguage.English).Find("#localisation-preview").GetAttribute("lang").ShouldBe("en");
        RenderPage(AppLanguage.Welsh).Find("#localisation-preview").GetAttribute("lang").ShouldBe("cy");
    }

    [Fact]
    public void Using_The_Language_Toggle_Switches_The_Preview_To_Welsh_And_Back()
    {
        var cut = RenderPage(AppLanguage.English);

        cut.Find(".cy-language-toggle").Click();

        _strings.Language.ShouldBe(AppLanguage.Welsh);
        cut.Find("#localisation-preview").GetAttribute("lang").ShouldBe("cy");
        RenderedStrings(cut.Find("#localisation-preview")).Intersect(EnglishDefaults()).ShouldBeEmpty();

        cut.Find(".cy-language-toggle").Click();

        _strings.Language.ShouldBe(AppLanguage.English);
        cut.Find("#localisation-preview").GetAttribute("lang").ShouldBe("en");
        RenderedStrings(cut.Find("#localisation-preview")).ShouldContain(AppStrings.EnglishLibrary.AlertDismiss);
    }

    [Fact]
    public void In_Welsh_The_Opened_Dialog_Uses_Welsh_For_Its_Title_And_Close_Button()
    {
        var cut = RenderPage(AppLanguage.Welsh);

        cut.Find("#l10n-open-dialog").Click();

        var region = cut.Find("#localisation-preview");
        cut.Find(".cy-dialog__title").TextContent.ShouldBe(AppStrings.WelshContent.DialogTitle);
        cut.Find(".cy-dialog__close").GetAttribute("aria-label").ShouldBe(AppStrings.WelshLibrary.DialogClose);
        RenderedStrings(region).Intersect(EnglishDefaults()).ShouldBeEmpty();
    }

    [Fact]
    public void In_Welsh_The_Notification_Button_Raises_A_Welsh_Toast()
    {
        var cut = RenderPage(AppLanguage.Welsh);

        cut.Find("#l10n-show-toast").Click();

        _toasts.Toasts.Single().Message.ShouldBe(AppStrings.WelshContent.NotificationText);
    }

    [Fact]
    public async Task The_Page_Rerenders_When_The_Language_Changes_Elsewhere()
    {
        var cut = RenderPage(AppLanguage.English);

        await cut.InvokeAsync(() => _strings.SetLanguage(AppLanguage.Welsh));

        cut.Find("#localisation-preview").GetAttribute("lang").ShouldBe("cy");
    }

    [Fact]
    public void The_Catalogue_Table_Lists_Every_Library_String_In_Both_Languages()
    {
        var cut = RenderPage(AppLanguage.English);

        var rows = cut.FindAll("table[aria-label='Localisable strings'] tbody tr");

        rows.Count.ShouldBe(AppStrings.Catalogue.Count);
        rows[0].QuerySelector("td[lang=cy]")!.TextContent.ShouldBe(AppStrings.Catalogue[0].Get(AppStrings.WelshLibrary));
    }
}
