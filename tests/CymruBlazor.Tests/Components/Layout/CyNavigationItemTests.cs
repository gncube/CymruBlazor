using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyNavigationItemTests : TestContextBase
{
    [Fact]
    public void Should_Render_Link_With_Text_And_Href()
    {
        // Act
        var cut = Render<CyNavigationItem>(parameters => parameters
            .Add(p => p.Text, "Components")
            .Add(p => p.Href, "components"));

        // Assert
        var link = cut.Find("a");
        link.TextContent.ShouldContain("Components");
        link.GetAttribute("href").ShouldBe("components");
    }

    [Fact]
    public void Should_Render_As_List_Item()
    {
        // Act
        var cut = Render<CyNavigationItem>(parameters => parameters
            .Add(p => p.Text, "Components")
            .Add(p => p.Href, "components"));

        // Assert
        cut.Nodes[0].NodeName.ToLowerInvariant().ShouldBe("li");
    }
}
