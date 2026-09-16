namespace StarterApp.Layout.Models;

public enum MobileNavVariant
{
    Standard = 0,
    ActionCenter = 1
}

public sealed record MobileNavItemModel(
    string Label,
    string IconName,
    string? Href = null,
    bool IsAction = false,
    string? ActionKey = null,
    int? BadgeCount = null);
