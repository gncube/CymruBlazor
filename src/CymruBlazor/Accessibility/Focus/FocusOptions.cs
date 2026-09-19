namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Options controlling focus behaviour.
/// </summary>
/// <param name="PreventScroll">Ask the focus manager not to scroll the target into view.</param>
/// <param name="RestorePreviousFocus">Ask the focus manager to remember the currently focused element so
/// <c>IFocusManager.RestoreFocusAsync</c> can return to it.</param>
/// <remarks>
/// Only honoured by an <see cref="IFocusManager"/> that performs real focus
/// management; the default <see cref="FocusManager"/> ignores these options.
/// </remarks>
public sealed record FocusOptions(
    bool PreventScroll = true,
    bool RestorePreviousFocus = false);
