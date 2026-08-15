using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyClusterTests : TestContextBase
{
    [Fact]
    public void Should_Render_Default_Cluster()
    {
        // Act
        var cut = Render<CyCluster>();

        // Assert - Wrap defaults to true, AlignItems defaults to Center.
        var element = cut.Find("div");
        element.ClassList.ShouldContain("cy-cluster");
        element.ClassList.ShouldContain("cy-cluster--wrap");
        element.ClassList.ShouldContain("cy-align-center");
    }

    [Fact]
    public void Should_Omit_Wrap_Class_When_Wrap_Is_False()
    {
        // Act
        var cut = Render<CyCluster>(parameters => parameters.Add(p => p.Wrap, false));

        // Assert
        cut.Find("div").ClassList.ShouldNotContain("cy-cluster--wrap");
    }
}
