using System.Windows;
using System.Windows.Input;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;
using ScreenToGif.Util.Settings;

namespace ScreenToGif.ViewModel;

public class RecorderViewModel : BindableBase
{
    #region Properties

    private RecorderStages _stage = RecorderStages.Stopped;
    private int _frameCount = 1;
    private bool _hasImpreciseCapture;
    private RecordingProject _project;

    /// <summary>
    /// The actual stage of the recording process.
    /// </summary>
    public RecorderStages Stage
    {
        get => _stage;
        set
        {
            SetProperty(ref _stage, value);

            OnPropertyChanged(nameof(CanOpenOptions));
            OnPropertyChanged(nameof(CanSwitchFrequency));
            OnPropertyChanged(nameof(CanRecord));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanSnap));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanStopLarge));
            OnPropertyChanged(nameof(CanDiscard));
        }
    }

    /// <summary>
    /// The capture frequency for the recorder.
    /// </summary>
    public CaptureFrequencies CaptureFrequency
    {
        get => _captureFrequency;
        set
        {
            SetProperty(ref _captureFrequency, value);

            OnPropertyChanged(nameof(CanOpenOptions));
            OnPropertyChanged(nameof(CanSwitchFrequency));
            OnPropertyChanged(nameof(CanRecord));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanSnap));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanStopLarge));
            OnPropertyChanged(nameof(CanDiscard));

            OnPropertyChanged(nameof(UserInteractionWarningVisibility));
        }
    }

    /// <summary>
    /// The frame count of the current recording.
    /// </summary>
    public int FrameCount
    {
        get => _frameCount;
        set
        {
            SetProperty(ref _frameCount, value);

            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanStopLarge));
            OnPropertyChanged(nameof(CanDiscard));

            OnPropertyChanged(nameof(UserInteractionWarningVisibility));
        }
    }

    /// <summary>
    /// True if the capture system cannot detect smaller intervals of time.
    /// </summary>
    public bool HasImpreciseCapture
    {
        get => _hasImpreciseCapture;
        set => SetProperty(ref _hasImpreciseCapture, value);
    }

    /// <summary>
    /// The project information about the current recording.
    /// </summary>
    public RecordingProject Project
    {
        get => _project;
        set
        {
            SetProperty(ref _project, value);

            OnPropertyChanged(nameof(CanSwitchFrequency));
        }
    }

    public bool CanOpenOptions => (Stage != RecorderStages.Recording || CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction) && Stage != RecorderStages.PreStarting;
    public bool CanSwitchFrequency => ((Stage != RecorderStages.Recording || Project == null) || CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction) && Stage != RecorderStages.PreStarting;
    public bool CanRecord => Stage is RecorderStages.Stopped or RecorderStages.Paused && CaptureFrequency != CaptureFrequencies.Manual;
    public bool CanPause => Stage == RecorderStages.Recording && CaptureFrequency != CaptureFrequencies.Manual;
    public bool CanSnap => Stage == RecorderStages.Recording && CaptureFrequency == CaptureFrequencies.Manual;

    public bool CanStop => (Stage == RecorderStages.Recording && (CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction || UserSettings.All.RecorderDisplayDiscard) && FrameCount > 0) || (Stage == RecorderStages.Paused && FrameCount > 0);
    public bool CanStopLarge => (Stage == RecorderStages.Recording && CaptureFrequency != CaptureFrequencies.Manual && CaptureFrequency != CaptureFrequencies.Interaction && !UserSettings.All.RecorderDisplayDiscard) || Stage == RecorderStages.PreStarting;

    public bool CanDiscard => (Stage == RecorderStages.Paused && FrameCount > 0) || (Stage == RecorderStages.Recording && (CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction || UserSettings.All.RecorderDisplayDiscard) && FrameCount > 0);

    public Visibility UserInteractionWarningVisibility => CaptureFrequency == CaptureFrequencies.Interaction && FrameCount == 0 ? Visibility.Visible : Visibility.Collapsed;

    #endregion

    #region Commands

    private KeyGesture _recordKeyGesture = null;
    private KeyGesture _stopKeyGesture = null;
    private KeyGesture _discardKeyGesture = null;
    private CaptureFrequencies _captureFrequency;

    public KeyGesture RecordKeyGesture
    {
        get => _recordKeyGesture;
        set => SetProperty(ref _recordKeyGesture, value);
    }

    public KeyGesture StopKeyGesture
    {
        get => _stopKeyGesture;
        set => SetProperty(ref _stopKeyGesture, value);
    }

    public KeyGesture DiscardKeyGesture
    {
        get => _discardKeyGesture;
        set => SetProperty(ref _discardKeyGesture, value);
    }


    public RoutedUICommand OptionsCommand { get; set; } = new()
    {
        Text = "S.Command.Options",
        InputGestures = { new KeyGesture(UserSettings.All.OptionsShortcut, UserSettings.All.OptionsModifiers) }
    };

    public RoutedUICommand RecordCommand { get; set; } = new()
    {
        Text = "S.Command.Record"
    };

    public RoutedUICommand SnapCommand { get; set; } = new()
    {
        Text = "S.Command.Snap"
    };

    public RoutedUICommand PauseCommand { get; set; } = new()
    {
        Text = "S.Command.PauseCapture"
    };

    public RoutedUICommand StopCommand { get; set; } = new()
    {
        Text = "S.Command.StopCapture"
    };

    public RoutedUICommand StopLargeCommand { get; set; } = new()
    {
        Text = "S.Command.StopCapture"
    };

    public RoutedUICommand DiscardCommand { get; set; } = new()
    {
        Text = "S.Command.DiscardCapture"
    };

    public RoutedUICommand SwitchFrequencyCommand { get; set; } = new()
    {
        Text = "S.Command.SwitchCaptureFrequency",
    };

    #endregion

    public void RefreshKeyGestures()
    {
        try
        {
            RecordKeyGesture = new KeyGesture(UserSettings.All.StartPauseShortcut, UserSettings.All.StartPauseModifiers);
            StopKeyGesture = new KeyGesture(UserSettings.All.StopShortcut, UserSettings.All.StopModifiers);
            DiscardKeyGesture = new KeyGesture(UserSettings.All.DiscardShortcut, UserSettings.All.DiscardModifiers);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to set the key gestures for the recorder.");
        }
    }
}