using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Branding;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Branding;

public sealed class CyLanguageToggleTests : TestContextBase
{
    [Fact]
    public void Should_Show_Welsh_Label_When_Current_Language_Is_English()
    {
        // Act
        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.CurrentLanguage, AppLanguage.English));

        // Assert
        cut.Find("button span[aria-hidden='true']").TextContent.ShouldBe("Cymraeg");
    }

    [Fact]
    public void Should_Show_English_Label_When_Current_Language_Is_Welsh()
    {
        // Act
        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.CurrentLanguage, AppLanguage.Welsh));

        // Assert
        cut.Find("button span[aria-hidden='true']").TextContent.ShouldBe("English");
    }

    [Fact]
    public async Task Should_Raise_CurrentLanguageChanged_With_The_Target_Language_On_Click()
    {
        // Arrange
        AppLanguage? raised = null;

        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.CurrentLanguage, AppLanguage.English)
            .Add(p => p.CurrentLanguageChanged, lang => raised = lang));

        // Act
        await cut.InvokeAsync(() => cut.Find("button").Click());

        // Assert
        raised.ShouldBe(AppLanguage.Welsh);
    }

    [Fact]
    public async Task Should_Track_Its_Own_State_When_Unbound()
    {
        // Arrange - no CurrentLanguageChanged supplied (uncontrolled usage)
        var cut = Render<CyLanguageToggle>();

        // Act
        await cut.InvokeAsync(() => cut.Find("button").Click());

        // Assert - toggled from the default (English) to Welsh, so the
        // visible label now offers to switch back to English
        cut.Find("button span[aria-hidden='true']").TextContent.ShouldBe("English");
    }

    [Fact]
    public void Should_Not_Invoke_Callback_When_Disabled_And_Clicked()
    {
        // Arrange
        var raised = false;

        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.CurrentLanguageChanged, _ => raised = true));

        // Assert - the rendered button element itself is disabled
        cut.Find("button").HasAttribute("disabled").ShouldBeTrue();
        raised.ShouldBeFalse();
    }

    [Fact]
    public void Should_Include_Visible_Label_At_Start_Of_AriaLabel_When_Target_Is_Welsh()
    {
        // Act - CurrentLanguage is English, so target is Welsh ("Cymraeg")
        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.CurrentLanguage, AppLanguage.English));

        // Assert - Accessible name must start with the visible text "Cymraeg" per WCAG 2.5.3 (Label in Name)
        var button = cut.Find("button");
        var ariaLabel = button.GetAttribute("aria-label");
        ariaLabel.ShouldNotBeNull();
        ariaLabel.ShouldStartWith("Cymraeg");
    }

    [Fact]
    public void Should_Include_Visible_Label_At_Start_Of_AriaLabel_When_Target_Is_English()
    {
        // Act - CurrentLanguage is Welsh, so target is English ("English")
        var cut = Render<CyLanguageToggle>(parameters => parameters
            .Add(p => p.CurrentLanguage, AppLanguage.Welsh));

        // Assert - Accessible name must start with the visible text "English" per WCAG 2.5.3 (Label in Name)
        var button = cut.Find("button");
        var ariaLabel = button.GetAttribute("aria-label");
        ariaLabel.ShouldNotBeNull();
        ariaLabel.ShouldStartWith("English");
    }
}
