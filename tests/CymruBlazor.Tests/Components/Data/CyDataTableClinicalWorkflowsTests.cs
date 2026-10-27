using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Data;

namespace CymruBlazor.Tests.Components.Data;

public sealed class CyDataTableClinicalWorkflowsTests : TestContextBase
{
    public sealed record PatientRecord(int Id, string Name, string Clinic, int Age);

    private static readonly IReadOnlyList<PatientRecord> Patients =
    [
        new(1, "Aled Jones", "Cardiology", 54),
        new(2, "Bethan Davies", "Oncology", 42),
        new(3, "Carys Evans", "Cardiology", 38),
        new(4, "Dafydd Thomas", "Pediatrics", 12),
        new(5, "Elin Roberts", "Oncology", 67)
    ];

    private static IReadOnlyList<CyDataColumn<PatientRecord>> CreateColumns(bool filterable = false) =>
    [
        new CyDataColumn<PatientRecord>
        {
            Header = "Name",
            Value = p => p.Name,
            RowHeader = true,
            Filterable = filterable
        },
        new CyDataColumn<PatientRecord>
        {
            Header = "Clinic",
            Value = p => p.Clinic,
            Filterable = filterable
        },
        new CyDataColumn<PatientRecord>
        {
            Header = "Age",
            Value = p => p.Age,
            Align = CyColumnAlign.End,
            Filterable = filterable
        }
    ];

