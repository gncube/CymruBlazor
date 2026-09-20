using Bunit;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyTooltipTests : TestContextBase
{
    private const string ModulePath = "./_content/CymruBlazor/js/cymru-overlay.js";

    private readonly BunitJSModuleInterop _module;

    public CyTooltipTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
        _module.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<CyTooltip> RenderTooltip(Action<ComponentParameterCollectionBuilder<CyTooltip>>? configure = null)
        => Render<CyTooltip>(p =>
        {
            p.Add(t => t.Text, "Occupied beds out of total beds.");
            p.AddChildContent("Info");
            configure?.Invoke(p);
        });

    [Fact]
    public void Renders_The_Text_In_A_Tooltip_Role_Element()
    {
        var cut = RenderTooltip();

        var content = cut.Find("[role=tooltip]");
        content.TextContent.ShouldBe("Occupied beds out of total beds.");
        content.ClassList.ShouldContain("cy-tooltip__content");
    }

    [Fact]
    public void Trigger_Is_Focusable_And_Described_By_The_Tooltip()
    {
        var cut = RenderTooltip();

        var trigger = cut.Find(".cy-tooltip__trigger");
        trigger.TextContent.ShouldBe("Info");
        trigger.GetAttribute("tabindex").ShouldBe("0");
        trigger.GetAttribute("aria-describedby").ShouldBe(cut.Find("[role=tooltip]").Id);
    }

    [Fact]
    public void Tooltip_Element_Id_Is_Derived_From_The_Component_Id()
    {
        var cut = RenderTooltip(p => p.Add(t => t.Id, "save-tip"));

        cut.Find("[role=tooltip]").Id.ShouldBe("save-tip-content");
        cut.Instance.ContentId.ShouldBe("save-tip-content");
    }

    [Fact]
    public void Focusable_False_Leaves_The_Trigger_Out_Of_The_Tab_Order()
    {
        var cut = RenderTooltip(p => p.Add(t => t.Focusable, false));

        var trigger = cut.Find(".cy-tooltip__trigger");
        trigger.HasAttribute("tabindex").ShouldBeFalse();
        trigger.HasAttribute("aria-describedby").ShouldBeFalse();
    }

    [Theory]
    [InlineData(TooltipPlacement.Top, "cy-tooltip--top")]
    [InlineData(TooltipPlacement.Bottom, "cy-tooltip--bottom")]
    [InlineData(TooltipPlacement.Start, "cy-tooltip--start")]
    [InlineData(TooltipPlacement.End, "cy-tooltip--end")]
    public void Placement_Maps_To_A_Modifier_Class(TooltipPlacement placement, string expected)
    {
        var cut = RenderTooltip(p => p.Add(t => t.Placement, placement));

        cut.Find(".cy-tooltip").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Defaults_To_Top()
    {
        RenderTooltip().Find(".cy-tooltip").ClassList.ShouldContain("cy-tooltip--top");
    }

    [Fact]
    public void Tooltip_Text_Is_Encoded_Not_Rendered_As_Markup()
    {
        var cut = Render<CyTooltip>(p => p
            .Add(t => t.Text, "<b>bold</b>")
            .AddChildContent("Info"));

        cut.Find("[role=tooltip]").InnerHtml.ShouldBe("&lt;b&gt;bold&lt;/b&gt;");
        cut.FindAll("[role=tooltip] b").Count.ShouldBe(0);
    }

    [Fact]
    public void Blank_Text_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            Render<CyTooltip>(p => p.Add(t => t.Text, " ")));
    }

    [Fact]
    public void Installs_The_Escape_Handler_Once_Per_Instance()
    {
        var cut = RenderTooltip();
        cut.Render();
        cut.Render();

        _module.VerifyInvoke("installTooltips", calledTimes: 1);
    }
}
