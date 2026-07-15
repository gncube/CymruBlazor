namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Options controlling focus behaviour.
/// </summary>
public sealed record FocusOptions(
    bool PreventScroll = true,
    bool RestorePreviousFocus = false);
