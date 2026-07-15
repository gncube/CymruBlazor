using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Tests;

namespace CymruBlazor.Tests.Components.Accessibility;

public sealed class CyScreenReaderOnlyTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_Div_By_Default()
    {
        // Act
        var cut = Render<CyScreenReaderOnly>(parameters => parameters
            .Add(p => p.ChildContent, "Hidden Accessibility Instruction"));

        // Assert
        var element = cut.Find("div");
        element.ClassList.ShouldContain("sr-only");
        element.TextContent.ShouldBe("Hidden Accessibility Instruction");
    }

    [Fact]
    public void Should_Merge_User_Provided_Css_Classes()
    {
        // Act
        var cut = Render<CyScreenReaderOnly>(parameters => parameters
            .Add(p => p.Class, "custom-helper-class"));

        // Assert
        var element = cut.Find("div");
        element.ClassList.ShouldContain("sr-only");
        element.ClassList.ShouldContain("custom-helper-class");
    }

    [Fact]
    public void Should_Forward_Additional_HTML_Attributes()
    {
        // Act
        var cut = Render<CyScreenReaderOnly>(parameters => parameters
            .AddUnmatched("data-testid", "accessibility-node")
            .AddUnmatched("title", "Hidden metadata"));

        // Assert
        var element = cut.Find("div");
        element.GetAttribute("data-testid").ShouldBe("accessibility-node");
        element.GetAttribute("title").ShouldBe("Hidden metadata");
    }
}
