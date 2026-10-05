using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Content;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CySummaryListTests : TestContextBase
{
    private static RenderFragment Row(string key, string? value = null, string? changeHref = null, string? changeText = null, EventCallback onChange = default) =>
        b =>
        {
            b.OpenComponent<CySummaryRow>(0);
            b.AddComponentParameter(1, nameof(CySummaryRow.Key), key);

            if (value is not null)
            {
                b.AddComponentParameter(2, nameof(CySummaryRow.Value), value);
            }

            if (changeHref is not null)
            {
                b.AddComponentParameter(3, nameof(CySummaryRow.ChangeHref), changeHref);
            }

            if (changeText is not null)
            {
                b.AddComponentParameter(4, nameof(CySummaryRow.ChangeText), changeText);
            }

            if (onChange.HasDelegate)
            {
                b.AddComponentParameter(5, nameof(CySummaryRow.OnChange), onChange);
            }

            b.CloseComponent();
        };

    private IRenderedComponent<CySummaryList> RenderList(params RenderFragment[] rows) =>
        Render<CySummaryList>(p => p.AddChildContent(b =>
        {
            foreach (var row in rows)
            {
                b.AddContent(0, row);
            }
        }));

    [Fact]
    public void Should_Render_A_Description_List_With_Key_And_Value()
    {
        var cut = RenderList(Row("Name", "Gwen Davies"));

        cut.Find("dl.cy-summary-list").ShouldNotBeNull();
        cut.Find("dt.cy-summary-list__key").TextContent.ShouldBe("Name");
        cut.Find("dd.cy-summary-list__value").TextContent.Trim().ShouldBe("Gwen Davies");
    }

    [Fact]
    public void Should_Render_A_Change_Link_Whose_Name_Includes_The_Key()
    {
        var cut = RenderList(Row("Date of birth", "1 May 1980", changeHref: "/dob"));

        var link = cut.Find("a.cy-summary-list__link");
        link.GetAttribute("href").ShouldBe("/dob");
        Regex.Replace(link.TextContent, @"\s+", " ").Trim().ShouldBe("Change Date of birth");
        link.QuerySelector(".u-sr-only")!.TextContent.Trim().ShouldBe("Date of birth");
    }

    [Fact]
    public void Should_Render_A_Change_Button_For_A_Callback_And_Raise_It()
    {
        var clicks = 0;
        var callback = EventCallback.Factory.Create(this, () => clicks++);
        var cut = RenderList(Row("Postcode", "CF14 4UJ", onChange: callback));

        cut.FindAll("a.cy-summary-list__link").Count.ShouldBe(0);
        var button = cut.Find("button.cy-summary-list__link");
        button.GetAttribute("type").ShouldBe("button");
        button.Click();

        clicks.ShouldBe(1);
    }

    [Fact]
    public void Should_Prefer_The_Link_When_Both_Are_Set()
    {
        var callback = EventCallback.Factory.Create(this, () => { });
        var cut = RenderList(Row("Name", "x", changeHref: "/name", onChange: callback));

        cut.FindAll("a.cy-summary-list__link").Count.ShouldBe(1);
        cut.FindAll("button.cy-summary-list__link").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_No_Actions_Cell_Without_A_Change_Action()
    {
        var cut = RenderList(Row("Name", "x"));

        cut.FindAll(".cy-summary-list__actions").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Show_Not_Provided_For_An_Empty_Value()
    {
        var cut = RenderList(Row("Allergies"));

        cut.Find(".cy-summary-list__empty").TextContent.ShouldBe("Not provided");
    }

    [Fact]
    public void Should_Use_The_List_Text_For_Localisation()
    {
        var cut = Render<CySummaryList>(p => p
            .Add(c => c.Text, new CySummaryListText { Change = "Newid", NotProvided = "Heb ei ddarparu" })
            .AddChildContent(b =>
            {
                b.AddContent(0, Row("Enw", changeHref: "/enw"));
            }));

        cut.Find(".cy-summary-list__empty").TextContent.ShouldBe("Heb ei ddarparu");
        cut.Find("a.cy-summary-list__link").TextContent.ShouldStartWith("Newid");
    }

    [Fact]
    public void Should_Let_A_Row_Override_The_Change_Text()
    {
        var cut = RenderList(Row("Name", "x", changeHref: "/name", changeText: "Edit"));

        cut.Find("a.cy-summary-list__link").TextContent.ShouldStartWith("Edit");
    }

    [Fact]
    public void Should_Encode_The_Value()
    {
        var cut = RenderList(Row("Notes", "<script>alert(1)</script>"));

        cut.FindAll("script").Count.ShouldBe(0);
        cut.Find(".cy-summary-list__value").TextContent.ShouldContain("<script>");
    }

    [Fact]
    public void Should_Render_Rich_Value_Content_In_Preference_To_Value()
    {
        var cut = Render<CySummaryList>(p => p.AddChildContent(b =>
        {
            b.OpenComponent<CySummaryRow>(0);
            b.AddComponentParameter(1, nameof(CySummaryRow.Key), "Address");
            b.AddComponentParameter(2, nameof(CySummaryRow.Value), "ignored");
            b.AddComponentParameter(3, nameof(CySummaryRow.ChildContent), (RenderFragment)(inner => inner.AddMarkupContent(0, "<strong>1 High St</strong>")));
            b.CloseComponent();
        }));

        cut.Find(".cy-summary-list__value strong").TextContent.ShouldBe("1 High St");
        cut.Markup.ShouldNotContain("ignored");
    }

    [Fact]
    public void Should_Reject_An_Empty_Key()
    {
        Should.Throw<InvalidOperationException>(() => RenderList(Row(" ", "x")));
    }

    [Fact]
    public void Should_Toggle_Border_And_Card_Classes()
    {
        var plain = Render<CySummaryList>(p => p.Add(c => c.Borders, false).Add(c => c.Card, true));

        plain.Find("dl").ClassList.ShouldContain("cy-summary-list--no-borders");
        plain.Find("dl").ClassList.ShouldContain("cy-summary-list--card");
        Render<CySummaryList>().Find("dl").ClassList.ShouldNotContain("cy-summary-list--no-borders");
    }
}
