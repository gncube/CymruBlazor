namespace CymruBlazor.Components.Feedback;

/// <summary>
/// Service managing visual toast notifications.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Event triggered whenever the active toasts collection changes.
    /// </summary>
    event Action? OnChange;

    /// <summary>
    /// Gets the current ordered list of active notifications.
    /// </summary>
    IReadOnlyList<ToastNotification> Toasts { get; }

    /// <summary>
    /// Displays a toast notification.
    /// </summary>
    void Show(string message, ToastVariant variant = ToastVariant.Success, TimeSpan? duration = null);

    /// <summary>
    /// Removes a notification by its unique identifier.
    /// </summary>
    void Remove(Guid id);

    /// <summary>
    /// Clears all active notifications.
    /// </summary>
    void Clear();
}
