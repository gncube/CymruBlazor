using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyFooterTests : TestContextBase
{
    [Fact]
    public void Should_Render_Copyright_When_Provided()
    {
        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.Copyright, "(c) 2026 CymruBlazor contributors"));

        // Assert
        cut.Find(".cy-footer__copyright").TextContent.ShouldContain("2026 CymruBlazor contributors");
    }

    [Fact]
    public void Should_Render_Links_When_ChildContent_Provided()
    {
        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .AddChildContent("<a href=\"/privacy\">Privacy</a>"));

        // Assert
        cut.Find(".cy-footer__links a").TextContent.ShouldContain("Privacy");
    }

    [Fact]
    public void Should_Not_Render_Empty_Wrappers_When_Nothing_Provided()
    {
        // Act
        var cut = Render<CyFooter>();

        // Assert
        cut.FindAll(".cy-footer__links").Count.ShouldBe(0);
        cut.FindAll(".cy-footer__copyright").Count.ShouldBe(0);
    }
}
