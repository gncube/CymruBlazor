using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;

namespace CymruBlazor.Tests.Components.Accessibility;

public sealed class CyConfirmTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _module;

    public CyConfirmTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
        _module.Mode = JSRuntimeMode.Loose;
        _module.Setup<int>("showDialog", _ => true).SetResult(7);

        Services.AddScoped<CyConfirmService>();
        Services.AddScoped<ICyConfirmService>(sp => sp.GetRequiredService<CyConfirmService>());
    }

    private ICyConfirmService Service => Services.GetRequiredService<ICyConfirmService>();

    private IRenderedComponent<CyConfirmDialog> RenderHost() => Render<CyConfirmDialog>();

    private static void ClickButton(IRenderedComponent<CyConfirmDialog> host, string text) =>
        host.FindAll("button").First(b => b.TextContent.Trim() == text).Click();

    private static async Task<bool> WithTimeout(Task<bool> task) =>
        await task.WaitAsync(TimeSpan.FromSeconds(5));

    private static List<string> ButtonTexts(IRenderedComponent<CyConfirmDialog> host) =>
        host.FindAll("button").Select(b => b.TextContent.Trim()).ToList();

    [Fact]
    public void Should_Throw_When_No_Host_Is_On_The_Page()
    {
        var ex = Should.Throw<InvalidOperationException>(() => Service.ConfirmAsync("Publish?"));

        ex.Message.ShouldContain(nameof(CyConfirmDialog));
    }

    [Fact]
    public void Should_Render_Nothing_While_Nothing_Is_Pending()
    {
        var host = RenderHost();

        host.FindAll("dialog").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Show_A_Dialog_Named_By_The_Title_And_Described_By_The_Message()
    {
        var host = RenderHost();

        _ = Service.ConfirmAsync("Publish questionnaire?", "It becomes visible to everyone.");

        host.WaitForAssertion(() =>
        {
            host.Find(".cy-dialog__title").TextContent.ShouldBe("Publish questionnaire?");
            host.Find(".cy-dialog__description").TextContent.ShouldBe("It becomes visible to everyone.");
        });
    }

    [Fact]
    public async Task Should_Answer_True_When_Confirm_Is_Pressed()
    {
        var host = RenderHost();
        var answer = Service.ConfirmAsync("Publish?");
        host.WaitForAssertion(() => _module.VerifyInvoke("showDialog"));

        ClickButton(host, "Confirm");

        (await WithTimeout(answer)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Answer_False_When_Cancel_Is_Pressed()
    {
        var host = RenderHost();
        var answer = Service.ConfirmAsync("Publish?");
        host.WaitForAssertion(() => _module.VerifyInvoke("showDialog"));

        ClickButton(host, "Cancel");

        (await WithTimeout(answer)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Answer_False_When_The_Close_Button_Is_Pressed()
    {
        var host = RenderHost();
        var answer = Service.ConfirmAsync("Publish?");
        host.WaitForAssertion(() => _module.VerifyInvoke("showDialog"));

        host.Find(".cy-dialog__close").Click();

        (await WithTimeout(answer)).ShouldBeFalse();
    }

    [Fact]
    public void Should_Use_Custom_Button_Text()
    {
        var host = RenderHost();

        _ = Service.ConfirmAsync("Discard?", confirmText: "Discard draft", cancelText: "Keep editing");

        host.WaitForAssertion(() =>
        {
            ButtonTexts(host).ShouldContain("Discard draft");
            ButtonTexts(host).ShouldContain("Keep editing");
        });
    }

    [Fact]
    public void Should_Use_The_Hosts_Localised_Default_Text()
    {
        var host = Render<CyConfirmDialog>(p => p.Add(c => c.Text, new CyConfirmText { Confirm = "Cadarnhau", Cancel = "Canslo" }));

        _ = Service.ConfirmAsync("Cyhoeddi?");

        host.WaitForAssertion(() =>
        {
            ButtonTexts(host).ShouldContain("Cadarnhau");
            ButtonTexts(host).ShouldContain("Canslo");
        });
    }

    [Fact]
    public void Should_Focus_Cancel_And_Use_The_Danger_Style_For_A_Destructive_Request()
    {
        var host = RenderHost();

        _ = Service.ConfirmAsync("Remove question?", destructive: true, confirmText: "Remove question");

        host.WaitForAssertion(() =>
        {
            var cancel = host.FindAll("button").First(b => b.TextContent.Trim() == "Cancel");
            var confirm = host.FindAll("button").First(b => b.TextContent.Trim() == "Remove question");
            cancel.HasAttribute("autofocus").ShouldBeTrue();
            confirm.HasAttribute("autofocus").ShouldBeFalse();
            confirm.ClassList.ShouldContain("cy-button--danger");
        });
    }

    [Fact]
    public void Should_Focus_Confirm_For_A_Non_Destructive_Request()
    {
        var host = RenderHost();

        _ = Service.ConfirmAsync("Publish?");

        host.WaitForAssertion(() =>
        {
            host.FindAll("button").First(b => b.TextContent.Trim() == "Confirm").HasAttribute("autofocus").ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Should_Queue_A_Second_Request_Until_The_First_Is_Answered()
    {
        var host = RenderHost();
        var first = Service.ConfirmAsync("First?");
        var second = Service.ConfirmAsync("Second?");
        host.WaitForAssertion(() => host.Find(".cy-dialog__title").TextContent.ShouldBe("First?"));
        second.IsCompleted.ShouldBeFalse();

        ClickButton(host, "Confirm");
        (await WithTimeout(first)).ShouldBeTrue();

        host.WaitForAssertion(() => host.Find(".cy-dialog__title").TextContent.ShouldBe("Second?"));
        second.IsCompleted.ShouldBeFalse();

        ClickButton(host, "Cancel");
        (await WithTimeout(second)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Answer_False_When_The_Token_Is_Cancelled()
    {
        var host = RenderHost();
        using var cts = new CancellationTokenSource();
        var answer = Service.ConfirmAsync("Publish?", cancellationToken: cts.Token);
        host.WaitForAssertion(() => host.Find(".cy-dialog__title"));

        await cts.CancelAsync();

        (await WithTimeout(answer)).ShouldBeFalse();
        host.WaitForAssertion(() => host.FindAll(".cy-dialog__title").Count.ShouldBe(0));
    }

    [Fact]
    public async Task Should_Answer_False_Immediately_For_An_Already_Cancelled_Token()
    {
        _ = RenderHost();

        var answer = Service.ConfirmAsync("Publish?", cancellationToken: new CancellationToken(true));

        (await WithTimeout(answer)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Answer_False_For_Pending_Requests_When_The_Host_Is_Removed()
    {
        var host = RenderHost();
        var answer = Service.ConfirmAsync("Publish?");
        host.WaitForAssertion(() => host.Find(".cy-dialog__title"));

        DisposeComponents();

        (await WithTimeout(answer)).ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => Service.ConfirmAsync("Again?"));
    }

    [Fact]
    public void Should_Reject_An_Empty_Title()
    {
        _ = RenderHost();

        Should.Throw<ArgumentException>(() => Service.ConfirmAsync(new CyConfirmOptions { Title = " " }));
    }

    [Fact]
    public void Should_Encode_The_Message()
    {
        var host = RenderHost();

        _ = Service.ConfirmAsync("Title", "<img src=x onerror=alert(1)>");

        host.WaitForAssertion(() =>
        {
            host.FindAll("dialog img").Count.ShouldBe(0);
            host.Find(".cy-dialog__description").TextContent.ShouldContain("<img");
        });
    }
}
