using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Data;

namespace CymruBlazor.Tests.Components.Data;

/// <summary>
/// <see cref="CyDataTable{TItem}"/> follow-ups: the "Rows per page" select (<c>PageSizeOptions</c>) and <c>OnSortChanged</c>.
/// </summary>
public sealed class CyDataTablePageSizeTests : TestContextBase
{
    public sealed record Person(int Id, string Name, int Age);

    private static readonly IReadOnlyList<Person> People =
    [
        new(1, "Carys", 41),
        new(2, "Aled", 29),
        new(3, "Bethan", 35),
        new(4, "Dafydd", 29),
        new(5, "Elin", 52)
    ];

    private static readonly IReadOnlyList<int> Sizes = [2, 5];

    private static readonly CyDataColumn<Person>[] Columns =
    [
        new CyDataColumn<Person> { Header = "Name", Value = p => p.Name, Sortable = true, RowHeader = true },
        new CyDataColumn<Person> { Header = "Age", Value = p => p.Age, Sortable = true }
    ];

    private IRenderedComponent<CyDataTable<Person>> RenderTable(Action<ComponentParameterCollectionBuilder<CyDataTable<Person>>>? configure = null) =>
        Render<CyDataTable<Person>>(p =>
        {
            p.Add(c => c.Caption, "People").Add(c => c.Columns, Columns).Add(c => c.Items, People);
            configure?.Invoke(p);
        });

    private static List<string> Names(IRenderedComponent<CyDataTable<Person>> cut) =>
        cut.FindAll("tbody th[scope=row]").Select(n => n.TextContent.Trim()).ToList();

    private static List<string> L(params string[] items) => items.ToList();

    private static string Status(IRenderedComponent<CyDataTable<Person>> cut) =>
        cut.Find("[role=status]").TextContent.Trim();

    // ---------------------------------------------------------------- page size

