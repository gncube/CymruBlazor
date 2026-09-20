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
    /// Pauses the auto-dismiss countdown of a toast, for example while it is hovered or
    /// focused (WCAG 2.2.1 Timing Adjustable). Has no effect on a toast without a countdown.
    /// </summary>
    /// <remarks>
    /// This is a default interface member so existing implementations keep compiling; the default does nothing.
    /// </remarks>
    /// <param name="id">The toast's id.</param>
    void PauseAutoDismiss(Guid id)
    {
    }

    /// <summary>
    /// Resumes a countdown paused by <see cref="PauseAutoDismiss"/> with the time that was left, but never
    /// less than one second so the user can act on the toast they just left.
    /// </summary>
    /// <remarks>The default does nothing.</remarks>
    /// <param name="id">The toast's id.</param>
    void ResumeAutoDismiss(Guid id)
    {
    }

    /// <summary>
    /// Clears all active notifications.
    /// </summary>
    void Clear();
}
