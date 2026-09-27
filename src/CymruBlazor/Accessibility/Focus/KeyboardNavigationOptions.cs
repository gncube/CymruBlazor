namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Configures supported keyboard interactions.
/// </summary>
[Obsolete("Nothing in CymruBlazor consumes this; it will be removed in 2.0.0.", error: false)]
public sealed class KeyboardNavigationOptions
{
    public bool EnableArrowKeys { get; init; } = true;

    public bool EnableHomeEnd { get; init; } = true;

    public bool EnableTabNavigation { get; init; } = true;

    public bool EnableActivation { get; init; } = true;

    public bool EnableEscape { get; init; } = true;
}
