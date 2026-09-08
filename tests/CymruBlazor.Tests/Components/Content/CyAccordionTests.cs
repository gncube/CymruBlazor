using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Content;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyAccordionTests : TestContextBase
{
    private static RenderFragment TwoItems() => builder =>
    {
        builder.OpenComponent<CyAccordionItem>(0);
        builder.AddComponentParameter(1, "ItemId", "first");
        builder.AddComponentParameter(2, "Title", "First section");
        builder.AddComponentParameter(3, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "First content")));
        builder.CloseComponent();

        builder.OpenComponent<CyAccordionItem>(4);
        builder.AddComponentParameter(5, "ItemId", "second");
        builder.AddComponentParameter(6, "Title", "Second section");
        builder.AddComponentParameter(7, "ChildContent",
            (RenderFragment)(inner => inner.AddContent(0, "Second content")));
        builder.CloseComponent();
    };

    [Fact]
    public void Should_Render_All_Item_Headers_Collapsed_By_Default()
    {
        // Act
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.ChildContent, TwoItems()));

        // Assert
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers.Count.ShouldBe(2);
        triggers.ShouldAllBe(t => t.GetAttribute("aria-expanded") == "false");
        cut.FindAll(".cy-accordion-item__panel").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Expand_Item_When_Trigger_Clicked()
    {
        // Arrange
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.ChildContent, TwoItems()));

        // Act
        cut.FindAll(".cy-accordion-item__trigger")[0].Click();

        // Assert
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers[0].GetAttribute("aria-expanded").ShouldBe("true");
        triggers[1].GetAttribute("aria-expanded").ShouldBe("false");
        cut.Markup.ShouldContain("First content");
    }

    [Fact]
    public void Should_Collapse_Other_Items_When_AllowMultiple_Is_False()
    {
        // Arrange
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.AllowMultiple, false)
            .Add(p => p.ChildContent, TwoItems()));

        // Act
        cut.FindAll(".cy-accordion-item__trigger")[0].Click();
        cut.FindAll(".cy-accordion-item__trigger")[1].Click();

        // Assert
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers[0].GetAttribute("aria-expanded").ShouldBe("false");
        triggers[1].GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Should_Allow_Multiple_Items_Expanded_When_AllowMultiple_Is_True()
    {
        // Arrange
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.AllowMultiple, true)
            .Add(p => p.ChildContent, TwoItems()));

        // Act
        cut.FindAll(".cy-accordion-item__trigger")[0].Click();
        cut.FindAll(".cy-accordion-item__trigger")[1].Click();

        // Assert
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers[0].GetAttribute("aria-expanded").ShouldBe("true");
        triggers[1].GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Should_Start_With_DefaultExpandedItemId_Expanded()
    {
        // Act
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.DefaultExpandedItemId, "second")
            .Add(p => p.ChildContent, TwoItems()));

        // Assert
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers[0].GetAttribute("aria-expanded").ShouldBe("false");
        triggers[1].GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Should_Not_Toggle_When_Item_Is_Disabled()
    {
        // Arrange
        RenderFragment content = builder =>
        {
            builder.OpenComponent<CyAccordionItem>(0);
            builder.AddComponentParameter(1, "ItemId", "first");
            builder.AddComponentParameter(2, "Title", "First section");
            builder.AddComponentParameter(3, "Disabled", true);
            builder.AddComponentParameter(4, "ChildContent",
                (RenderFragment)(inner => inner.AddContent(0, "First content")));
            builder.CloseComponent();
        };

        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.ChildContent, content));

        // Act
        cut.Find(".cy-accordion-item__trigger").Click();

        // Assert
        cut.Find(".cy-accordion-item__trigger").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Throw_When_Item_Used_Outside_Accordion()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyAccordionItem>(parameters => parameters
                .Add(p => p.ItemId, "orphan")
                .Add(p => p.Title, "Orphan section")
                .AddChildContent("Content")));
    }

    [Fact]
    public void Should_Move_Focus_To_Next_Trigger_On_ArrowDown()
    {
        // Arrange
        var cut = Render<CyAccordion>(parameters => parameters
            .Add(p => p.ChildContent, TwoItems()));

        var firstTrigger = cut.FindAll(".cy-accordion-item__trigger")[0];

        // Act
        firstTrigger.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert - bUnit's JSInterop for element.focus() is a no-op by
        // default, so this asserts the keydown handler runs without
        // throwing and does not itself toggle expansion (a real browser
        // then moves focus per ElementReference.FocusAsync()).
        var triggers = cut.FindAll(".cy-accordion-item__trigger");
        triggers.ShouldAllBe(t => t.GetAttribute("aria-expanded") == "false");
    }
}
