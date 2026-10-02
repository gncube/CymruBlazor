using System.Xml;

namespace CymruBlazor.Icons;

/// <summary>
/// Allow-list validator for icon markup passed to
/// <see cref="IconRegistry.Register"/>. Icon markup is rendered as raw HTML
/// (<c>MarkupString</c>), so anything that can execute or load content must be
/// rejected rather than escaped: only plain SVG shape elements and a fixed set
/// of geometry/presentation attributes are accepted.
/// </summary>
internal static class IconMarkupValidator
{
    private static readonly HashSet<string> AllowedElements = new(StringComparer.Ordinal)
    {
        "path", "circle", "ellipse", "rect", "line", "polyline", "polygon", "g",
    };

    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.Ordinal)
    {
        "d", "cx", "cy", "r", "rx", "ry", "x", "y", "x1", "x2", "y1", "y2",
        "width", "height", "points", "transform", "pathLength",
        "fill", "fill-rule", "fill-opacity", "clip-rule",
        "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin",
        "stroke-miterlimit", "stroke-dasharray", "stroke-dashoffset", "stroke-opacity",
        "opacity",
    };

    /// <exception cref="ArgumentException">The markup is empty, malformed, or uses anything outside the allow-list.</exception>
    public static void EnsureSafe(string markup)
    {
        if (string.IsNullOrWhiteSpace(markup))
        {
            throw new ArgumentException("Icon markup must not be empty.", nameof(markup));
        }

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            ConformanceLevel = ConformanceLevel.Fragment,
            IgnoreComments = false,
        };

        var sawElement = false;

        try
        {
            using var reader = XmlReader.Create(new StringReader(markup), settings);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        sawElement = true;
                        CheckElement(reader);
                        break;

                    case XmlNodeType.EndElement:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        break;

                    default:
                        // Text, CDATA, comments, processing instructions, entity references...
                        throw Reject($"unsupported content ({reader.NodeType}).");
                }
            }
        }
        catch (XmlException ex)
        {
            throw Reject($"not well-formed ({ex.Message}).");
        }

        if (!sawElement)
        {
            throw Reject("it contains no shape elements.");
        }
    }

    private static void CheckElement(XmlReader reader)
    {
        if (reader.NamespaceURI.Length != 0 || reader.Prefix.Length != 0)
        {
            throw Reject($"element '{reader.Name}' uses a namespace.");
        }

        if (!AllowedElements.Contains(reader.LocalName))
        {
            throw Reject($"element <{reader.LocalName}> is not allowed.");
        }

        if (!reader.HasAttributes)
        {
            return;
        }

        while (reader.MoveToNextAttribute())
        {
            if (!AllowedAttributes.Contains(reader.Name))
            {
                throw Reject($"attribute '{reader.Name}' is not allowed on <{reader.LocalName}>.");
            }

            // Paranoia: none of the allowed attributes take URLs or script, but
            // a value that looks like either is never legitimate icon geometry.
            var v = reader.Value;
            if (v.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
                || v.Contains("data:", StringComparison.OrdinalIgnoreCase)
                || v.Contains('<'))
            {
                throw Reject($"attribute '{reader.Name}' has a disallowed value.");
            }
        }

        reader.MoveToElement();
    }

    private static ArgumentException Reject(string reason) =>
        new($"Icon markup rejected: {reason} Only SVG shapes (path, circle, ellipse, rect, line, polyline, polygon, g) with geometry/presentation attributes are accepted.");
}
