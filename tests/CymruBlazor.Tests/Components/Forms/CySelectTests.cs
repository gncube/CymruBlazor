using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CySelectTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Options_From_ChildContent()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CySelect<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Country")
            .Add(p => p.Value, model.Choice)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
            .Add(p => p.ValueExpression, () => model.Choice)
            .AddChildContent(
                "<option value=\"wales\">Wales</option><option value=\"england\">England</option>"));

        // Assert
        var options = cut.FindAll("option");
        options.Count.ShouldBe(2);
        cut.Find("select").GetAttribute("id").ShouldBe(cut.Find("label").GetAttribute("for"));
    }
}
