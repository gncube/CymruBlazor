using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CySkipLinkTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Text_And_Target()
    {
        // Act
        var cut = Render<CySkipLink>();

        // Assert
        var link = cut.Find("a");
        link.GetAttribute("href").ShouldBe("#main-content");
        link.TextContent.ShouldContain("Skip to main content");
        link.ClassList.ShouldContain("u-sr-only-focusable");
    }

    [Fact]
    public void Should_Use_Custom_TargetId()
    {
        // Act
        var cut = Render<CySkipLink>(parameters => parameters
            .Add(p => p.TargetId, "content"));

        // Assert
        cut.Find("a").GetAttribute("href").ShouldBe("#content");
    }

    [Fact]
    public void Should_Render_Custom_ChildContent_When_Provided()
    {
        // Act
        var cut = Render<CySkipLink>(parameters => parameters
            .AddChildContent("Skip navigation"));

        // Assert
        cut.Find("a").TextContent.ShouldContain("Skip navigation");
    }
}
