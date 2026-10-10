using Bunit;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// <see cref="CyRuleBuilder"/> in the shapes a service will show: empty, a nested rule using every value editor,
/// incomplete conditions with their messages, limits reached, disabled and read-only. Rendered into one markup per
/// theme. The page has one h1; the builder adds no headings.
/// </summary>
public sealed class CyRuleBuilderAccessibilityTests : AxeTestBase
{
    public CyRuleBuilderAccessibilityTests()
    {
        // The combobox and the row menus load small scripts on first render; the scan renders markup only.
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-inputs.js");
        JSInterop.SetupModule("./_content/CymruBlazor/js/cymru-editing.js");
        JSInterop.SetupModule("./_content/CymruBlazor/js/sortable-list.js");
    }

    private static readonly IReadOnlyList<CyRuleField> Fields =
    [
        new CyRuleField { Key = "name", Label = "Name" },
        new CyRuleField { Key = "age", Label = "Age", Type = CyRuleFieldType.Number, Min = 0, Max = 120, Unit = "years" },
        new CyRuleField { Key = "dob", Label = "Date of birth", Type = CyRuleFieldType.Date },
        new CyRuleField
        {
            Key = "status",
            Label = "Status",
            Type = CyRuleFieldType.Choice,
            Options = [new CyOption<string>("a", "Active"), new CyOption<string>("p", "Pending")]
        },
        new CyRuleField
        {
            Key = "dx",
            Label = "Diagnosis",
            Type = CyRuleFieldType.Coded,
            ItemsProvider = (request, _) => Task.FromResult<IReadOnlyList<CyOption<string>>>(
                [new CyOption<string>("x", "Asthma")])
        },
        new CyRuleField { Key = "smoker", Label = "Smoker", Type = CyRuleFieldType.Boolean }
    ];

    private static CyRuleGroup FullRule() => new()
    {
        Children =
        [
            new CyRuleCondition { FieldKey = "age", OperatorKey = CyRuleOperators.Between, Value = 18m, Value2 = 65m },
            new CyRuleCondition { FieldKey = "status", OperatorKey = CyRuleOperators.Eq, Value = "a", ValueLabel = "Active" },
            new CyRuleGroup
            {
                Combinator = CyRuleCombinator.Or,
                Not = true,
                Children =
                [
                    new CyRuleCondition { FieldKey = "dob", OperatorKey = CyRuleOperators.Before, Value = new DateOnly(2000, 1, 1) },
                    new CyRuleCondition { FieldKey = "dx", OperatorKey = CyRuleOperators.Eq, Value = "x", ValueLabel = "Asthma" },
                    new CyRuleCondition { FieldKey = "smoker", OperatorKey = CyRuleOperators.IsTrue },
                    new CyRuleCondition { FieldKey = "name", OperatorKey = CyRuleOperators.Contains, Value = "Jo" }
                ]
            }
        ]
    };

    private string Builder(
        CyRuleGroup? rule,
        bool showIssues = false,
        bool disabled = false,
        bool readOnly = false,
        CyRuleLimits? limits = null,
        bool drag = false) =>
        Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Eligibility rule")
            .Add(c => c.AllowDragReorder, drag)
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, rule)
            .Add(c => c.ShowIssues, showIssues)
            .Add(c => c.Disabled, disabled)
            .Add(c => c.ReadOnly, readOnly)
            .Add(c => c.Limits, limits)).Markup;

    private string BuildMarkup() => string.Join(
        Environment.NewLine,
        "<h1>Rule builder</h1>",
        Builder(null),
        Builder(FullRule()),
        Builder(
            new CyRuleGroup { Children = [new CyRuleCondition(), new CyRuleCondition { FieldKey = "age" }] },
            showIssues: true),
        Builder(FullRule(), limits: new CyRuleLimits(MaxDepth: 2, MaxConditions: 6, MinConditions: 6)),
        Builder(FullRule(), disabled: true),
        Builder(FullRule(), readOnly: true));

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_For_All_Rule_Builder_Shapes(string theme)
    {
        var result = await ScanMarkupAsync(BuildMarkup(), theme);

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_With_Drag_Grips_And_An_Empty_Drop_Zone(string theme)
    {
        var withEmptyGroup = new CyRuleGroup
        {
            Children =
            [
                new CyRuleCondition { FieldKey = "name", OperatorKey = CyRuleOperators.Contains, Value = "Jo" },
                new CyRuleGroup(),
                new CyRuleGroup { Children = [new CyRuleCondition { FieldKey = "smoker", OperatorKey = CyRuleOperators.IsTrue }] }
            ]
        };

        var markup = string.Join(
            Environment.NewLine,
            "<h1>Draggable rule builder</h1>",
            Builder(FullRule(), drag: true),
            Builder(withEmptyGroup, drag: true),
            Builder(FullRule(), disabled: true, drag: true),
            Builder(FullRule(), readOnly: true, drag: true));

        markup.ShouldContain("cy-rule__grip");
        markup.ShouldContain("cy-rule__drop-zone");

        var result = await ScanMarkupAsync(markup, theme);

        result.Violations.ShouldBeEmpty();
    }

    private static RenderFragment<CyRuleEditorContext> LabelledEditor => context => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "text");
        builder.AddAttribute(2, "id", context.ValueId);
        builder.AddAttribute(3, "aria-label", context.Label);
        builder.AddAttribute(4, "aria-invalid", context.Invalid ? "true" : "false");
        builder.AddAttribute(5, "disabled", context.Disabled);
        builder.CloseElement();
    };

    private static RenderFragment<CyRuleEditorContext> ThrowingEditor => context => builder => throw new InvalidOperationException("boom");

    private string CustomEditorBuilder(string label, RenderFragment<CyRuleEditorContext> editor, bool showIssues = false) =>
        Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, label)
            .Add(c => c.Fields, new[] { new CyRuleField { Key = "x", Label = "Custom field", ValueEditor = editor } })
            .Add(c => c.Value, new CyRuleGroup
            {
                Children = [new CyRuleCondition { FieldKey = "x", OperatorKey = CyRuleOperators.Eq }]
            })
            .Add(c => c.ShowIssues, showIssues)).Markup;

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_With_Custom_Value_Editors_Including_A_Failed_One(string theme)
    {
        var markup = string.Join(
            Environment.NewLine,
            "<h1>Custom value editors</h1>",
            CustomEditorBuilder("Custom editor rule", LabelledEditor),
            CustomEditorBuilder("Invalid custom editor rule", LabelledEditor, showIssues: true),
            CustomEditorBuilder("Failed editor rule", ThrowingEditor));

        markup.ShouldContain("cy-rule__control--custom");
        markup.ShouldContain("This value editor could not be shown.");

        var result = await ScanMarkupAsync(markup, theme);

        result.Violations.ShouldBeEmpty();
    }
}
