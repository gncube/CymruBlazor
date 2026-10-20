using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyUserNavAccessibilityTests : AxeTestBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-editing.js";

    public CyUserNavAccessibilityTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule(ModulePath).Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Unauthenticated()
    {
        var result = await ScanComponentAsync<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Unauthenticated_Link()
    {
        var result = await ScanComponentAsync<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, false)
            .Add(c => c.LoginHref, "/login"));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Authenticated_Trigger()
    {
        var result = await ScanComponentAsync<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Email, "jane.doe@wales.nhs.uk")
            .Add(c => c.Status, AvatarStatus.Online));

        result.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Have_No_Violations_When_Menu_Open()
    {
        var result = await ScanComponentAsync<CyUserNav>(p => p
            .Add(c => c.IsAuthenticated, true)
            .Add(c => c.DisplayName, "Jane Doe")
            .Add(c => c.Email, "jane.doe@wales.nhs.uk")
            .Add(c => c.Status, AvatarStatus.Online)
            .Add(c => c.Open, true)
            .Add(c => c.MenuItems, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "a");
                builder.AddAttribute(1, "href", "/account");
                builder.AddAttribute(2, "role", "menuitem");
                builder.AddContent(3, "Account Settings");
                builder.CloseElement();
            }))
            .Add(c => c.OnLogout, () => { }));

        result.Violations.ShouldBeEmpty();
    }
}
