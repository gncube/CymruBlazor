using Microsoft.AspNetCore.Components;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// A wrapper element that <em>asks</em> an <see cref="IFocusManager"/> to move
/// focus into it and to restore focus when it is removed.
/// </summary>
/// <remarks>
/// <para>
/// <b>Preview - this component does not trap focus yet.</b> It renders
/// <c>&lt;div class="cy-focus-trap" tabindex="-1"&gt;</c> around its content.
/// When <see cref="Enabled"/> and <see cref="AutoFocus"/> are both true it
/// calls <c>IFocusManager.FocusAsync</c> after the first render, and when
/// <see cref="RestoreFocus"/> is true it calls
/// <c>IFocusManager.RestoreFocusAsync</c> on disposal.
/// </para>
/// <para>
/// The <see cref="IFocusManager"/> registered by <c>AddCymruBlazor()</c> is a
/// logging placeholder: it records the request at Debug level and returns
/// success without touching the DOM. So by default focus is not moved, is not
/// restored, and <c>Tab</c> is not contained. Do not rely on this component to
/// satisfy WCAG 2.4.3 (Focus Order) or to build a modal dialog.
/// </para>
/// <para>
/// To get real behaviour today, register your own <see cref="IFocusManager"/>
/// after <c>AddCymruBlazor()</c> (the last registration wins). A functional
/// implementation, with Tab containment, is planned for 1.3.0.
/// </para>
/// </remarks>
public partial class CyFocusTrap : CyComponentBase, IAsyncDisposable
{
    /// <summary>The focus manager the trap calls; see the class remarks for the default's limits.</summary>
    [Inject]
    protected IFocusManager FocusManager { get; set; } = default!;

    /// <summary>The content rendered inside the trap element.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// When false, no focus request is made on first render. Defaults to true.
    /// This does not remove the wrapper element.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// When true (and <see cref="Enabled"/> is true), requests focus for the
    /// trap element after the first render. The default focus manager does not
    /// act on the request. Defaults to true.
    /// </summary>
    [Parameter]
    public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// When true, requests that the previously focused element be refocused when
    /// the trap is disposed. The default focus manager does not act on the
    /// request. Defaults to true.
    /// </summary>
    [Parameter]
    public bool RestoreFocus { get; set; } = true;

    protected override string BaseCssClass => "cy-focus-trap";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !Enabled || !AutoFocus)
        {
            return;
        }

        await FocusManager.FocusAsync(
            Id,
            new FocusOptions(
                PreventScroll: true,
                RestorePreviousFocus: RestoreFocus));
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!RestoreFocus)
        {
            return;
        }

        await FocusManager.RestoreFocusAsync();
    }
}
