using System.Globalization;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A compact, overlapping row of <see cref="CyAvatar"/>s with a "+N" badge for the people who do not fit.
/// </summary>
/// <remarks>
/// Rendered as a list, so assistive technology announces "list, 5 items" and each person by name. The overflow badge
/// is read as "and 3 more" (localisable through <see cref="Text"/>).
/// </remarks>
public partial class CyAvatarGroup : CyComponentBase
{
    private static readonly CyAvatarGroupText s_defaultText = new();

    private IReadOnlyList<CyAvatarItem> _people = [];

    /// <summary>The people, in order.</summary>
    [Parameter]
    public IEnumerable<CyAvatarItem>? Items { get; set; }

    /// <summary>The most avatars to show before collapsing the rest into "+N". At least 1; defaults to 4.</summary>
    [Parameter]
    public int Max { get; set; } = 4;

    /// <summary>The size of every avatar.</summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>Optional accessible name of the list, for example "Reviewers".</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>The phrase added for assistive technology. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyAvatarGroupText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-avatar-group";

    private IEnumerable<CyAvatarItem> Visible => _people.Take(Max);

    private int Hidden => Math.Max(0, _people.Count - Max);

    private string OverflowClass =>
        $"cy-avatar cy-avatar--{CyAvatar.SizeSuffix(Size)} cy-avatar--overflow";

    private string OverflowText =>
        string.Format(CultureInfo.CurrentCulture, (Text ?? s_defaultText).MoreFormat, Hidden);

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Max < 1)
        {
            throw new InvalidOperationException(
                $"{nameof(CyAvatarGroup)}.{nameof(Max)} must be at least 1. Received '{Max}'.");
        }

        _people = Items is null ? [] : [.. Items];
    }
}
