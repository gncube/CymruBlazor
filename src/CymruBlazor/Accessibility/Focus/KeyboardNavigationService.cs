using Microsoft.AspNetCore.Components.Web;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Default keyboard navigation implementation.
/// </summary>
[Obsolete("Nothing in CymruBlazor consumes this; it will be removed in 2.0.0.", error: false)]
public sealed class KeyboardNavigationService
    : IKeyboardNavigationService
{
    public KeyboardNavigationResult GetNavigation(
        KeyboardEventArgs args,
        KeyboardNavigationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        options ??= new KeyboardNavigationOptions();

        return args.Key switch
        {
            "ArrowLeft" when options.EnableArrowKeys
                => new(FocusNavigationMode.Left),

            "ArrowRight" when options.EnableArrowKeys
                => new(FocusNavigationMode.Right),

            "ArrowUp" when options.EnableArrowKeys
                => new(FocusNavigationMode.Up),

            "ArrowDown" when options.EnableArrowKeys
                => new(FocusNavigationMode.Down),

            "Home" when options.EnableHomeEnd
                => new(FocusNavigationMode.First),

            "End" when options.EnableHomeEnd
                => new(FocusNavigationMode.Last),

            "Tab" when options.EnableTabNavigation && args.ShiftKey
                => new(FocusNavigationMode.Previous),

            "Tab" when options.EnableTabNavigation
                => new(FocusNavigationMode.Next),

            "Enter" when options.EnableActivation
                => new(FocusNavigationMode.Activate),

            " " when options.EnableActivation
                => new(FocusNavigationMode.Activate),

            "Escape" when options.EnableEscape
                => new(FocusNavigationMode.Cancel),

            _ => new(FocusNavigationMode.None, PreventDefault: false)
        };
    }
}
