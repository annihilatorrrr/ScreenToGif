using System.Windows;

namespace ScreenToGif.Domain.Interfaces;

public interface IMonitor
{
    IntPtr Handle { get; }

    Rect Bounds { get; set; }

    Rect NativeBounds { get; set; }

    Rect WorkingArea { get; set; }

    string Name { get; set; }

    string AdapterName { get; set; }

    string FriendlyName { get; set; }

    int Dpi { get; set; }

    double Scale { get; }

    bool IsPrimary { get; set; }


}