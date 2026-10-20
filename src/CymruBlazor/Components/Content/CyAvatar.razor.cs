using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A person, team, or profile entity shown as a photo or initials on a coloured circular surface,
/// with optional status indicator and interactive click handling.
/// </summary>
/// <remarks>
/// <para>
/// The avatar is announced as an image named by <see cref="Alt"/> (if provided) or <see cref="Name"/>.
/// When the person's name is written next to the avatar, set <see cref="Decorative"/> to hide it from
/// assistive technology instead of repeating the name.
/// </para>
/// <para>
/// When <see cref="ImageUrl"/> is not supplied or fails to load, smart 2-letter initials are computed
/// from <see cref="Name"/> (or <see cref="Initials"/>) and displayed over a deterministic, accessible
/// background hue or tone derived from the name hash.
/// </para>
/// </remarks>
public partial class CyAvatar : CyComponentBase
{
    private const int ToneCount = 6;
    private bool _imageFailedToLoad;
    private string? _previousImageUrl;

    /// <summary>
    /// Gets or sets the person's display name or email address used for initials, accessible name, and deterministic colour.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an optional image URL for the avatar photo.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets explicit initials to display, overriding the initials worked out from <see cref="Name"/>.
    /// </summary>
    [Parameter]
    public string? Initials { get; set; }

    /// <summary>
    /// Gets or sets the size of the avatar. Defaults to <see cref="ComponentSize.Medium"/>.
    /// </summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>
    /// Gets or sets the activity or presence status of the user (e.g. Online, Busy, Away, Offline).
    /// </summary>
    [Parameter]
    public AvatarStatus Status { get; set; } = AvatarStatus.None;

    /// <summary>
    /// Gets or sets an accessible alt text label (<c>role="img"</c>). When omitted, <see cref="Name"/> is used.
    /// </summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets the colour variant of the avatar background.
    /// When <see cref="ComponentColour.Unspecified"/>, a deterministic tone is computed from <see cref="Name"/>.
    /// </summary>
    [Parameter]
    public ComponentColour Colour { get; set; } = ComponentColour.Unspecified;

