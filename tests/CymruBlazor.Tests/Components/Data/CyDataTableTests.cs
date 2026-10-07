using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Data;

namespace CymruBlazor.Tests.Components.Data;

/// <summary>
/// <see cref="CyDataTable{TItem}"/>: structure, sorting, selection, paging, provider loading and parameter validation.
/// </summary>
public sealed class CyDataTableTests : TestContextBase
{
    public sealed record Person(int Id, string Name, int Age, string Note = "");

    private static readonly IReadOnlyList<Person> People =
    [
        new(1, "Carys", 41),
        new(2, "Aled", 29),
        new(3, "Bethan", 35),
        new(4, "Dafydd", 29),
        new(5, "Elin", 52)
    ];

    private static IReadOnlyList<CyDataColumn<Person>> Cols(bool sortable = true) =>
    [
        new CyDataColumn<Person> { Header = "Name", Value = p => p.Name, Sortable = sortable, RowHeader = true },
        new CyDataColumn<Person> { Header = "Age", Value = p => p.Age, Sortable = sortable, Align = CyColumnAlign.End }
    ];

    private IRenderedComponent<CyDataTable<Person>> RenderTable(
        Action<ComponentParameterCollectionBuilder<CyDataTable<Person>>>? configure = null,
        bool withItems = true,
        bool withCaption = true,
        bool withColumns = true) =>
        Render<CyDataTable<Person>>(p =>
        {
            if (withCaption)
            {
                p.Add(c => c.Caption, "People");
            }

            if (withColumns)
            {
                p.Add(c => c.Columns, Cols());
            }

            if (withItems)
            {
                p.Add(c => c.Items, People);
            }

            configure?.Invoke(p);
        });

    private static List<string> L(params string[] items) => items.ToList();

    private static List<string> Names(IRenderedComponent<CyDataTable<Person>> cut) =>
        cut.FindAll("tbody th[scope=row]").Select(n => n.TextContent.Trim()).ToList();

    private static string Status(IRenderedComponent<CyDataTable<Person>> cut) =>
        cut.Find("[role=status]").TextContent.Trim();

    private static AngleSharp.Dom.IElement SortButton(IRenderedComponent<CyDataTable<Person>> cut, string header) =>
        cut.FindAll("thead button.cy-data-table__sort").First(b => b.TextContent.Contains(header, StringComparison.Ordinal));

    private static Task<CyDataTableResult<Person>> Page(CyDataTableRequest request, IEnumerable<Person>? source = null)
    {
        IEnumerable<Person> rows = source ?? People;

        if (request.SortKey == "Name")
        {
            rows = request.SortDescending ? rows.OrderByDescending(p => p.Name) : rows.OrderBy(p => p.Name);
        }

        var all = rows.ToList();
        var page = request.PageSize > 0 ? all.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList() : all;
        return Task.FromResult(new CyDataTableResult<Person>(page, all.Count));
    }

    // ---------------------------------------------------------------- structure

    [Fact]
    public void Should_Render_A_Captioned_Table_With_Scoped_Headers_And_Row_Headers()
    {
        var cut = RenderTable();

        cut.Find("caption").TextContent.Trim().ShouldBe("People");
        cut.FindAll("thead th").Select(h => h.GetAttribute("scope")).ShouldAllBe(s => s == "col");
        cut.FindAll("thead th").Count.ShouldBe(2);
        Names(cut).ShouldBe(L("Carys", "Aled", "Bethan", "Dafydd", "Elin"));
        cut.FindAll("tbody tr")[0].QuerySelectorAll("td").Length.ShouldBe(1);
        cut.Find("th.cy-data-table__cell--end, td.cy-data-table__cell--end").ShouldNotBeNull();
    }

