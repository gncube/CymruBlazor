using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// The real <c>cymru-overlay.js</c> in a real browser: focus-trap containment and
/// the native dialog wiring behind <c>CyFocusTrap</c>, <c>CySidebar</c>'s drawer and
/// <c>CyDialog</c>. bUnit cannot test any of this because it has no browser.
/// </summary>
public sealed class OverlayModuleBrowserTests : AxeTestBase
{
    private const string TrapMarkup = """
        <button id="opener">open</button>
        <div id="trap" tabindex="-1">
          <button id="a">a</button>
          <input id="b" aria-label="b" />
          <button id="c" disabled>c</button>
          <a id="d" href="#">d</a>
          <button id="hidden" style="display:none">h</button>
        </div>
        <button id="outside">outside</button>
        """;

    private const string DialogMarkup = """
        <button id="opener">open</button>
        <button id="behind">behind</button>
        <dialog id="dlg" style="padding:0">
          <div style="width:200px;height:100px">
            <button id="close">x</button><input id="field" aria-label="field" /><button id="ok" autofocus>ok</button>
          </div>
        </dialog>
        """;

    private async Task<int> ActivateTrapAsync(string options = "{ autoFocus: true, restoreFocus: true }") =>
        await Page.EvaluateAsync<int>($"() => overlay.activateTrapById('trap', {options})");

    [Fact]
    public async Task Trap_Moves_Focus_In_And_Wraps_Tab_At_Both_Ends_Skipping_Disabled_And_Hidden()
    {
        await LoadHostedAsync(TrapMarkup);
        await Page.FocusAsync("#opener");

        var token = await ActivateTrapAsync();

        token.ShouldBeGreaterThan(0);
        (await ActiveElementIdAsync()).ShouldBe("a");

        await Page.Keyboard.PressAsync("Tab");
        (await ActiveElementIdAsync()).ShouldBe("b");

        await Page.Keyboard.PressAsync("Tab");
        (await ActiveElementIdAsync()).ShouldBe("d");

        await Page.Keyboard.PressAsync("Tab");
        (await ActiveElementIdAsync()).ShouldBe("a");

        await Page.Keyboard.PressAsync("Shift+Tab");
        (await ActiveElementIdAsync()).ShouldBe("d");
    }

    [Fact]
    public async Task Trap_Pulls_Back_Focus_That_Escapes()
    {
        await LoadHostedAsync(TrapMarkup);
        await ActivateTrapAsync();

        await Page.EvaluateAsync("() => document.getElementById('outside').focus()");

        (await ActiveElementIdAsync()).ShouldBe("a");
    }

    [Fact]
    public async Task Releasing_The_Trap_Restores_Focus_To_The_Element_That_Opened_It()
    {
        await LoadHostedAsync(TrapMarkup);
        await Page.FocusAsync("#opener");
        var token = await ActivateTrapAsync();

        await Page.EvaluateAsync("(t) => overlay.releaseTrap(t)", token);

        (await ActiveElementIdAsync()).ShouldBe("opener");
    }

    [Fact]
    public async Task Releasing_Without_RestoreFocus_Leaves_Focus_Alone()
    {
        await LoadHostedAsync(TrapMarkup);
        await Page.FocusAsync("#opener");
        var token = await ActivateTrapAsync("{ autoFocus: true, restoreFocus: false }");

        await Page.EvaluateAsync("(t) => overlay.releaseTrap(t)", token);

        (await ActiveElementIdAsync()).ShouldBe("a");
    }

    [Fact]
    public async Task Nested_Traps_Only_The_Innermost_Acts_And_Release_Returns_To_The_Outer()
    {
        await LoadHostedAsync(TrapMarkup);
        await ActivateTrapAsync();
        var inner = await Page.EvaluateAsync<int>(
            """
            () => {
              document.body.insertAdjacentHTML('beforeend', '<div id="inner" tabindex="-1"><button id="i1">1</button><button id="i2">2</button></div>');
              return overlay.activateTrapById('inner', { autoFocus: true, restoreFocus: true });
            }
            """);

        (await ActiveElementIdAsync()).ShouldBe("i1");
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");
        (await ActiveElementIdAsync()).ShouldBe("i1");

        await Page.EvaluateAsync("(t) => overlay.releaseTrap(t)", inner);

        (await ActiveElementIdAsync()).ShouldBe("a");
    }

