using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A bell button (or link) with an unread count, for the application header.
/// </summary>
/// <remarks>
/// <para>
/// The count is part of the control's accessible name ("Notifications, 3 unread"); the visible badge is hidden from
/// assistive technology so the number is not read twice. Counts above <see cref="Max"/> show as "99+".
/// </para>
/// <para>
/// Use it to open a panel (<see cref="OnClick"/>, with <see cref="Expanded"/> so assistive technology knows whether the
/// panel is open) or to go to a page (<see cref="Href"/>). Set <see cref="Announce"/> to have a polite announcement
/// made when the count changes while the user is on the page.
/// </para>
/// </remarks>
public partial class CyNotificationBell : CyComponentBase
{
    private static readonly CyNotificationBellText s_defaultText = new();

    /// <summary>The number of unread notifications. Zero hides the badge.</summary>
    [Parameter]
    public int Count { get; set; }

    /// <summary>The highest count shown exactly; larger counts show as "{Max}+". Defaults to 99.</summary>
    [Parameter]
    public int Max { get; set; } = 99;

    /// <summary>The accessible name without the count. Defaults to the localised "Notifications".</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>When set, the bell is a link to this URL instead of a button.</summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>Raised when the button is pressed (ignored for a link).</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Whether the panel the button opens is open. Set it when the bell controls a panel so <c>aria-expanded</c> is
    /// rendered; leave it null when it does not.
    /// </summary>
    [Parameter]
    public bool? Expanded { get; set; }

    /// <summary>Whether to announce the unread count politely whenever it changes. Defaults to false.</summary>
    [Parameter]
    public bool Announce { get; set; }

    /// <summary>The phrases used for the accessible name. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyNotificationBellText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-bell";

    private CyNotificationBellText Strings => Text ?? s_defaultText;

    private string BaseLabel => string.IsNullOrWhiteSpace(Label) ? Strings.Label : Label;

    private string AccessibleName =>
        Count > 0
            ? string.Format(CultureInfo.CurrentCulture, Strings.UnreadFormat, BaseLabel, Count)
            : BaseLabel;

    private string CountText =>
        Count > Max
            ? string.Create(CultureInfo.InvariantCulture, $"{Max}+")
            : Count.ToString(CultureInfo.CurrentCulture);

    private string? ExpandedValue => Expanded is { } open ? (open ? "true" : "false") : null;

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-bell--unread", Count > 0)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Count < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyNotificationBell)}.{nameof(Count)} must not be negative. Received '{Count}'.");
        }

        if (Max < 1)
        {
            throw new InvalidOperationException(
                $"{nameof(CyNotificationBell)}.{nameof(Max)} must be at least 1. Received '{Max}'.");
        }
    }
}
