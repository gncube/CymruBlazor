using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyBreadcrumbTests : TestContextBase
{
    [Fact]
    public void Should_Render_Nav_With_Aria_Label()
    {
        // Act
        var cut = Render<CyBreadcrumb>(parameters => parameters
            .AddChildContent("<li>Item</li>"));

        // Assert
        cut.Find("nav").GetAttribute("aria-label").ShouldBe("Breadcrumb");
    }

    [Fact]
    public void Item_With_Href_Should_Render_As_Link()
    {
        // Act
        var cut = Render<CyBreadcrumbItem>(parameters => parameters
            .Add(p => p.Text, "Home")
            .Add(p => p.Href, "/"));

        // Assert
        var link = cut.Find("a");
        link.GetAttribute("href").ShouldBe("/");
        link.TextContent.ShouldContain("Home");
    }

    [Fact]
    public void Item_Without_Href_Should_Render_As_Current_Page_Text()
    {
        // Act
        var cut = Render<CyBreadcrumbItem>(parameters => parameters
            .Add(p => p.Text, "Current page"));

        // Assert
        cut.FindAll("a").Count.ShouldBe(0);
        var span = cut.Find("span");
        span.GetAttribute("aria-current").ShouldBe("page");
        span.TextContent.ShouldContain("Current page");
    }
}
