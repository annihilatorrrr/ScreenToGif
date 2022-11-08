using ScreenToGif.Native.External;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Util;

public static class CursorHelper
{
    public static void SetToPosition(FrameworkElement element, bool centerOnElement = false)
    {
        var relativePoint = centerOnElement ? new Point(element.ActualWidth / 2, element.ActualHeight / 2) : Mouse.GetPosition(element);
        var screenPoint = element.PointToScreen(new Point(0, 0));
        var scale = element.GetVisualScale();

        User32.SetCursorPos((int)(screenPoint.X + relativePoint.X * scale), (int)(screenPoint.Y + relativePoint.Y * scale));
    }
}