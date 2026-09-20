using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Tests.Components.Layout;

/// <summary>
/// The mobile drawer (roadmap F6): Escape closes it, focus is trapped while it is open on a small
/// screen and returned afterwards. Works with no <see cref="IFocusManager"/> registered.
/// </summary>
public sealed class CySidebarDrawerFocusTests : TestContextBase
{
    private readonly Mock<IFocusManager> _focusManager = new(MockBehavior.Loose);
    private readonly Mock<IAsyncDisposable> _handle = new();

    private void RegisterFocusManager()
    {
        _handle.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _focusManager
            .Setup(m => m.TrapAsync(It.IsAny<string>(), It.IsAny<FocusTrapOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_handle.Object);
        Services.AddSingleton(_focusManager.Object);
    }

    [Fact]
    public void Escape_Closes_The_Open_Drawer()
    {
        var closed = 0;
        var cut = Render<CySidebar>(p => p
            .Add(s => s.MobileOpen, true)
            .Add(s => s.MobileOpenChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<bool>(this, open => { if (!open) closed++; })));

        cut.Find("aside").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        closed.ShouldBe(1);
    }

    [Fact]
    public void Other_Keys_Do_Not_Close_The_Drawer()
    {
        var closed = 0;
        var cut = Render<CySidebar>(p => p
            .Add(s => s.MobileOpen, true)
            .Add(s => s.MobileOpenChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<bool>(this, _ => closed++)));

        cut.Find("aside").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        closed.ShouldBe(0);
    }

    [Fact]
    public void Escape_Does_Nothing_When_The_Drawer_Is_Closed()
    {
        var closed = 0;
        var cut = Render<CySidebar>(p => p
            .Add(s => s.MobileOpenChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<bool>(this, _ => closed++)));

        cut.Find("aside").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        closed.ShouldBe(0);
    }

    [Fact]
    public void Open_Drawer_Is_A_Programmatic_Focus_Target()
    {
        Render<CySidebar>(p => p.Add(s => s.MobileOpen, true))
            .Find("aside").GetAttribute("tabindex").ShouldBe("-1");

        Render<CySidebar>().Find("aside").HasAttribute("tabindex").ShouldBeFalse();
    }

    [Fact]
    public void Opening_The_Drawer_Traps_Focus_Gated_On_The_Mobile_Media_Query()
    {
        RegisterFocusManager();

        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true).Add(s => s.MobileBreakpoint, "40rem"));

        var id = cut.Find("aside").Id!;

        _focusManager.Verify(m => m.TrapAsync(
            id,
            It.Is<FocusTrapOptions?>(o => o!.AutoFocus && o.RestoreFocus && o.MediaQuery == "(max-width: 40rem)"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void A_Closed_Drawer_Does_Not_Trap_Focus()
    {
        RegisterFocusManager();

        Render<CySidebar>();

        _focusManager.Verify(m => m.TrapAsync(
            It.IsAny<string>(), It.IsAny<FocusTrapOptions?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Closing_The_Drawer_Releases_The_Trap()
    {
        RegisterFocusManager();
        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        cut.Render(p => p.Add(s => s.MobileOpen, false));

        _handle.Verify(h => h.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Disposing_An_Open_Drawer_Releases_The_Trap()
    {
        RegisterFocusManager();
        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        await cut.Instance.DisposeAsync();

        _handle.Verify(h => h.DisposeAsync(), Times.Once);
    }

    [Fact]
    public void Drawer_Works_Without_A_Registered_Focus_Manager()
    {
        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true));

        cut.Find("aside").ClassList.ShouldContain("cy-sidebar--mobile-open");
    }
}