    /// <summary>
    /// Gets or sets an optional click event callback. When specified, the avatar renders as an accessible interactive control.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets whether to hide the avatar from assistive technology (e.g. when beside a visible name).
    /// </summary>
    [Parameter]
    public bool Decorative { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-avatar";

    private bool HasActiveImage => !string.IsNullOrWhiteSpace(ImageUrl) && !_imageFailedToLoad;

    private string InitialsText => !string.IsNullOrWhiteSpace(Initials) ? Initials.Trim() : ComputeInitials(Name);

    private string AccessibleLabel
    {
        get
        {
            var baseLabel = !string.IsNullOrWhiteSpace(Alt)
                ? Alt
                : !string.IsNullOrWhiteSpace(Name)
                    ? Name
                    : "Avatar";

            if (OnClick.HasDelegate && !HasActiveImage && !string.IsNullOrWhiteSpace(InitialsText))
            {
                if (!baseLabel.StartsWith(InitialsText, StringComparison.OrdinalIgnoreCase))
                {
                    baseLabel = $"{InitialsText}, {baseLabel}";
                }
            }

            if (Status != AvatarStatus.None && !Decorative)
            {
                baseLabel = $"{baseLabel} ({Status})";
            }

            return baseLabel;
        }
    }

    private string StatusCssClass =>
        CssBuilder.Empty
            .AddClass("cy-avatar__status")
            .AddClass($"cy-avatar__status--{Status.ToString().ToLowerInvariant()}")
            .Build();

    /// <inheritdoc />
    protected override string BuildCssClass()
    {
        var hasImage = HasActiveImage;
        var colourSuffix = Colour != ComponentColour.Unspecified
            ? Colour.ToString().ToLowerInvariant()
            : null;

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass($"cy-avatar--{SizeSuffix(Size)}")
            .AddClass($"cy-avatar--tone-{ToneFor(Name ?? string.Empty)}", !hasImage && colourSuffix is null)
            .AddClass($"cy-avatar--{colourSuffix}", !hasImage && colourSuffix is not null)
            .AddClass("cy-avatar--image", hasImage)
            .AddClass("cy-avatar--interactive", OnClick.HasDelegate)
            .AddClass(Class)
            .Build();
    }

    /// <inheritdoc />
    protected override string BuildCssStyle()
    {
        var hue = HueFor(Name);
        return StyleBuilder.Empty
            .AddStyle("--cy-avatar-hue", $"{hue}deg")
            .AddStyle(Style)
            .Build();
    }

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (!Decorative && string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Alt))
        {
            throw new InvalidOperationException(
                $"{nameof(CyAvatar)} must have a non-empty {nameof(Name)} or {nameof(Alt)} for accessibility.");
        }
    }

    /// <inheritdoc />
    protected override void OnParametersValidated()
    {
        base.OnParametersValidated();

        if (ImageUrl != _previousImageUrl)
        {
            _previousImageUrl = ImageUrl;
            _imageFailedToLoad = false;
        }
    }

    private void HandleImageError()
    {
        _imageFailedToLoad = true;
        RefreshCssState();
        StateHasChanged();
    }

    internal static string SizeSuffix(ComponentSize size) => size switch
    {
        ComponentSize.ExtraSmall => "xs",
        ComponentSize.Small => "sm",
        ComponentSize.Large => "lg",
        ComponentSize.ExtraLarge => "xl",
        _ => "md"
    };

    /// <summary>
    /// A stable colour index: the same name always maps to the same tone (0-5).
    /// </summary>
    public static int ToneFor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        var hash = 17;

        foreach (var c in name.Trim().ToUpperInvariant())
        {
            hash = unchecked((hash * 31) + c);
        }

        return Math.Abs(hash % ToneCount);
    }

    /// <summary>
    /// Generates a deterministic hue angle (0 to 359) from the string hash of the name.
    /// </summary>
    public static int HueFor(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return 210;
        }

        var hash = 17;

        foreach (var c in name.Trim().ToUpperInvariant())
        {
            hash = unchecked((hash * 31) + c);
        }

        return Math.Abs(hash % 360);
    }

    /// <summary>
    /// Computes initials from a person's name or an email address.
    /// Handles single names ("Gwen" -> "G"), multi-word names ("Jane Doe" -> "JD", "Rhys ap Gruffudd" -> "RG"),
    /// titles/honorifics ("(Dr) Sam Lee" -> "DL"), and email addresses ("user.name@org.uk" -> "UN").
    /// </summary>
    public static string ComputeInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var trimmed = name.Trim();

        // Check if it's an email address
        var atIndex = trimmed.IndexOf('@');
        if (atIndex > 0)
        {
            var localPart = trimmed[..atIndex];
            var emailParts = localPart.Split(['.', '_', '-', '+', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (emailParts.Length >= 2)
            {
                var first = FirstLetter(emailParts[0]);
                var second = FirstLetter(emailParts[1]);
                var combined = (first ?? string.Empty) + (second ?? string.Empty);
                if (combined.Length > 0)
                {
                    return combined;
                }
            }
            else if (emailParts.Length == 1)
            {
                var first = FirstLetter(emailParts[0]);
                if (!string.IsNullOrEmpty(first))
                {
                    return first;
                }
            }
        }

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "?";
        }

        var firstWord = FirstLetter(parts[0]);

        if (parts.Length == 1)
        {
            return firstWord ?? "?";
        }

        var lastWord = FirstLetter(parts[^1]);

        var both = (firstWord ?? string.Empty) + (lastWord ?? string.Empty);

        return both.Length > 0 ? both : "?";
    }

    private static string? FirstLetter(string word)
    {
        foreach (var rune in word.EnumerateRunes())
        {
            if (Rune.IsLetterOrDigit(rune))
            {
                return Rune.ToUpper(rune, CultureInfo.CurrentCulture).ToString();
            }
        }

        return null;
    }
}
