using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mediator;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Enums;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Feedback;

namespace CymruBlazor.Components.Content;

/// <summary>
/// Displays a labelled, read-only code sample with a copy-to-clipboard
/// button.
///
/// Copying calls the browser's native <c>navigator.clipboard.writeText</c>
/// API directly via <see cref="IJSRuntime"/> - per this project's
/// "minimise JavaScript" principle (see PROMPT.md), this is a single
/// interop call to a built-in browser API, not a custom JS module.
///
/// The "Copied"/"Copy failed" confirmation is a visual button-label swap
/// (for sighted mouse users watching the button they just clicked), a
/// screen-reader announcement published through the Mediator pipeline to
/// any <see cref="CymruBlazor.Components.Accessibility.CyLiveRegion"/>
/// registered in the app, and a toast published the same way to whatever
/// <see cref="CyToastContainer"/> the host app has mounted - a screen
/// reader user (or a sighted user who has looked away) would otherwise
/// never learn the copy succeeded or failed. Publishing
/// <see cref="ShowToastNotification"/> through <see cref="IMediator"/>
/// rather than injecting <see cref="IToastService"/> directly keeps
/// CyCodeBlock decoupled from the concrete toast implementation, exactly
/// like the existing <see cref="LiveRegionAnnouncement"/> call below.
/// </summary>
public partial class CyCodeBlock : CyComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IMediator Mediator { get; set; } = default!;

    private bool _copied;
    private bool _copyFailed;

    /// <summary>
    /// The code sample text. Rendered verbatim (via Razor's default HTML
    /// encoding) inside a <c>&lt;pre&gt;&lt;code&gt;</c> block - no syntax
    /// highlighting is applied.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Code { get; set; }

    /// <summary>
    /// The language label shown in the header, e.g. "razor", "csharp",
    /// "bash". Purely a display label - no highlighting is driven by it.
    /// </summary>
    [Parameter]
    public string Language { get; set; } = "text";

    /// <summary>
    /// When <see langword="false"/>, hides the copy button entirely (for
    /// read-only reference snippets that aren't meant to be copied
    /// verbatim, e.g. abbreviated "...")
    /// </summary>
    [Parameter]
    public bool ShowCopyButton { get; set; } = true;

    /// <summary>
    /// The screen-reader announcement politeness used when reporting a
    /// successful copy. Copy failures always announce as
    /// <see cref="LiveRegionPoliteness.Assertive"/> regardless of this
    /// setting, since a failure needs to interrupt.
    /// </summary>
    [Parameter]
    public LiveRegionPoliteness AnnouncementPoliteness { get; set; } = LiveRegionPoliteness.Polite;

    /// <summary>
    /// When <see langword="false"/>, suppresses the toast notification
    /// published on copy (success or failure) - the screen-reader live
    /// region announcement and the visual button-label swap still
    /// happen regardless. Useful when a page already renders several
    /// code blocks and a toast per click would be noisy.
    /// </summary>
    [Parameter]
    public bool ShowCopyToast { get; set; } = true;

    /// <summary>
    /// Header label used when <see cref="Language"/> is blank. Defaults to the English "Code".
    /// </summary>
    [Parameter]
    public string? CodeLabel { get; set; }

    /// <summary>
    /// Copy button text at rest. Defaults to the English "Copy".
    /// </summary>
    [Parameter]
    public string? CopyLabel { get; set; }

    /// <summary>
    /// Copy button text after a successful copy. Defaults to the English "Copied".
    /// </summary>
    [Parameter]
    public string? CopiedLabel { get; set; }

    /// <summary>
    /// Copy button text after a failed copy. Defaults to the English "Copy failed".
    /// </summary>
    [Parameter]
    public string? CopyFailedLabel { get; set; }

    /// <summary>
    /// Message announced (and shown in the toast) after a successful copy.
    /// Defaults to the English "Code copied to clipboard."
    /// </summary>
    [Parameter]
    public string? CopiedMessage { get; set; }

    /// <summary>
    /// Message announced (and shown in the toast) after a failed copy.
    /// Defaults to the English "Copying to clipboard failed."
    /// </summary>
    [Parameter]
    public string? CopyFailedMessage { get; set; }

    protected override string BaseCssClass => "cy-code-block";

    private string LanguageLabel => string.IsNullOrWhiteSpace(Language) ? (CodeLabel ?? "Code") : Language;

    private string CopyButtonText => _copyFailed
        ? (CopyFailedLabel ?? "Copy failed")
        : (_copied ? (CopiedLabel ?? "Copied") : (CopyLabel ?? "Copy"));

    private string CopiedText => CopiedMessage ?? "Code copied to clipboard.";

    private string CopyFailedText => CopyFailedMessage ?? "Copying to clipboard failed.";

    private async Task CopyToClipboardAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Code);

            _copied = true;
            _copyFailed = false;

            await Mediator.Publish(new LiveRegionAnnouncement(
                CopiedText,
                AnnouncementPoliteness));

            if (ShowCopyToast)
            {
                await Mediator.Publish(new ShowToastNotification(
                    CopiedText,
                    ToastVariant.Success));
            }
        }
        catch (JSException)
        {
            // navigator.clipboard is unavailable in some contexts (e.g.
            // non-HTTPS origins, certain embedded webviews, or a denied
            // permission) - fail visibly rather than silently.
            _copied = false;
            _copyFailed = true;

            await Mediator.Publish(new LiveRegionAnnouncement(
                CopyFailedText,
                LiveRegionPoliteness.Assertive));

            if (ShowCopyToast)
            {
                await Mediator.Publish(new ShowToastNotification(
                    CopyFailedText,
                    ToastVariant.Danger));
            }
        }

        StateHasChanged();

        await Task.Delay(2000);

        _copied = false;
        _copyFailed = false;

        StateHasChanged();
    }
}
