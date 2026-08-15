using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyAlertTests : TestContextBase
{
    [Fact]
    public void Should_Render_ChildContent_With_Info_Severity_By_Default()
    {
        // Act
        var cut = Render<CyAlert>(parameters => parameters
            .AddChildContent("<p>Something happened.</p>"));

        // Assert
        var element = cut.Find("div.cy-alert");
        element.ClassList.ShouldContain("cy-alert--info");
        cut.Markup.ShouldContain("Something happened.");
    }

    [Theory]
    [InlineData(ComponentColour.Info, "status")]
    [InlineData(ComponentColour.Success, "status")]
    [InlineData(ComponentColour.Warning, "alert")]
    [InlineData(ComponentColour.Danger, "alert")]
    public void Should_Apply_Correct_Aria_Role_For_Severity(ComponentColour severity, string expectedRole)
    {
        // Act
        var cut = Render<CyAlert>(parameters => parameters
            .Add(p => p.Severity, severity)
            .AddChildContent("Message"));

        // Assert
        cut.Find(".cy-alert").GetAttribute("role").ShouldBe(expectedRole);
    }

    [Fact]
    public void Should_Throw_When_Severity_Is_Not_A_Supported_Value()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            Render<CyAlert>(parameters => parameters
                .Add(p => p.Severity, ComponentColour.Secondary)
                .AddChildContent("Message")));
    }

    [Fact]
    public void Should_Render_Title_When_Provided()
    {
        // Act
        var cut = Render<CyAlert>(parameters => parameters
            .Add(p => p.Title, "Heads up")
            .AddChildContent("Message"));

        // Assert
        cut.Find(".cy-alert__title").TextContent.ShouldContain("Heads up");
    }

    [Fact]
    public void Should_Not_Render_Dismiss_Button_By_Default()
    {
        // Act
        var cut = Render<CyAlert>(parameters => parameters
            .AddChildContent("Message"));

        // Assert
        cut.FindAll(".cy-alert__dismiss").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Invoke_OnDismiss_When_Dismiss_Button_Clicked()
    {
        // Arrange
        var dismissed = false;

        var cut = Render<CyAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, () => dismissed = true)
            .AddChildContent("Message"));

        // Act
        cut.Find(".cy-alert__dismiss").Click();

        // Assert
        dismissed.ShouldBeTrue();
    }
}
