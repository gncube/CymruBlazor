using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Button;
using CymruBlazor.Diagnostics;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Button;

public sealed class CyButtonIconTests : TestContextBase
{
    [Fact]
    public void Icon_Defaults_To_Start_And_Is_Decorative()
    {
        var cut = Render<CyButton>(p => p
            .Add(x => x.Icon, "save")
            .AddChildContent("Save"));

        var html = cut.Markup;
        html.IndexOf("<svg", StringComparison.Ordinal).ShouldBeLessThan(html.IndexOf("Save", StringComparison.Ordinal));
        cut.Find("svg").GetAttribute("aria-hidden").ShouldBe("true");
        cut.Find("svg").ClassList.ShouldContain("cy-button__icon--start");
    }

    [Fact]
    public void Icon_Can_Be_Placed_At_End()
    {
        var cut = Render<CyButton>(p => p
            .Add(x => x.Icon, "arrow-right")
            .Add(x => x.IconPlacement, ButtonIconPlacement.End)
            .AddChildContent("Next"));

        var html = cut.Markup;
        html.IndexOf("Next", StringComparison.Ordinal).ShouldBeLessThan(html.IndexOf("<svg", StringComparison.Ordinal));
        cut.Find("svg").ClassList.ShouldContain("cy-button__icon--end");
    }

    [Fact]
    public void IconOnly_With_AriaLabel_Has_An_Accessible_Name()
    {
        var cut = Render<CyButton>(p => p
            .Add(x => x.Icon, "close")
            .Add(x => x.IconOnly, true)
            .Add(x => x.AriaLabel, "Close panel"));

        var button = cut.Find("button");
        button.GetAttribute("aria-label").ShouldBe("Close panel");
        button.ClassList.ShouldContain("cy-button--icon-only");
        cut.FindAll(".cy-button__content").ShouldBeEmpty();
    }

    [Fact]
    public void IconOnly_Keeps_ChildContent_For_Assistive_Technology()
    {
        var cut = Render<CyButton>(p => p
            .Add(x => x.Icon, "close")
            .Add(x => x.IconOnly, true)
            .AddChildContent("Close"));

        cut.Find(".u-sr-only").TextContent.ShouldBe("Close");
    }

    [Fact]
    public void IconOnly_Without_Accessible_Name_Throws_When_Strict()
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyButton>(p => p.Add(x => x.Icon, "close").Add(x => x.IconOnly, true)));
    }

    [Fact]
    public void IconOnly_Without_Icon_Throws_When_Strict()
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyButton>(p => p.Add(x => x.IconOnly, true).Add(x => x.AriaLabel, "x")));
    }

    [Fact]
    public void IconOnly_Without_Accessible_Name_Only_Warns_When_Lenient()
    {
        Services.AddSingleton<ICyDiagnostics>(new CyDiagnostics(
            new CymruBlazorOptions { Diagnostics = CyDiagnosticsMode.Lenient }));

        var cut = Render<CyButton>(p => p.Add(x => x.Icon, "close").Add(x => x.IconOnly, true));

        cut.Find("button").ShouldNotBeNull();
    }

    [Fact]
    public void Default_Markup_Is_Unchanged_When_No_Icon_Is_Set()
    {
        var cut = Render<CyButton>(p => p.AddChildContent("Go"));

        cut.FindAll("svg").ShouldBeEmpty();
        cut.Find(".cy-button__content").TextContent.ShouldBe("Go");
    }

    [Fact]
    public async Task OnClick_Is_Not_Raised_While_Loading()
    {
        // Regression lock: "@onclick" on <CyButton> binds to the OnClick parameter
        // (parameters bind case-insensitively), so it is behind the guard.
        var calls = 0;
        var cut = Render<CyButton>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.OnClick, _ => calls++)
            .AddChildContent("Go"));

        await cut.Find("button").ClickAsync(new MouseEventArgs());

        calls.ShouldBe(0);
    }

    [Fact]
    public async Task AutoLoading_Ignores_A_Second_Click_While_The_Handler_Runs()
    {
        var calls = 0;
        var gate = new TaskCompletionSource();
        var cut = Render<CyButton>(p => p
            .Add(x => x.AutoLoading, true)
            .Add(x => x.OnClick, async _ => { calls++; await gate.Task; })
            .AddChildContent("Publish"));
        var button = cut.Find("button");

        var first = button.ClickAsync(new MouseEventArgs());
        var second = button.ClickAsync(new MouseEventArgs());
        gate.SetResult();
        await Task.WhenAll(first, second);

        calls.ShouldBe(1);
    }

    [Fact]
    public async Task Without_AutoLoading_An_Async_Handler_Can_Be_Re_Entered()
    {
        // Documents the existing (unchanged) default behaviour.
        var calls = 0;
        var gate = new TaskCompletionSource();
        var cut = Render<CyButton>(p => p
            .Add(x => x.OnClick, async _ => { calls++; await gate.Task; })
            .AddChildContent("Publish"));
        var button = cut.Find("button");

        var first = button.ClickAsync(new MouseEventArgs());
        var second = button.ClickAsync(new MouseEventArgs());
        gate.SetResult();
        await Task.WhenAll(first, second);

        calls.ShouldBe(2);
    }
}
