using Mediator;

namespace CymruBlazor.Components.Feedback;

/// <summary>
/// Visual variant for toast notifications.
/// </summary>
public enum ToastVariant
{
    Info,
    Success,
    Warning,
    Danger
}

/// <summary>
/// Immutable representation of an active toast notification.
/// </summary>
public sealed record ToastNotification(
    Guid Id,
    string Message,
    ToastVariant Variant,
    TimeSpan Duration);

/// <summary>
/// Mediator notification used to display an accessible toast notification.
/// </summary>
public sealed record ShowToastNotification(
    string Message,
    ToastVariant Variant = ToastVariant.Success,
    TimeSpan? Duration = null) : INotification;
