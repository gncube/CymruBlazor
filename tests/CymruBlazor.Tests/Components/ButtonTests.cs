using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Button;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Covers the Button component's current, intentionally minimal surface
/// area. Button does not yet support variants, sizes, disabled state, or
/// @onclick/attribute pass-through - see plan/ for the tracked follow-up
/// to bring it in line with the other form controls before v1.0.0.
/// </summary>
public sealed class ButtonTests : TestContextBase
{
    [Fact]
    public void Should_Render_As_A_Button_Element_With_Base_Class()
    {
        // Act
        var cut = Render<CyButton>();

        // Assert
        var element = cut.Find("button");
        element.ClassList.ShouldContain("cymru-btn");
    }

    [Fact]
    public void Should_Render_ChildContent()
    {
        // Act
        var cut = Render<CyButton>(parameters => parameters
            .AddChildContent("Save changes"));

        // Assert
        cut.Find("button").TextContent.ShouldContain("Save changes");
    }
}
