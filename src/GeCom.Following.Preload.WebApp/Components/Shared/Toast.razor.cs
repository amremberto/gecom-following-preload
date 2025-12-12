using GeCom.Following.Preload.WebApp.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Shared;

/// <summary>
/// Reusable toast component that supports different types of messages (Success, Info, Warning, Error).
/// </summary>
public partial class Toast : ComponentBase
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// The unique identifier for this toast instance.
    /// </summary>
    [Parameter] public string ToastId { get; set; } = "app-toast";

    /// <summary>
    /// The message to display in the toast.
    /// </summary>
    private string Message { get; set; } = string.Empty;

    /// <summary>
    /// The type of toast (Success, Info, Warning, Error).
    /// </summary>
    private ToastType Type { get; set; } = ToastType.Info;

    /// <summary>
    /// Updates the toast message and type, then shows it.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of toast.</param>
    public async Task ShowAsync(string message, ToastType type)
    {
        Message = message;
        Type = type;
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("showBlazorToast", ToastId);
    }

    /// <summary>
    /// Gets the CSS classes for the toast based on its type.
    /// </summary>
    private string GetToastClasses()
    {
        return Type switch
        {
            ToastType.Success => "bg-success-subtle text-dark border border-success",
            ToastType.Info => "bg-info-subtle text-dark border border-info",
            ToastType.Warning => "bg-warning-subtle text-dark border border-warning",
            ToastType.Error => "bg-danger-subtle text-dark border border-danger",
            _ => "bg-info-subtle text-dark border border-info"
        };
    }

    /// <summary>
    /// Gets the CSS classes for the toast header based on its type.
    /// </summary>
    private string GetHeaderClasses()
    {
        return Type switch
        {
            ToastType.Success => "bg-success text-white",
            ToastType.Info => "bg-info text-white",
            ToastType.Warning => "bg-warning text-dark",
            ToastType.Error => "bg-danger text-white",
            _ => "bg-info text-white"
        };
    }

    /// <summary>
    /// Gets the CSS classes for the close button based on its type.
    /// </summary>
    private string GetCloseButtonClasses()
    {
        return Type switch
        {
            ToastType.Warning => "", // Warning uses dark text, so no white close button
            _ => "btn-close-white"
        };
    }

    /// <summary>
    /// Gets the title text for the toast header based on its type.
    /// </summary>
    private string GetTitle()
    {
        return Type switch
        {
            ToastType.Success => "Éxito",
            ToastType.Info => "Información",
            ToastType.Warning => "Advertencia",
            ToastType.Error => "Error",
            _ => "Información"
        };
    }
}
