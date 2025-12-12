using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service interface for displaying toast notifications.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Shows a success toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    Task ShowSuccessAsync(string message);

    /// <summary>
    /// Shows an information toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    Task ShowInfoAsync(string message);

    /// <summary>
    /// Shows a warning toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    Task ShowWarningAsync(string message);

    /// <summary>
    /// Shows an error toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    Task ShowErrorAsync(string message);

    /// <summary>
    /// Shows a toast message with the specified type.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of toast to display.</param>
    Task ShowAsync(string message, ToastType type);

    /// <summary>
    /// Sets the toast component reference.
    /// </summary>
    /// <param name="toast">The toast component instance.</param>
    void SetToast(Toast? toast);
}
