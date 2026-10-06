using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Feedback;

namespace CymruBlazor.Tests.Feedback;

public sealed class CyUnsavedChangesTests : TestContextBase
{
    public CyUnsavedChangesTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-overlay.js");
        module.Mode = JSRuntimeMode.Loose;
        module.Setup<int>("showDialog", _ => true).SetResult(7);
    }

    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    private void AddConfirmService()
    {
        Services.AddScoped<CyConfirmService>();
        Services.AddScoped<ICyConfirmService>(sp => sp.GetRequiredService<CyConfirmService>());
    }

    [Fact]
    public void Should_Render_An_Always_Present_Status_Region()
    {
        var cut = Render<CyUnsavedChanges>();

        var region = cut.Find("[role='status']");
        region.TextContent.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void Should_Not_Render_The_Region_When_The_Indicator_Is_Hidden()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.ShowIndicator, false));

        cut.FindAll("[role='status']").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Say_Unsaved_Changes_When_Dirty()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));

        cut.Find("[role='status']").TextContent.Trim().ShouldBe("Unsaved changes");
        cut.Find("[role='status']").ClassList.ShouldContain("cy-unsaved--dirty");
    }

    [Fact]
    public void Should_Say_Saving_In_Preference_To_Unsaved_Changes()
    {
        var cut = Render<CyUnsavedChanges>(p => p
            .Add(c => c.Dirty, true)
            .Add(c => c.SaveState, CySaveState.Saving));

        cut.Find("[role='status']").TextContent.Trim().ShouldBe("Saving...");
        cut.Find("[role='status']").ClassList.ShouldContain("cy-unsaved--saving");
    }

    [Fact]
    public void Should_Say_Saved_When_Clean_After_A_Save()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.SaveState, CySaveState.Saved));

        cut.Find("[role='status']").TextContent.Trim().ShouldBe("Saved");
    }

    [Fact]
    public void Should_Include_The_Time_When_SavedAt_Is_Known()
    {
        var at = new DateTimeOffset(2026, 10, 5, 14, 32, 0, TimeSpan.Zero);
        var cut = Render<CyUnsavedChanges>(p => p
            .Add(c => c.SaveState, CySaveState.Saved)
            .Add(c => c.SavedAt, at));

        var expected = string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            "Saved at {0}",
            at.ToString("t", System.Globalization.CultureInfo.CurrentCulture));
        cut.Find("[role='status']").TextContent.Trim().ShouldBe(expected);
    }

    [Fact]
    public void Should_Say_Could_Not_Save_On_Error()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.SaveState, CySaveState.Error));

        cut.Find("[role='status']").TextContent.Trim().ShouldBe("Could not save");
        cut.Find("[role='status']").ClassList.ShouldContain("cy-unsaved--error");
    }

    [Fact]
    public void Should_Use_Localised_Text()
    {
        var cut = Render<CyUnsavedChanges>(p => p
            .Add(c => c.Dirty, true)
            .Add(c => c.Text, new CyUnsavedChangesText { Unsaved = "Newidiadau heb eu cadw" }));

        cut.Find("[role='status']").TextContent.Trim().ShouldBe("Newidiadau heb eu cadw");
    }

    [Fact]
    public void Should_Hide_The_Indicator_Icon_From_Assistive_Technology()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));

        cut.Find(".cy-unsaved__icon").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Should_Allow_Navigation_When_Clean()
    {
        _ = Render<CyUnsavedChanges>();

        Navigation.NavigateTo("/elsewhere");

        Navigation.Uri.ShouldEndWith("/elsewhere");
    }

    [Fact]
    public void Should_Block_Navigation_And_Report_The_Target_When_Dirty_Without_A_Dialog_Host()
    {
        string? blocked = null;
        _ = Render<CyUnsavedChanges>(p => p
            .Add(c => c.Dirty, true)
            .Add(c => c.OnNavigationBlocked, target => blocked = target));
        var start = Navigation.Uri;

        Navigation.NavigateTo("/elsewhere");

        Navigation.Uri.ShouldBe(start);
        blocked.ShouldNotBeNull();
        blocked.ShouldEndWith("/elsewhere");
    }

    [Fact]
    public void Should_Allow_A_Fragment_Only_Change_When_Dirty()
    {
        _ = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));

        Navigation.NavigateTo(Navigation.Uri + "#section");

        Navigation.Uri.ShouldEndWith("#section");
    }

    [Fact]
    public void Should_Ask_For_Confirmation_And_Stay_When_The_User_Cancels()
    {
        AddConfirmService();
        var host = Render<CyConfirmDialog>();
        _ = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));
        var start = Navigation.Uri;

        Navigation.NavigateTo("/elsewhere");

        host.WaitForAssertion(() => host.Find(".cy-dialog__title").TextContent.ShouldBe("Leave this page?"));
        host.FindAll("button").First(b => b.TextContent.Trim() == "Stay on page").Click();
        host.WaitForAssertion(() => host.FindAll(".cy-dialog__title").Count.ShouldBe(0));
        Navigation.Uri.ShouldBe(start);
    }

    [Fact]
    public void Should_Navigate_When_The_User_Confirms_Leaving()
    {
        AddConfirmService();
        var host = Render<CyConfirmDialog>();
        _ = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));

        Navigation.NavigateTo("/elsewhere");

        host.WaitForAssertion(() => host.Find(".cy-dialog__title"));
        host.FindAll("button").First(b => b.TextContent.Trim() == "Leave page").Click();
        host.WaitForAssertion(() => Navigation.Uri.ShouldEndWith("/elsewhere"));
    }

    [Fact]
    public void Should_Stop_Guarding_After_It_Is_Disposed()
    {
        var cut = Render<CyUnsavedChanges>(p => p.Add(c => c.Dirty, true));

        cut.Dispose();
        Navigation.NavigateTo("/elsewhere");

        Navigation.Uri.ShouldEndWith("/elsewhere");
    }
}
