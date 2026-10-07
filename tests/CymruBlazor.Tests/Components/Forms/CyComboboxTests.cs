using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// The WAI-ARIA combobox behaviour of <see cref="CyCombobox{TValue}"/> and <see cref="CyMultiCombobox{TValue}"/> that does
/// not need a browser. The few key defaults prevented in <c>cymru-inputs.js</c> are covered by the browser tests.
/// </summary>
public sealed class CyComboboxTests : FormFieldTestContext
{
    private static readonly CyOption<string>[] Items =
    [
        new("al", "Alpha"),
        new("be", "Beta"),
        new("ga", "Gamma") { Disabled = true },
        new("de", "Delta")
    ];

    private const string ModulePath = "./_content/CymruBlazor/js/cymru-inputs.js";

    private readonly BunitJSModuleInterop _module;

    public CyComboboxTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
        _module.Mode = JSRuntimeMode.Loose;
    }

    private static List<string> L(params string[] items) => [.. items];

    private IRenderedComponent<CyCombobox<string>> RenderSingle(
        TestFormModel model,
        EditContext? editContext = null,
        Action<ComponentParameterCollectionBuilder<CyCombobox<string>>>? configure = null,
        bool withItems = true) =>
        Render<CyCombobox<string>>(parameters =>
        {
            if (editContext is not null)
            {
                parameters.AddCascadingValue(editContext);
            }

            if (withItems)
            {
                parameters.Add(p => p.Items, Items);
            }

            parameters
                .Add(p => p.Label, "Condition")
                .Add(p => p.Value, model.Choice)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => model.Choice = v))
                .Add(p => p.ValueExpression, () => model.Choice);
            configure?.Invoke(parameters);
        });

    private IRenderedComponent<CyMultiCombobox<string>> RenderMulti(
        TestFormModel model,
        Action<ComponentParameterCollectionBuilder<CyMultiCombobox<string>>>? configure = null) =>
        Render<CyMultiCombobox<string>>(parameters =>
        {
            parameters
                .Add(p => p.Label, "Conditions")
                .Add(p => p.Items, Items)
                .Add(p => p.Value, model.Tags)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => model.Tags = v))
                .Add(p => p.ValueExpression, () => model.Tags);
            configure?.Invoke(parameters);
        });

    private static void Press(IRenderedComponent<CyCombobox<string>> cut, string key) =>
        cut.Find("input").TriggerEvent("onkeydown", new KeyboardEventArgs { Key = key });

    private static void Press(IRenderedComponent<CyMultiCombobox<string>> cut, string key) =>
        cut.Find("input").TriggerEvent("onkeydown", new KeyboardEventArgs { Key = key });

    private static string Status(IRenderedComponent<CyCombobox<string>> cut) => cut.Find("[role=status]").TextContent.Trim();

    // ---------------------------------------------------------------- markup and ARIA

    [Fact]
    public void Should_Render_A_Closed_Combobox_With_The_ARIA_1_2_Attributes()
    {
        var cut = RenderSingle(new TestFormModel());

        var input = cut.Find("input");
        input.GetAttribute("role").ShouldBe("combobox");
        input.GetAttribute("aria-autocomplete").ShouldBe("list");
        input.GetAttribute("aria-expanded").ShouldBe("false");
        input.HasAttribute("aria-activedescendant").ShouldBeFalse();
        input.GetAttribute("aria-controls").ShouldBe(cut.Find("[role=listbox]").Id);
        cut.Find("label").GetAttribute("for").ShouldBe(input.Id);
        cut.Find("[role=listbox]").GetAttribute("aria-labelledby").ShouldBe(cut.Find("label").Id);
    }

    [Fact]
    public void Should_Keep_The_Popup_Hidden_While_Closed()
    {
        var cut = RenderSingle(new TestFormModel());

        cut.Find(".cy-combobox__popup").HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void Should_Show_The_Text_Of_The_Initial_Value()
    {
        var cut = RenderSingle(new TestFormModel { Choice = "be" });

        cut.Find("input").GetAttribute("value").ShouldBe("Beta");
    }

    [Fact]
    public void Should_Show_SelectedText_For_A_Value_That_Is_Not_In_Items()
    {
        var cut = RenderSingle(new TestFormModel { Choice = "zz" }, configure: p => p.Add(c => c.SelectedText, "Zeta"));

        cut.Find("input").GetAttribute("value").ShouldBe("Zeta");
    }

    // ---------------------------------------------------------------- typing and status

    [Fact]
    public void Should_Open_And_Filter_As_The_User_Types_And_Announce_The_Count()
    {
        var cut = RenderSingle(new TestFormModel());

        cut.Find("input").Input("a");

        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("true");
        cut.FindAll("[role=option]").Select(o => o.TextContent.Trim()).ShouldBe(L("Alpha", "Beta", "Gamma", "Delta"));
        Status(cut).ShouldBe("4 results available.");

        cut.Find("input").Input("alp");

        cut.FindAll("[role=option]").Count.ShouldBe(1);
        Status(cut).ShouldBe("1 result available.");
    }

    [Fact]
    public void Should_Announce_No_Results_In_The_Live_Region_And_Show_It()
    {
        var cut = RenderSingle(new TestFormModel());

        cut.Find("input").Input("zzz");

        Status(cut).ShouldBe("No results found.");
        cut.Find("[role=status]").GetAttribute("aria-live").ShouldBe("polite");
        cut.Find(".cy-combobox__message").TextContent.ShouldBe("No results found.");
        cut.Find(".cy-combobox__message").GetAttribute("aria-hidden").ShouldBe("true");
        cut.FindAll("[role=option]").Count.ShouldBe(0);
        cut.Find("[role=listbox]").HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void Should_Use_Localised_Phrases()
    {
        var cut = RenderSingle(new TestFormModel(), configure: p =>
            p.Add(c => c.Text, new CyComboboxText { NoResults = "Dim canlyniadau." }));

        cut.Find("input").Input("zzz");

        Status(cut).ShouldBe("Dim canlyniadau.");
    }

    [Fact]
    public void Should_Cap_The_Rendered_Options_And_Say_So()
    {
        var cut = RenderSingle(new TestFormModel(), configure: p => p.Add(c => c.MaxResults, 2));

        cut.Find("input").Input("");

        cut.FindAll("[role=option]").Count.ShouldBe(2);
        Status(cut).ShouldBe("Showing the first 2 of 4 results. Keep typing to narrow the list.");
    }

    // ---------------------------------------------------------------- keyboard

    [Fact]
    public void Should_Open_On_ArrowDown_And_Move_The_Active_Option_Without_Moving_Focus()
    {
        var cut = RenderSingle(new TestFormModel());

        Press(cut, "ArrowDown");

        var input = cut.Find("input");
        input.GetAttribute("aria-expanded").ShouldBe("true");
        var options = cut.FindAll("[role=option]");
        input.GetAttribute("aria-activedescendant").ShouldBe(options[0].Id);

        Press(cut, "ArrowDown");

        cut.Find("input").GetAttribute("aria-activedescendant").ShouldBe(cut.FindAll("[role=option]")[1].Id);
    }

    [Fact]
    public void Should_Skip_Disabled_Options_When_Moving()
    {
        var cut = RenderSingle(new TestFormModel());

        Press(cut, "ArrowDown");
        Press(cut, "ArrowDown");
        Press(cut, "ArrowDown");

        // Alpha, Beta, (Gamma is disabled) Delta.
        var options = cut.FindAll("[role=option]");
        cut.Find("input").GetAttribute("aria-activedescendant").ShouldBe(options[3].Id);
        options[2].GetAttribute("aria-disabled").ShouldBe("true");
    }

    [Fact]
    public void Should_Return_To_The_Input_When_ArrowUp_Leaves_The_First_Option()
    {
        var cut = RenderSingle(new TestFormModel());

        Press(cut, "ArrowDown");
        Press(cut, "ArrowUp");

        cut.Find("input").HasAttribute("aria-activedescendant").ShouldBeFalse();
    }

    [Fact]
    public void Should_Open_On_The_Last_Option_With_ArrowUp()
    {
        var cut = RenderSingle(new TestFormModel());

        Press(cut, "ArrowUp");

        cut.Find("input").GetAttribute("aria-activedescendant").ShouldBe(cut.FindAll("[role=option]")[3].Id);
    }

    [Fact]
    public void Should_Choose_The_Active_Option_On_Enter_And_Close()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        Press(cut, "ArrowDown");
        Press(cut, "ArrowDown");
        Press(cut, "Enter");

        model.Choice.ShouldBe("be");
        cut.Find("input").GetAttribute("value").ShouldBe("Beta");
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Not_Choose_Anything_On_Enter_When_No_Option_Is_Active()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        cut.Find("input").Input("a");
        Press(cut, "Enter");

        model.Choice.ShouldBe(string.Empty);
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Should_Close_On_Escape_And_Clear_On_A_Second_Escape()
    {
        var model = new TestFormModel { Choice = "be" };
        var cut = RenderSingle(model);

        Press(cut, "ArrowDown");
        Press(cut, "Escape");

        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
        model.Choice.ShouldBe("be");

        Press(cut, "Escape");

        model.Choice.ShouldBe(string.Empty);
        cut.Find("input").GetAttribute("value").ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Close_Without_Choosing_On_Tab()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        Press(cut, "ArrowDown");
        Press(cut, "Tab");

        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
        model.Choice.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Open_On_The_Selected_Option()
    {
        var cut = RenderSingle(new TestFormModel { Choice = "de" });

        Press(cut, "ArrowDown");

        // Every option is offered, not only the one matching the text, and the selected one is active.
        var options = cut.FindAll("[role=option]");
        options.Count.ShouldBe(4);
        cut.Find("input").GetAttribute("aria-activedescendant").ShouldBe(options[3].Id);
        options[3].GetAttribute("aria-selected").ShouldBe("true");
    }

    // ---------------------------------------------------------------- pointer and blur

    [Fact]
    public void Should_Choose_An_Option_On_Click()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        cut.Find("input").Input("de");
        cut.Find("[role=option]").Click();

        model.Choice.ShouldBe("de");
        cut.Find("input").GetAttribute("value").ShouldBe("Delta");
    }

    [Fact]
    public void Should_Ignore_A_Click_On_A_Disabled_Option()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        cut.Find("input").Input("gam");
        cut.Find("[role=option]").Click();

        model.Choice.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Put_Back_The_Chosen_Text_On_Blur_When_The_Typed_Text_Is_Not_A_Choice()
    {
        var model = new TestFormModel { Choice = "be" };
        var cut = RenderSingle(model);

        cut.Find("input").Input("Alp");
        cut.Find("input").TriggerEvent("onblur", new FocusEventArgs());

        model.Choice.ShouldBe("be");
        cut.Find("input").GetAttribute("value").ShouldBe("Beta");
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Should_Clear_The_Value_On_Blur_When_The_Text_Was_Emptied()
    {
        var model = new TestFormModel { Choice = "be" };
        var cut = RenderSingle(model);

        cut.Find("input").Input("");
        cut.Find("input").TriggerEvent("onblur", new FocusEventArgs());

        model.Choice.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Clear_With_The_Clear_Button_Only_While_Something_Is_Chosen()
    {
        var model = new TestFormModel { Choice = "be" };
        var cut = RenderSingle(model);

        cut.Find("button.cy-combobox__clear").GetAttribute("aria-label").ShouldBe("Clear Condition");
        cut.Find("button.cy-combobox__clear").Click();

        model.Choice.ShouldBe(string.Empty);
        cut.FindAll("button.cy-combobox__clear").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Hide_The_Clear_Button_When_AllowClear_Is_False()
    {
        var cut = RenderSingle(new TestFormModel { Choice = "be" }, configure: p => p.Add(c => c.AllowClear, false));

        cut.FindAll("button.cy-combobox__clear").Count.ShouldBe(0);
    }

    // ---------------------------------------------------------------- disabled, read-only, validation

    [Fact]
    public void Should_Not_Open_When_Disabled_Or_ReadOnly()
    {
        var disabled = RenderSingle(new TestFormModel(), configure: p => p.Add(c => c.Disabled, true));
        disabled.Find("input").HasAttribute("disabled").ShouldBeTrue();

        var readOnly = RenderSingle(new TestFormModel(), configure: p => p.Add(c => c.ReadOnly, true));
        Press(readOnly, "ArrowDown");

        readOnly.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
        readOnly.Find("input").HasAttribute("readonly").ShouldBeTrue();
    }

    [Fact]
    public void Should_Show_Validation_Errors_And_Link_Them_To_The_Input()
    {
        var model = new TestFormModel();
        var context = CreateEditContext(model);
        var store = new ValidationMessageStore(context);
        store.Add(context.Field(nameof(TestFormModel.Choice)), "Choose a condition");
        context.NotifyValidationStateChanged();

        var cut = RenderSingle(model, context, p => p.Add(c => c.HintText, "Start typing"));

        var input = cut.Find("input");
        input.GetAttribute("aria-invalid").ShouldBe("true");
        var error = cut.Find(".cy-field__error");
        error.TextContent.Trim().ShouldBe("Choose a condition");
        input.GetAttribute("aria-describedby")!.ShouldContain(error.Id!);
        input.GetAttribute("aria-describedby")!.ShouldContain(cut.Find(".cy-field__hint").Id!);
    }

    [Fact]
    public void Should_Work_Without_An_EditContext()
    {
        var model = new TestFormModel();
        var cut = RenderSingle(model);

        cut.Find("input").Input("bet");
        cut.Find("[role=option]").Click();

        model.Choice.ShouldBe("be");
    }

    // ---------------------------------------------------------------- parameters

    [Fact]
    public void Should_Reject_Items_And_ItemsProvider_Together()
    {
        Should.Throw<InvalidOperationException>(() => RenderSingle(new TestFormModel(), configure: p =>
            p.Add(c => c.ItemsProvider, (_, _) => Task.FromResult<IReadOnlyList<CyOption<string>>>([]))));
    }

    [Theory]
    [InlineData("calc(10px)")]
    [InlineData("10rem; color: red")]
    [InlineData("url(x)")]
    public void Should_Reject_An_Unsafe_MaxListHeight(string height)
    {
        Should.Throw<InvalidOperationException>(() =>
            RenderSingle(new TestFormModel(), configure: p => p.Add(c => c.MaxListHeight, height)));
    }

    [Fact]
    public void Should_Write_A_Valid_MaxListHeight_As_A_Custom_Property()
    {
        var cut = RenderSingle(new TestFormModel(), configure: p => p.Add(c => c.MaxListHeight, "12rem"));

        cut.Find(".cy-combobox").GetAttribute("style")!.ShouldContain("--cy-combobox-max-height: 12rem");
    }

    [Fact]
    public void Should_Support_A_Nullable_Value_Type_Where_Null_Means_Nothing_Chosen()
    {
        var model = new TestFormModel();
        var cut = Render<CyCombobox<int?>>(p => p
            .Add(c => c.Label, "Number")
            .Add(c => c.Items, new List<CyOption<int?>> { new(1, "One"), new(2, "Two") })
            .Add(c => c.Value, model.Count)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<int?>(this, v => model.Count = v))
            .Add(c => c.ValueExpression, () => model.Count));

        cut.Find("input").GetAttribute("value").ShouldBe(string.Empty);

        cut.Find("input").Input("tw");
        cut.Find("[role=option]").Click();

        model.Count.ShouldBe(2);
    }

    // ---------------------------------------------------------------- async provider

    [Fact]
    public void Should_Call_The_Provider_With_The_Typed_Text_And_Show_The_Results()
    {
        var queries = new List<string>();
        var cut = RenderSingle(new TestFormModel(), configure: p => p
            .Add(c => c.DebounceMilliseconds, 0)
            .Add(c => c.ItemsProvider, (request, _) =>
            {
                queries.Add(request.Query);
                return Task.FromResult<IReadOnlyList<CyOption<string>>>([new("x1", "Result for " + request.Query)]);
            }), withItems: false);

        cut.Find("input").Input("hea");

        cut.WaitForAssertion(() => cut.FindAll("[role=option]").Count.ShouldBe(1));
        queries.ShouldBe(L("hea"));
        cut.Find("[role=option]").TextContent.Trim().ShouldBe("Result for hea");
    }

    [Fact]
    public void Should_Not_Call_The_Provider_Before_MinSearchLength_Is_Reached()
    {
        var calls = 0;
        var cut = RenderSingle(new TestFormModel(), configure: p => p
            .Add(c => c.DebounceMilliseconds, 0)
            .Add(c => c.MinSearchLength, 3)
            .Add(c => c.ItemsProvider, (_, _) =>
            {
                calls++;
                return Task.FromResult<IReadOnlyList<CyOption<string>>>([]);
            }), withItems: false);

        cut.Find("input").Input("ab");

        calls.ShouldBe(0);
        Status(cut).ShouldBe("Type 3 or more characters to see results.");
    }

    [Fact]
    public void Should_Announce_Loading_Then_Discard_A_Stale_Response()
    {
        var pending = new List<(string Query, TaskCompletionSource<IReadOnlyList<CyOption<string>>> Source)>();
        var cut = RenderSingle(new TestFormModel(), configure: p => p
            .Add(c => c.DebounceMilliseconds, 0)
            .Add(c => c.ItemsProvider, (request, _) =>
            {
                var source = new TaskCompletionSource<IReadOnlyList<CyOption<string>>>();
                pending.Add((request.Query, source));
                return source.Task;
            }), withItems: false);

        cut.Find("input").Input("a");
        Status(cut).ShouldBe("Loading results.");
        cut.Find("input").Input("ab");
        pending.Count.ShouldBe(2);

        // The newer request answers first, then the older one arrives late and must be ignored.
        pending[1].Source.SetResult([new("ab", "Newer")]);
        cut.WaitForAssertion(() => cut.Find("[role=option]").TextContent.Trim().ShouldBe("Newer"));

        pending[0].Source.SetResult([new("a", "Older")]);
        Thread.Sleep(50);

        cut.Find("[role=option]").TextContent.Trim().ShouldBe("Newer");
    }

    [Fact]
    public void Should_Pass_A_Token_That_Is_Cancelled_When_The_User_Types_Again()
    {
        var tokens = new List<CancellationToken>();
        var cut = RenderSingle(new TestFormModel(), configure: p => p
            .Add(c => c.DebounceMilliseconds, 0)
            .Add(c => c.ItemsProvider, (_, token) =>
            {
                tokens.Add(token);
                return new TaskCompletionSource<IReadOnlyList<CyOption<string>>>().Task;
            }), withItems: false);

        cut.Find("input").Input("a");
        cut.Find("input").Input("ab");

        tokens.Count.ShouldBe(2);
        tokens[0].IsCancellationRequested.ShouldBeTrue();
        tokens[1].IsCancellationRequested.ShouldBeFalse();
    }

    [Fact]
    public void Should_Show_An_Error_Message_When_The_Provider_Throws()
    {
        var cut = RenderSingle(new TestFormModel(), configure: p => p
            .Add(c => c.DebounceMilliseconds, 0)
            .Add(c => c.ItemsProvider, (_, _) => throw new InvalidOperationException("boom")), withItems: false);

        cut.Find("input").Input("a");

        cut.WaitForAssertion(() => Status(cut).ShouldBe("Could not load results. Try again."));
        cut.Find(".cy-combobox__message").TextContent.ShouldBe("Could not load results. Try again.");
    }

    // ---------------------------------------------------------------- script and disposal

    [Fact]
    public void Should_Attach_The_Script_Module_On_First_Render()
    {
        _ = RenderSingle(new TestFormModel());

        _module.VerifyInvoke("attachCombobox");
    }

    [Fact]
    public void Should_Detach_The_Script_When_Removed_From_The_Tree()
    {
        _module.Setup<int>("attachCombobox", _ => true).SetResult(7);
        var model = new TestFormModel();

        var host = Render<RemovableHost>(p => p.Add(c => c.ChildContent, b =>
        {
            b.OpenComponent<CyCombobox<string>>(0);
            b.AddComponentParameter(1, nameof(CyCombobox<string>.Label), "Condition");
            b.AddComponentParameter(2, nameof(CyCombobox<string>.Items), (IReadOnlyList<CyOption<string>>)Items);
            b.AddComponentParameter(3, nameof(CyCombobox<string>.Value), model.Choice);
            b.AddComponentParameter(4, nameof(CyCombobox<string>.ValueExpression), (System.Linq.Expressions.Expression<Func<string>>)(() => model.Choice));
            b.CloseComponent();
        }));

        host.Render(p => p.Add(c => c.Show, false));

        host.WaitForAssertion(() => _module.VerifyInvoke("detachCombobox"));
    }

    [Fact]
    public void Should_Still_Work_When_The_Script_Module_Cannot_Be_Loaded()
    {
        _module.Setup<int>("attachCombobox", _ => true).SetException(new JSException("no module"));
        var model = new TestFormModel();

        var cut = RenderSingle(model);
        cut.Find("input").Input("bet");
        cut.Find("[role=option]").Click();

        model.Choice.ShouldBe("be");
    }

    // ================================================================ multiple

    [Fact]
    public void Multiple_Should_Mark_The_Listbox_Multiselectable_And_Render_Chips_For_The_Value()
    {
        var cut = RenderMulti(new TestFormModel { Tags = ["al", "de"] });

        cut.Find("[role=listbox]").GetAttribute("aria-multiselectable").ShouldBe("true");
        cut.FindAll(".cy-combobox__chip-text").Select(c => c.TextContent).ShouldBe(L("Alpha", "Delta"));
        cut.Find("ul.cy-combobox__chips").GetAttribute("aria-label").ShouldBe("Selected: Conditions");
        cut.FindAll(".cy-combobox__chip-remove")[0].GetAttribute("aria-label").ShouldBe("Remove Alpha");
    }

    [Fact]
    public void Multiple_Should_Add_A_Value_Keep_The_List_Open_And_Announce_It()
    {
        var model = new TestFormModel();
        var cut = RenderMulti(model);

        Press(cut, "ArrowDown");
        Press(cut, "Enter");

        model.Tags.ShouldBe(L("al"));
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("true");
        cut.Find("[role=status]").TextContent.Trim().ShouldBe("Alpha selected.");
        cut.FindAll("[role=option]")[0].GetAttribute("aria-selected").ShouldBe("true");
    }

    [Fact]
    public void Multiple_Should_Keep_The_Order_In_Which_Values_Were_Chosen()
    {
        var model = new TestFormModel();
        var cut = RenderMulti(model);

        cut.Find("input").Input("del");
        cut.Find("[role=option]").Click();
        cut.Find("input").Input("alp");
        cut.Find("[role=option]").Click();

        model.Tags.ShouldBe(L("de", "al"));
        cut.FindAll(".cy-combobox__chip-text").Select(c => c.TextContent).ShouldBe(L("Delta", "Alpha"));
    }

    [Fact]
    public void Multiple_Should_Remove_A_Value_When_A_Selected_Option_Is_Chosen_Again()
    {
        var model = new TestFormModel { Tags = ["al"] };
        var cut = RenderMulti(model);

        Press(cut, "ArrowDown");
        cut.FindAll("[role=option]")[0].Click();

        model.Tags.ShouldBeEmpty();
        cut.Find("[role=status]").TextContent.Trim().ShouldBe("Alpha removed.");
    }

    [Fact]
    public void Multiple_Should_Remove_A_Value_With_Its_Chip_Button()
    {
        var model = new TestFormModel { Tags = ["al", "be"] };
        var cut = RenderMulti(model);

        cut.FindAll(".cy-combobox__chip-remove")[0].Click();

        model.Tags.ShouldBe(L("be"));
        cut.FindAll(".cy-combobox__chip-text").Select(c => c.TextContent).ShouldBe(L("Beta"));
        cut.Find("[role=status]").TextContent.Trim().ShouldBe("Alpha removed.");
    }

    [Fact]
    public void Multiple_Should_Use_SelectedOptions_For_Values_That_Are_Not_In_Items()
    {
        var cut = RenderMulti(new TestFormModel { Tags = ["zz", "qq"] }, p =>
            p.Add(c => c.SelectedOptions, new List<CyOption<string>> { new("zz", "Zeta") }));

        // A value with no known text falls back to its string form.
        cut.FindAll(".cy-combobox__chip-text").Select(c => c.TextContent).ShouldBe(L("Zeta", "qq"));
    }

    [Fact]
    public void Multiple_Should_Close_After_Each_Choice_When_CloseOnSelect_Is_True()
    {
        var cut = RenderMulti(new TestFormModel(), p => p.Add(c => c.CloseOnSelect, true));

        Press(cut, "ArrowDown");
        Press(cut, "Enter");

        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Multiple_Should_Clear_Typed_Text_On_Blur()
    {
        var cut = RenderMulti(new TestFormModel());

        cut.Find("input").Input("xyz");
        cut.Find("input").TriggerEvent("onblur", new FocusEventArgs());

        cut.Find("input").GetAttribute("value").ShouldBe(string.Empty);
    }

    [Fact]
    public void Multiple_Should_Not_Offer_Remove_Buttons_When_Disabled()
    {
        var cut = RenderMulti(new TestFormModel { Tags = ["al"] }, p => p.Add(c => c.Disabled, true));

        cut.FindAll(".cy-combobox__chip-remove").Count.ShouldBe(0);
    }
}
