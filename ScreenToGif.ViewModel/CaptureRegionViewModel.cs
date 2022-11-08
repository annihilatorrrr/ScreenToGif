using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.ViewModel;

public class CaptureRegionViewModel : BaseViewModel
{
    private Visibility _thirdsGuidelineVisibility;
    private Visibility _crosshairGuidelineVisibility;
    private Brush _regionSelectionBrush;
    private Brush _thirdsGuidelineBrush;
    private Brush _crosshairGuidelineBrush;
    private RegionSelectionModes _mode;
    private Rect _selection;
    private bool _displayPanner;
    private WindowState _windowState;

    public Visibility ThirdsGuidelineVisibility
    {
        get => _thirdsGuidelineVisibility;
        set => SetProperty(ref _thirdsGuidelineVisibility, value);
    }

    public Visibility CrosshairGuidelineVisibility
    {
        get => _crosshairGuidelineVisibility;
        set => SetProperty(ref _crosshairGuidelineVisibility, value);
    }

    public Brush RegionSelectionBrush
    {
        get => _regionSelectionBrush;
        set => SetProperty(ref _regionSelectionBrush, value);
    }

    public Brush ThirdsGuidelineBrush
    {
        get => _thirdsGuidelineBrush;
        set => SetProperty(ref _thirdsGuidelineBrush, value);
    }

    public Brush CrosshairGuidelineBrush
    {
        get => _crosshairGuidelineBrush;
        set => SetProperty(ref _crosshairGuidelineBrush, value);
    }

    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            SetProperty(ref _windowState, value);

            OnPropertyChanged(nameof(Opacity));
        }
    }

    public RegionSelectionModes Mode
    {
        get => _mode;
        set
        {
            SetProperty(ref _mode, value);

            OnPropertyChanged(nameof(IsStatic));
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public Rect Selection
    {
        get => _selection;
        set => SetProperty(ref _selection, value);
    }

    public bool DisplayPanner
    {
        get => _displayPanner;
        set
        {
            SetProperty(ref _displayPanner, value);

            OnPropertyChanged(nameof(IsStatic));
        }
    }

    public bool IsStatic => !DisplayPanner || Mode == RegionSelectionModes.Fullscreen;

    public double Opacity => Mode == RegionSelectionModes.Fullscreen || WindowState == WindowState.Minimized ? 0 : 1;
}