    [Fact]
    public void Should_Not_Render_A_Page_Size_Select_By_Default()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2));

        cut.FindAll(".cy-data-table__page-size").Count.ShouldBe(0);
        cut.FindAll("select").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_A_Labelled_Select_With_The_Options_And_The_Current_Size_Selected()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.PageSizeOptions, Sizes));

        var select = cut.Find(".cy-data-table__page-size select");
        var label = cut.Find(".cy-data-table__page-size label");

        label.TextContent.Trim().ShouldBe("Rows per page");
        label.GetAttribute("for").ShouldBe(select.GetAttribute("id"));
        cut.FindAll(".cy-data-table__page-size option").Select(o => o.GetAttribute("value")!).ToList().ShouldBe(L("2", "5"));
        // Blazor writes the bound value on the select itself; fall back to a selected option if a renderer marks that instead.
        (select.GetAttribute("value") ?? cut.Find(".cy-data-table__page-size option[selected]").GetAttribute("value")).ShouldBe("2");
    }

    [Fact]
    public void Should_Always_Offer_The_Current_Page_Size()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 3).Add(c => c.PageSizeOptions, Sizes));

        cut.FindAll(".cy-data-table__page-size option").Select(o => o.GetAttribute("value")!).ToList().ShouldBe(L("2", "3", "5"));
    }

    [Fact]
    public void Should_Not_Render_The_Select_When_Paging_Is_Off()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSizeOptions, Sizes));

        cut.FindAll("select").Count.ShouldBe(0);
        cut.FindAll(".cy-data-table__footer").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Return_To_Page_One_Announce_And_Report_When_The_Page_Size_Changes()
    {
        int? size = null;
        int? page = null;
        var cut = RenderTable(p => p
            .Add(c => c.PageSize, 2)
            .Add(c => c.Page, 2)
            .Add(c => c.PageSizeOptions, Sizes)
            .Add(c => c.PageSizeChanged, EventCallback.Factory.Create<int>(this, n => size = n))
            .Add(c => c.PageChanged, EventCallback.Factory.Create<int>(this, n => page = n)));
        Names(cut).ShouldBe(L("Bethan", "Dafydd"));

        cut.Find(".cy-data-table__page-size select").Change("5");

        Names(cut).ShouldBe(L("Carys", "Aled", "Bethan", "Dafydd", "Elin"));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 1 to 5 of 5");
        Status(cut).ShouldBe("5 rows per page. Page 1 of 1.");
        size.ShouldBe(5);
        page.ShouldBe(1);
    }

    [Fact]
    public void Should_Not_Report_A_Page_Change_When_Already_On_Page_One()
    {
        var pageCalls = 0;
        var cut = RenderTable(p => p
            .Add(c => c.PageSize, 2)
            .Add(c => c.PageSizeOptions, Sizes)
            .Add(c => c.PageChanged, EventCallback.Factory.Create<int>(this, _ => pageCalls++)));

        cut.Find(".cy-data-table__page-size select").Change("5");

        pageCalls.ShouldBe(0);
    }

    [Fact]
    public void Should_Keep_The_Chosen_Size_When_The_Parent_Rerenders_Without_Changing_It()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.PageSizeOptions, Sizes));
        cut.Find(".cy-data-table__page-size select").Change("5");

        cut.Render(p => p.Add(c => c.Wrap, true));

        Names(cut).Count.ShouldBe(5);
    }

    [Fact]
    public void Should_Ask_The_Provider_For_The_New_Page_Size()
    {
        var requests = new List<CyDataTableRequest>();
        var cut = Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Columns, Columns)
            .Add(c => c.PageSize, 2)
            .Add(c => c.PageSizeOptions, Sizes)
            .Add(c => c.ItemsProvider, (r, _) =>
            {
                requests.Add(r);
                var rows = People.Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToList();
                return Task.FromResult(new CyDataTableResult<Person>(rows, People.Count));
            }));
        cut.WaitForAssertion(() => requests.Count.ShouldBe(1));

        cut.Find(".cy-data-table__page-size select").Change("5");

        cut.WaitForAssertion(() => Names(cut).Count.ShouldBe(5));
        requests.Last().ShouldBe(new CyDataTableRequest(1, 5, null, false));
    }

    [Fact]
    public void Should_Use_The_Supplied_Text()
    {
        var text = new CyDataTableText { RowsPerPage = "Rhesi y dudalen", RowsPerPageChanged = "{0} rhes y dudalen. Tudalen 1 o {1}." };
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.PageSizeOptions, Sizes).Add(c => c.Text, text));

        cut.Find(".cy-data-table__page-size label").TextContent.Trim().ShouldBe("Rhesi y dudalen");

        cut.Find(".cy-data-table__page-size select").Change("5");

        Status(cut).ShouldBe("5 rhes y dudalen. Tudalen 1 o 1.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Reject_Options_That_Are_Not_Above_Zero(int bad)
    {
        var ex = Should.Throw<InvalidOperationException>(() =>
            RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.PageSizeOptions, new[] { 2, bad })));

        ex.Message.ShouldContain("PageSizeOptions");
    }

    // ---------------------------------------------------------------- OnSortChanged

    [Fact]
    public void Should_Raise_OnSortChanged_Once_Per_Change_With_The_New_Key_And_Direction()
    {
        var changes = new List<CyDataSortChange>();
        var cut = RenderTable(p => p
            .Add(c => c.OnSortChanged, EventCallback.Factory.Create<CyDataSortChange>(this, changes.Add)));

        cut.FindAll("thead button.cy-data-table__sort")[0].Click();
        cut.FindAll("thead button.cy-data-table__sort")[0].Click();
        cut.FindAll("thead button.cy-data-table__sort")[1].Click();

        changes.ShouldBe(new List<CyDataSortChange> { new("Name", false), new("Name", true), new("Age", false) });
    }

    [Fact]
    public void Should_Raise_OnSortChanged_With_A_Null_Key_When_Sorting_Is_Cleared()
    {
        var changes = new List<CyDataSortChange>();
        var cut = RenderTable(p => p
            .Add(c => c.AllowUnsorted, true)
            .Add(c => c.OnSortChanged, EventCallback.Factory.Create<CyDataSortChange>(this, changes.Add)));

        var name = () => cut.FindAll("thead button.cy-data-table__sort")[0];
        name().Click();
        name().Click();
        name().Click();

        changes.Last().ShouldBe(new CyDataSortChange(null, false));
    }

    [Fact]
    public void Should_Raise_OnSortChanged_Alongside_The_Two_Way_Parameters()
    {
        string? key = null;
        var raised = false;
        var cut = RenderTable(p => p
            .Add(c => c.SortKeyChanged, EventCallback.Factory.Create<string?>(this, k => key = k))
            .Add(c => c.OnSortChanged, EventCallback.Factory.Create<CyDataSortChange>(this, _ => raised = true)));

        cut.FindAll("thead button.cy-data-table__sort")[0].Click();

        key.ShouldBe("Name");
        raised.ShouldBeTrue();
    }
}