    [Fact]
    public void Should_Set_Indeterminate_State_And_Accessible_Labels_On_Header_Checkbox()
    {
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name));

        var headerCheckbox = cut.Find("thead input[type=checkbox]");
        headerCheckbox.HasAttribute("checked").ShouldBeFalse();
        headerCheckbox.ClassList.ShouldNotContain("cy-data-table__checkbox--indeterminate");

        // Select 1 row
        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);
        headerCheckbox = cut.Find("thead input[type=checkbox]");
        headerCheckbox.ClassList.ShouldContain("cy-data-table__checkbox--indeterminate");
        headerCheckbox.GetAttribute("aria-label")!.ShouldContain("Some rows selected");

        // Select remaining rows
        for (var i = 1; i < Patients.Count; i++)
        {
            cut.FindAll("tbody input[type=checkbox]")[i].Change(true);
        }

        headerCheckbox = cut.Find("thead input[type=checkbox]");
        headerCheckbox.ClassList.ShouldNotContain("cy-data-table__checkbox--indeterminate");
        headerCheckbox.HasAttribute("checked").ShouldBeTrue();
        headerCheckbox.GetAttribute("aria-label")!.ShouldContain("Deselect all rows");
    }

    [Fact]
    public void Should_Support_Shift_Click_Range_Selection()
    {
        IReadOnlyList<PatientRecord>? selected = null;
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name)
            .Add(c => c.OnSelectionChanged, items => selected = items));

        // Click row 0 (Aled)
        cut.FindAll("tbody input[type=checkbox]")[0].Click(new MouseEventArgs { ShiftKey = false });
        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);

        selected!.Count.ShouldBe(1);

        // Shift+Click row 3 (Dafydd)
        cut.FindAll("tbody input[type=checkbox]")[3].Click(new MouseEventArgs { ShiftKey = true });

        // Rows 0 to 3 should now be selected (Aled, Bethan, Carys, Dafydd)
        selected!.Count.ShouldBe(4);
        selected.Select(x => x.Name).ShouldBe(["Aled Jones", "Bethan Davies", "Carys Evans", "Dafydd Thomas"]);
    }

    [Fact]
    public void Should_Toggle_Selection_On_Space_Key()
    {
        IReadOnlyList<PatientRecord>? selected = null;
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name)
            .Add(c => c.OnSelectionChanged, items => selected = items));

        cut.FindAll("tbody tr")[1].KeyDown(new KeyboardEventArgs { Key = " " });

        selected!.Count.ShouldBe(1);
        selected[0].Name.ShouldBe("Bethan Davies");

        // Press space again to toggle off
        cut.FindAll("tbody tr")[1].KeyDown(new KeyboardEventArgs { Key = " " });
        selected!.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Render_Bulk_Actions_Toolbar_When_Rows_Selected()
    {
        var actionInvoked = false;
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name)
            .Add(c => c.BulkActions, (RenderFragment<IReadOnlyList<PatientRecord>>)(items => builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "type", "button");
                builder.AddAttribute(2, "class", "test-bulk-action");
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => actionInvoked = true));
                builder.AddContent(4, $"Export {items.Count}");
                builder.CloseElement();
            })));

        // Initially no bulk bar
        cut.FindAll(".cy-data-table__bulk-bar").Count.ShouldBe(0);

        // Select 2 rows
        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);
        cut.FindAll("tbody input[type=checkbox]")[1].Change(true);

        cut.FindAll(".cy-data-table__bulk-bar").Count.ShouldBe(1);
        cut.Find(".cy-data-table__bulk-count").TextContent.ShouldContain("2 selected");
        cut.Find(".cy-data-table__bulk-bar").GetAttribute("aria-label").ShouldBe("2 selected");

        var actionBtn = cut.Find(".test-bulk-action");
        actionBtn.TextContent.ShouldBe("Export 2");
        actionBtn.Click();
        actionInvoked.ShouldBeTrue();

        // Clear selection button
        var clearBtn = cut.Find(".cy-data-table__bulk-clear");
        clearBtn.Click();

        cut.FindAll(".cy-data-table__bulk-bar").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Select_All_Across_Pages_When_Paged_In_Memory()
    {
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.PageSize, 2)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name));

        // Select row on page 1
        cut.FindAll("tbody input[type=checkbox]")[0].Change(true);

        var selectAllLink = cut.Find(".cy-data-table__bulk-link");
        selectAllLink.TextContent.ShouldBe("Select all 5 items");

        selectAllLink.Click();

        cut.Find(".cy-data-table__bulk-count").TextContent.ShouldContain("5 selected");
    }

    [Fact]
    public void Should_Filter_Rows_By_Column_Input()
    {
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns(filterable: true))
            .Add(c => c.Items, Patients)
            .Add(c => c.FilterDebounceMs, 0));

        cut.FindAll(".cy-data-table__filter-row").Count.ShouldBe(1);

        var filterInputs = cut.FindAll(".cy-data-table__column-filter-input");
        filterInputs.Count.ShouldBe(3);

        // Filter clinic by "Cardiology"
        cut.FindAll(".cy-data-table__column-filter-input")[1].Input("Cardiology");

        var rows = cut.FindAll("tbody tr");
        rows.Count.ShouldBe(2);
        cut.FindAll("tbody th[scope=row]").Select(th => th.TextContent.Trim()).ShouldBe(["Aled Jones", "Carys Evans"]);

        // Clear filter
        cut.FindAll(".cy-data-table__column-filter-input")[1].Input("");
        cut.FindAll("tbody tr").Count.ShouldBe(5);
    }

    [Fact]
    public void Should_Perform_Global_Search_Across_Columns()
    {
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.ShowSearch, true)
            .Add(c => c.FilterDebounceMs, 0));

        var searchInput = cut.Find(".cy-data-table__search-input");
        searchInput.Input("Oncology");

        var rows = cut.FindAll("tbody th[scope=row]").Select(th => th.TextContent.Trim()).ToList();
        rows.ShouldBe(["Bethan Davies", "Elin Roberts"]);

        // Search by age
        searchInput.Input("12");
        cut.FindAll("tbody th[scope=row]").Select(th => th.TextContent.Trim()).ShouldBe(["Dafydd Thomas"]);

        // Clear button
        cut.Find(".cy-data-table__search-clear").Click();
        cut.FindAll("tbody tr").Count.ShouldBe(5);
    }

    [Fact]
    public void Should_Pass_Filters_And_Search_To_ItemsProvider()
    {
        CyDataTableRequest? lastRequest = null;

        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns(filterable: true))
            .Add(c => c.ShowSearch, true)
            .Add(c => c.FilterDebounceMs, 0)
            .Add(c => c.ItemsProvider, (request, _) =>
            {
                lastRequest = request;
                return Task.FromResult(new CyDataTableResult<PatientRecord>([Patients[0]], 1));
            }));

        lastRequest.ShouldNotBeNull();
        lastRequest.SearchQuery.ShouldBeNull();
        lastRequest.Filters.ShouldBeNull();

        // Search
        cut.Find(".cy-data-table__search-input").Input("cardio");
        lastRequest.SearchQuery.ShouldBe("cardio");

        // Column filter
        cut.FindAll(".cy-data-table__column-filter-input")[0].Input("Aled");
        lastRequest.Filters.ShouldNotBeNull();
        lastRequest.Filters["Name"].ShouldBe("Aled");
    }

    [Fact]
    public void Should_Render_Responsive_Data_Labels_On_Cells()
    {
        var cut = Render<CyDataTable<PatientRecord>>(p => p
            .Add(c => c.Caption, "Patients")
            .Add(c => c.Columns, CreateColumns())
            .Add(c => c.Items, Patients)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, p => p.Name)
            .Add(c => c.Responsive, true));

        cut.Find(".cy-data-table").ClassList.ShouldContain("cy-data-table--responsive");

        var firstRow = cut.Find("tbody tr");
        var cells = firstRow.Children;

        // Selection cell
        cells[0].GetAttribute("data-label").ShouldBe("Select");

        // Name cell (row header)
        cells[1].GetAttribute("data-label").ShouldBe("Name");

        // Clinic cell
        cells[2].GetAttribute("data-label").ShouldBe("Clinic");

        // Age cell
        cells[3].GetAttribute("data-label").ShouldBe("Age");
    }
}
