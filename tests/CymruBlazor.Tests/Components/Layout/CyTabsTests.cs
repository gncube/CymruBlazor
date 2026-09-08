using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyTabsTests : TestContextBase
{
    private static RenderFragment ThreePanels(bool secondDisabled = false) => builder =>
    {
        builder.OpenComponent<CyTabPanel>(0);
        builder.AddComponentParameter(1, "TabId", "examples");
        builder.AddComponentParameter(2, "Title", "Examples");
        builder.AddComponentParameter(3, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Examples content")));
        builder.CloseComponent();

        builder.OpenComponent<CyTabPanel>(4);
        builder.AddComponentParameter(5, "TabId", "api");
        builder.AddComponentParameter(6, "Title", "API");
        builder.AddComponentParameter(7, "Disabled", secondDisabled);
        builder.AddComponentParameter(8, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "API content")));
        builder.CloseComponent();

        builder.OpenComponent<CyTabPanel>(9);
        builder.AddComponentParameter(10, "TabId", "accessibility");
        builder.AddComponentParameter(11, "Title", "Accessibility");
        builder.AddComponentParameter(12, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Accessibility content")));
        builder.CloseComponent();
    };

    [Fact]
    public void Should_Render_A_Tab_Button_Per_Panel_And_Activate_The_First_By_Default()
    {
        // Act
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        var tabs = cut.FindAll("[role='tab']");
        tabs.Count.ShouldBe(3);
        tabs[0].GetAttribute("aria-selected").ShouldBe("true");
        tabs[1].GetAttribute("aria-selected").ShouldBe("false");
        tabs[2].GetAttribute("aria-selected").ShouldBe("false");

        cut.FindAll("[role='tabpanel']").Count.ShouldBe(1);
        cut.Markup.ShouldContain("Examples content");
        cut.Markup.ShouldNotContain("API content");
    }

    [Fact]
    public void Should_Respect_Explicit_ActiveTabId()
    {
        // Act
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabId, "api")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        cut.Markup.ShouldContain("API content");
        cut.Find("[role='tabpanel']").GetAttribute("aria-labelledby")
            .ShouldBe(cut.FindAll("[role='tab']")[1].GetAttribute("id"));
    }

    [Fact]
    public void Should_Switch_Panel_When_Tab_Clicked()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.FindAll("[role='tab']")[2].Click();

        // Assert
        cut.Markup.ShouldContain("Accessibility content");
        cut.FindAll("[role='tab']")[2].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Invoke_ActiveTabIdChanged_When_Tab_Activated()
    {
        // Arrange
        string? changedTo = null;

        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabIdChanged, id => changedTo = id)
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.FindAll("[role='tab']")[1].Click();

        // Assert
        changedTo.ShouldBe("api");
    }

    [Fact]
    public void Should_Use_Roving_Tabindex()
    {
        // Act
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        var tabs = cut.FindAll("[role='tab']");
        tabs[0].GetAttribute("tabindex").ShouldBe("0");
        tabs[1].GetAttribute("tabindex").ShouldBe("-1");
        tabs[2].GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public void Should_Activate_Next_Tab_On_ArrowRight()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Wrap_To_First_Tab_When_ArrowRight_On_Last_Tab()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabId, "accessibility")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Activate_Previous_Tab_On_ArrowLeft()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabId, "api")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Jump_To_Last_Tab_On_End()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        cut.FindAll("[role='tab']")[2].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Jump_To_First_Tab_On_Home()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ActiveTabId, "accessibility")
            .Add(p => p.ChildContent, ThreePanels()));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Should_Skip_Disabled_Tab_On_ArrowRight()
    {
        // Arrange
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels(secondDisabled: true)));

        // Act
        cut.Find(".cy-tabs__list").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert - the disabled "api" tab (index 1) is skipped, landing on
        // "accessibility" (index 2).
        cut.FindAll("[role='tab']")[2].GetAttribute("aria-selected").ShouldBe("true");
        cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected").ShouldBe("false");
    }

    [Fact]
    public void Should_Throw_When_Panel_Used_Outside_Tabs()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyTabPanel>(parameters => parameters
                .Add(p => p.TabId, "orphan")
                .Add(p => p.Title, "Orphan")
                .AddChildContent("Content")));
    }

    [Fact]
    public void Should_Link_Tabpanel_To_Its_Tab_Via_Aria_Attributes()
    {
        // Act
        var cut = Render<CyTabs>(parameters => parameters
            .Add(p => p.TabListAriaLabel, "Component documentation")
            .Add(p => p.ChildContent, ThreePanels()));

        // Assert
        var tab = cut.Find("[role='tab']");
        var panel = cut.Find("[role='tabpanel']");

        tab.GetAttribute("aria-controls").ShouldBe(panel.GetAttribute("id"));
        panel.GetAttribute("aria-labelledby").ShouldBe(tab.GetAttribute("id"));
        panel.GetAttribute("tabindex").ShouldBe("0");
    }
}
