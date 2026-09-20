using Bunit;
using CymruBlazor.Components.Branding;
using CymruBlazor.Components.Content;
using CymruBlazor.Components.Feedback;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Localisation step 1 (roadmap D1): every built-in English string has a
/// <c>string?</c> override, and the override - not the English default -
/// reaches the rendered markup. Each test uses a Welsh value so a component
/// that ignored the parameter would fail.
/// </summary>
public sealed class LocalisationOverrideTests : TestContextBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly ToastService _toasts = new();

    public LocalisationOverrideTests()
    {
        Services.AddSingleton(_mediator.Object);
        Services.AddSingleton<IToastService>(_toasts);
    }

    [Fact]
    public void CyAlert_DismissAriaLabel_Overrides_Default()
    {
        var cut = Render<CyAlert>(p => p.Add(a => a.Dismissible, true).Add(a => a.DismissAriaLabel, "Diystyru"));

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe("Diystyru");
    }

    [Fact]
    public void CyAlert_Defaults_To_English_Dismiss()
    {
        var cut = Render<CyAlert>(p => p.Add(a => a.Dismissible, true));

        cut.Find(".cy-alert__dismiss").GetAttribute("aria-label").ShouldBe("Dismiss");
    }

    [Fact]
    public void CyBadge_DismissAriaLabel_Still_Overrides()
    {
        var cut = Render<CyBadge>(p => p.Add(b => b.Dismissible, true).Add(b => b.DismissAriaLabel, "Dileu"));

        cut.Find(".cy-badge__dismiss").GetAttribute("aria-label").ShouldBe("Dileu");
    }

    [Fact]
    public void CyBreadcrumb_AriaLabel_Overrides_Default()
    {
        var cut = Render<CyBreadcrumb>(p => p.Add(b => b.AriaLabel, "Briwsion bara"));

        cut.Find("nav").GetAttribute("aria-label").ShouldBe("Briwsion bara");
    }

    [Fact]
    public void CyNavigation_Landmark_And_Toggle_Labels_Are_Overridable()
    {
        var cut = Render<CyNavigation>(p => p
            .Add(n => n.AriaLabel, "Prif")
            .Add(n => n.OpenMenuLabel, "Agor y ddewislen")
            .Add(n => n.CloseMenuLabel, "Cau'r ddewislen"));

        cut.Find("nav").GetAttribute("aria-label").ShouldBe("Prif");
        cut.Find(".cy-navigation__toggle .u-sr-only").TextContent.ShouldBe("Agor y ddewislen");
    }

    [Fact]
    public void CyLanguageToggle_AriaLabel_Overrides_Computed_Label()
    {
        var cut = Render<CyLanguageToggle>(p => p.Add(t => t.AriaLabel, "Newid iaith"));

        cut.Find("button").GetAttribute("aria-label").ShouldBe("Newid iaith");
    }

    [Fact]
    public void CyToastContainer_Region_And_Close_Labels_Are_Overridable()
    {
        _toasts.Show("Hello", ToastVariant.Info, TimeSpan.Zero);

        var cut = Render<CyToastContainer>(p => p
            .Add(c => c.AriaLabel, "Hysbysiadau")
            .Add(c => c.DismissAriaLabel, "Cau'r hysbysiad"));

        cut.Find(".cy-toast-container").GetAttribute("aria-label").ShouldBe("Hysbysiadau");
        cut.Find(".cy-toast__close").GetAttribute("aria-label").ShouldBe("Cau'r hysbysiad");
    }

    [Fact]
    public void CyCodeBlock_Copy_Labels_Reach_The_Button_And_Language_Fallback()
    {
        var cut = Render<CyCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, " ")
            .Add(c => c.CodeLabel, "Cod")
            .Add(c => c.CopyLabel, "Copio"));

        cut.Find(".cy-code-block__language").TextContent.ShouldBe("Cod");
        cut.Find(".cy-code-block__copy span").TextContent.ShouldBe("Copio");
    }

    [Fact]
    public void CyCodeBlock_Copied_Label_And_Message_Are_Used_After_A_Copy()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "x").SetVoidResult();

        var cut = Render<CyCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.CopiedLabel, "Copiwyd")
            .Add(c => c.CopiedMessage, "Cod wedi'i gopïo."));

        cut.Find(".cy-code-block__copy").Click();

        cut.WaitForAssertion(() => cut.Find(".cy-code-block__copy span").TextContent.ShouldBe("Copiwyd"));
        _mediator.Verify(m => m.Publish(
            It.Is<ShowToastNotification>(n => n.Message == "Cod wedi'i gopïo."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CyCodeBlock_CopyFailed_Label_And_Message_Are_Used_After_A_Failure()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "x").SetException(new Microsoft.JSInterop.JSException("denied"));

        var cut = Render<CyCodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.CopyFailedLabel, "Methwyd copio")
            .Add(c => c.CopyFailedMessage, "Methwyd copio i'r clipfwrdd."));

        cut.Find(".cy-code-block__copy").Click();

        cut.WaitForAssertion(() => cut.Find(".cy-code-block__copy span").TextContent.ShouldBe("Methwyd copio"));
        _mediator.Verify(m => m.Publish(
            It.Is<ShowToastNotification>(n => n.Message == "Methwyd copio i'r clipfwrdd."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CySidebar_MobileDrawer_CloseLabel_Is_Overridable()
    {
        var cut = Render<CySidebar>(p => p.Add(s => s.MobileOpen, true).Add(s => s.CloseLabel, "Cau'r bar ochr"));

        cut.Find(".cy-sidebar__close").GetAttribute("aria-label").ShouldBe("Cau'r bar ochr");
    }

    [Fact]
    public void CySidebar_RevealLabel_Is_Overridable_When_Hidden()
    {
        var cut = Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Hidden)
            .Add(s => s.RevealLabel, "Dangos y bar ochr"));

        cut.Find(".cy-sidebar__reveal-handle").GetAttribute("aria-label").ShouldBe("Dangos y bar ochr");
    }

    [Fact]
    public void CySidebar_Toggle_Uses_Collapse_And_Expand_Labels()
    {
        var expanded = Render<CySidebar>(p => p
            .Add(s => s.CollapseLabel, "Culhau")
            .Add(s => s.ExpandLabel, "Ehangu"));

        expanded.Find(".cy-sidebar__toggle").GetAttribute("aria-label").ShouldBe("Culhau");

        var compact = Render<CySidebar>(p => p
            .Add(s => s.State, SidebarState.Compact)
            .Add(s => s.CollapseLabel, "Culhau")
            .Add(s => s.ExpandLabel, "Ehangu"));

        compact.Find(".cy-sidebar__toggle").GetAttribute("aria-label").ShouldBe("Ehangu");
    }

    [Fact]
    public void CySidebar_Step_Controls_Use_Per_State_Labels_And_Group_Label()
    {
        var cut = Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.IconOnly])
            .Add(s => s.State, SidebarState.Compact)
            .Add(s => s.SizeGroupLabel, "Maint y bar ochr")
            .Add(s => s.ExpandLabel, "Ehangu")
            .Add(s => s.IconOnlyLabel, "Eiconau yn unig"));

        cut.Find(".cy-sidebar__steps").GetAttribute("aria-label").ShouldBe("Maint y bar ochr");
        cut.Find(".cy-sidebar__step--widen").GetAttribute("aria-label").ShouldBe("Ehangu");
        cut.Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe("Eiconau yn unig");
    }

    [Fact]
    public void CySidebar_CompactLabel_And_HideLabel_Are_Overridable()
    {
        var cut = Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Expanded)
            .Add(s => s.CompactLabel, "Bar ochr cryno"));

        cut.Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe("Bar ochr cryno");

        var hide = Render<CySidebar>(p => p
            .Add(s => s.States, [SidebarState.Expanded, SidebarState.Compact, SidebarState.Hidden])
            .Add(s => s.State, SidebarState.Compact)
            .Add(s => s.HideLabel, "Cuddio'r bar ochr"));

        hide.Find(".cy-sidebar__step--narrow").GetAttribute("aria-label").ShouldBe("Cuddio'r bar ochr");
    }
}
