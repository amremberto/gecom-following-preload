using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Shared;

/// <summary>
/// Reusable confirmation dialog component that displays a modal with confirm and cancel buttons.
/// </summary>
public partial class ConfirmDialog : ComponentBase
{
    /// <summary>
    /// Gets or sets whether the dialog is visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets or sets the message to display in the dialog.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback that will be invoked when the user confirms.
    /// </summary>
    private Func<Task>? OnConfirmCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback that will be invoked when the user cancels.
    /// </summary>
    private Func<Task>? OnCancelCallback { get; set; }

    /// <summary>
    /// Shows the confirmation dialog.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="onConfirm">Callback to invoke when user confirms.</param>
    /// <param name="onCancel">Optional callback to invoke when user cancels.</param>
    public async Task<bool> ShowAsync(string message, Func<Task>? onConfirm = null, Func<Task>? onCancel = null)
    {
        Message = message;
        OnConfirmCallback = onConfirm;
        OnCancelCallback = onCancel;
        IsVisible = true;
        StateHasChanged();

        // Create a TaskCompletionSource to wait for user response
        var tcs = new TaskCompletionSource<bool>();

        // Store the TCS so we can complete it when user responds
        _pendingConfirmation = tcs;

        // Return the task that will complete when user responds
        return await tcs.Task;
    }

    private TaskCompletionSource<bool>? _pendingConfirmation;

    /// <summary>
    /// Handles the confirm button click.
    /// </summary>
    private async Task HandleConfirm()
    {
        IsVisible = false;
        StateHasChanged();

        if (OnConfirmCallback != null)
        {
            await OnConfirmCallback();
        }

        // Complete the task with true (confirmed)
        _pendingConfirmation?.SetResult(true);
        _pendingConfirmation = null;
    }

    /// <summary>
    /// Handles the cancel button click.
    /// </summary>
    private async Task HandleCancel()
    {
        IsVisible = false;
        StateHasChanged();

        if (OnCancelCallback != null)
        {
            await OnCancelCallback();
        }

        // Complete the task with false (cancelled)
        _pendingConfirmation?.SetResult(false);
        _pendingConfirmation = null;
    }
}

