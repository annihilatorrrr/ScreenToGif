using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.Presets.Export;
using ScreenToGif.ViewModel.Project;
using ScreenToGif.ViewModel.Project.Sequences;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenToGif.ViewModel;

public class ExporterViewModel : BaseViewModel, IPreviewerViewModel, IDisposable
{
    private readonly AutoResetEvent _event = new(false);
    private readonly BackgroundWorker _renderWorker = new() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
    private readonly BackgroundWorker _playbackWorker = new() { WorkerSupportsCancellation = true };

    private bool _isLoading;
    private bool _hasImprecisePlayback;
    private bool _isPlaybackEnabled;
    private bool _loopedPlayback;
    private int _playbackFramerate;
    private bool _comesFromRecorder;
    private WriteableBitmap _renderedImage;
    private IntPtr _renderedImageBackBuffer;
    private long _currentTime;
    private long _startTime;
    private long _endTime;
    private ProjectViewModel _project;
    private RecordingProject _projectSource;
    private ExportFormats _exportFormat;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasImprecisePlayback
    {
        get => _hasImprecisePlayback;
        set => SetProperty(ref _hasImprecisePlayback, value);
    }

    public bool IsPlaybackEnabled
    {
        get => _isPlaybackEnabled;
        set
        {
            SetProperty(ref _isPlaybackEnabled, value);

            OnPropertyChanged(nameof(PlayButtonVisibility));
            OnPropertyChanged(nameof(PauseButtonVisibility));
        }
    }

    public bool LoopedPlayback
    {
        get => _loopedPlayback;
        set
        {
            SetProperty(ref _loopedPlayback, value);

            OnPropertyChanged(nameof(LoopedPlaybackVisibility));
            OnPropertyChanged(nameof(LoopedPlaybackOffVisibility));
        }
    }

    public int PlaybackFramerate
    {
        get => _playbackFramerate;
        set => SetProperty(ref _playbackFramerate, value);
    }

    public bool ComesFromRecorder
    {
        get => _comesFromRecorder;
        set
        {
            SetProperty(ref _comesFromRecorder, value);

            OnPropertyChanged(nameof(EditButtonVisibility));
        }
    }

    public WriteableBitmap RenderedImage
    {
        get => _renderedImage;
        set
        {
            SetProperty(ref _renderedImage, value);

            _renderedImageBackBuffer = value.BackBuffer;
        }
    }

