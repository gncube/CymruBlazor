using Bunit;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Button;
using CymruBlazor.Components.Content;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Real-layout assertions (roadmap 1.3.0-D): axe checks semantics and contrast, but not
/// "is this on screen, at a sensible width, without making the page scroll sideways". These render
/// components with deliberately awkward content (long words, long code lines, long labels) and check
/// the resolved geometry at phone, tablet and desktop widths in light and dark themes - WCAG 1.4.10 Reflow.
/// </summary>
public sealed class ComputedStyleTests : AxeTestBase
{
    private const string LongWord =
        "Averyveryveryverylongunbrokenwordthatcouldoverflowthecontainerifnotwrappedproperlyaslongasaurl";

    public ComputedStyleTests()
    {
        Services.AddSingleton(new Mock<IMediator>().Object);
    }

    private string RenderScenario(string scenario) => scenario switch
    {
        "alert" => Render<CyAlert>(p => p
            .Add(a => a.Title, "A long alert title that has to wrap on a narrow viewport")
            .Add(a => a.Dismissible, true)
            .Add(a => a.ChildContent, (RenderFragment)(b => b.AddContent(0, "Body text. " + LongWord)))).Markup,

        "code-block" => Render<CyCodeBlock>(p => p
            .Add(c => c.Language, "csharp")
            .Add(c => c.Code, "var extremelyLongIdentifierName = SomeService.CallWithManyArguments(argumentOne, argumentTwo, argumentThree, argumentFour, argumentFive);")).Markup,

        "button" => Render<CyButton>(p => p
            .AddChildContent("A button label that is much longer than a phone screen is wide but should still wrap sensibly")).Markup,

        "badge" => Render<CyBadge>(p => p.AddChildContent("Status")).Markup,

        _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
    };

    [Theory]
    [InlineData("alert", 320, "light")]
    [InlineData("alert", 320, "dark")]
    [InlineData("alert", 768, "light")]
    [InlineData("alert", 1280, "light")]
    [InlineData("code-block", 320, "light")]
    [InlineData("code-block", 320, "dark")]
    [InlineData("code-block", 768, "light")]
    [InlineData("code-block", 1280, "light")]
    [InlineData("button", 320, "light")]
    [InlineData("button", 1280, "dark")]
    [InlineData("badge", 320, "light")]
    public async Task Component_Is_Visible_On_Screen_And_Does_Not_Make_The_Page_Scroll_Sideways(
        string scenario, int width, string theme)
    {
        await LoadHostedAsync(RenderScenario(scenario), theme, width);

        var metrics = await MeasureAsync(".cy-theme-provider > :first-child");

        metrics.Found.ShouldBeTrue(scenario);
        metrics.Visible.ShouldBeTrue(metrics.ToString());
        metrics.Width.ShouldBeGreaterThan(0);
        metrics.InViewportHorizontally.ShouldBeTrue(metrics.ToString());
        metrics.PageOverflowsHorizontally.ShouldBeFalse(metrics.ToString());
    }

    [Theory]
    [InlineData(320)]
    [InlineData(1280)]
    public async Task Alert_Fills_The_Available_Width(int width)
    {
        await LoadHostedAsync(RenderScenario("alert"), width: width);

        var metrics = await MeasureAsync(".cy-alert");

        // The theme wrapper adds no padding, so a block-level alert resolves to the viewport width.
        metrics.Width.ShouldBeGreaterThan(width * 0.9, metrics.ToString());
    }
}
