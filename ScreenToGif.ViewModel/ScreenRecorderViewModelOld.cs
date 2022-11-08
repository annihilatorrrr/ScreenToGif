using ScreenToGif.Domain.Interfaces;
using System.Windows;

namespace ScreenToGif.ViewModel;

public class ScreenRecorderViewModelOld : RecorderViewModel
{
    #region Properties

    private bool _isDirectMode = false;
    private Rect _region = Rect.Empty;
    private List<IMonitor> _monitors = new();
    private IMonitor _previousMonitor = null;
    private IMonitor _currentMonitor = null;
    private IMonitor _currentControlMonitor = null;
    private Style _buttonStyle = null;
    
    public bool IsDirectMode
    {
        get => _isDirectMode;
        set
        {
            SetProperty(ref _isDirectMode, value);

            OnPropertyChanged(nameof(MaximumBounds));
        }
    }
        
    public Rect Region
    {
        get => _region;
        set
        {
            var sizeChanged = Math.Abs(_region.Width - value.Width) > 0.01 || Math.Abs(_region.Height - value.Height) > 0.01;

            SetProperty(ref _region, value);

            if (sizeChanged)
            {
                OnPropertyChanged(nameof(RegionWidth));
                OnPropertyChanged(nameof(RegionHeight));
            }
        }
    }

    public double RegionWidth
    {
        get => Region.IsEmpty ? 0 : Region.Width;
        set
        {
            Region = new Rect(Region.Location, new Size(value, Region.Height));

            OnPropertyChanged(nameof(Region));
            OnPropertyChanged(nameof(RegionWidth));
        }
    }

    public double RegionHeight
    {
        get => Region.IsEmpty ? 0 : Region.Height;
        set
        {
            Region = new Rect(Region.Location, new Size(Region.Width, value));

            OnPropertyChanged(nameof(Region));
            OnPropertyChanged(nameof(RegionHeight));
        }
    }

    public List<IMonitor> Monitors
    {
        get => _monitors;
        set => SetProperty(ref _monitors, value);
    }

    public IMonitor CurrentMonitor
    {
        get => _currentMonitor;
        set
        {
            SetProperty(ref _currentMonitor, value);
            OnPropertyChanged(nameof(MaximumBounds));
        }
    }

    public IMonitor PreviousMonitor
    {
        get => _previousMonitor;
        set => SetProperty(ref _previousMonitor, value);
    }

    public IMonitor CurrentControlMonitor
    {
        get => _currentControlMonitor;
        set => SetProperty(ref _currentControlMonitor, value);
    }

    public Rect MaximumBounds => IsDirectMode && CurrentMonitor != null ? CurrentMonitor.Bounds : 
        new Rect(Monitors.Min(m => m.Bounds.X), Monitors.Min(m => m.Bounds.Y), Monitors.Max(m => m.Bounds.Right), Monitors.Max(m => m.Bounds.Bottom));

    public Style ButtonStyle
    {
        get => _buttonStyle;
        set => SetProperty(ref _buttonStyle, value);
    }

    #endregion
}