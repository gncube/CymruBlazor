using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// An open <c>CyDialog</c> in a real browser: axe in all three themes, and real-layout
/// assertions (inside the viewport, no horizontal overflow, page scroll locked) at desktop and
/// phone widths.
/// </summary>
public sealed class CyDialogAccessibilityTests : AxeTestBase
{
    private string RenderOpenDialog()
    {
        var cut = Render<CyDialog>(p => p
            .Add(d => d.Open, true)
            .Add(d => d.Title, "Appointment details")
            .Add(d => d.Description, "Review the details below before you continue.")
            .Add(d => d.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0,
                "<p>Body text.</p><label>Name <input id=\"name\" autofocus /></label>")))
            .Add(d => d.Footer, (RenderFragment)(b => b.AddMarkupContent(0,
                "<button type=\"button\">Cancel</button><button type=\"button\">Confirm</button>"))));

        return cut.Markup;
    }

    private async Task OpenAsync() =>
        await Page.EvaluateAsync("() => document.querySelector('dialog').showModal()");

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Open_Dialog_Has_No_Axe_Violations(string theme)
    {
        await LoadHostedAsync(RenderOpenDialog(), theme);
        await OpenAsync();

        var result = await RunAxeOnPageAsync();

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1280, 800)]
    [InlineData(768, 1024)]
    [InlineData(320, 568)]
    public async Task Open_Dialog_Fits_The_Viewport_Without_Horizontal_Overflow(int width, int height)
    {
        await LoadHostedAsync(RenderOpenDialog(), width: width, height: height);
        await OpenAsync();

        var dialog = await MeasureAsync("dialog");

        dialog.Found.ShouldBeTrue();
        dialog.Visible.ShouldBeTrue(dialog.ToString());
        dialog.InViewport.ShouldBeTrue(dialog.ToString());
        dialog.Width.ShouldBeLessThanOrEqualTo(dialog.ViewportWidth);
        dialog.PageOverflowsHorizontally.ShouldBeFalse(dialog.ToString());
    }

    [Fact]
    public async Task Open_Dialog_Locks_Page_Scrolling_And_Closed_Dialog_Does_Not()
    {
        await LoadHostedAsync(RenderOpenDialog());

        (await Page.EvaluateAsync<string>("() => getComputedStyle(document.documentElement).overflow")).ShouldNotBe("hidden");

        await OpenAsync();

        (await Page.EvaluateAsync<string>("() => getComputedStyle(document.documentElement).overflow")).ShouldBe("hidden");

        await Page.EvaluateAsync("() => document.querySelector('dialog').close()");

        (await Page.EvaluateAsync<string>("() => getComputedStyle(document.documentElement).overflow")).ShouldNotBe("hidden");
    }

    [Fact]
    public async Task Close_Button_Meets_The_Minimum_Target_Size()
    {
        await LoadHostedAsync(RenderOpenDialog());
        await OpenAsync();

        var close = await MeasureAsync(".cy-dialog__close");

        close.Width.ShouldBeGreaterThanOrEqualTo(44);
        close.Height.ShouldBeGreaterThanOrEqualTo(44);
    }

    [Fact]
    public async Task Dialog_Element_Is_Hidden_Until_ShowModal_Is_Called()
    {
        await LoadHostedAsync(RenderOpenDialog());

        (await MeasureAsync("dialog")).Visible.ShouldBeFalse();
    }

    [Fact]
    public async Task Dialog_Is_Named_By_Its_Heading_In_The_Accessibility_Tree()
    {
        await LoadHostedAsync(RenderOpenDialog());
        await OpenAsync();

        var name = await Page.EvaluateAsync<string>(
            "() => document.getElementById(document.querySelector('dialog').getAttribute('aria-labelledby')).textContent");

        name.ShouldBe("Appointment details");
    }
}
