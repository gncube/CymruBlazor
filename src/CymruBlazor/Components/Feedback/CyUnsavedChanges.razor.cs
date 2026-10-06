using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// Warns before the user navigates away from a page with unsaved work and shows a small
/// "Saving... / Saved / Unsaved changes" indicator.
/// </summary>
/// <remarks>
/// <para>
/// While <see cref="Dirty"/> is true, in-app navigation (links, <c>NavigationManager.NavigateTo</c>) is intercepted
/// with <c>NavigationManager.RegisterLocationChangingHandler</c> and the user is asked, through
/// <see cref="ICyConfirmService"/>, whether to leave. Without a <see cref="CyConfirmDialog"/> host the navigation is
/// simply cancelled and <see cref="OnNavigationBlocked"/> tells you where the user was heading, so you can show your own
/// prompt. Changes that only alter the URL fragment are not guarded.
/// </para>
/// <para>
/// <b>Limits.</b> Reloading the page, closing the tab and external links are browser-level navigation that this
/// component does not guard (that needs a <c>beforeunload</c> script, which the library does not ship). Interception is
/// not available while prerendering; the guard starts once the component is interactive.
/// </para>
/// <para>
/// The indicator is a polite <c>role="status"</c> region that is always in the page, so a change of text is announced
/// when it happens. Priority: saving, then error, then unsaved changes, then saved. <see cref="SavedAt"/> is shown as
/// given (pass local time).
/// </para>
/// </remarks>
public partial class CyUnsavedChanges : CyComponentBase, IDisposable
{
    private static readonly CyUnsavedChangesText s_defaultText = new();

    private IDisposable? _registration;
    private bool _disposed;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    /// <summary>Whether the page has changes that are not saved. Turns the navigation guard on.</summary>
    [Parameter]
    public bool Dirty { get; set; }

    /// <summary>Progress of the current save. Defaults to <see cref="CySaveState.Idle"/>.</summary>
    [Parameter]
    public CySaveState SaveState { get; set; }

    /// <summary>When the last save finished, shown beside "Saved". Pass local time.</summary>
    [Parameter]
    public DateTimeOffset? SavedAt { get; set; }

    /// <summary>Message of the leave-the-page confirmation. Defaults to the localised standard message.</summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>Whether to show the indicator. The guard works either way. Defaults to true.</summary>
    [Parameter]
    public bool ShowIndicator { get; set; } = true;

    /// <summary>
    /// Raised with the target URL whenever the guard stopped a navigation (the user chose to stay, or no
    /// confirmation dialog is available).
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNavigationBlocked { get; set; }

    /// <summary>The phrases the component shows. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyUnsavedChangesText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-unsaved";

    private CyUnsavedChangesText Strings => Text ?? s_defaultText;

    private string? IndicatorText
    {
        get
        {
            if (SaveState == CySaveState.Saving)
            {
                return Strings.Saving;
            }

            if (SaveState == CySaveState.Error)
            {
                return Strings.Error;
            }

            if (Dirty)
            {
                return Strings.Unsaved;
            }

            if (SaveState == CySaveState.Saved)
            {
                return SavedAt is { } at
                    ? string.Format(CultureInfo.CurrentCulture, Strings.SavedAt, at.ToString("t", CultureInfo.CurrentCulture))
                    : Strings.Saved;
            }

            return null;
        }
    }

    private string IconName => SaveState switch
    {
        CySaveState.Saving => "loading",
        CySaveState.Error => "error-circle",
        _ when Dirty => "edit",
        _ => "check"
    };

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-unsaved--saving", SaveState == CySaveState.Saving)
            .AddClass("cy-unsaved--error", SaveState == CySaveState.Error)
            .AddClass("cy-unsaved--dirty", Dirty && SaveState != CySaveState.Saving && SaveState != CySaveState.Error)
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            if (!_disposed)
            {
                _registration = Navigation.RegisterLocationChangingHandler(OnLocationChangingAsync);
            }
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException)
        {
            // Prerendering or a host without navigation interception: the guard is simply unavailable.
        }
    }

    private async ValueTask OnLocationChangingAsync(LocationChangingContext context)
    {
        if (_disposed || !Dirty || IsSamePageFragment(context.TargetLocation))
        {
            return;
        }

        var leave = false;
        var confirm = Services.GetService(typeof(ICyConfirmService)) as ICyConfirmService;

        if (confirm is not null)
        {
            try
            {
                leave = await confirm.ConfirmAsync(
                    new CyConfirmOptions
                    {
                        Title = Strings.ConfirmTitle,
                        Message = string.IsNullOrWhiteSpace(Message) ? Strings.ConfirmMessage : Message,
                        ConfirmText = Strings.Leave,
                        CancelText = Strings.Stay,
                        Destructive = true
                    },
                    context.CancellationToken);
            }
            catch (InvalidOperationException)
            {
                // No CyConfirmDialog host on the page: fall back to blocking.
            }
        }

        if (leave)
        {
            return;
        }

        context.PreventNavigation();

        if (OnNavigationBlocked.HasDelegate)
        {
            await OnNavigationBlocked.InvokeAsync(context.TargetLocation);
        }
    }

    private bool IsSamePageFragment(string target)
    {
        var current = Navigation.Uri;
        var currentBase = StripFragment(current);

        return string.Equals(
            StripFragment(Navigation.ToAbsoluteUri(target).ToString()),
            currentBase,
            StringComparison.Ordinal);
    }

    private static string StripFragment(string uri)
    {
        var hash = uri.IndexOf('#', StringComparison.Ordinal);

        return hash < 0 ? uri : uri[..hash];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Dirty = false;
        GC.SuppressFinalize(this);

        _registration?.Dispose();
        _registration = null;
    }
}
