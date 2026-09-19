namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// Provides a high-level abstraction for focus management.
/// </summary>
/// <remarks>
/// This is the extension point used by <c>CyFocusTrap</c>. The implementation
/// registered by <c>AddCymruBlazor()</c> (<see cref="FocusManager"/>) is a
/// logging placeholder that does not move focus. To move or restore focus for
/// real, register your own implementation after <c>AddCymruBlazor()</c>:
/// <code>
/// builder.Services.AddCymruBlazor();
/// builder.Services.AddScoped&lt;IFocusManager, MyJsFocusManager&gt;();
/// </code>
/// </remarks>
public interface IFocusManager
{
    /// <summary>Requests focus for the element with the given id.</summary>
    /// <param name="elementId">The DOM <c>id</c> of the element to focus.</param>
    /// <param name="options">Optional focus behaviour.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> FocusAsync(
        string elementId,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Requests focus for a relative destination such as the first or last focusable element.</summary>
    /// <param name="target">The destination to focus.</param>
    /// <param name="options">Optional focus behaviour.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> FocusAsync(
        FocusTarget target,
        FocusOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Requests that focus return to the element that had it before the last focus request.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The outcome of the request.</returns>
    Task<FocusResult> RestoreFocusAsync(
        CancellationToken cancellationToken = default);
}
