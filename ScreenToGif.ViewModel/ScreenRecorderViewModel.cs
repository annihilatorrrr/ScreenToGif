using ScreenToGif.Domain.Interfaces;
using System.Windows;

namespace ScreenToGif.ViewModel;

public class ScreenRecorderViewModel : RecorderViewModel
{
    private Rect _selection;
    private IMonitor _monitor;

    public Rect Selection
    {
        get => _selection;
        set => SetProperty(ref _selection, value);
    }

    public IMonitor Monitor
    {
        get => _monitor;
        set => SetProperty(ref _monitor, value);
    }
}