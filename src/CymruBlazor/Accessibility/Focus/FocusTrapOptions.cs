namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Options for <see cref="IFocusManager.TrapAsync"/>.
/// </summary>
/// <param name="AutoFocus">Move focus into the trap when it is activated (the first
/// <c>autofocus</c> element, else the first tabbable element, else the trap itself).</param>
/// <param name="RestoreFocus">Return focus to the element that had it before activation when
/// the trap is released.</param>
/// <param name="PreventScroll">Ask the browser not to scroll when focus moves.</param>
/// <param name="MediaQuery">When set (for example <c>(max-width: 47.99rem)</c>), the trap is only activated
/// if the query matches at activation time. Used by the sidebar so its drawer traps focus on small screens
/// but the same element does not on desktop. Implementations that cannot evaluate a media query ignore it.</param>
public sealed record FocusTrapOptions(
    bool AutoFocus = true,
    bool RestoreFocus = true,
    bool PreventScroll = true,
    string? MediaQuery = null);
