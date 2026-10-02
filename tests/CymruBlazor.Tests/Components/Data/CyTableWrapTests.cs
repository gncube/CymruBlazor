using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Data;

namespace CymruBlazor.Tests.Components.Data;

public sealed class CyTableWrapTests : TestContextBase
{
    [Fact]
    public void Default_Has_No_Wrap_Class()
    {
        var cut = Render<CyTable>(p => p.Add(x => x.Caption, "Patients").AddChildContent("<tbody></tbody>"));

        cut.Find("table").ClassList.ShouldNotContain("cy-table--wrap");
    }

    [Fact]
    public void Wrap_Adds_The_Wrap_Modifier()
    {
        var cut = Render<CyTable>(p => p
            .Add(x => x.Caption, "Patients")
            .Add(x => x.Wrap, true)
            .AddChildContent("<tbody></tbody>"));

        cut.Find("table").ClassList.ShouldContain("cy-table--wrap");
    }
}
