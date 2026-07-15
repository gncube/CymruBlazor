using Microsoft.AspNetCore.Components.Web;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Converts keyboard events into semantic navigation actions.
/// </summary>
public interface IKeyboardNavigationService
{
    KeyboardNavigationResult GetNavigation(
        KeyboardEventArgs args,
        KeyboardNavigationOptions? options = null);
}
