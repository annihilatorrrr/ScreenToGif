using ScreenToGif.Domain.ViewModels;
using System.Windows;

namespace ScreenToGif.ViewModel;

public class SetupDialogViewModel : BaseViewModel
{
    private int _startupWindow = 0;
    private int _afterRecording = 0;

    /// <summary>
    /// First window to appear when openning app.
    /// 0: Screen recorder
    /// 1: Webcam recorder
    /// 2: Sketchboard recorder
    /// 3: Editor
    /// 4: Welcome
    /// 5: Nothing
    /// </summary>
    public int StartupWindow
    {
        get => _startupWindow;
        set
        {
            SetProperty(ref _startupWindow, value);
            OnPropertyChanged(nameof(SecondStepVisibility));
        }
    }

    public int AfterRecording
    {
        get => _afterRecording;
        set => SetProperty(ref _afterRecording, value);
    }

    public Visibility SecondStepVisibility => StartupWindow is 4 or 5 ? Visibility.Visible : Visibility.Collapsed;
}