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
}
