using Bunit;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Forms;
using CymruBlazor.Components.Theming;
using CymruBlazor.Diagnostics;
using CymruBlazor.Localisation;
using CymruBlazor.Testing;
using CymruBlazor.Themes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Testing;

public sealed class CymruTestContextTests : CymruTestContext
{
    private static readonly IReadOnlyList<CyRuleField> Fields = [new CyRuleField { Key = "name", Label = "Name" }];

    [Fact]
    public void The_Library_Is_Registered()
    {
        Services.GetRequiredService<IComponentIdGenerator>().ShouldNotBeNull();
        Services.GetRequiredService<IThemeService>().ShouldNotBeNull();
        Services.GetRequiredService<ICyLocalizer>().ShouldNotBeNull();
        Services.GetRequiredService<ICyDiagnostics>().Mode.ShouldBe(CyDiagnosticsMode.Strict);
    }

    [Fact]
    public void A_Component_That_Loads_A_Script_Renders_Without_A_Browser()
    {
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, new CyRuleGroup { Children = [new CyRuleCondition()] })
            .Add(c => c.AllowDragReorder, true));

        cut.Markup.ShouldContain("cy-rule__grip");
    }

    [Fact]
    public void A_Component_That_Uses_The_Theme_Service_Renders_And_The_Context_Disposes_Cleanly()
    {
        // Teardown is the test: the real ThemeService only disposes asynchronously, which a synchronous
        // container dispose rejects.
        var cut = Render<CyThemeProvider>(p => p.AddChildContent("<p>Hello</p>"));

        cut.Markup.ShouldContain("Hello");
        cut.Find("[data-theme]").GetAttribute("data-theme").ShouldBe("light");
    }

    [Fact]
    public void A_Rendered_Component_Can_Be_Checked_For_Markup_Problems()
    {
        var cut = Render<CyRuleBuilder>(p => p
            .Add(c => c.Label, "Rule")
            .Add(c => c.Fields, Fields)
            .Add(c => c.Value, new CyRuleGroup
            {
                Children = [new CyRuleCondition { FieldKey = "name", OperatorKey = CyRuleOperators.Eq, Value = "Jo" }]
            }));

        cut.FindMarkupProblems().ShouldBeEmpty();
        cut.ShouldHaveNoMarkupProblems();
    }
}

public sealed class CymruTestContextOptionsTests : CymruTestContext
{
    public CymruTestContextOptionsTests()
        : base(options => options.Diagnostics = CyDiagnosticsMode.Lenient)
    {
    }

    [Fact]
    public void Options_Are_Passed_To_AddCymruBlazor()
    {
        Services.GetRequiredService<ICyDiagnostics>().Mode.ShouldBe(CyDiagnosticsMode.Lenient);
    }
}
