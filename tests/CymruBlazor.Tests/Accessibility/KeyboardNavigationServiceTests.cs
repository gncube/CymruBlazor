using Microsoft.AspNetCore.Components.Web;

using Shouldly;

using Xunit;

using CymruBlazor.Accessibility.Focus;

namespace CymruBlazor.Tests.Accessibility;

public class KeyboardNavigationServiceTests
{
    private readonly KeyboardNavigationService _service = new();

    [Fact]
    public void ArrowLeft_Returns_Left()
    {
        // Arrange
        var args = new KeyboardEventArgs
        {
            Key = "ArrowLeft"
        };

        // Act
        var result = _service.GetNavigation(args);

        // Assert
        result.NavigationMode.ShouldBe(FocusNavigationMode.Left);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void ArrowRight_Returns_Right()
    {
        var args = new KeyboardEventArgs
        {
            Key = "ArrowRight"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Right);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Home_Returns_First()
    {
        var args = new KeyboardEventArgs
        {
            Key = "Home"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.First);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void End_Returns_Last()
    {
        var args = new KeyboardEventArgs
        {
            Key = "End"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Last);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void ShiftTab_Returns_Previous()
    {
        var args = new KeyboardEventArgs
        {
            Key = "Tab",
            ShiftKey = true
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Previous);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Tab_Returns_Next()
    {
        var args = new KeyboardEventArgs
        {
            Key = "Tab"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Next);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Enter_Returns_Activate()
    {
        var args = new KeyboardEventArgs
        {
            Key = "Enter"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Activate);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Space_Returns_Activate()
    {
        var args = new KeyboardEventArgs
        {
            Key = " "
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Activate);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Escape_Returns_Cancel()
    {
        var args = new KeyboardEventArgs
        {
            Key = "Escape"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.Cancel);
        result.PreventDefault.ShouldBeTrue();
    }

    [Fact]
    public void Unknown_Key_Returns_None_Without_PreventDefault()
    {
        var args = new KeyboardEventArgs
        {
            Key = "F13"
        };

        var result = _service.GetNavigation(args);

        result.NavigationMode.ShouldBe(FocusNavigationMode.None);
        result.PreventDefault.ShouldBeFalse();
    }

    [Theory]
    [InlineData("ArrowLeft", FocusNavigationMode.None)]
    [InlineData("ArrowRight", FocusNavigationMode.None)]
    [InlineData("ArrowUp", FocusNavigationMode.None)]
    [InlineData("ArrowDown", FocusNavigationMode.None)]
    public void Disabled_Arrow_Keys_Return_None(
        string key,
        FocusNavigationMode expected)
    {
        // Arrange
        var options = new KeyboardNavigationOptions
        {
            EnableArrowKeys = false
        };

        var args = new KeyboardEventArgs
        {
            Key = key
        };

        // Act
        var result = _service.GetNavigation(args, options);

        // Assert
        result.NavigationMode.ShouldBe(expected);
        result.PreventDefault.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("End")]
    public void Disabled_Home_End_Return_None(string key)
    {
        var options = new KeyboardNavigationOptions
        {
            EnableHomeEnd = false
        };

        var args = new KeyboardEventArgs
        {
            Key = key
        };

        var result = _service.GetNavigation(args, options);

        result.NavigationMode.ShouldBe(FocusNavigationMode.None);
        result.PreventDefault.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void Disabled_Activation_Return_None(string key)
    {
        var options = new KeyboardNavigationOptions
        {
            EnableActivation = false
        };

        var args = new KeyboardEventArgs
        {
            Key = key
        };

        var result = _service.GetNavigation(args, options);

        result.NavigationMode.ShouldBe(FocusNavigationMode.None);
        result.PreventDefault.ShouldBeFalse();
    }

    [Fact]
    public void Disabled_Escape_Returns_None()
    {
        var options = new KeyboardNavigationOptions
        {
            EnableEscape = false
        };

        var args = new KeyboardEventArgs
        {
            Key = "Escape"
        };

        var result = _service.GetNavigation(args, options);

        result.NavigationMode.ShouldBe(FocusNavigationMode.None);
        result.PreventDefault.ShouldBeFalse();
    }
}
