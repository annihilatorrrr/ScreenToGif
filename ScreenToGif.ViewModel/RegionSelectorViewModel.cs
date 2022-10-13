using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Native;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScreenToGif.ViewModel;

public class RegionSelectorViewModel : BaseViewModel
{
    private RegionSelectionModes _selectionMode = RegionSelectionModes.Region;
    private IMonitor _monitor;
    private List<DetectedRegion> _windows;
    private List<DetectedRegion> _monitors;

    public RegionSelectionModes SelectionMode
    {
        get => _selectionMode;
        set => SetProperty(ref _selectionMode, value);
    }

    public IMonitor Monitor
    {
        get => _monitor;
        set => SetProperty(ref _monitor, value);
    }

    public List<DetectedRegion> Windows
    {
        get => _windows;
        set => SetProperty(ref _windows, value);
    }

    public List<DetectedRegion> Monitors
    {
        get => _monitors;
        set => SetProperty(ref _monitors, value);
    }

    public RoutedUICommand ChangeModeCommand { get; set; } = new();

    public RoutedUICommand CancelCommand { get; set; } = new()
    {
        Text = "S.Command.Cancel",
        InputGestures = { new KeyGesture(Key.Escape) }
    };

    public BitmapSource CaptureBackground(bool addPadding = true)
    {
        //A 7 pixel offset is added to allow the crop by the magnifying glass.
        if (addPadding)
            return Capture.CaptureScreenAsBitmapSource((int)Math.Round((Monitor.Bounds.Width + 14 + 1) * Monitor.Scale), (int)Math.Round((Monitor.Bounds.Height + 14 + 1) * Monitor.Scale),
                (int)Math.Round((Monitor.Bounds.Left - 7) * Monitor.Scale), (int)Math.Round((Monitor.Bounds.Top - 7) * Monitor.Scale));

        return Capture.CaptureScreenAsBitmapSource((int)Math.Round(Monitor.Bounds.Width * Monitor.Scale), (int)Math.Round(Monitor.Bounds.Height * Monitor.Scale),
            (int)Math.Round(Monitor.Bounds.Left * Monitor.Scale), (int)Math.Round(Monitor.Bounds.Top * Monitor.Scale));
    }
}
