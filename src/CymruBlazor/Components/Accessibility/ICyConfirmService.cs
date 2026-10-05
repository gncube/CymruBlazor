namespace CymruBlazor.Components.Accessibility;

/// <summary>
/// Asks the user to confirm an action in a modal dialog and returns their answer.
/// </summary>
/// <remarks>
/// Requires one <see cref="CyConfirmDialog"/> in the page layout (like <c>CyToastContainer</c>); without it
/// <see cref="ConfirmAsync(CyConfirmOptions, CancellationToken)"/> throws <see cref="InvalidOperationException"/>
/// rather than returning a task that never completes. Requests made while a dialog is open are queued and shown in
/// order. Closing the dialog any other way than the confirm button (Escape, the close button, the cancel button or
/// a cancelled token) answers <see langword="false"/>.
/// </remarks>
public interface ICyConfirmService
{
    /// <summary>Shows the dialog and completes with <see langword="true"/> only if the user chose to confirm.</summary>
    /// <param name="options">What to ask.</param>
    /// <param name="cancellationToken">When cancelled, the request is withdrawn and completes with <see langword="false"/>.</param>
    Task<bool> ConfirmAsync(CyConfirmOptions options, CancellationToken cancellationToken = default);
}

/// <summary>Convenience overloads for <see cref="ICyConfirmService"/>.</summary>
public static class CyConfirmServiceExtensions
{
    /// <summary>Asks a yes/no question with the default button text.</summary>
    public static Task<bool> ConfirmAsync(
        this ICyConfirmService service,
        string title,
        string? message = null,
        bool destructive = false,
        string? confirmText = null,
        string? cancelText = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.ConfirmAsync(
            new CyConfirmOptions
            {
                Title = title,
                Message = message,
                Destructive = destructive,
                ConfirmText = confirmText,
                CancelText = cancelText
            },
            cancellationToken);
    }
}
