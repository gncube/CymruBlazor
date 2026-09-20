namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Real-layout measurements of one element, from <c>AxeTestBase.MeasureAsync</c>.
/// </summary>
public sealed class ElementMetrics
{
    public bool Found { get; set; }

    public bool Visible { get; set; }

    public double Left { get; set; }

    public double Top { get; set; }

    public double Right { get; set; }

    public double Bottom { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double ViewportWidth { get; set; }

    public double ViewportHeight { get; set; }

    public double PageScrollWidth { get; set; }

    public string? ZIndex { get; set; }

    public double Opacity { get; set; }

    /// <summary>The element is entirely inside the viewport.</summary>
    public bool InViewport => Left >= -0.5 && Top >= -0.5 && Right <= ViewportWidth + 0.5 && Bottom <= ViewportHeight + 0.5;

    /// <summary>The element's left and right edges are inside the viewport (its height may exceed it).</summary>
    public bool InViewportHorizontally => Left >= -0.5 && Right <= ViewportWidth + 0.5;

    /// <summary>The page is wider than the viewport, i.e. it scrolls horizontally (WCAG 1.4.10 Reflow).</summary>
    public bool PageOverflowsHorizontally => PageScrollWidth > ViewportWidth + 0.5;

    public override string ToString() =>
        $"found={Found} visible={Visible} box=({Left:0.#},{Top:0.#})-({Right:0.#},{Bottom:0.#}) " +
        $"size={Width:0.#}x{Height:0.#} viewport={ViewportWidth:0.#}x{ViewportHeight:0.#} " +
        $"pageScrollWidth={PageScrollWidth:0.#} z={ZIndex} opacity={Opacity}";
}
