using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service implementation for displaying toast notifications.
/// </summary>
public class ToastService : IToastService
{
    private Toast? _toast;

    /// <inheritdoc />
    public void SetToast(Toast? toast)
    {
        _toast = toast;
    }

    /// <inheritdoc />
    public async Task ShowSuccessAsync(string message)
    {
        await ShowAsync(message, ToastType.Success);
    }

    /// <inheritdoc />
    public async Task ShowInfoAsync(string message)
    {
        await ShowAsync(message, ToastType.Info);
    }

    /// <inheritdoc />
    public async Task ShowWarningAsync(string message)
    {
        await ShowAsync(message, ToastType.Warning);
    }

    /// <inheritdoc />
    public async Task ShowErrorAsync(string message)
    {
        await ShowAsync(message, ToastType.Error);
    }

    /// <inheritdoc />
    public async Task ShowAsync(string message, ToastType type)
    {
        if (_toast is null)
        {
            throw new InvalidOperationException("Toast component has not been initialized. Make sure to add the <Toast /> component to your page or layout.");
        }

        await _toast.ShowAsync(message, type);
    }
}
