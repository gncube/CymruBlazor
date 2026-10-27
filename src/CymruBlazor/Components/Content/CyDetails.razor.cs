using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Content;

/// <summary>
/// An expandable details disclosure component following the NHS Design System pattern.
/// Uses native HTML5 &lt;details&gt; and &lt;summary&gt; elements for maximum accessibility,
/// zero-JavaScript baseline operation, and native keyboard interaction.
/// </summary>
public partial class CyDetails : CyLayoutComponentBase
{
    private static readonly CyDetailsText s_defaultText = new();

    /// <summary>
    /// Gets or sets the summary heading text.
    /// If omitted and <see cref="SummaryTemplate"/> is null, falls back to <see cref="CyDetailsText.DefaultSummary"/>.
    /// </summary>
    [Parameter]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets an optional custom template for the summary heading content.
    /// Takes precedence over <see cref="Summary"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? SummaryTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether the disclosure details panel is currently expanded/open.
    /// Supports two-way data binding via <c>@bind-Open</c>.
    /// </summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// Callback invoked when the open/closed state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Optional event callback fired when the disclosure is toggled.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnToggle { get; set; }

    /// <summary>
    /// Gets or sets the localized text configuration.
    /// </summary>
    [Parameter]
    public CyDetailsText? Text { get; set; }

    /// <summary>
    /// Gets the effective localized phrases.
    /// </summary>
    protected CyDetailsText EffectiveText => Text ?? s_defaultText;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-details";

    /// <summary>
    /// Programmatically toggles the disclosure open state.
    /// </summary>
    public async Task ToggleAsync()
    {
        await SetOpenAsync(!Open);
    }

    /// <summary>
    /// Programmatically opens the disclosure panel.
    /// </summary>
    public async Task OpenAsync()
    {
        if (!Open)
        {
            await SetOpenAsync(true);
        }
    }

    /// <summary>
    /// Programmatically closes the disclosure panel.
    /// </summary>
    public async Task CloseAsync()
    {
        if (Open)
        {
            await SetOpenAsync(false);
        }
    }

    private async Task HandleToggleAsync()
    {
        await SetOpenAsync(!Open);
    }

    private async Task SetOpenAsync(bool newOpen)
    {
        if (Open == newOpen)
        {
            return;
        }

        Open = newOpen;
        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(newOpen);
        }
        if (OnToggle.HasDelegate)
        {
            await OnToggle.InvokeAsync(newOpen);
        }
        StateHasChanged();
    }
}
