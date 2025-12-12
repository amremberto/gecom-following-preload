namespace GeCom.Following.Preload.WebApp.Enums;

/// <summary>
/// Defines the different types of toast messages that can be displayed.
/// </summary>
public enum ToastType
{
    /// <summary>
    /// Success toast - typically shown in green.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Information toast - typically shown in blue.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning toast - typically shown in yellow/orange.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error toast - typically shown in red.
    /// </summary>
    Error = 3
}