    public long StartTime
    {
        get => _startTime;
        set
        {
            SetProperty(ref _startTime, value);

            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TimeLeftAsTimeSpan));
        }
    }

    public long CurrentTime
    {
        get => _currentTime;
        set
        {
            SetProperty(ref _currentTime, value);

            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TimeLeftAsTimeSpan));
            OnPropertyChanged(nameof(CurrentTimeAsTimeSpan));

            if (RenderedImage != null)
                _event.Set();
        }
    }

    public long EndTime
    {
        get => _endTime;
        set
        {
            SetProperty(ref _endTime, value);

            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TimeLeftAsTimeSpan));
        }
    }

    public long TimeLeft => EndTime - CurrentTime - StartTime;

    public TimeSpan CurrentTimeAsTimeSpan => TimeSpan.FromTicks(CurrentTime);

    public TimeSpan TimeLeftAsTimeSpan => TimeSpan.FromTicks(TimeLeft);

    public ProjectViewModel Project
    {
        get => _project;
        set
        {
            SetProperty(ref _project, value);

            var endTime = _project.Tracks.Max(s => s.Sequences.Max(ss => ss.EndTime.Ticks));

            CurrentTime = CurrentTime > EndTime ? EndTime : CurrentTime;
            EndTime = endTime;

            OnPropertyChanged(nameof(Tracks));
            OnPropertyChanged(nameof(HasMouseTrack));
            OnPropertyChanged(nameof(HasKeyboardTrack));
        }
    }

    public RecordingProject ProjectSource
    {
        get => _projectSource;
        set => SetProperty(ref _projectSource, value);
    }

    //TODO: Display tracks as menu items, isChecked == visible
    public ObservableCollection<TrackViewModel> Tracks => Project.Tracks ?? new ObservableCollection<TrackViewModel>();

    public bool HasMouseTrack => Project.Tracks.Any(a => a.Sequences.OfType<CursorSequenceViewModel>().Any());

    public bool HasKeyboardTrack => Project.Tracks.Any(a => a.Sequences.OfType<CursorSequenceViewModel>().Any());

    public Visibility EditButtonVisibility => ComesFromRecorder ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PlayButtonVisibility => IsPlaybackEnabled ? Visibility.Collapsed : Visibility.Visible;

    public Visibility PauseButtonVisibility => IsPlaybackEnabled ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackVisibility => LoopedPlayback ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackOffVisibility => LoopedPlayback ? Visibility.Collapsed : Visibility.Visible;

    //Exporter

    public ExportFormats ExportFormat
    {
        get => _exportFormat;
        set
        {
            SetProperty(ref _exportFormat, value);

            OnPropertyChanged(nameof(Extensions));
            //OnPropertyChanged(nameof(OutputVisibility));
            //OnPropertyChanged(nameof(UploadVisibility));
        }
    }

    public List<ExportPresetViewModel> ExportPresets { get; set; }

    public List<string> Extensions => ExportFormat switch
    {
        ExportFormats.Apng => new List<string> { ".apng", ".png" },
        ExportFormats.Gif => new List<string> { ".gif" },
        ExportFormats.Webp => new List<string> { ".webp" },
        ExportFormats.Avi => new List<string> { ".avi" },
        ExportFormats.Mkv => new List<string> { ".mkv" },
        ExportFormats.Mov => new List<string> { ".mov" },
        ExportFormats.Mp4 => new List<string> { ".mp4" },
        ExportFormats.Webm => new List<string> { ".webm" },
        ExportFormats.Jpeg => new List<string> { ".jpg", ".jpeg", ".zip" },
        ExportFormats.Png => new List<string> { ".png", ".zip" },
        ExportFormats.Bmp => new List<string> { ".bmp", ".zip" },
        ExportFormats.Stg => new List<string> { ".stg", ".zip" },
        ExportFormats.Psd => new List<string> { ".psd" },
        _ => new List<string>()
    };

    //OutputVisibility
    //UploadVisibility

    public ExporterViewModel()
    {
        LoadSettings();

        _renderWorker.DoWork += RenderWorker_DoWork;
        _renderWorker.ProgressChanged += RenderWorker_ProgressChanged;
        _renderWorker.RunWorkerAsync();

        _playbackWorker.DoWork += PlaybackWorder_DoWork;
    }
    
    #region Commands

    public RoutedUICommand PlayPauseCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Space, ModifierKeys.Control) }
    };

    public RoutedUICommand ToggleLoppedPlaybackCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.L, ModifierKeys.Alt) }
    };

    public RoutedUICommand SkipBackwardCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Left, ModifierKeys.Alt) }
    };

    public RoutedUICommand SkipForwardCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Right, ModifierKeys.Alt) }
    };

    public RoutedUICommand LayersCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.L, ModifierKeys.Alt) }
    };

    public RoutedUICommand CropCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.C, ModifierKeys.Alt) }
    };

    public RoutedUICommand TrimCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.T, ModifierKeys.Alt) }
    };

    public RoutedUICommand EditCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.E, ModifierKeys.Alt) }
    };

    public RoutedUICommand SettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.S, ModifierKeys.Alt) }
    };

    public RoutedUICommand MouseSettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.M, ModifierKeys.Alt) }
    };

    public RoutedUICommand KeyboardSettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.K, ModifierKeys.Alt) }
    };

    public RoutedUICommand ExportCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.S, ModifierKeys.Control) }
    };

    public RoutedUICommand CancelCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Escape, ModifierKeys.Control) }
    };

    #endregion

    //Layers
    //  Recording: Frames, Events (cursor, keys);
    //  Editor: Frames, Cursor, Keys, Overlays, etc.
    //  Mark layers as visible
    //Trim
    //  Don't actually trim, just store Start/End time in ticks/long
    //  Enter trim mode, hide top bar (display Accept, Cancel), hide bottom pill (display range selector, with start/end, current preview).
    //  Current preview must be tied to current time.
    //Crop
    //  Don't actually crop, just store rect
    //  Change Redered image after crop, to follow correct size.
    //  Enter crop mode, show adorner with dragging and resizing support.
    //Export
    //  Render, based on Frames framerate?
    //  Get start/end time
    //  Get crop rect

    private void LoadSettings()
    {
        LoopedPlayback = UserSettings.All.ExporterLoopedPlayback;
        PlaybackFramerate = UserSettings.All.ExporterPlaybackFramerate;

        ExportPresets = UserSettings.All.ExportPresets.OfType<ExportPreset>().Select(s => ExportPresetViewModel.FromModel(s, this)).ToList();
        ExportFormat = UserSettings.All.SaveType;
    }

    public void ImportFromRecording(RecordingProject project)
    {
        IsLoading = true;

        Project = ProjectViewModel.FromModel(project, this);
        ComesFromRecorder = true;

        InitializePreview();

        IsLoading = false;
    }

    public void ImportFromEditor(CachedProject project)
    {
        IsLoading = true;

        Project = ProjectViewModel.FromModel(project, this);

        InitializePreview();

        IsLoading = false;
    }

    internal void InitializePreview()
    {
        //Reduce along with crop.
        RenderedImage = new WriteableBitmap(Project.Width, Project.Height, Project.HorizontalDpi, Project.VerticalDpi, PixelFormats.Bgra32, null);

        _event.Set();
    }

    public void Render()
    {
        _event.Set();
    }

    public void Skip(int seconds)
    {
        Pause();

        CurrentTime = Math.Min(Math.Max(CurrentTime + (TimeSpan.FromSeconds(seconds).Ticks), 0), EndTime);
    }

    public void PlayPause()
    {
        if (IsPlaybackEnabled)
        {
            _playbackWorker.CancelAsync();

            IsPlaybackEnabled = false;
            return;
        }

        _playbackWorker.RunWorkerAsync();
    }

    public void Pause()
    {
        _playbackWorker.CancelAsync();
        IsPlaybackEnabled = false;
    }

    public void ToggleLoopedPlayback()
    {
        LoopedPlayback = !LoopedPlayback;
    }

    public void EndPreview()
    {
        _renderWorker.CancelAsync();
        _playbackWorker.CancelAsync();
        _event.Close();

        _renderWorker.DoWork -= RenderWorker_DoWork;
        _renderWorker.ProgressChanged -= RenderWorker_ProgressChanged;
        _playbackWorker.DoWork -= PlaybackWorder_DoWork;
    }

    public void PersistSettings()
    {
        UserSettings.All.ExporterLoopedPlayback = LoopedPlayback;
        UserSettings.All.ExporterPlaybackFramerate = PlaybackFramerate;

        UserSettings.All.SaveType = ExportFormat;
    }

    public void Dispose()
    {
        _event?.Dispose();
        _renderWorker?.Dispose();
        _playbackWorker?.Dispose();
    }
    
    private void RenderWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        while (!_playbackWorker.CancellationPending)
        {
            _event.WaitOne();

            //TODO: Send crop params, to render in time.
            //Send Time + TrimStart
            //Maybe detect if current sequence/subsequence changed, if so rerender frame.

            foreach (var track in Project.Tracks)
                track.RenderAt(_renderedImageBackBuffer, Project.Width, Project.Height, CurrentTime, 100);

            _renderWorker.ReportProgress(1);
        }
    }

    private void RenderWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            RenderedImage.Lock();
            RenderedImage.AddDirtyRect(new Int32Rect(0, 0, RenderedImage.PixelWidth, RenderedImage.PixelHeight));
            RenderedImage.Unlock();
        }, DispatcherPriority.Render);
    }

    private void PlaybackWorder_DoWork(object sender, DoWorkEventArgs e)
    {
        IsPlaybackEnabled = true;
        HasImprecisePlayback = false;

        using (var resolution = new TimerResolution(1))
        {
            if (!resolution.SuccessfullySetTargetResolution)
            {
                LogWriter.Log($"Imprecise timer resolution... Target: {resolution.TargetResolution}, Current: {resolution.CurrentResolution}");
                HasImprecisePlayback = true;
            }

            //TODO: Implement option to control playback speed, based on a set FPS or based on the available timings of the tracks.

            //Reset timing if at the end (or past it).
            //When LoopedPlayback is disable, this is necessary.
            if (CurrentTime >= EndTime)
                CurrentTime = StartTime;

            var sw = new Stopwatch();

            while (!_playbackWorker.CancellationPending)
            {
                sw.Restart();

                if (CurrentTime >= EndTime)
                {
                    if (!LoopedPlayback)
                    {
                        e.Cancel = true;
                        break;
                    }

                    CurrentTime = StartTime;
                }

                CurrentTime += TimeSpan.FromMilliseconds(60).Ticks;

                //Wait rest of actual frame delay time
                if (sw.ElapsedMilliseconds >= 60)
                    continue;

                while (sw.Elapsed.TotalMilliseconds < 60)
                    Thread.Sleep(1);
            }
        }

        IsPlaybackEnabled = false;
    }
}