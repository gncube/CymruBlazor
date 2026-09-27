using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

/// <summary>
/// The mobile drawer makes the rest of the page <c>inert</c> while open (roadmap F4), independently
/// of whichever <see cref="Accessibility.Focus.IFocusManager"/> is registered (see
/// <see cref="CySidebarDrawerFocusTests"/> for that, separate, concern): it calls the overlay
/// module's <c>makeBackgroundInertById</c>/<c>releaseBackgroundInert</c> directly, the same way
/// <see cref="Accessibility.CyDialog"/> gets inertness for free from the native
/// <c>&lt;dialog&gt;</c> element's top layer.
/// </summary>
public sealed class CySidebarBackgroundInertTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _module;

    public CySidebarBackgroundInertTests()
    {
        _module = JSInterop.SetupModule(ModulePath);
    }

    [Fact]
    public void Opening_The_Drawer_Makes_The_Background_Inert_Gated_On_The_Mobile_Media_Query()
    {
        _module.Setup<int>("makeBackgroundInertById", _ => true).SetResult(7);

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true).Add(s => s.MobileBreakpoint, "40rem"));

        var id = cut.Find("aside").Id!;
        var arguments = _module.Invocations["makeBackgroundInertById"].Single().Arguments;
        arguments[0].ShouldBe(id);

        var json = System.Text.Json.JsonSerializer.Serialize(arguments[1]);
        json.ShouldContain("(max-width: 40rem)");
    }

    [Fact]
    public void A_Closed_Drawer_Does_Not_Touch_The_Overlay_Module()
    {
        Render<CySidebar>();

        _module.VerifyNotInvoke("makeBackgroundInertById");
    }

    [Fact]
    public void Closing_The_Drawer_Releases_The_Inert_Token()
    {
        _module.Setup<int>("makeBackgroundInertById", _ => true).SetResult(7);
        _module.SetupVoid("releaseBackgroundInert", _ => true).SetVoidResult();

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        cut.Render(p => p.Add(s => s.MobileOpen, false));

        _module.VerifyInvoke("releaseBackgroundInert", calledTimes: 1);
        _module.Invocations["releaseBackgroundInert"].Single().Arguments[0].ShouldBe(7);
    }

    [Fact]
    public async Task Disposing_An_Open_Drawer_Releases_The_Inert_Token()
    {
        _module.Setup<int>("makeBackgroundInertById", _ => true).SetResult(7);
        _module.SetupVoid("releaseBackgroundInert", _ => true).SetVoidResult();

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        await cut.Instance.DisposeAsync();

        _module.VerifyInvoke("releaseBackgroundInert", calledTimes: 1);
    }

    [Fact]
    public void Does_Not_Call_Release_When_The_Media_Query_Did_Not_Match_And_No_Token_Was_Issued()
    {
        // makeBackgroundInertById returns 0 when the media query gate did not match, exactly like
        // activateTrapById - nothing was made inert, so there is nothing to release.
        _module.Setup<int>("makeBackgroundInertById", _ => true).SetResult(0);

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        cut.Render(p => p.Add(s => s.MobileOpen, false));

        _module.VerifyNotInvoke("releaseBackgroundInert");
    }

    [Fact]
    public void Works_Without_A_Registered_IFocusManager()
    {
        _module.Setup<int>("makeBackgroundInertById", _ => true).SetResult(7);

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        cut.Find("aside").ClassList.ShouldContain("cy-sidebar--mobile-open");
        _module.VerifyInvoke("makeBackgroundInertById");
    }
}
