using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyBadgeTests : TestContextBase
{
    [Fact]
    public void Should_Render_ChildContent_With_Neutral_Variant_By_Default()
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .AddChildContent("Cardiology"));

        // Assert
        var element = cut.Find("span.cy-badge");
        element.ClassList.ShouldContain("cy-badge--neutral");
        element.ClassList.ShouldContain("cy-badge--pill");
        cut.Markup.ShouldContain("Cardiology");
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-badge--primary")]
    [InlineData(ComponentColour.Secondary, "cy-badge--secondary")]
    [InlineData(ComponentColour.Tertiary, "cy-badge--tertiary")]
    [InlineData(ComponentColour.Success, "cy-badge--success")]
    [InlineData(ComponentColour.Warning, "cy-badge--warning")]
    [InlineData(ComponentColour.Danger, "cy-badge--danger")]
    [InlineData(ComponentColour.Info, "cy-badge--info")]
    [InlineData(ComponentColour.Surface, "cy-badge--surface")]
    [InlineData(ComponentColour.Neutral, "cy-badge--neutral")]
    public void Should_Apply_Variant_Css_Class(ComponentColour variant, string expectedClass)
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Label"));

        // Assert
        cut.Find("span.cy-badge").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Throw_When_Variant_Is_Unspecified()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyBadge>(parameters => parameters
                .Add(p => p.Variant, ComponentColour.Unspecified)
                .AddChildContent("Label")));
    }

    [Fact]
    public void Should_Not_Apply_Pill_Class_When_Pill_Is_False()
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .Add(p => p.Pill, false)
            .AddChildContent("Label"));

        // Assert
        cut.Find("span.cy-badge").ClassList.ShouldNotContain("cy-badge--pill");
    }

    [Fact]
    public void Should_Not_Render_Dismiss_Button_By_Default()
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .AddChildContent("Label"));

        // Assert
        cut.FindAll(".cy-badge__dismiss").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Dismiss_Button_When_Dismissible()
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .AddChildContent("Cardiology"));

        // Assert
        var dismiss = cut.Find(".cy-badge__dismiss");
        dismiss.GetAttribute("aria-label").ShouldBe("Remove");
    }

    [Fact]
    public void Should_Use_Custom_Dismiss_Aria_Label_When_Provided()
    {
        // Act
        var cut = Render<CyBadge>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.DismissAriaLabel, "Remove Cardiology filter")
            .AddChildContent("Cardiology"));

        // Assert
        cut.Find(".cy-badge__dismiss").GetAttribute("aria-label").ShouldBe("Remove Cardiology filter");
    }

    [Fact]
    public void Should_Invoke_OnDismiss_When_Dismiss_Button_Clicked()
    {
        // Arrange
        var dismissed = false;

        var cut = Render<CyBadge>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, () => dismissed = true)
            .AddChildContent("Cardiology"));

        // Act
        cut.Find(".cy-badge__dismiss").Click();

        // Assert
        dismissed.ShouldBeTrue();
    }
}
