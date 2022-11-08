using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Util;
using ScreenToGif.Util.Extensions;
using System.Windows;

namespace ScreenToGif.ViewModel;

public class ScreenRecorderViewModel : RecorderViewModel
{
    private bool _isDirectMode = false;
    private Rect _selection;
    private double _selectionScale;
    private RegionSelectionModes _selectionMode;
    private IMonitor _currentMonitor;
    private IMonitor _previousMonitor;
    private IMonitor _mainMonitor;
    private List<IMonitor> _monitors;
    private bool _useDesktopDuplication;
    private bool _regionWasForceSelected;
    private int _framerate;
    private bool _isFollowing;
    
    public bool IsDirectMode
    {
        get => _isDirectMode;
        set
        {
            SetProperty(ref _isDirectMode, value);

            OnPropertyChanged(nameof(MaximumBounds));
        }
    }

    public Rect Selection
    {
        get => _selection;
        set
        {
            var equals = _selection.Equals(value);

            SetProperty(ref _selection, value);

            if (equals)
                return;

            OnPropertyChanged(nameof(SelectionScaled));
            OnPropertyChanged(nameof(SizeControlsVisibility));
            OnPropertyChanged(nameof(SizeDetailsVisibility));
            OnPropertyChanged(nameof(MonitorDescription));
            OnPropertyChanged(nameof(PixelWidth));
            OnPropertyChanged(nameof(PixelHeight));
        }
    }

    public double SelectionScale
    {
        get => _selectionScale;
        set
        {
            var nearlyEquals = _selectionScale.NearlyEquals(value);

            SetProperty(ref _selectionScale, value);

            if (nearlyEquals)
                return;

            OnPropertyChanged(nameof(SelectionScaled));
            OnPropertyChanged(nameof(PixelWidth));
            OnPropertyChanged(nameof(PixelHeight));
        }
    }

    public Rect SelectionScaled => Selection.IsEmpty != true ? Selection.Scale(SelectionScale).Offset(MathExtensions.RoundUpValue(SelectionScale)) : Rect.Empty;

    public int PixelWidth
    {
        get => (int)((Selection.Width - 2) * SelectionScale);
        set
        {
            if (!Selection.IsEmpty)
                Selection = Selection with { Width = (value + 2) / SelectionScale };

        }
    }

    public int PixelHeight
    {
        get => (int)((Selection.Height - 2) * SelectionScale);
        set
        {
            if (!Selection.IsEmpty)
                Selection = Selection with { Height = (value + 2) / SelectionScale };
        }
    }

    public RegionSelectionModes SelectionMode
    {
        get => _selectionMode;
        set
        {
            SetProperty(ref _selectionMode, value);

            OnPropertyChanged(nameof(SizeControlsVisibility));
            OnPropertyChanged(nameof(SizeDetailsVisibility));
        }
    }

    public IMonitor CurrentMonitor
    {
        get => _currentMonitor;
        set
        {
            SetProperty(ref _currentMonitor, value);

            OnPropertyChanged(nameof(MonitorDescription));
            OnPropertyChanged(nameof(MonitorDescriptionTooltip));
            OnPropertyChanged(nameof(MaximumBounds));
        }
    }

    public IMonitor PreviousMonitor
    {
        get => _previousMonitor;
        set => SetProperty(ref _previousMonitor, value);
    }

    public Rect MaximumBounds => IsDirectMode && CurrentMonitor != null ? CurrentMonitor.Bounds :
        new Rect(Monitors.Min(m => m.Bounds.X), Monitors.Min(m => m.Bounds.Y), Monitors.Max(m => m.Bounds.Right), Monitors.Max(m => m.Bounds.Bottom));
    
    public IMonitor MainMonitor
    {
        get => _mainMonitor;
        set => SetProperty(ref _mainMonitor, value);
    }

    public List<IMonitor> Monitors
    {
        get => _monitors;
        set
        {
            SetProperty(ref _monitors, value);

            MainMonitor = Monitors.FirstOrDefault(f => f.IsPrimary) ?? Monitors.FirstOrDefault();
        }
    }

    public bool UseDesktopDuplication
    {
        get => _useDesktopDuplication;
        set => SetProperty(ref _useDesktopDuplication, value);
    }

    public bool RegionWasForceSelected
    {
        get => _regionWasForceSelected;
        set => SetProperty(ref _regionWasForceSelected, value);
    }
    
    public int Framerate
    {
        get => _framerate;
        set => SetProperty(ref _framerate, value);
    }

    public bool IsFollowing
    {
        get => _isFollowing;
        set => SetProperty(ref _isFollowing, value);
    }

    public Visibility SizeControlsVisibility => Selection.IsEmpty || SelectionMode == RegionSelectionModes.Fullscreen ? Visibility.Collapsed : Visibility.Visible;

    public Visibility SizeDetailsVisibility => Selection.IsEmpty || SelectionMode == RegionSelectionModes.Fullscreen ? Visibility.Visible : Visibility.Collapsed;

    public string MonitorDescription
    {
        get
        {
            switch (SelectionMode)
            {
                case RegionSelectionModes.Window:
                    return Selection.IsEmpty ? LocalizationHelper.Get("S.Recorder.Window.Select") : null;

                case RegionSelectionModes.Fullscreen:
                {
                    if (Selection.IsEmpty)
                        return LocalizationHelper.Get("S.Recorder.Screen.Select");

                    return CurrentMonitor.FriendlyName;
                }

                default:
                    return Selection.IsEmpty ? LocalizationHelper.Get("S.Recorder.Area.Select") : null;
            }
        }
    }

    public string MonitorDescriptionTooltip
    {
        get
        {
            switch (SelectionMode)
            {
                case RegionSelectionModes.Window:
                    return Selection.IsEmpty ? "No window selected." : null;

                case RegionSelectionModes.Fullscreen:
                {
                    if (Selection.IsEmpty)
                        return "No screen selected.";

                    return
                        LocalizationHelper.GetWithFormat("S.Recorder.Screen.Name.Info", "Display: {0}", CurrentMonitor.FriendlyName) +
                        Environment.NewLine +
                        LocalizationHelper.GetWithFormat("S.Recorder.Screen.Name.Info1", "Graphics adapter: {0}", CurrentMonitor.AdapterName) +
                        Environment.NewLine +
                        LocalizationHelper.GetWithFormat("S.Recorder.Screen.Name.Info2", "Resolution: {0} x {1}", CurrentMonitor.Bounds.Width, CurrentMonitor.Bounds.Height) +
                        (Math.Abs(CurrentMonitor.Scale - 1) > 0.001 ? Environment.NewLine + LocalizationHelper.GetWithFormat("S.Recorder.Screen.Name.Info3", "Native resolution: {0} x {1}", CurrentMonitor.NativeBounds.Width, CurrentMonitor.NativeBounds.Height) : "") +
                        Environment.NewLine +
                        LocalizationHelper.GetWithFormat("S.Recorder.Screen.Name.Info4", "DPI: {0} ({1:0.##}%)", CurrentMonitor.Dpi, CurrentMonitor.Scale * 100d);
                }

                default:
                    return Selection.IsEmpty ? "No region selected." : null;
            }
        }
    }
}