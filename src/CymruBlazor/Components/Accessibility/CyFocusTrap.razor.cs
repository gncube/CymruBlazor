using Microsoft.AspNetCore.Components;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// A wrapper element that keeps keyboard focus inside its content: it moves
/// focus in when activated, contains <c>Tab</c>/<c>Shift+Tab</c> while active
/// and returns focus to where it came from when it is removed or disabled.
/// </summary>
/// <remarks>
/// <para>
/// Renders <c>&lt;div class="cy-focus-trap" tabindex="-1"&gt;</c> around its
/// content and, after the first render, asks the registered
/// <see cref="IFocusManager"/> to <see cref="IFocusManager.TrapAsync"/> it.
/// The default manager (<see cref="JsFocusManager"/>, registered by
/// <c>AddCymruBlazor()</c> since 1.3.0) does this with a small on-demand
/// JavaScript module: focus goes to the first <c>autofocus</c> or tabbable
/// element (else the wrapper), <c>Tab</c> wraps at both ends, and focus that
/// escapes (for example by a mouse click outside) is pulled back. Traps nest:
/// only the most recently activated one acts.
/// </para>
/// <para>
/// This is a <em>focus</em> trap only. It does not make the rest of the page
/// inert, add a dialog role, or handle <c>Escape</c>. For a modal dialog use
/// <see cref="CyDialog"/>, which builds on the native <c>&lt;dialog&gt;</c>.
/// </para>
/// <para>
/// A custom <see cref="IFocusManager"/> that only implements the three
/// original methods still works: the interface's default
/// <c>TrapAsync</c> calls <c>FocusAsync</c> and <c>RestoreFocusAsync</c>, but
/// does not contain <c>Tab</c>.
/// </para>
/// </remarks>
public partial class CyFocusTrap : CyComponentBase, IAsyncDisposable
{
    private IAsyncDisposable? _trap;
    private bool _active;

    /// <summary>The focus manager the trap uses; see the class remarks.</summary>
    [Inject]
    protected IFocusManager FocusManager { get; set; } = default!;

    /// <summary>The content rendered inside the trap element.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// When true (the default) the trap is active. Setting it to false releases
    /// the trap (restoring focus if <see cref="RestoreFocus"/>); setting it
    /// back to true activates it again. The wrapper element is always rendered.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// When true (and <see cref="Enabled"/> is true), moves focus into the trap
    /// each time it is activated. Defaults to true. Set to false when your own
    /// code focuses a specific control (for example a search box).
    /// </summary>
    [Parameter]
    public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// When true, returns focus to the element that had it before the trap was
    /// activated when the trap is released or disposed. Defaults to true.
    /// </summary>
    [Parameter]
    public bool RestoreFocus { get; set; } = true;

    protected override string BaseCssClass => "cy-focus-trap";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Enabled && !_active)
        {
            _active = true;
            var trap = await FocusManager.TrapAsync(
                Id,
                new FocusTrapOptions(AutoFocus, RestoreFocus, PreventScroll: true));

            if (_active)
            {
                _trap = trap;
            }
            else if (trap is not null)
            {
                // Disabled or disposed while the trap was being activated.
                await trap.DisposeAsync();
            }
        }
        else if (!Enabled && _active)
        {
            await ReleaseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await ReleaseAsync();
    }

    private async ValueTask ReleaseAsync()
    {
        var trap = _trap;
        _trap = null;
        _active = false;

        if (trap is not null)
        {
            await trap.DisposeAsync();
        }
    }
}
