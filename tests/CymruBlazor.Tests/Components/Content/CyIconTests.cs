using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Icons;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyIconTests : TestContextBase
{
    [Fact]
    public void Should_Render_Svg_With_Correct_Grid()
    {
        // Act
        var cut = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search"));

        // Assert
        var svg = cut.Find("svg");
        svg.GetAttribute("viewBox").ShouldBe("0 0 24 24");
        svg.GetAttribute("width").ShouldBe("24");
        svg.GetAttribute("height").ShouldBe("24");
        svg.GetAttribute("stroke").ShouldBe("currentColor");
    }

    [Fact]
    public void Should_Render_Path_Content_For_Known_Icon()
    {
        // Act
        var cut = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "check"));

        // Assert
        cut.FindAll("svg path").Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Should_Be_Hidden_From_Assistive_Technology_By_Default()
    {
        // Act
        var cut = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search"));

        // Assert
        var svg = cut.Find("svg");
        svg.GetAttribute("aria-hidden").ShouldBe("true");
        svg.HasAttribute("role").ShouldBeFalse();
    }

    [Fact]
    public void Should_Expose_As_Img_Role_When_Label_Is_Set()
    {
        // Act
        var cut = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search")
            .Add(p => p.Label, "Search"));

        // Assert
        var svg = cut.Find("svg");
        svg.GetAttribute("role").ShouldBe("img");
        svg.GetAttribute("aria-label").ShouldBe("Search");
        svg.HasAttribute("aria-hidden").ShouldBeFalse();
    }

    [Fact]
    public void Should_Respect_Custom_Size()
    {
        // Act
        var cut = Render<CyIcon>(parameters => parameters
            .Add(p => p.Name, "search")
            .Add(p => p.Size, 32));

        // Assert
        var svg = cut.Find("svg");
        svg.GetAttribute("width").ShouldBe("32");
        svg.GetAttribute("height").ShouldBe("32");
    }

    [Fact]
    public void Should_Throw_For_Unknown_Icon_Name()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            Render<CyIcon>(parameters => parameters
                .Add(p => p.Name, "this-icon-does-not-exist")));
    }
}

public sealed class IconRegistryTests
{
    [Fact]
    public void Should_Contain_106_Or_More_Icons()
    {
        IconRegistry.AllNames.Count.ShouldBeGreaterThanOrEqualTo(106);
    }

    [Theory]
    [InlineData("patient")]
    [InlineData("critical")]
    [InlineData("search")]
    [InlineData("appointment")]
    [InlineData("moon")]
    [InlineData("sun")]
    public void Should_Contain_Well_Known_Icons(string name)
    {
        IconRegistry.Exists(name).ShouldBeTrue();
    }

    [Fact]
    public void Should_Have_A_Domain_For_Moon_And_Sun()
    {
        IconRegistry.GetDomain("moon").ShouldBe("ui");
        IconRegistry.GetDomain("sun").ShouldBe("ui");
    }

    [Fact]
    public void Moon_Markup_Should_Match_Verified_Lucide_Source()
    {
        // Verified byte-for-byte against lucide-static@1.34.0's moon.svg
        IconRegistry.GetMarkup("moon").ShouldBe(
            "<path d=\"M20.985 12.486a9 9 0 1 1-9.473-9.472c.405-.022.617.46.402.803a6 6 0 0 0 8.268 8.268c.344-.215.825-.004.803.401\" />");
    }

    [Fact]
    public void GetMarkup_Should_Throw_For_Unknown_Name()
    {
        Should.Throw<KeyNotFoundException>(() =>
            IconRegistry.GetMarkup("not-a-real-icon"));
    }

    [Fact]
    public void Every_Registered_Icon_Should_Have_Non_Empty_Markup()
    {
        foreach (var name in IconRegistry.AllNames)
        {
            IconRegistry.GetMarkup(name).ShouldNotBeNullOrWhiteSpace();
        }
    }
}
