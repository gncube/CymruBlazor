using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyPageHeaderTests : TestContextBase
{
    [Fact]
    public void Should_Render_Title_As_H1()
    {
        // Act
        var cut = Render<CyPageHeader>(parameters => parameters
            .Add(p => p.Title, "Manage patients"));

        // Assert
        cut.Find("h1").TextContent.ShouldContain("Manage patients");
    }

    [Fact]
    public void Should_Render_Subtitle_When_Provided()
    {
        // Act
        var cut = Render<CyPageHeader>(parameters => parameters
            .Add(p => p.Title, "Manage patients")
            .Add(p => p.Subtitle, "View and update patient records"));

        // Assert
        cut.Markup.ShouldContain("View and update patient records");
    }

    [Fact]
    public void Should_Not_Render_Breadcrumb_Or_Actions_Wrappers_When_Not_Provided()
    {
        // Act
        var cut = Render<CyPageHeader>(parameters => parameters
            .Add(p => p.Title, "Manage patients"));

        // Assert
        cut.FindAll(".cy-page-header__breadcrumb").Count.ShouldBe(0);
        cut.FindAll(".cy-page-header__actions").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Actions_When_Provided()
    {
        // Act
        var cut = Render<CyPageHeader>(parameters => parameters
            .Add(p => p.Title, "Manage patients")
            .Add(p => p.Actions, "<button>Edit</button>"));

        // Assert
        cut.Find(".cy-page-header__actions button").TextContent.ShouldContain("Edit");
    }
}
