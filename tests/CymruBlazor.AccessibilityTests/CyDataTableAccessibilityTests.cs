using Bunit;
using CymruBlazor.Components.Data;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyDataTableAccessibilityTests : AxeTestBase
{
    public sealed record Row(int Id, string Name, int Minutes);

    private static readonly IReadOnlyList<Row> Rows =
    [
        new(1, "Carys Evans", 20),
        new(2, "Aled Jones", 15),
        new(3, "Bethan Morgan", 30)
    ];

    private static IReadOnlyList<CyDataColumn<Row>> Columns() =>
    [
        new CyDataColumn<Row> { Header = "Patient", Value = r => r.Name, Sortable = true, RowHeader = true },
        new CyDataColumn<Row> { Header = "Minutes", Value = r => r.Minutes, Sortable = true, Align = CyColumnAlign.End }
    ];

    private string Markup(Action<ComponentParameterCollectionBuilder<CyDataTable<Row>>>? configure = null) =>
        Render<CyDataTable<Row>>(p =>
        {
            p.Add(c => c.Caption, "Appointments");
            p.Add(c => c.Columns, Columns());
            configure?.Invoke(p);
        }).Markup;

    private string AllStates() => string.Join(
        Environment.NewLine,
        Markup(p => p.Add(c => c.Items, Rows)),
        Markup(p => p.Add(c => c.Items, Rows).Add(c => c.SortKey, "Patient").Add(c => c.SortDescending, true).Add(c => c.PageSize, 2)),
        Markup(p => p
            .Add(c => c.Items, Rows)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Multiple)
            .Add(c => c.RowLabel, r => r.Name)
            .Add(c => c.SelectedItems, [Rows[1]])),
        Markup(p => p
            .Add(c => c.Items, Rows)
            .Add(c => c.SelectionMode, CyDataSelectionMode.Single)
            .Add(c => c.RowLabel, r => r.Name)
            .Add(c => c.MaxHeight, "12rem")),
        Markup(p => p.Add(c => c.Items, new List<Row>())),
        Markup(p => p.Add(c => c.Items, new List<Row>()).Add(c => c.Loading, true)));

    [Fact]
    public async Task Should_Have_No_Violations_In_All_States()
    {
        var result = await ScanMarkupAsync(AllStates());

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var result = await ScanMarkupAsync(AllStates(), theme);

        result.Violations.ShouldBeEmpty();
    }
}
