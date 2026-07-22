using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Content;

/// <summary>
/// An inline status/alert banner. <see cref="Severity"/> drives both the
/// visual treatment and the ARIA role: <c>Warning</c>/<c>Danger</c> render
/// <c>role="alert"</c> (assertive - interrupts screen readers, appropriate
/// for messages the user must not miss), while <c>Info</c>/<c>Success</c>
/// render <c>role="status"</c> (polite - announced without interrupting).
/// </summary>
public partial class CyAlert : CyLayoutComponentBase
{
    /// <summary>
    /// Gets or sets the alert's severity. Must be <see cref="ComponentColour.Info"/>,
    /// <see cref="ComponentColour.Success"/>, <see cref="ComponentColour.Warning"/>,
    /// or <see cref="ComponentColour.Danger"/>.
    /// </summary>
    [Parameter]
    public ComponentColour Severity { get; set; } = ComponentColour.Info;

    /// <summary>
    /// Optional bold title rendered above the message.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// When <see langword="true"/>, renders a dismiss button.
    /// </summary>
    [Parameter]
    public bool Dismissible { get; set; }

    /// <summary>
    /// Raised when the dismiss button is activated. CyAlert does not
    /// remove itself from the DOM - the parent owns whether/how the
    /// alert disappears (e.g. conditionally rendering it), consistent
    /// with this library not managing state on the consumer's behalf.
    /// </summary>
    [Parameter]
    public EventCallback OnDismiss { get; set; }

    protected override string BaseCssClass => "cy-alert";

    private string ComputedRole =>
        Severity is ComponentColour.Warning or ComponentColour.Danger
            ? "alert"
            : "status";

    protected override string BuildCssClass()
    {
        var severitySuffix = Severity.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-alert--{severitySuffix}")
            .AddClass("cy-alert--dismissible", Dismissible)
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Severity is not (ComponentColour.Info
            or ComponentColour.Success
            or ComponentColour.Warning
            or ComponentColour.Danger))
        {
            throw new InvalidOperationException(
                $"{nameof(CyAlert)}.{nameof(Severity)} must be Info, Success, " +
                $"Warning, or Danger. Received '{Severity}'.");
        }
    }

    private async Task HandleDismissAsync()
    {
        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }
    }
}
