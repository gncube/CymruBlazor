using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A bar of related controls (Save status, Undo/Redo, Preview, Publish) exposed as a single tab stop
/// with arrow-key navigation between the controls, per the WAI-ARIA Authoring Practices toolbar pattern.
/// Optionally sticks to the top or bottom of its scroll container.
/// </summary>
/// <remarks>
/// <para>
/// Renders <c>role="toolbar"</c> named by <see cref="Label"/> (required). After the first render a small
/// on-demand script (<c>cymru-editing.js</c>) gives the toolbar a roving <c>tabindex</c>: one control is in the
/// tab sequence, <c>Left</c>/<c>Right</c> (<c>Up</c>/<c>Down</c> when <see cref="Orientation"/> is vertical)
/// and <c>Home</c>/<c>End</c> move between controls, and the last-focused control is where <c>Tab</c> returns.
/// Text fields keep their own arrow keys. Controls inside a nested <c>role="menu"</c> are left alone, so a
/// <see cref="CyMenu"/> can sit in a toolbar. Without the script, every control stays in the normal tab
/// order, so nothing becomes unreachable.
/// </para>
/// <para>
/// Keep controls at least 24x24 CSS px (WCAG 2.5.8); <c>CyButton</c> already is. A toolbar groups
/// <em>controls</em>; do not put headings or long text in it.
/// </para>
/// </remarks>
public partial class CyToolbar : CyLayoutComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private IJSObjectReference? _module;
    private int _token;
    private Orientation _attachedOrientation;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>The toolbar's accessible name. Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>The direction controls are laid out and navigated in. Defaults to horizontal.</summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>Sticks the bar to the top or bottom edge of its scroll container. Defaults to none.</summary>
    [Parameter]
    public StickyPosition Sticky { get; set; } = StickyPosition.None;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-toolbar";

    private string OrientationValue => Orientation == Orientation.Vertical ? "vertical" : "horizontal";

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-toolbar--vertical", Orientation == Orientation.Vertical)
            .AddClass("cy-toolbar--sticky-top", Sticky == StickyPosition.Top)
            .AddClass("cy-toolbar--sticky-bottom", Sticky == StickyPosition.Bottom)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyToolbar)}.{nameof(Label)} must not be empty: it names the toolbar.");
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_token != 0 && _attachedOrientation == Orientation)
        {
            return;
        }

        try
        {
            _module ??= await EditingInterop.ImportAsync(JSRuntime);

            if (_token != 0)
            {
                await _module.InvokeVoidAsync("detachToolbar", _token);
                _token = 0;
            }

            _attachedOrientation = Orientation;
            _token = await _module.InvokeAsync<int>(
                "attachToolbar",
                _element,
                Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        }
        catch (Exception)
        {
            // Without the script every control stays in the tab order.
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        try
        {
            if (_module is not null)
            {
                if (_token != 0)
                {
                    await _module.InvokeVoidAsync("detachToolbar", _token);
                    _token = 0;
                }

                await _module.DisposeAsync();
                _module = null;
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or ObjectDisposedException or InvalidOperationException)
        {
            // Browser or circuit is already gone.
        }
    }
}
