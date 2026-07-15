using Bunit;
using CymruBlazor.Components.Layout;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components;

public class CyCenterTests : BunitContext
{
    [Fact]
    public void Should_Render_Default_Center()
    {
        var cut = Render<CyCenter>(p => p.AddChildContent("Hello"));

        var div = cut.Find("div");

        div.ClassList.ShouldContain("cy-center");
        div.ClassList.ShouldContain("cy-center--horizontal");
        div.ClassList.ShouldContain("cy-center--vertical");
    }


    [Fact]
    public void Should_Disable_Horizontal_Centering()
    {
        var cut = Render<CyCenter>(p =>
            p.Add(x => x.Horizontal, false));

        var div = cut.Find("div");

        div.ClassList.ShouldNotContain("cy-center--horizontal");
        div.ClassList.ShouldContain("cy-center--vertical");
    }


    [Fact]
    public void Should_Enable_FullHeight()
    {
        var cut = Render<CyCenter>(p =>
            p.Add(x => x.FullHeight, true));

        cut.Find("div")
            .ClassList
            .ShouldContain("cy-center--full-height");
    }
}
