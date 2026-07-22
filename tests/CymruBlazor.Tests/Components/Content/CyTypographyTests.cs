using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyTypographyTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_Paragraph_By_Default()
    {
        // Act
        var cut = Render<CyTypography>(parameters => parameters
            .AddChildContent("Hello"));

        // Assert
        var element = cut.Find("p");
        element.ClassList.ShouldContain("cy-typography");
        element.ClassList.ShouldContain("cy-typography--body");
        element.TextContent.ShouldContain("Hello");
    }

    [Theory]
    [InlineData(TypographyVariant.H1, "h1")]
    [InlineData(TypographyVariant.H2, "h2")]
    [InlineData(TypographyVariant.H3, "h3")]
    [InlineData(TypographyVariant.H4, "h4")]
    [InlineData(TypographyVariant.H5, "h5")]
    [InlineData(TypographyVariant.H6, "h6")]
    [InlineData(TypographyVariant.Caption, "span")]
    [InlineData(TypographyVariant.Body, "p")]
    [InlineData(TypographyVariant.BodyLarge, "p")]
    [InlineData(TypographyVariant.BodySmall, "p")]
    public void Should_Render_Correct_Tag_For_Variant(TypographyVariant variant, string expectedTag)
    {
        // Act
        var cut = Render<CyTypography>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Hello"));

        // Assert
        cut.Nodes[0].NodeName.ToLowerInvariant().ShouldBe(expectedTag);
    }

    [Fact]
    public void As_Should_Override_The_Rendered_Tag_While_Keeping_The_Variant_Class()
    {
        // Act - visually an H2, but semantically an H3 (correct heading order)
        var cut = Render<CyTypography>(parameters => parameters
            .Add(p => p.Variant, TypographyVariant.H2)
            .Add(p => p.As, "h3")
            .AddChildContent("Hello"));

        // Assert
        cut.Nodes[0].NodeName.ToLowerInvariant().ShouldBe("h3");
        cut.Find("h3").ClassList.ShouldContain("cy-typography--h2");
    }
}
