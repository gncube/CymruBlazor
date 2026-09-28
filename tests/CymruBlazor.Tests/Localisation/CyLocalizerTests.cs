using System.Reflection;
using CymruBlazor.Enums;
using CymruBlazor.Localisation;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Localisation;

/// <summary>
/// The step-2 localiser (ADR-0002, 1.6.0). These tests cover the service's own
/// language-switching behaviour in isolation; <c>LocalisationOverrideTests</c>
/// and <c>AppStringsTests</c> cover components actually consuming it.
/// </summary>
public sealed class CyLocalizerTests
{
    private static IEnumerable<PropertyInfo> StringProperties<T>() =>
        typeof(T).GetProperties().Where(p => p.PropertyType == typeof(string));

    private static Dictionary<string, string> Values<T>(T instance) =>
        StringProperties<T>().ToDictionary(p => p.Name, p => (string)p.GetValue(instance)!);

    [Fact]
    public void Defaults_To_English()
    {
        var localizer = new CyLocalizer();

        localizer.Language.ShouldBe(AppLanguage.English);
        localizer.Strings.ShouldBeSameAs(CyLocalizer.English);
        localizer.LangTag.ShouldBe("en");
    }

    [Fact]
    public void SetLanguage_Welsh_Switches_Strings_And_LangTag()
    {
        var localizer = new CyLocalizer();

        localizer.SetLanguage(AppLanguage.Welsh);

        localizer.Language.ShouldBe(AppLanguage.Welsh);
        localizer.Strings.ShouldBeSameAs(CyLocalizer.Welsh);
        localizer.LangTag.ShouldBe("cy");
    }

    [Fact]
    public void SetLanguage_Raises_LanguageChanged_Exactly_Once_Per_Real_Change()
    {
        var localizer = new CyLocalizer();
        var changes = 0;
        localizer.LanguageChanged += () => changes++;

        localizer.SetLanguage(AppLanguage.Welsh);
        localizer.SetLanguage(AppLanguage.Welsh); // no-op: already Welsh
        localizer.SetLanguage(AppLanguage.Welsh); // still a no-op

        changes.ShouldBe(1);

        localizer.SetLanguage(AppLanguage.English);

        changes.ShouldBe(2);
    }

    [Fact]
    public void SetLanguage_To_Current_Language_Does_Not_Raise_LanguageChanged()
    {
        var localizer = new CyLocalizer();
        var changes = 0;
        localizer.LanguageChanged += () => changes++;

        // Already English - this must be a no-op, matching AppStrings.SetLanguage's contract.
        localizer.SetLanguage(AppLanguage.English);

        changes.ShouldBe(0);
    }

    [Fact]
    public void Unsubscribed_Handlers_Are_Not_Invoked()
    {
        var localizer = new CyLocalizer();
        var changes = 0;
        void Handler() => changes++;

        localizer.LanguageChanged += Handler;
        localizer.LanguageChanged -= Handler;
        localizer.SetLanguage(AppLanguage.Welsh);

        changes.ShouldBe(0);
    }

    [Fact]
    public void Welsh_Strings_Are_Complete_And_Differ_From_English()
    {
        var english = Values(CyLocalizer.English);
        var welsh = Values(CyLocalizer.Welsh);

        // Same set of keys on both sides - nothing added to one and forgotten on the other.
        welsh.Keys.ShouldBe(english.Keys, ignoreOrder: true);

        foreach (var (name, value) in welsh)
        {
            value.ShouldNotBeNullOrWhiteSpace(name);
            value.ShouldNotBe(english[name], $"{name} has not been translated.");
        }
    }

    [Fact]
    public void No_Two_Consecutive_Renders_Share_Mutable_State()
    {
        // Strings always returns one of the two static, immutable instances - never a
        // per-instance copy - so multiple CyLocalizer instances (e.g. across circuits)
        // can't accidentally observe or corrupt each other's translations.
        var first = new CyLocalizer();
        var second = new CyLocalizer();

        second.SetLanguage(AppLanguage.Welsh);

        first.Strings.ShouldBeSameAs(CyLocalizer.English);
        second.Strings.ShouldBeSameAs(CyLocalizer.Welsh);
    }
}