    [Fact]
    public void Should_Encode_Cell_Text()
    {
        var cut = Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Columns, Cols(false))
            .Add(c => c.Items, [new Person(1, "<b>x</b>", 1)]));

        cut.FindAll("tbody b").Count.ShouldBe(0);
        cut.Markup.ShouldContain("&lt;b&gt;x&lt;/b&gt;");
    }

    [Fact]
    public void Should_Render_Custom_Cells_And_Row_Actions_Under_A_Hidden_Heading()
    {
        var cut = Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Items, People)
            .Add(c => c.Columns, new[]
            {
                new CyDataColumn<Person> { Header = "Name", Cell = person => b => b.AddContent(0, $"[{person.Name}]") }
            })
            .Add(c => c.RowActions, person => b => b.AddMarkupContent(0, "<button type=\"button\">Edit</button>")));

        cut.FindAll("tbody td")[0].TextContent.ShouldBe("[Carys]");
        cut.FindAll("thead th")[1].QuerySelector(".u-sr-only")!.TextContent.ShouldBe("Actions");
        cut.FindAll("tbody button").Count.ShouldBe(5);
    }

    [Fact]
    public void Should_Show_The_Empty_Message_Or_Custom_Content()
    {
        var cut = RenderTable(p => p.Add(c => c.Items, new List<Person>()), withItems: false);
        cut.Find(".cy-data-table__empty").TextContent.ShouldBe("No results");
        cut.Find("tbody td").GetAttribute("colspan").ShouldBe("2");

        var custom = RenderTable(
            p => p
                .Add(c => c.Items, new List<Person>())
                .Add(c => c.EmptyContent, b => b.AddMarkupContent(0, "<p id=\"none\">Nobody</p>")),
            withItems: false);
        custom.Find("#none").TextContent.ShouldBe("Nobody");
    }

    [Fact]
    public void Should_Show_Skeleton_Rows_And_Busy_While_Loading()
    {
        var cut = RenderTable(
            p => p
                .Add(c => c.Items, new List<Person>())
                .Add(c => c.Loading, true),
            withItems: false);

        cut.FindAll(".cy-data-table__skeleton-row").Count.ShouldBe(5);
        cut.Find("table").GetAttribute("aria-busy").ShouldBe("true");
    }

    [Fact]
    public void Should_Use_Localised_Phrases()
    {
        var cut = RenderTable(
            p => p
                .Add(c => c.Items, new List<Person>())
                .Add(c => c.Text, new CyDataTableText { NoResults = "Dim canlyniadau" }),
            withItems: false);

        cut.Find(".cy-data-table__empty").TextContent.ShouldBe("Dim canlyniadau");
    }

    [Fact]
    public void Should_Apply_MaxHeight_And_Reject_Anything_That_Is_Not_A_Plain_Length()
    {
        var cut = RenderTable(p => p.Add(c => c.MaxHeight, "24rem"));
        var style = cut.Find(".cy-data-table").GetAttribute("style")!;

        style.Replace(" ", string.Empty)
            .ShouldContain("--cy-data-table-max-height:24rem");
        cut.Find(".cy-data-table").ClassList.ShouldContain("cy-data-table--sticky");

        foreach (var bad in new[] { "calc(1px+2px)", "24rem;color:red", "url(x)" })
        {
            Should.Throw<InvalidOperationException>(() => RenderTable(p => p.Add(c => c.MaxHeight, bad)));
        }
    }

    // ---------------------------------------------------------------- validation

    [Fact]
    public void Should_Reject_Invalid_Parameter_Combinations()
    {
        Should.Throw<InvalidOperationException>(() => RenderTable(
            p => p.Add(c => c.Caption, " "),
            withCaption: false));
        Should.Throw<InvalidOperationException>(() => RenderTable(
            p => p.Add(c => c.Columns, new List<CyDataColumn<Person>>()),
            withColumns: false));
        Should.Throw<InvalidOperationException>(() => RenderTable(p => p.Add(c => c.ItemsProvider, (r, t) => Page(r))));
        Should.Throw<InvalidOperationException>(() => RenderTable(p => p.Add(c => c.PageSize, -1)));
        Should.Throw<InvalidOperationException>(() => RenderTable(p => p.Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)));
        Should.Throw<InvalidOperationException>(() => RenderTable(
            p => p
                .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
                .Add(c => c.RowLabel, x => x.Name)
                .Add(c => c.ItemsProvider, (r, t) => Page(r)),
            withItems: false));
        Should.Throw<InvalidOperationException>(() => Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Items, People)
            .Add(c => c.Columns, new[] { new CyDataColumn<Person> { Header = "Name", Sortable = true } })));
        Should.Throw<InvalidOperationException>(() => Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Items, People)
            .Add(c => c.Columns, new[] { new CyDataColumn<Person> { Header = "Name", Value = x => x.Name, Width = "calc(1px)" } })));
    }

    // ---------------------------------------------------------------- sorting

    [Fact]
    public void Should_Mark_Only_The_Sorted_Column_With_AriaSort()
    {
        var cut = RenderTable();
        cut.FindAll("[aria-sort]").Count.ShouldBe(0);

        SortButton(cut, "Name").Click();

        var sorted = cut.FindAll("[aria-sort]");
        sorted.Count.ShouldBe(1);
        sorted[0].TagName.ShouldBe("TH");
        sorted[0].GetAttribute("aria-sort").ShouldBe("ascending");
        sorted[0].TextContent.ShouldContain("Name");
    }

    [Fact]
    public void Should_Cycle_Ascending_Descending_And_Back_Without_Unsorted_By_Default()
    {
        var cut = RenderTable();

        SortButton(cut, "Name").Click();
        Names(cut).ShouldBe(L("Aled", "Bethan", "Carys", "Dafydd", "Elin"));
        Status(cut).ShouldBe("Sorted by Name, ascending.");

        SortButton(cut, "Name").Click();
        Names(cut).ShouldBe(L("Elin", "Dafydd", "Carys", "Bethan", "Aled"));
        cut.Find("[aria-sort]").GetAttribute("aria-sort").ShouldBe("descending");
        Status(cut).ShouldBe("Sorted by Name, descending.");

        SortButton(cut, "Name").Click();
        cut.Find("[aria-sort]").GetAttribute("aria-sort").ShouldBe("ascending");
    }

    [Fact]
    public void Should_Offer_A_Third_Unsorted_Step_When_AllowUnsorted()
    {
        var cut = RenderTable(p => p.Add(c => c.AllowUnsorted, true));

        SortButton(cut, "Name").Click();
        SortButton(cut, "Name").Click();
        SortButton(cut, "Name").Click();

        cut.FindAll("[aria-sort]").Count.ShouldBe(0);
        Names(cut).ShouldBe(L("Carys", "Aled", "Bethan", "Dafydd", "Elin"));
        Status(cut).ShouldBe("Sorting removed.");
    }

    [Fact]
    public void Should_Keep_Equal_Rows_In_Their_Original_Order_When_Sorting()
    {
        var cut = RenderTable();

        SortButton(cut, "Age").Click();

        // Aled (2) and Dafydd (4) are both 29: stable, so Aled stays first.
        Names(cut).ShouldBe(L("Aled", "Dafydd", "Bethan", "Carys", "Elin"));
    }

    [Fact]
    public void Should_Sort_With_A_Custom_Comparison_And_Report_The_Change()
    {
        string? key = "unset";
        bool? descending = null;
        var cut = Render<CyDataTable<Person>>(p => p
            .Add(c => c.Caption, "People")
            .Add(c => c.Items, People)
            .Add(c => c.Columns, new[]
            {
                new CyDataColumn<Person> { Header = "Name", Value = x => x.Name, Sortable = true, SortKey = "n", Compare = (a, b) => b.Id.CompareTo(a.Id), RowHeader = true }
            })
            .Add(c => c.SortKeyChanged, EventCallback.Factory.Create<string?>(this, k => key = k))
            .Add(c => c.SortDescendingChanged, EventCallback.Factory.Create<bool>(this, d => descending = d)));

        cut.Find("thead button").Click();

        Names(cut).ShouldBe(L("Elin", "Dafydd", "Bethan", "Aled", "Carys"));
        key.ShouldBe("n");
        descending.ShouldBe(false);
    }

    [Fact]
    public void Should_Start_Sorted_From_The_SortKey_Parameter()
    {
        var cut = RenderTable(p => p.Add(c => c.SortKey, "Name").Add(c => c.SortDescending, true));

        Names(cut).ShouldBe(L("Elin", "Dafydd", "Carys", "Bethan", "Aled"));
        cut.Find("[aria-sort]").GetAttribute("aria-sort").ShouldBe("descending");
    }

    [Fact]
    public void Should_Name_The_Sort_Control_By_Its_Heading_Inside_The_Header_Cell()
    {
        var cut = RenderTable();

        var button = cut.Find("thead th button");
        button.ParentElement!.TagName.ShouldBe("TH");
        button.GetAttribute("type").ShouldBe("button");
        button.TextContent.Trim().ShouldBe("Name");
    }

    // ---------------------------------------------------------------- selection

    [Fact]
    public void Should_Name_Each_Selection_Control_And_Report_The_Selection()
    {
        IReadOnlyList<Person>? selected = null;
        var cut = RenderTable(p => p
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, x => x.Name)
            .Add(c => c.SelectedItemsChanged, EventCallback.Factory.Create<IReadOnlyList<Person>>(this, s => selected = s)));

        var boxes = cut.FindAll("tbody input[type=checkbox]");
        boxes.Count.ShouldBe(5);
        boxes[0].GetAttribute("aria-label").ShouldBe("Select Carys");

        boxes[0].Change(true);
        cut.FindAll("tbody input[type=checkbox]")[2].Change(true);

        selected!.Select(x => x.Name).ToList().ShouldBe(L("Carys", "Bethan"));
        Status(cut).ShouldBe("2 selected");
        cut.Find(".cy-data-table__selection").TextContent.ShouldBe("2 selected");
        cut.FindAll(".cy-data-table__row--selected").Count.ShouldBe(2);
        cut.FindAll("[aria-selected]").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Select_And_Clear_All_Rows_On_The_Page_From_The_Header()
    {
        var cut = RenderTable(p => p
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, x => x.Name)
            .Add(c => c.PageSize, 2));

        var header = cut.Find("thead input[type=checkbox]");
        header.GetAttribute("aria-label").ShouldBe("Select all rows on this page");
        header.Change(true);

        cut.FindAll("tbody input:checked").Count.ShouldBe(2);
        cut.Find("thead input[type=checkbox]").GetAttribute("aria-label").ShouldBe("Deselect all rows on this page");
        Status(cut).ShouldBe("2 selected");

        cut.Find("thead input[type=checkbox]").Change(false);
        cut.FindAll("tbody input:checked").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Select_One_Row_At_A_Time_With_Radio_Buttons()
    {
        IReadOnlyList<Person>? selected = null;
        var cut = RenderTable(p => p
            .Add(c => c.SelectionMode, CyDataSelectionMode.Single)
            .Add(c => c.RowLabel, x => x.Name)
            .Add(c => c.SelectedItemsChanged, EventCallback.Factory.Create<IReadOnlyList<Person>>(this, s => selected = s)));

        cut.FindAll("thead input").Count.ShouldBe(0);
        cut.Find("thead .u-sr-only").TextContent.ShouldBe("Select");
        var radios = cut.FindAll("tbody input[type=radio]");
        radios.Select(r => r.GetAttribute("name")).Distinct().Count().ShouldBe(1);

        radios[0].Change(true);
        cut.FindAll("tbody input[type=radio]")[1].Change(true);

        selected!.Count.ShouldBe(1);
        selected[0].Name.ShouldBe("Aled");
    }

    [Fact]
    public void Should_Keep_A_Selection_Across_Pages_By_Key()
    {
        IReadOnlyList<Person>? selected = null;
        var cut = RenderTable(p => p
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, x => x.Name)
            .Add(c => c.KeySelector, x => x.Id)
            .Add(c => c.PageSize, 2)
            .Add(c => c.SelectedItemsChanged, EventCallback.Factory.Create<IReadOnlyList<Person>>(this, s => selected = s)));

        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);
        cut.Find("button.cy-pagination__control--next").Click();
        cut.FindAll("tbody input:checked").Count.ShouldBe(0);
        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);
        cut.Find("button.cy-pagination__control--previous").Click();

        cut.FindAll("tbody input:checked").Count.ShouldBe(1);
        selected!.Select(x => x.Id).OrderBy(i => i).ToList().ShouldBe(new List<int> { 1, 3 });
    }

    [Fact]
    public void Should_Show_The_Selection_Passed_In()
    {
        var cut = RenderTable(p => p
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, x => x.Name)
            .Add(c => c.SelectedItems, [People[1]]));

        cut.FindAll("tbody input:checked").Count.ShouldBe(1);
        cut.Find(".cy-data-table__selection").TextContent.ShouldBe("1 selected");
    }

    // ---------------------------------------------------------------- paging

    [Fact]
    public void Should_Page_In_Memory_And_Show_The_Range()
    {
        int? page = null;
        var cut = RenderTable(p => p
            .Add(c => c.PageSize, 2)
            .Add(c => c.PageChanged, EventCallback.Factory.Create<int>(this, n => page = n)));

        Names(cut).ShouldBe(L("Carys", "Aled"));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 1 to 2 of 5");
        cut.Find("nav").GetAttribute("aria-label").ShouldBe("Pagination for People");

        cut.Find("button.cy-pagination__control--next").Click();

        Names(cut).ShouldBe(L("Bethan", "Dafydd"));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 3 to 4 of 5");
        Status(cut).ShouldBe("Page 2 of 3.");
        page.ShouldBe(2);
    }

    [Fact]
    public void Should_Show_A_Short_Last_Page_And_Clamp_A_Page_That_Is_Too_High()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.Page, 9));

        Names(cut).ShouldBe(L("Elin"));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 5 to 5 of 5");
    }

    [Fact]
    public void Should_Return_To_Page_One_When_The_Sort_Changes()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2).Add(c => c.Page, 2));
        Names(cut).ShouldBe(L("Bethan", "Dafydd"));

        SortButton(cut, "Name").Click();

        Names(cut).ShouldBe(L("Aled", "Bethan"));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 1 to 2 of 5");
    }

    [Fact]
    public void Should_Not_Render_A_Footer_When_Paging_Is_Off()
    {
        var cut = RenderTable();

        cut.FindAll(".cy-data-table__footer").Count.ShouldBe(0);
        cut.FindAll("nav").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Make_The_Range_Line_Focusable_Programmatically_Only()
    {
        var cut = RenderTable(p => p.Add(c => c.PageSize, 2));

        cut.Find(".cy-data-table__range").GetAttribute("tabindex").ShouldBe("-1");
    }

    // ---------------------------------------------------------------- provider

    [Fact]
    public void Should_Ask_The_Provider_For_The_Current_Page_And_Sort()
    {
        var requests = new List<CyDataTableRequest>();
        var cut = RenderTable(
            p => p
                .Add(c => c.PageSize, 2)
                .Add(c => c.ItemsProvider, (r, t) =>
                {
                    requests.Add(r);
                    return Page(r);
                }),
            withItems: false);

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("Carys", "Aled")));
        requests[0].ShouldBe(new CyDataTableRequest(1, 2, null, false));
        cut.Find(".cy-data-table__range").TextContent.ShouldBe("Showing 1 to 2 of 5");

        SortButton(cut, "Name").Click();
        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("Aled", "Bethan")));
        requests.Last().ShouldBe(new CyDataTableRequest(1, 2, "Name", false));

        cut.Find("button.cy-pagination__control--next").Click();
        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("Carys", "Dafydd")));
        requests.Last().ShouldBe(new CyDataTableRequest(2, 2, "Name", false));
    }

    [Fact]
    public void Should_Show_Busy_And_Announce_While_The_Provider_Works()
    {
        var gate = new TaskCompletionSource<CyDataTableResult<Person>>();
        var cut = RenderTable(p => p.Add(c => c.ItemsProvider, (r, t) => gate.Task), withItems: false);

        cut.Find("table").GetAttribute("aria-busy").ShouldBe("true");
        cut.FindAll(".cy-data-table__skeleton-row").Count.ShouldBeGreaterThan(0);
        Status(cut).ShouldBe("Loading");

        gate.SetResult(new CyDataTableResult<Person>(People, People.Count));

        cut.WaitForAssertion(() => cut.Find("table").GetAttribute("aria-busy").ShouldBe("false"));
        Names(cut).Count.ShouldBe(5);
        Status(cut).ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Discard_A_Slow_Answer_That_Arrives_After_A_Newer_One()
    {
        var gates = new List<TaskCompletionSource<CyDataTableResult<Person>>>();
        var cut = RenderTable(
            p => p.Add(c => c.ItemsProvider, (r, t) =>
            {
                var gate = new TaskCompletionSource<CyDataTableResult<Person>>();
                gates.Add(gate);
                return gate.Task;
            }),
            withItems: false);

        SortButton(cut, "Name").Click();
        cut.WaitForAssertion(() => gates.Count.ShouldBe(2));

        gates[1].SetResult(new CyDataTableResult<Person>([People[1]], 1));
        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("Aled")));

        gates[0].SetResult(new CyDataTableResult<Person>([People[0]], 1));
        Thread.Sleep(50);

        Names(cut).ShouldBe(L("Aled"));
    }

    [Fact]
    public void Should_Show_An_Error_Row_With_Retry_And_Never_The_Exception_Text()
    {
        var calls = 0;
        var cut = RenderTable(
            p => p.Add(c => c.ItemsProvider, (r, t) =>
            {
                calls++;

                return calls == 1
                    ? throw new InvalidOperationException("Server=secret")
                    : Page(r);
            }),
            withItems: false);

        cut.WaitForAssertion(() => cut.Find(".cy-data-table__error").TextContent.ShouldBe("Could not load the data."));
        cut.Find(".cy-data-table__error").GetAttribute("role").ShouldBe("alert");
        cut.Markup.ShouldNotContain("secret");

        cut.Find("button.cy-data-table__retry").Click();

        cut.WaitForAssertion(() => Names(cut).Count.ShouldBe(5));
        calls.ShouldBe(2);
        cut.FindAll(".cy-data-table__error").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Reload_On_RefreshAsync_Keeping_The_Sort()
    {
        var calls = 0;
        var cut = RenderTable(
            p => p
                .Add(c => c.SortKey, "Name")
                .Add(c => c.ItemsProvider, (r, t) =>
                {
                    calls++;
                    return Page(r);
                }),
            withItems: false);
        cut.WaitForAssertion(() => calls.ShouldBe(1));

        cut.InvokeAsync(() => cut.Instance.RefreshAsync());

        cut.WaitForAssertion(() => calls.ShouldBe(2));
        cut.Find("[aria-sort]").GetAttribute("aria-sort").ShouldBe("ascending");
    }

    [Fact]
    public void Should_Cancel_The_Request_And_Not_Update_After_Disposal()
    {
        CancellationToken token = default;
        var calls = 0;
        var host = Render<RemovableHost>(p => p.Add(c => c.ChildContent, b =>
        {
            b.OpenComponent<CyDataTable<Person>>(0);
            b.AddComponentParameter(1, nameof(CyDataTable<Person>.Caption), "People");
            b.AddComponentParameter(2, nameof(CyDataTable<Person>.Columns), Cols());
            b.AddComponentParameter(3, nameof(CyDataTable<Person>.ItemsProvider),
                (Func<CyDataTableRequest, CancellationToken, Task<CyDataTableResult<Person>>>)(async (r, t) =>
                {
                    token = t;
                    calls++;
                    await Task.Delay(Timeout.Infinite, t);
                    return new CyDataTableResult<Person>(People, 5);
                }));
            b.CloseComponent();
        }));
        host.WaitForAssertion(() => calls.ShouldBe(1));

        host.Render(p => p.Add(c => c.Show, false));

        host.WaitForAssertion(() => token.IsCancellationRequested.ShouldBeTrue());
    }

    [Fact]
    public void Should_Not_Reload_The_Provider_When_Only_Unrelated_State_Changes()
    {
        var calls = 0;
        var cut = RenderTable(
            p => p.Add(c => c.ItemsProvider, (r, t) =>
            {
                calls++;
                return Page(r);
            }),
            withItems: false);
        cut.WaitForAssertion(() => calls.ShouldBe(1));

        cut.Render(p => p.Add(c => c.Wrap, true));

        calls.ShouldBe(1);
    }
}
