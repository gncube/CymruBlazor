using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyDetailsTests : TestContextBase
{
    [Fact]
    public void Should_Render_Semantic_Details_And_Summary_Elements()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.Summary, "Where can I find my NHS number?")
            .AddChildContent("<p>An NHS number is a 10 digit number.</p>"));

        var details = cut.Find("details.cy-details");
        details.ShouldNotBeNull();
        details.HasAttribute("open").ShouldBeFalse();

        var summary = cut.Find("summary.cy-details__summary");
        summary.ShouldNotBeNull();

        var summaryText = cut.Find(".cy-details__summary-text");
        summaryText.TextContent.Trim().ShouldBe("Where can I find my NHS number?");

        var content = cut.Find(".cy-details__text");
        content.InnerHtml.ShouldContain("<p>An NHS number is a 10 digit number.</p>");
    }

    [Fact]
    public void Should_Fallback_To_Default_Summary_When_Summary_Is_Null()
    {
        var cut = Render<CyDetails>(p => p
            .AddChildContent("Help content"));

        var summaryText = cut.Find(".cy-details__summary-text");
        summaryText.TextContent.Trim().ShouldBe("Help");
    }

    [Fact]
    public void Should_Support_Custom_SummaryTemplate()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.SummaryTemplate, (RenderFragment)(b =>
            {
                b.OpenElement(0, "strong");
                b.AddContent(1, "Important details");
                b.CloseElement();
            }))
            .AddChildContent("Panel text"));

        var strong = cut.Find(".cy-details__summary-text strong");
        strong.TextContent.ShouldBe("Important details");
    }

    [Fact]
    public void Should_Render_Open_Attribute_When_Open_Is_True()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Summary, "Open disclosure")
            .AddChildContent("Visible text"));

        var details = cut.Find("details");
        details.HasAttribute("open").ShouldBeTrue();
    }

    [Fact]
    public void Should_Not_Render_Open_Attribute_When_Open_Is_False()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.Summary, "Closed disclosure")
            .AddChildContent("Hidden text"));

        var details = cut.Find("details");
        details.HasAttribute("open").ShouldBeFalse();
    }

    [Fact]
    public void Should_Update_Open_And_Invoke_Callbacks_On_Toggle_Event()
    {
        bool changedValue = false;
        bool toggleInvoked = false;

        var cut = Render<CyDetails>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.Summary, "Toggle test")
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, val => changedValue = val))
            .Add(c => c.OnToggle, EventCallback.Factory.Create<bool>(this, val => toggleInvoked = val))
            .AddChildContent("Toggled content"));

        cut.Find("details").TriggerEvent("ontoggle", EventArgs.Empty);

        cut.Instance.Open.ShouldBeTrue();
        changedValue.ShouldBeTrue();
        toggleInvoked.ShouldBeTrue();
        cut.Find("details").HasAttribute("open").ShouldBeTrue();

        // Toggle back to closed
        cut.Find("details").TriggerEvent("ontoggle", EventArgs.Empty);
        cut.Instance.Open.ShouldBeFalse();
        changedValue.ShouldBeFalse();
        cut.Find("details").HasAttribute("open").ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Support_Programmatic_ToggleAsync_OpenAsync_CloseAsync()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.Open, false)
            .Add(c => c.Summary, "Programmatic test")
            .AddChildContent("Details"));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());
        cut.Instance.Open.ShouldBeTrue();
        cut.Find("details").HasAttribute("open").ShouldBeTrue();

        await cut.InvokeAsync(() => cut.Instance.CloseAsync());
        cut.Instance.Open.ShouldBeFalse();
        cut.Find("details").HasAttribute("open").ShouldBeFalse();

        await cut.InvokeAsync(() => cut.Instance.ToggleAsync());
        cut.Instance.Open.ShouldBeTrue();
        cut.Find("details").HasAttribute("open").ShouldBeTrue();
    }

    [Fact]
    public void Should_Support_Welsh_Localisation_Via_CyDetailsText()
    {
        var welshText = new CyDetailsText
        {
            DefaultSummary = "Cymorth gyda hyn"
        };

        var cut = Render<CyDetails>(p => p
            .Add(c => c.Text, welshText)
            .AddChildContent("Cynnwys cymorth"));

        var summaryText = cut.Find(".cy-details__summary-text");
        summaryText.TextContent.Trim().ShouldBe("Cymorth gyda hyn");
    }

    [Fact]
    public void Should_Apply_Custom_Class_Style_Id_And_Attributes()
    {
        var cut = Render<CyDetails>(p => p
            .Add(c => c.Id, "custom-details-id")
            .Add(c => c.Class, "my-custom-class")
            .Add(c => c.Style, "margin-top: 2rem;")
            .AddUnmatched("data-testid", "clinical-details")
            .Add(c => c.Summary, "Summary")
            .AddChildContent("Content"));

        var details = cut.Find("details");
        details.Id.ShouldBe("custom-details-id");
        details.ClassList.ShouldContain("cy-details");
        details.ClassList.ShouldContain("my-custom-class");
        details.GetAttribute("style").ShouldNotBeNull().ShouldContain("margin-top: 2rem;");
        details.GetAttribute("data-testid").ShouldBe("clinical-details");
    }
}
