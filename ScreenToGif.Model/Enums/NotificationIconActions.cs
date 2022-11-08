namespace ScreenToGif.Domain.Enums;

public enum NotificationIconActions
{
    Nothing = 0,

    /// <summary>
    /// Open a window of choice.
    /// </summary>
    OpenWindow = 1,

    /// <summary>
    /// Toggle Minimize/Maximize all windows.
    /// </summary>
    ToggleWindows = 2,

    /// <summary>
    /// Minimize all windows.
    /// </summary>
    MinimizeWindows = 3,

    /// <summary>
    /// Maximize all windows.
    /// </summary>
    MaximizeWindows = 4,
}