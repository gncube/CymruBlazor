using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// A short, plain-text description that appears when its trigger is hovered
/// or focused, meeting WCAG 1.4.13 (Content on Hover or Focus).
/// </summary>
/// <remarks>
/// <para>
/// Shown on hover <em>and</em> keyboard focus, using CSS only (so it works before
/// the page is interactive). <b>Dismissible</b>: <c>Escape</c> hides it without moving the
/// pointer or focus (a tiny delegated script from <c>cymru-overlay.js</c>, loaded on demand, does this).
/// <b>Hoverable</b>: the pointer can move onto the tooltip without it disappearing.
/// <b>Persistent</b>: it stays until hover/focus leaves or it is dismissed.
/// </para>
/// <para>
/// The trigger (<see cref="CyLayoutComponentBase.ChildContent"/>) is wrapped in a focusable
/// element with <c>aria-describedby</c> pointing at the tooltip (<c>role="tooltip"</c>), so the
/// trigger content should carry its own accessible name (visible text, or for an icon a
/// <see cref="CyIcon.Label"/>). If your trigger is already interactive (a button or link), set
/// <see cref="Focusable"/> to <see langword="false"/> and add
/// <c>aria-describedby="{Id}-content"</c> to it yourself, giving the tooltip an explicit <c>Id</c>.
/// </para>
/// <para>
/// Content is plain text by design: a tooltip cannot contain interactive controls. Use
/// <see cref="Accessibility.CyDialog"/> for anything richer. The tooltip does not flip at
/// viewport edges; choose a <see cref="Placement"/> with room.
/// </para>
/// </remarks>
public partial class CyTooltip : CyLayoutComponentBase
{
    private bool _installed;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>The tooltip text. Plain text only.</summary>
    [Parameter, EditorRequired]
    public required string Text { get; set; }

    /// <summary>Where the tooltip appears relative to the trigger. Defaults to <see cref="TooltipPlacement.Top"/>.</summary>
    [Parameter]
    public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// When true (the default) the trigger wrapper is a tab stop and carries <c>aria-describedby</c>.
    /// Set to false when the trigger content is itself focusable; see the class remarks.
    /// </summary>
    [Parameter]
    public bool Focusable { get; set; } = true;

    protected override string BaseCssClass => "cy-tooltip";

    /// <summary>The id of the tooltip text element, for <c>aria-describedby</c>.</summary>
    public string ContentId => $"{Id}-content";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-tooltip--{Placement.ToString().ToLowerInvariant()}")
            .Build();

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Text))
        {
            throw new InvalidOperationException($"{nameof(CyTooltip)}.{nameof(Text)} must not be empty.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_installed)
        {
            return;
        }

        _installed = true;

        try
        {
            await using var module = await OverlayInterop.ImportAsync(JSRuntime);
            await module.InvokeVoidAsync("installTooltips");
        }
        catch (Exception ex) when (OverlayInterop.IsTeardown(ex) || ex is JSException)
        {
            // Without JS the tooltip still shows on hover/focus; only Escape-to-dismiss is unavailable.
        }
    }
}
