using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// WCAG 1.4.13 (Content on Hover or Focus) for <c>CyTooltip</c> in a real browser: shown on hover
/// and on focus, dismissible with Escape without moving focus, hoverable, and axe-clean while
/// visible in every theme.
/// </summary>
public sealed class CyTooltipAccessibilityTests : AxeTestBase
{
    public CyTooltipAccessibilityTests()
    {
        // CyTooltip imports the overlay module after render; the page loads the real one instead.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private string RenderTooltip(TooltipPlacement placement = TooltipPlacement.Top)
    {
        var tooltip = Render<CyTooltip>(p => p
            .Add(t => t.Text, "Occupied beds out of total available beds.")
            .Add(t => t.Placement, placement)
            .AddChildContent("About ward occupancy"));

        return "<button id=\"before\" type=\"button\">before</button>"
            + "<div style=\"padding:120px 200px\">" + tooltip.Markup + "</div>"
            + "<button id=\"after\" type=\"button\">after</button>";
    }

    private async Task InstallAsync() => await Page.EvaluateAsync("() => overlay.installTooltips()");

    private async Task<string> VisibilityAsync() =>
        await Page.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('[role=tooltip]')).visibility");

    // The tooltip fades in over ~120ms; scanning mid-fade would blend colours and report false contrast failures.
    private Task SettleAsync() => Page.WaitForTimeoutAsync(400);

    [Fact]
    public async Task Is_Hidden_Until_Hovered_Or_Focused()
    {
        await LoadHostedAsync(RenderTooltip());

        (await VisibilityAsync()).ShouldBe("hidden");
    }

    [Fact]
    public async Task Shows_On_Keyboard_Focus_And_Hides_On_Blur()
    {
        await LoadHostedAsync(RenderTooltip());
        await Page.FocusAsync("#before");

        await Page.Keyboard.PressAsync("Tab");
        (await ActiveElementIdAsync()).ShouldBe(string.Empty); // the trigger has no id; focus is on it, not on "before"
        (await VisibilityAsync()).ShouldBe("visible");

        await Page.FocusAsync("#after");
        (await VisibilityAsync()).ShouldBe("hidden");
    }

    [Fact]
    public async Task Shows_On_Hover_And_Stays_Visible_When_The_Pointer_Moves_Onto_The_Tooltip()
    {
        await LoadHostedAsync(RenderTooltip());
        await InstallAsync();

        var trigger = await Page.Locator(".cy-tooltip__trigger").BoundingBoxAsync();
        await Page.Mouse.MoveAsync((float)(trigger!.X + trigger.Width / 2), (float)(trigger.Y + trigger.Height / 2));
        (await VisibilityAsync()).ShouldBe("visible");

        var tip = await Page.Locator("[role=tooltip]").BoundingBoxAsync();
        await Page.Mouse.MoveAsync((float)(tip!.X + tip.Width / 2), (float)(tip.Y + tip.Height / 2), new() { Steps = 8 });

        (await VisibilityAsync()).ShouldBe("visible");
    }

    [Fact]
    public async Task Escape_Dismisses_It_While_Focus_Stays_And_It_Returns_On_Refocus()
    {
        await LoadHostedAsync(RenderTooltip());
        await InstallAsync();
        await Page.FocusAsync("#before");
        await Page.Keyboard.PressAsync("Tab");

        await Page.Keyboard.PressAsync("Escape");

        (await VisibilityAsync()).ShouldBe("hidden");
        (await Page.EvaluateAsync<bool>("() => document.activeElement.classList.contains('cy-tooltip__trigger')")).ShouldBeTrue();

        await Page.Keyboard.PressAsync("Shift+Tab");
        await Page.Keyboard.PressAsync("Tab");

        (await VisibilityAsync()).ShouldBe("visible");
    }

    [Theory]
    [InlineData("light", TooltipPlacement.Top)]
    [InlineData("dark", TooltipPlacement.Top)]
    [InlineData("high-contrast", TooltipPlacement.Top)]
    [InlineData("light", TooltipPlacement.Bottom)]
    [InlineData("dark", TooltipPlacement.Bottom)]
    public async Task Has_No_Axe_Violations_While_Visible(string theme, TooltipPlacement placement)
    {
        await LoadHostedAsync(RenderTooltip(placement), theme);
        await Page.FocusAsync("#before");
        await Page.Keyboard.PressAsync("Tab");
        await SettleAsync();

        var result = await RunAxeOnPageAsync();

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(TooltipPlacement.Top)]
    [InlineData(TooltipPlacement.Bottom)]
    public async Task Sits_On_The_Requested_Side_Inside_The_Viewport_Above_Modal_Layers(TooltipPlacement placement)
    {
        await LoadHostedAsync(RenderTooltip(placement), width: 800, height: 500);
        await Page.FocusAsync("#before");
        await Page.Keyboard.PressAsync("Tab");
        await SettleAsync();

        var tip = await MeasureAsync("[role=tooltip]");
        var trigger = await MeasureAsync(".cy-tooltip__trigger");

        tip.Visible.ShouldBeTrue(tip.ToString());
        tip.InViewport.ShouldBeTrue(tip.ToString());
        tip.PageOverflowsHorizontally.ShouldBeFalse(tip.ToString());

        if (placement == TooltipPlacement.Top)
        {
            tip.Bottom.ShouldBeLessThanOrEqualTo(trigger.Top);
        }
        else
        {
            tip.Top.ShouldBeGreaterThanOrEqualTo(trigger.Bottom);
        }

        // Tooltips must not sit behind modals (tokens: tooltip z-index above modal).
        int.Parse(tip.ZIndex!, System.Globalization.CultureInfo.InvariantCulture).ShouldBeGreaterThan(1050);
    }
}
