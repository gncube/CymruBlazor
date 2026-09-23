using Bunit;
using CymruBlazor.Components.Data;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components.Data;

public sealed class CyPaginationTests : TestContextBase
{
    [Fact]
    public void Should_Render_Nothing_When_TotalPages_Is_One_Or_Fewer()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 1));

        // Assert
        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void Should_Render_Nav_With_Default_Aria_Label()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 5));

        // Assert
        cut.Find("nav.cy-pagination").GetAttribute("aria-label").ShouldBe("Pagination");
    }

    [Fact]
    public void Should_Show_All_Pages_Without_Ellipsis_When_Total_Is_Small()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 3)
            .Add(p => p.TotalPages, 5));

        // Assert
        cut.FindAll(".cy-pagination__page").Count.ShouldBe(5);
        cut.FindAll(".cy-pagination__ellipsis").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Mark_Current_Page_With_Aria_Current_And_Not_As_A_Button()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 3)
            .Add(p => p.TotalPages, 5));

        // Assert
        var current = cut.Find(".cy-pagination__page--current");
        current.TagName.ShouldBe("SPAN");
        current.GetAttribute("aria-current").ShouldBe("page");
        current.TextContent.Trim().ShouldBe("3");
    }

    [Fact]
    public void Should_Render_Ellipsis_For_Large_Page_Counts()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 5)
            .Add(p => p.TotalPages, 10));

        // Assert
        cut.FindAll(".cy-pagination__ellipsis").Count.ShouldBe(2);

        // 1, ellipsis, 4, 5, 6, ellipsis, 10 -> page numbers 1,4,5,6,10
        var pageTexts = cut.FindAll(".cy-pagination__page, .cy-pagination__page--current")
            .Select(e => e.TextContent.Trim())
            .ToList();
        pageTexts.ShouldBe(["1", "4", "5", "6", "10"]);
    }

    [Fact]
    public void Should_Disable_Previous_Button_On_First_Page()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 5));

        // Assert
        cut.Find(".cy-pagination__control--previous").HasAttribute("disabled").ShouldBeTrue();
        cut.Find(".cy-pagination__control--next").HasAttribute("disabled").ShouldBeFalse();
    }

    [Fact]
    public void Should_Disable_Next_Button_On_Last_Page()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 5)
            .Add(p => p.TotalPages, 5));

        // Assert
        cut.Find(".cy-pagination__control--next").HasAttribute("disabled").ShouldBeTrue();
        cut.Find(".cy-pagination__control--previous").HasAttribute("disabled").ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Invoke_CurrentPageChanged_When_A_Page_Button_Is_Clicked()
    {
        // Arrange
        int? changedTo = null;

        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 5)
            .Add(p => p.CurrentPageChanged, EventCallback.Factory.Create<int>(this, page => changedTo = page)));

        // Act
        var pageTwoButton = cut.FindAll(".cy-pagination__page")
            .First(e => e.TextContent.Trim() == "2");
        await pageTwoButton.ClickAsync(new());

        // Assert
        changedTo.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Invoke_CurrentPageChanged_When_Next_Is_Clicked()
    {
        // Arrange
        int? changedTo = null;

        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 2)
            .Add(p => p.TotalPages, 5)
            .Add(p => p.CurrentPageChanged, EventCallback.Factory.Create<int>(this, page => changedTo = page)));

        // Act
        await cut.Find(".cy-pagination__control--next").ClickAsync(new());

        // Assert
        changedTo.ShouldBe(3);
    }

    [Fact]
    public void Should_Use_Custom_Aria_Label_Formats()
    {
        // Act
        var cut = Render<CyPagination>(parameters => parameters
            .Add(p => p.CurrentPage, 2)
            .Add(p => p.TotalPages, 3)
            .Add(p => p.PageAriaLabelFormat, "Tudalen {0}")
            .Add(p => p.CurrentPageAriaLabelFormat, "Tudalen gyfredol, tudalen {0}"));

        // Assert
        cut.Find(".cy-pagination__page--current").GetAttribute("aria-label").ShouldBe("Tudalen gyfredol, tudalen 2");

        var otherPage = cut.FindAll(".cy-pagination__page")
            .First(e => e.TextContent.Trim() == "1");
        otherPage.GetAttribute("aria-label").ShouldBe("Tudalen 1");
    }

    [Fact]
    public void Should_Throw_When_CurrentPage_Is_Out_Of_Range()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyPagination>(parameters => parameters
                .Add(p => p.CurrentPage, 6)
                .Add(p => p.TotalPages, 5)));
    }

    [Fact]
    public void Should_Throw_When_TotalPages_Is_Negative()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyPagination>(parameters => parameters
                .Add(p => p.TotalPages, -1)));
    }
}
