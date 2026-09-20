using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Tests.Accessibility;

/// <summary>
/// Unit tests for <see cref="CyFocusTrap"/>: it must activate a trap through
/// <see cref="IFocusManager.TrapAsync"/> and release it again. The behaviour
/// of the trap itself lives in <c>cymru-overlay.js</c> and is covered by the
/// browser tests.
/// </summary>
public sealed class FocusTrapTests : BunitContext
{
    private readonly Mock<IFocusManager> _focusManagerMock = new(MockBehavior.Strict);
    private readonly Mock<IAsyncDisposable> _handleMock = new();

    public FocusTrapTests()
    {
        Services.AddSingleton<IComponentIdGenerator, ComponentIdGenerator>();

        _handleMock.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _focusManagerMock
            .Setup(m => m.TrapAsync(
                It.IsAny<string>(),
                It.IsAny<FocusTrapOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_handleMock.Object);

        Services.AddSingleton(_focusManagerMock.Object);
    }

    [Fact]
    public void Should_Render_Child_Content()
    {
        var cut = Render<CyFocusTrap>(p => p.AddChildContent("Hello"));

        cut.Find("div").TextContent.ShouldBe("Hello");
    }

    [Fact]
    public void Should_Render_Css_Class()
    {
        var cut = Render<CyFocusTrap>();

        cut.Find("div").ClassList.ShouldContain("cy-focus-trap");
    }

    [Fact]
    public void Should_Render_TabIndex()
    {
        var cut = Render<CyFocusTrap>();

        cut.Find("div").GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public void Should_Activate_A_Trap_On_Its_Own_Element_After_First_Render()
    {
        var cut = Render<CyFocusTrap>();

        var id = cut.Find("div").Id!;
        id.ShouldNotBeNullOrWhiteSpace();
        _focusManagerMock.Verify(m => m.TrapAsync(
            id,
            It.Is<FocusTrapOptions?>(o => o!.AutoFocus && o.RestoreFocus && o.PreventScroll),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Should_Pass_AutoFocus_And_RestoreFocus_Through()
    {
        Render<CyFocusTrap>(p => p.Add(t => t.AutoFocus, false).Add(t => t.RestoreFocus, false));

        _focusManagerMock.Verify(m => m.TrapAsync(
            It.IsAny<string>(),
            It.Is<FocusTrapOptions?>(o => !o!.AutoFocus && !o.RestoreFocus),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Should_Not_Activate_A_Trap_When_Disabled()
    {
        Render<CyFocusTrap>(p => p.Add(t => t.Enabled, false));

        _focusManagerMock.Verify(m => m.TrapAsync(
            It.IsAny<string>(), It.IsAny<FocusTrapOptions?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Should_Release_The_Trap_When_Disabled_And_Reactivate_When_Enabled_Again()
    {
        var cut = Render<CyFocusTrap>();

        cut.Render(p => p.Add(t => t.Enabled, false));
        _handleMock.Verify(h => h.DisposeAsync(), Times.Once);

        cut.Render(p => p.Add(t => t.Enabled, true));
        _focusManagerMock.Verify(m => m.TrapAsync(
            It.IsAny<string>(), It.IsAny<FocusTrapOptions?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_Release_The_Trap_When_Disposed()
    {
        var cut = Render<CyFocusTrap>();

        await cut.Instance.DisposeAsync();

        _handleMock.Verify(h => h.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Release_Twice_When_Disposed_After_Being_Disabled()
    {
        var cut = Render<CyFocusTrap>();
        cut.Render(p => p.Add(t => t.Enabled, false));

        await cut.Instance.DisposeAsync();

        _handleMock.Verify(h => h.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task A_Manager_That_Only_Implements_The_Original_Methods_Still_Gets_Focus_And_Restore_Calls()
    {
        // 1.2.x custom managers implement FocusAsync/RestoreFocusAsync only. The default
        // TrapAsync must keep calling them so those consumers are not regressed.
        var legacy = new LegacyFocusManager();
        var trapHandle = await ((IFocusManager)legacy).TrapAsync("panel", new FocusTrapOptions(AutoFocus: true, RestoreFocus: true));

        legacy.FocusedIds.ShouldBe(["panel"]);
        legacy.Restores.ShouldBe(0);

        await trapHandle.DisposeAsync();

        legacy.Restores.ShouldBe(1);
    }

    [Fact]
    public async Task Default_TrapAsync_Skips_Focus_And_Restore_When_Options_Disable_Them()
    {
        var legacy = new LegacyFocusManager();
        var trapHandle = await ((IFocusManager)legacy).TrapAsync("panel", new FocusTrapOptions(AutoFocus: false, RestoreFocus: false));

        await trapHandle.DisposeAsync();

        legacy.FocusedIds.ShouldBeEmpty();
        legacy.Restores.ShouldBe(0);
    }

    private sealed class LegacyFocusManager : IFocusManager
    {
        public List<string> FocusedIds { get; } = [];

        public int Restores { get; private set; }

        public Task<FocusResult> FocusAsync(string elementId, FocusOptions? options = null, CancellationToken cancellationToken = default)
        {
            FocusedIds.Add(elementId);
            return Task.FromResult(new FocusResult(true));
        }

        public Task<FocusResult> FocusAsync(FocusTarget target, FocusOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new FocusResult(true));

        public Task<FocusResult> RestoreFocusAsync(CancellationToken cancellationToken = default)
        {
            Restores++;
            return Task.FromResult(new FocusResult(true));
        }
    }
}
