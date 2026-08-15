namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Configures supported keyboard interactions.
/// </summary>
public sealed class KeyboardNavigationOptions
{
    public bool EnableArrowKeys { get; init; } = true;

    public bool EnableHomeEnd { get; init; } = true;

    public bool EnableTabNavigation { get; init; } = true;

    public bool EnableActivation { get; init; } = true;

    public bool EnableEscape { get; init; } = true;
}
