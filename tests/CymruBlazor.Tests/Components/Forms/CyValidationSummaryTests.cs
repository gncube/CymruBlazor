using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyValidationSummaryTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Default_Title()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyValidationSummary>(parameters => parameters
            .AddCascadingValue(editContext));

        // Assert
        cut.Markup.ShouldContain("There is a problem");
    }

    [Fact]
    public async Task Should_List_Validation_Messages_From_EditContextAsync()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var messages = new ValidationMessageStore(editContext);
        messages.Add(new FieldIdentifier(model, nameof(TestFormModel.Text)), "Enter your full name");

        // Act
        var cut = Render<CyValidationSummary>(parameters => parameters
            .AddCascadingValue(editContext));

        await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        cut.Markup.ShouldContain("Enter your full name");
        cut.Find("div.cy-validation-summary")
            .GetAttribute("role")
            .ShouldBe("alert");
    }

    [Fact]
    public void Should_Omit_Title_When_Set_To_Null()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyValidationSummary>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Title, null));

        // Assert
        cut.FindAll(".cy-validation-summary__title").Count.ShouldBe(0);
    }
}
