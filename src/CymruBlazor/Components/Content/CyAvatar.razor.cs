using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A person (or team) shown as a photo or as initials on a coloured circle.
/// </summary>
/// <remarks>
/// <para>
/// The avatar is announced as an image named by <see cref="Name"/>. When the person's name is written next to the
/// avatar, set <see cref="Decorative"/> to hide it from assistive technology instead of repeating the name.
/// </para>
/// <para>
/// Without <see cref="ImageUrl"/> the initials are the first letters of the first and last words of
/// <see cref="Name"/> (or <see cref="Initials"/> when set) and the background is one of six theme colours chosen
/// from the name, so the same person always gets the same colour. A photo that fails to load is not replaced
/// automatically (that would need a script); only pass <see cref="ImageUrl"/> when you know the image exists.
/// </para>
/// </remarks>
public partial class CyAvatar : CyComponentBase
{
    private const int ToneCount = 6;

    /// <summary>The person's name: used for the accessible name, the initials and the colour. Required.</summary>
    [Parameter, EditorRequired]
    public required string Name { get; set; }

    /// <summary>Optional photo. When set it replaces the initials.</summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>Overrides the initials worked out from <see cref="Name"/> (use at most two characters).</summary>
    [Parameter]
    public string? Initials { get; set; }

    /// <summary>The avatar size. <see cref="ComponentSize.Unspecified"/> means <see cref="ComponentSize.Medium"/>.</summary>
    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <summary>Hides the avatar from assistive technology, for use beside a visible name.</summary>
    [Parameter]
    public bool Decorative { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-avatar";

    private string InitialsText => string.IsNullOrWhiteSpace(Initials) ? ComputeInitials(Name) : Initials.Trim();

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass($"cy-avatar--{SizeSuffix(Size)}")
            .AddClass($"cy-avatar--tone-{ToneFor(Name)}", string.IsNullOrWhiteSpace(ImageUrl))
            .AddClass("cy-avatar--image", !string.IsNullOrWhiteSpace(ImageUrl))
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException(
                $"{nameof(CyAvatar)}.{nameof(Name)} must not be empty: it names the person.");
        }
    }

    internal static string SizeSuffix(ComponentSize size) => size switch
    {
        ComponentSize.ExtraSmall => "xs",
        ComponentSize.Small => "sm",
        ComponentSize.Large => "lg",
        ComponentSize.ExtraLarge => "xl",
        _ => "md"
    };

    /// <summary>A stable colour index: the same name always maps to the same tone (unlike <c>string.GetHashCode</c>).</summary>
    public static int ToneFor(string name)
    {
        var hash = 17;

        foreach (var c in name.Trim().ToUpperInvariant())
        {
            hash = unchecked((hash * 31) + c);
        }

        return Math.Abs(hash % ToneCount);
    }

    public static string ComputeInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "?";
        }

        var first = FirstLetter(parts[0]);

        if (parts.Length == 1)
        {
            return first ?? "?";
        }

        var last = FirstLetter(parts[^1]);

        var both = (first ?? string.Empty) + (last ?? string.Empty);

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
