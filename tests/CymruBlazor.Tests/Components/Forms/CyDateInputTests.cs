using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class CyDateInputTests : FormFieldTestContext
{
    [Fact]
    public void Should_Render_Fieldset_With_Legend_And_Three_Segments()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Assert
        cut.Find("legend").TextContent.ShouldContain("Date of birth");
        cut.Find("fieldset").GetAttribute("role").ShouldBe("group");

        var labels = cut.FindAll(".cy-date-input__label").Select(l => l.TextContent).ToList();
        labels.ShouldBe(["Day", "Month", "Year"]);

        cut.FindAll(".cy-date-input__input").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Use_Custom_Segment_Labels()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Dyddiad geni")
            .Add(p => p.DayLabel, "Dydd")
            .Add(p => p.MonthLabel, "Mis")
            .Add(p => p.YearLabel, "Blwyddyn")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Assert
        var labels = cut.FindAll(".cy-date-input__label").Select(l => l.TextContent).ToList();
        labels.ShouldBe(["Dydd", "Mis", "Blwyddyn"]);
    }

    [Fact]
    public void Should_Compose_DateOnly_From_Three_Segments_On_Change()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        DateOnly? composed = null;

        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => composed = v))
            .Add(p => p.ValueExpression, () => model.Date));

        cut.FindAll(".cy-date-input__input")[0].Change("15");
        cut.FindAll(".cy-date-input__input")[1].Change("3");
        cut.FindAll(".cy-date-input__input")[2].Change("1990");

        composed.ShouldBe(new DateOnly(1990, 3, 15));
    }

    [Fact]
    public void Should_Clear_Value_When_A_Valid_Date_Is_Edited_Into_An_Invalid_One()
    {
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        DateOnly? composed = null;

        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => composed = v))
            .Add(p => p.ValueExpression, () => model.Date));

        cut.FindAll(".cy-date-input__input")[0].Change("15");
        cut.FindAll(".cy-date-input__input")[1].Change("2");
        cut.FindAll(".cy-date-input__input")[2].Change("1990");

        composed.ShouldBe(new DateOnly(1990, 2, 15));

        cut.FindAll(".cy-date-input__input")[0].Change("31");

        composed.ShouldBeNull();
    }



    [Fact]
    public void Should_Populate_Segments_From_An_Externally_Supplied_Value()
    {
        // Arrange
        var model = new TestFormModel { Date = new DateOnly(2001, 12, 25) };
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Assert
        var inputs = cut.FindAll(".cy-date-input__input");
        inputs[0].GetAttribute("value").ShouldBe("25");
        inputs[1].GetAttribute("value").ShouldBe("12");
        inputs[2].GetAttribute("value").ShouldBe("2001");
    }

    [Fact]
    public void Should_Set_Date_Of_Birth_Autocomplete_Tokens_When_Requested()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);

        // Act
        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.AutocompleteDateOfBirth, true)
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Assert
        var inputs = cut.FindAll(".cy-date-input__input");
        inputs[0].GetAttribute("autocomplete").ShouldBe("bday-day");
        inputs[1].GetAttribute("autocomplete").ShouldBe("bday-month");
        inputs[2].GetAttribute("autocomplete").ShouldBe("bday-year");
    }

    [Fact]
    public async Task Should_Show_Validation_Error_On_Fieldset()
    {
        // Arrange
        var model = new TestFormModel();
        var editContext = CreateEditContext(model);
        var messages = new ValidationMessageStore(editContext);

        var cut = Render<CyDateInput>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Label, "Date of birth")
            .Add(p => p.Value, model.Date)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DateOnly?>(this, v => model.Date = v))
            .Add(p => p.ValueExpression, () => model.Date));

        // Act
        messages.Add(new FieldIdentifier(model, nameof(TestFormModel.Date)), "Enter your date of birth");

        await cut.InvokeAsync(editContext.NotifyValidationStateChanged);

        // Assert
        cut.FindAll(".cy-date-input__input")
            .ShouldAllBe(input => input.GetAttribute("aria-invalid") == "true");
        cut.Find(".cy-field__error").TextContent.ShouldContain("Enter your date of birth");
    }
}