    [Fact]
    public async Task Trap_Is_Not_Activated_When_Its_Media_Query_Does_Not_Match()
    {
        await LoadHostedAsync(TrapMarkup, width: 1280);

        var token = await ActivateTrapAsync("{ mediaQuery: '(max-width: 47.99rem)' }");

        token.ShouldBe(0);
    }

    [Fact]
    public async Task Trap_Is_Activated_When_Its_Media_Query_Matches()
    {
        await LoadHostedAsync(TrapMarkup, width: 400);

        var token = await ActivateTrapAsync("{ mediaQuery: '(max-width: 47.99rem)' }");

        token.ShouldBeGreaterThan(0);
    }

    private async Task<int> ShowDialogAsync(string options = "{ dismissible: true }") =>
        await Page.EvaluateAsync<int>(
            $$"""
            () => {
              window.closedCount = 0;
              const ref = { invokeMethodAsync: () => { window.closedCount++; return Promise.resolve(); } };
              return overlay.showDialog(document.getElementById('dlg'), ref, {{options}});
            }
            """);

    private Task<bool> DialogIsOpenAsync() => Page.EvaluateAsync<bool>("() => document.getElementById('dlg').open");

    [Fact]
    public async Task Dialog_Opens_Modally_Focuses_Autofocus_And_Keeps_Tab_Inside()
    {
        await LoadHostedAsync(DialogMarkup);
        await Page.FocusAsync("#opener");

        await ShowDialogAsync();

        (await DialogIsOpenAsync()).ShouldBeTrue();
        (await ActiveElementIdAsync()).ShouldBe("ok");

        for (var i = 0; i < 5; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
            (await ActiveElementIdAsync()).ShouldNotBeOneOf("behind", "opener");
        }
    }

    [Fact]
    public async Task Escape_Closes_The_Dialog_Notifies_DotNet_Once_And_Restores_Focus()
    {
        await LoadHostedAsync(DialogMarkup);
        await Page.FocusAsync("#opener");
        await ShowDialogAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForFunctionAsync("() => !document.getElementById('dlg').open");

        (await Page.EvaluateAsync<int>("() => window.closedCount")).ShouldBe(1);
        (await ActiveElementIdAsync()).ShouldBe("opener");
    }

    [Fact]
    public async Task A_Non_Dismissible_Dialog_Ignores_Escape_Until_Updated()
    {
        await LoadHostedAsync(DialogMarkup);
        var token = await ShowDialogAsync("{ dismissible: false }");

        await Page.Keyboard.PressAsync("Escape");
        (await DialogIsOpenAsync()).ShouldBeTrue();

        await Page.EvaluateAsync("(t) => overlay.updateDialog(t, { dismissible: true })", token);
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForFunctionAsync("() => !document.getElementById('dlg').open");
    }

    [Fact]
    public async Task Backdrop_Click_Closes_Only_When_Enabled()
    {
        await LoadHostedAsync(DialogMarkup);
        var token = await ShowDialogAsync("{ dismissible: true, closeOnBackdropClick: false }");

        await Page.Mouse.ClickAsync(5, 5);
        (await DialogIsOpenAsync()).ShouldBeTrue();

        await Page.EvaluateAsync("(t) => overlay.disposeDialog(t)", token);
        await ShowDialogAsync("{ dismissible: true, closeOnBackdropClick: true }");

        await Page.Mouse.ClickAsync(5, 5);
        await Page.WaitForFunctionAsync("() => !document.getElementById('dlg').open");
    }

    [Fact]
    public async Task Disposing_The_Dialog_Closes_It_Restores_Focus_And_Is_Safe_To_Repeat()
    {
        await LoadHostedAsync(DialogMarkup);
        await Page.FocusAsync("#opener");
        var token = await ShowDialogAsync();

        await Page.EvaluateAsync("(t) => { overlay.disposeDialog(t); overlay.disposeDialog(t); }", token);

        (await DialogIsOpenAsync()).ShouldBeFalse();
        (await ActiveElementIdAsync()).ShouldBe("opener");
    }
}
