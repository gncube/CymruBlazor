using Bunit;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Enums;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Feedback;

public sealed class CyProgressTests : TestContextBase
{
    [Fact]
    public void Should_Render_Determinate_Progress_With_Value_And_Max()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 42)
            .Add(p => p.Max, 100)
            .Add(p => p.Label, "Medical Ward A"));

        // Assert
        var progress = cut.Find("progress.cy-progress__bar");
        progress.GetAttribute("value").ShouldBe("42");
        progress.GetAttribute("max").ShouldBe("100");
        cut.Find("span.cy-progress__label").TextContent.ShouldBe("Medical Ward A");
    }

    [Fact]
    public void Should_Render_Indeterminate_Progress_Without_Value_Attribute_When_Value_Is_Null()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, (double?)null)
            .Add(p => p.Label, "Checking bed availability"));

        // Assert
        var progress = cut.Find("progress.cy-progress__bar");
        progress.HasAttribute("value").ShouldBeFalse();
        cut.Find("div.cy-progress").ClassList.ShouldContain("cy-progress--indeterminate");
    }

    [Fact]
    public void Should_Clamp_Value_Above_Max()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 150)
            .Add(p => p.Max, 100)
            .Add(p => p.Label, "Overfull"));

        // Assert
        cut.Find("progress.cy-progress__bar").GetAttribute("value").ShouldBe("100");
    }

    [Fact]
    public void Should_Show_Percentage_Value_Text_When_Requested()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 18)
            .Add(p => p.Max, 20)
            .Add(p => p.Label, "Critical Care")
            .Add(p => p.ShowValueText, true));

        // Assert
        cut.Find("span.cy-progress__value-text").TextContent.ShouldBe("90%");
        cut.Find("progress.cy-progress__bar").GetAttribute("aria-valuetext").ShouldBe("90%");
    }

    [Fact]
    public void Should_Use_Custom_Value_Text_Over_Computed_Percentage()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 18)
            .Add(p => p.Max, 20)
            .Add(p => p.Label, "Critical Care")
            .Add(p => p.ValueText, "18 of 20 beds"));

        // Assert
        cut.Find("span.cy-progress__value-text").TextContent.ShouldBe("18 of 20 beds");
    }

    [Fact]
    public void Should_Use_AriaLabel_When_No_Visible_Label_Is_Set()
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 50)
            .Add(p => p.AriaLabel, "Upload progress"));

        // Assert
        var progress = cut.Find("progress.cy-progress__bar");
        progress.GetAttribute("aria-label").ShouldBe("Upload progress");
        cut.FindAll(".cy-progress__label").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Throw_When_Neither_Label_Nor_AriaLabel_Is_Set()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyProgress>(parameters => parameters
                .Add(p => p.Value, 50)));
    }

    [Fact]
    public void Should_Throw_When_Both_Label_And_AriaLabel_Are_Set()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyProgress>(parameters => parameters
                .Add(p => p.Value, 50)
                .Add(p => p.Label, "Ward A")
                .Add(p => p.AriaLabel, "Ward A occupancy")));
    }

    [Fact]
    public void Should_Throw_When_Max_Is_Zero()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyProgress>(parameters => parameters
                .Add(p => p.Max, 0)
                .Add(p => p.Label, "Ward A")));
    }

    [Theory]
    [InlineData(ComponentColour.Danger, "cy-progress--danger")]
    [InlineData(ComponentColour.Success, "cy-progress--success")]
    public void Should_Apply_Colour_Css_Class(ComponentColour colour, string expectedClass)
    {
        // Act
        var cut = Render<CyProgress>(parameters => parameters
            .Add(p => p.Value, 50)
            .Add(p => p.Label, "Ward A")
            .Add(p => p.Colour, colour));

        // Assert
        cut.Find("div.cy-progress").ClassList.ShouldContain(expectedClass);
    }
}
