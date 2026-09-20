using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Accessibility;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Accessibility;

/// <summary>
/// Markup, parameter and lifecycle tests for <see cref="CyDialog"/>. What the
/// browser does with <c>showModal()</c> (focus containment, inert background,
/// Escape, focus return) is covered by the Playwright tests.
/// </summary>
public sealed class CyDialogTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _module;

    public CyDialogTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
        _module.Mode = JSRuntimeMode.Loose;
        _module.Setup<int>("showDialog", _ => true).SetResult(7);
    }

    private IRenderedComponent<CyDialog> RenderDialog(Action<ComponentParameterCollectionBuilder<CyDialog>>? configure = null)
        => Render<CyDialog>(p =>
        {
            p.Add(d => d.Title, "Appointment details");
            configure?.Invoke(p);
        });

    [Fact]
    public void Closed_Dialog_Renders_An_Empty_Dialog_Element()
    {
        var cut = RenderDialog(p => p.AddChildContent("<p id=\"body\">Body</p>"));

        cut.Find("dialog").ClassList.ShouldContain("cy-dialog");
        cut.FindAll("#body").Count.ShouldBe(0);
        cut.FindAll(".cy-dialog__title").Count.ShouldBe(0);
        cut.Find("dialog").HasAttribute("aria-labelledby").ShouldBeFalse();
    }

    [Fact]
    public void Closed_Dialog_Does_Not_Touch_The_Overlay_Module()
    {
        RenderDialog();

        _module.VerifyNotInvoke("showDialog");
    }

    [Fact]
    public void Open_Dialog_Is_Named_By_Its_Title()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true));

        var dialog = cut.Find("dialog");
        var title = cut.Find(".cy-dialog__title");
        title.TagName.ShouldBe("H2");
        title.TextContent.ShouldBe("Appointment details");
        dialog.GetAttribute("aria-labelledby").ShouldBe(title.Id);
    }

    [Fact]
    public void Description_Is_Rendered_And_Referenced_By_AriaDescribedBy()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true).Add(d => d.Description, "Check the details."));

        var description = cut.Find(".cy-dialog__description");
        description.TextContent.ShouldBe("Check the details.");
        cut.Find("dialog").GetAttribute("aria-describedby").ShouldBe(description.Id);
    }

    [Fact]
    public void Without_A_Description_There_Is_No_AriaDescribedBy()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true));

        cut.Find("dialog").HasAttribute("aria-describedby").ShouldBeFalse();
        cut.FindAll(".cy-dialog__description").Count.ShouldBe(0);
    }

    [Fact]
    public void Open_Dialog_Renders_Body_And_Footer()
    {
        var cut = RenderDialog(p => p
            .Add(d => d.Open, true)
            .Add(d => d.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<p id=\"body\">Body</p>")))
            .Add(d => d.Footer, (RenderFragment)(b => b.AddMarkupContent(0, "<button id=\"ok\">OK</button>"))));

        cut.Find(".cy-dialog__body #body").TextContent.ShouldBe("Body");
        cut.Find(".cy-dialog__footer #ok").TextContent.ShouldBe("OK");
    }

    [Fact]
    public void Footer_Container_Is_Omitted_When_There_Is_No_Footer()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true));

        cut.FindAll(".cy-dialog__footer").Count.ShouldBe(0);
    }

    [Fact]
    public void Opening_Calls_ShowDialog_On_The_Overlay_Module()
    {
        RenderDialog(p => p.Add(d => d.Open, true));

        _module.VerifyInvoke("showDialog");
    }

    [Fact]
    public void Opening_Later_Calls_ShowDialog_Once()
    {
        var cut = RenderDialog();

        cut.Render(p => p.Add(d => d.Open, true));
        cut.Render(p => p.Add(d => d.Open, true));

        _module.VerifyInvoke("showDialog", calledTimes: 1);
    }

    [Fact]
    public void Setting_Open_To_False_Disposes_The_Native_Dialog_And_Raises_OnClosed_Once()
    {
        var closed = 0;
        var cut = RenderDialog(p => p
            .Add(d => d.Open, true)
            .Add(d => d.OnClosed, EventCallback.Factory.Create(this, () => closed++)));

        cut.Render(p => p.Add(d => d.Open, false));

        _module.VerifyInvoke("disposeDialog");
        cut.FindAll(".cy-dialog__body").Count.ShouldBe(0);
        closed.ShouldBe(1);
    }

    [Fact]
    public void Close_Button_Closes_Notifies_OpenChanged_And_Raises_OnClosed()
    {
        bool? openChanged = null;
        var closed = 0;
        var cut = RenderDialog(p => p
            .Add(d => d.Open, true)
            .Add(d => d.OpenChanged, EventCallback.Factory.Create<bool>(this, v => openChanged = v))
            .Add(d => d.OnClosed, EventCallback.Factory.Create(this, () => closed++)));

        cut.Find(".cy-dialog__close").Click();

        openChanged.ShouldBe(false);
        closed.ShouldBe(1);
        cut.FindAll(".cy-dialog__body").Count.ShouldBe(0);
    }

    [Fact]
    public async Task A_Native_Close_Reported_By_The_Module_Behaves_Like_The_Close_Button()
    {
        bool? openChanged = null;
        var closed = 0;
        var cut = RenderDialog(p => p
            .Add(d => d.Open, true)
            .Add(d => d.OpenChanged, EventCallback.Factory.Create<bool>(this, v => openChanged = v))
            .Add(d => d.OnClosed, EventCallback.Factory.Create(this, () => closed++)));

        await cut.InvokeAsync(() => cut.Instance.OnNativeClosed());

        openChanged.ShouldBe(false);
        closed.ShouldBe(1);
        cut.FindAll(".cy-dialog__body").Count.ShouldBe(0);
    }

    [Fact]
    public async Task OnClosed_Is_Raised_Only_Once_When_The_Native_Close_Follows_The_Button()
    {
        var closed = 0;
        var cut = RenderDialog(p => p
            .Add(d => d.Open, true)
            .Add(d => d.OnClosed, EventCallback.Factory.Create(this, () => closed++)));

        cut.Find(".cy-dialog__close").Click();
        await cut.InvokeAsync(() => cut.Instance.OnNativeClosed());

        closed.ShouldBe(1);
    }

    [Fact]
    public void Dismissible_False_Hides_The_Close_Button()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true).Add(d => d.Dismissible, false));

        cut.FindAll(".cy-dialog__close").Count.ShouldBe(0);
    }

    [Fact]
    public void Close_Button_Label_Defaults_To_English_And_Can_Be_Translated()
    {
        RenderDialog(p => p.Add(d => d.Open, true))
            .Find(".cy-dialog__close").GetAttribute("aria-label").ShouldBe("Close");

        RenderDialog(p => p.Add(d => d.Open, true).Add(d => d.CloseLabel, "Cau"))
            .Find(".cy-dialog__close").GetAttribute("aria-label").ShouldBe("Cau");
    }

    [Theory]
    [InlineData(ComponentSize.Small, "cy-dialog--sm")]
    [InlineData(ComponentSize.Medium, "cy-dialog--md")]
    [InlineData(ComponentSize.Unspecified, "cy-dialog--md")]
    [InlineData(ComponentSize.Large, "cy-dialog--lg")]
    public void Size_Maps_To_A_Modifier_Class(ComponentSize size, string expected)
    {
        var cut = RenderDialog(p => p.Add(d => d.Size, size));

        cut.Find("dialog").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData(ComponentSize.ExtraSmall)]
    [InlineData(ComponentSize.ExtraLarge)]
    public void Unsupported_Sizes_Throw(ComponentSize size)
    {
        Should.Throw<InvalidOperationException>(() => RenderDialog(p => p.Add(d => d.Size, size)));
    }

    [Fact]
    public void A_Blank_Title_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyDialog>(p => p.Add(d => d.Title, " ")));
    }

    [Fact]
    public void Custom_Class_Is_Kept_Alongside_The_Component_Classes()
    {
        var cut = RenderDialog(p => p.Add(d => d.Class, "my-dialog"));

        cut.Find("dialog").ClassList.ShouldContain("cy-dialog");
        cut.Find("dialog").ClassList.ShouldContain("my-dialog");
    }

    [Fact]
    public async Task Disposing_An_Open_Dialog_Releases_The_Native_Dialog()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true));

        await cut.Instance.DisposeAsync();

        _module.VerifyInvoke("disposeDialog");
    }

    [Fact]
    public void Changing_Dismissible_While_Open_Updates_The_Module()
    {
        var cut = RenderDialog(p => p.Add(d => d.Open, true));

        cut.Render(p => p.Add(d => d.Open, true).Add(d => d.Dismissible, false));

        _module.VerifyInvoke("updateDialog");
    }
}
