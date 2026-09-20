using System.Reflection;
using Bunit;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Content;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Components.Layout;
using CymruBlazor.Demo.Localisation;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Samples;

/// <summary>
/// The demo's <see cref="AppStrings"/> is the reference implementation of localisation step 1, so it
/// must stay honest: complete, actually translated, and identical to the library's built-in English.
/// </summary>
public sealed class AppStringsTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private static IEnumerable<PropertyInfo> StringProperties<T>() =>
        typeof(T).GetProperties().Where(p => p.PropertyType == typeof(string));

    private static Dictionary<string, string> Values<T>(T instance) =>
        StringProperties<T>().ToDictionary(p => p.Name, p => (string)p.GetValue(instance)!);

    [Fact]
    public void Welsh_Library_Strings_Are_Complete_And_Differ_From_English()
    {
        var english = Values(AppStrings.EnglishLibrary);
        var welsh = Values(AppStrings.WelshLibrary);

        welsh.Keys.ShouldBe(english.Keys, ignoreOrder: true);

        foreach (var (name, value) in welsh)
        {
            value.ShouldNotBeNullOrWhiteSpace(name);
            value.ShouldNotBe(english[name], $"{name} has not been translated.");
        }
    }

    [Fact]
    public void Welsh_Sample_Content_Is_Complete_And_Differs_From_English()
    {
        var english = Values(AppStrings.EnglishContent);
        var welsh = Values(AppStrings.WelshContent);

        foreach (var (name, value) in welsh)
        {
            value.ShouldNotBeNullOrWhiteSpace(name);
            value.ShouldNotBe(english[name], $"{name} has not been translated.");
        }
    }

    [Fact]
    public void Catalogue_Covers_Every_Library_String_Exactly_Once()
    {
        // Each property is set to its own name, so the catalogue's selectors reveal which property they read.
        var probe = new LibraryStrings
        {
            AlertDismiss = nameof(LibraryStrings.AlertDismiss),
            BreadcrumbLabel = nameof(LibraryStrings.BreadcrumbLabel),
            NavigationLabel = nameof(LibraryStrings.NavigationLabel),
            NavigationOpen = nameof(LibraryStrings.NavigationOpen),
            NavigationClose = nameof(LibraryStrings.NavigationClose),
            ToastRegion = nameof(LibraryStrings.ToastRegion),
            ToastDismiss = nameof(LibraryStrings.ToastDismiss),
            DialogClose = nameof(LibraryStrings.DialogClose),
            CodeLabel = nameof(LibraryStrings.CodeLabel),
            CopyLabel = nameof(LibraryStrings.CopyLabel),
            CopiedLabel = nameof(LibraryStrings.CopiedLabel),
            CopyFailedLabel = nameof(LibraryStrings.CopyFailedLabel),
            CopiedMessage = nameof(LibraryStrings.CopiedMessage),
            CopyFailedMessage = nameof(LibraryStrings.CopyFailedMessage),
            SidebarReveal = nameof(LibraryStrings.SidebarReveal),
            SidebarClose = nameof(LibraryStrings.SidebarClose),
            SidebarSizeGroup = nameof(LibraryStrings.SidebarSizeGroup),
            SidebarExpand = nameof(LibraryStrings.SidebarExpand),
            SidebarCollapse = nameof(LibraryStrings.SidebarCollapse),
            SidebarCompact = nameof(LibraryStrings.SidebarCompact),
            SidebarIconOnly = nameof(LibraryStrings.SidebarIconOnly),
            SidebarHide = nameof(LibraryStrings.SidebarHide),
            SidebarResize = nameof(LibraryStrings.SidebarResize)
        };

        var covered = AppStrings.Catalogue.Select(e => e.Get(probe)).ToList();

        covered.ShouldBe(StringProperties<LibraryStrings>().Select(p => p.Name), ignoreOrder: true);
        covered.Distinct().Count().ShouldBe(covered.Count, "A property is listed twice in the catalogue.");
    }

    [Fact]
    public void Switching_Language_Raises_Changed_Once_And_Selects_The_Strings()
    {
        var strings = new AppStrings();
        var changes = 0;
        strings.Changed += () => changes++;

        strings.Language.ShouldBe(AppLanguage.English);
        strings.Library.ShouldBeSameAs(AppStrings.EnglishLibrary);
        strings.LangTag.ShouldBe("en");

        strings.SetLanguage(AppLanguage.Welsh);
        strings.SetLanguage(AppLanguage.Welsh);

        changes.ShouldBe(1);
        strings.Library.ShouldBeSameAs(AppStrings.WelshLibrary);
        strings.Content.ShouldBeSameAs(AppStrings.WelshContent);
        strings.LangTag.ShouldBe("cy");
    }

    /// <summary>
    /// Guards the "English" column against drift: render the components bare (no overrides) and check
    /// what they really show equals what <see cref="AppStrings.EnglishLibrary"/> claims is the default.
    /// </summary>
    [Fact]
    public void English_Library_Strings_Equal_The_Library_Built_In_Defaults()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule(ModulePath);
        module.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new Mock<IMediator>().Object);
        using var toasts = new ToastService();
        Services.AddSingleton<IToastService>(toasts);
        var e = AppStrings.EnglishLibrary;

        Render<CyAlert>(p => p.Add(a => a.Dismissible, true))
            .Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe(e.AlertDismiss);

        Render<CyBreadcrumb>().Find("nav").GetAttribute("aria-label").ShouldBe(e.BreadcrumbLabel);

        var navigation = Render<CyNavigation>();
        navigation.Find("nav").GetAttribute("aria-label").ShouldBe(e.NavigationLabel);
        navigation.Find(".cy-navigation__toggle .u-sr-only").TextContent.ShouldBe(e.NavigationOpen);

        toasts.Show("Hello", ToastVariant.Info, TimeSpan.Zero);
        var toast = Render<CyToastContainer>();
        toast.Find(".cy-toast-container").GetAttribute("aria-label").ShouldBe(e.ToastRegion);
        toast.Find(".cy-toast__close").GetAttribute("aria-label").ShouldBe(e.ToastDismiss);

        var code = Render<CyCodeBlock>(p => p.Add(c => c.Code, "x").Add(c => c.Language, " "));
        code.Find(".cy-code-block__language").TextContent.ShouldBe(e.CodeLabel);
        code.Find(".cy-code-block__copy span").TextContent.ShouldBe(e.CopyLabel);

        Render<CyDialog>(p => p.Add(d => d.Open, true).Add(d => d.Title, "T"))
            .Find(".cy-dialog__close").GetAttribute("aria-label").ShouldBe(e.DialogClose);

        Render<CySidebar>(p => p.Add(s => s.MobileOpen, true))
            .Find(".cy-sidebar__close").GetAttribute("aria-label").ShouldBe(e.SidebarClose);

        Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Hidden))
            .Find(".cy-sidebar__reveal-handle").GetAttribute("aria-label").ShouldBe(e.SidebarReveal);

        Render<CySidebar>().Find(".cy-sidebar__toggle").GetAttribute("aria-label").ShouldBe(e.SidebarCollapse);

        var steps = Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly])
            .Add(s => s.State, SidebarState.Compact));
        steps.Find(".cy-sidebar__steps").GetAttribute("aria-label").ShouldBe(e.SidebarSizeGroup);
        steps.Find(".cy-sidebar__step--widen").GetAttribute("aria-label").ShouldBe(e.SidebarExpand);
        steps.Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe(e.SidebarIconOnly);

        Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Expanded))
            .Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe(e.SidebarCompact);

        Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Compact))
            .Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe(e.SidebarHide);
    }
}
