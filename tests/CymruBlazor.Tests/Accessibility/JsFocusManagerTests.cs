using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;

namespace CymruBlazor.Tests.Accessibility;

/// <summary>
/// <see cref="JsFocusManager"/> against bUnit's JS interop: it must call the
/// overlay module with the right arguments and never throw for teardown or a
/// missing element.
/// </summary>
public sealed class JsFocusManagerTests : BunitContext
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _module;
    private readonly JsFocusManager _manager;

    public JsFocusManagerTests()
    {
        _module = JSInterop.SetupModule(ModulePath);
        _manager = new JsFocusManager(Services.GetRequiredService<IJSRuntime>(), NullLogger<JsFocusManager>.Instance);
    }

    [Fact]
    public async Task FocusAsync_By_Id_Calls_The_Module_And_Reports_Success()
    {
        _module.Setup<bool>("focusElement", "panel", true, true).SetResult(true);

        var result = await _manager.FocusAsync("panel", new FocusOptions(PreventScroll: true, RestorePreviousFocus: true));

        result.Success.ShouldBeTrue();
        _module.VerifyInvoke("focusElement");
    }

    [Fact]
    public async Task FocusAsync_By_Id_Reports_Failure_When_The_Element_Cannot_Be_Focused()
    {
        _module.Setup<bool>("focusElement", _ => true).SetResult(false);

        var result = await _manager.FocusAsync("missing");

        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task FocusAsync_By_Target_Sends_The_Target_Name()
    {
        _module.Setup<bool>("focusTarget", "Last", true).SetResult(true);

        var result = await _manager.FocusAsync(FocusTarget.Last);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task RestoreFocusAsync_Calls_The_Module()
    {
        _module.Setup<bool>("restoreFocus", _ => true).SetResult(true);

        var result = await _manager.RestoreFocusAsync();

        result.Success.ShouldBeTrue();
        _module.VerifyInvoke("restoreFocus");
    }

    [Fact]
    public async Task FocusAsync_Rejects_A_Blank_Id()
    {
        await Should.ThrowAsync<ArgumentException>(() => _manager.FocusAsync(" "));
    }

    [Fact]
    public async Task TrapAsync_Activates_And_Disposing_The_Handle_Releases_The_Trap_Once()
    {
        _module.Setup<int>("activateTrapById", _ => true).SetResult(42);
        _module.SetupVoid("releaseTrap", _ => true).SetVoidResult();

        var handle = await _manager.TrapAsync("panel", new FocusTrapOptions(AutoFocus: true, RestoreFocus: false));

        _module.VerifyInvoke("activateTrapById");

        await handle.DisposeAsync();
        await handle.DisposeAsync();

        _module.VerifyInvoke("releaseTrap", calledTimes: 1);
        _module.Invocations["releaseTrap"].Single().Arguments[0].ShouldBe(42);
    }

    [Fact]
    public async Task TrapAsync_Sends_The_Options_To_The_Module()
    {
        _module.Setup<int>("activateTrapById", _ => true).SetResult(1);

        await _manager.TrapAsync("panel", new FocusTrapOptions(AutoFocus: false, RestoreFocus: true, PreventScroll: false, MediaQuery: "(max-width: 10rem)"));

        var arguments = _module.Invocations["activateTrapById"].Single().Arguments;
        arguments[0].ShouldBe("panel");
        var json = System.Text.Json.JsonSerializer.Serialize(arguments[1]);
        json.ShouldContain("\"autoFocus\":false");
        json.ShouldContain("\"restoreFocus\":true");
        json.ShouldContain("\"preventScroll\":false");
        json.ShouldContain("(max-width: 10rem)");
    }

    [Fact]
    public async Task TrapAsync_Returns_A_Harmless_Handle_When_The_Element_Does_Not_Exist()
    {
        _module.Setup<int>("activateTrapById", _ => true).SetResult(0);

        var handle = await _manager.TrapAsync("missing");

        await Should.NotThrowAsync(async () => await handle.DisposeAsync());
        _module.VerifyNotInvoke("releaseTrap");
    }

    [Fact]
    public async Task Interop_Failure_Is_Reported_As_A_Failed_Result_Not_An_Exception()
    {
        _module.Setup<bool>("focusElement", _ => true).SetException(new JSException("boom"));

        var result = await _manager.FocusAsync("panel");

        result.Success.ShouldBeFalse();
    }
}
