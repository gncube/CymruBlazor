using Microsoft.AspNetCore.Components.Web;

namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Converts keyboard events into semantic navigation actions.
/// </summary>
/// <remarks>
/// Roadmap D5: nothing in CymruBlazor consumes this - it has never been
/// registered by <c>AddCymruBlazor()</c> and no component calls it. Kept
/// for now in case a consumer implemented against it directly; scheduled
/// for removal in 2.0.0 alongside <see cref="KeyboardNavigationService"/>,
/// <see cref="KeyboardNavigationResult"/> and <see cref="KeyboardNavigationOptions"/>.
/// If <c>CyDropdown</c>/menu (roadmap, deferred) is ever built, keyboard
/// navigation should most likely be designed fresh for that component's
/// actual needs rather than resurrecting this.
/// </remarks>
[Obsolete("Nothing in CymruBlazor consumes this; it will be removed in 2.0.0.", error: false)]
public interface IKeyboardNavigationService
{
    KeyboardNavigationResult GetNavigation(
        KeyboardEventArgs args,
        KeyboardNavigationOptions? options = null);
}
