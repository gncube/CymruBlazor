using Bunit;
using CymruBlazor.Components.Data;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Data;

public sealed class CyTableTests : TestContextBase
{
    private static readonly RenderFragment SimpleRows = builder =>
    {
        builder.OpenElement(0, "thead");
        builder.OpenElement(1, "tr");
        builder.OpenElement(2, "th");
        builder.AddAttribute(3, "scope", "col");
        builder.AddContent(4, "Specialty");
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
        builder.OpenElement(5, "tbody");
        builder.OpenElement(6, "tr");
        builder.OpenElement(7, "td");
        builder.AddContent(8, "Cardiology");
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
    };

    [Fact]
    public void Should_Render_Caption_And_ChildContent_Inside_A_Scroll_Region_By_Default()
    {
        // Act
        var cut = Render<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.ChildContent, SimpleRows));

        // Assert
        var region = cut.Find("div.cy-table__scroll");
        region.GetAttribute("role").ShouldBe("region");
        region.GetAttribute("tabindex").ShouldBe("0");

        var caption = cut.Find("table.cy-table caption");
        caption.TextContent.Trim().ShouldBe("Waiting list by specialty");
        caption.ClassList.ShouldNotContain("u-sr-only");

        region.GetAttribute("aria-labelledby").ShouldBe(caption.GetAttribute("id"));
        cut.Markup.ShouldContain("Cardiology");
    }

    [Fact]
    public void Should_Visually_Hide_Caption_When_Requested()
    {
        // Act
        var cut = Render<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.CaptionVisuallyHidden, true)
            .Add(p => p.ChildContent, SimpleRows));

        // Assert
        cut.Find("table.cy-table caption").ClassList.ShouldContain("u-sr-only");
    }

    [Fact]
    public void Should_Omit_Scroll_Region_When_Disabled()
    {
        // Act
        var cut = Render<CyTable>(parameters => parameters
            .Add(p => p.Caption, "Waiting list by specialty")
            .Add(p => p.ScrollContainer, false)
            .Add(p => p.ChildContent, SimpleRows));

        // Assert
        cut.FindAll("div.cy-table__scroll").Count.ShouldBe(0);
        cut.Find("table.cy-table").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Throw_When_Caption_Is_Empty()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyTable>(parameters => parameters
                .Add(p => p.Caption, string.Empty)
                .Add(p => p.ChildContent, SimpleRows)));
    }
}
