using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.ViewModel.Project;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenToGif.ViewModel;

public class ExporterViewModel : BaseViewModel, IEditorViewModel
{
    private readonly AutoResetEvent _event = new(false);
    private readonly BackgroundWorker _worker = new() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

    private bool _isLoading;
    private WriteableBitmap _renderedImage;
    private IntPtr _renderedImageBackBuffer;
    private long _currentTime;
    private long _startTime;
    private long _endTime;
    private ProjectViewModel _project;

    public ExporterViewModel()
    {
        _worker.DoWork += Worker_DoWork;
        _worker.ProgressChanged += Worker_ProgressChanged;
        _worker.RunWorkerAsync();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
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
        }
    }

    //Display tracks as menu items, isChecked == visible
    public ObservableCollection<TrackViewModel> Tracks => Project.Tracks ?? new ObservableCollection<TrackViewModel>();

    public RoutedUICommand PlayPauseCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Space, ModifierKeys.Control) }
    };

    public RoutedUICommand SkipBackwardCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Left, ModifierKeys.Alt) }
    };

    public RoutedUICommand SkipForwardCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Right, ModifierKeys.Alt) }
    };

    public RoutedUICommand ExportCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.S, ModifierKeys.Control) }
    };

    public RoutedUICommand CancelCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.Escape, ModifierKeys.Control) }
    };

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

    public void ImportFromRecording(RecordingProject project)
    {
        IsLoading = true;

        Project = ProjectViewModel.FromModel(project, this);

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
        CurrentTime = Math.Min(Math.Max(CurrentTime + (TimeSpan.FromSeconds(seconds).Ticks), 0), EndTime);
    }

    public void EndPreview()
    {
        _worker.CancelAsync();
        _worker.Dispose();
        _event.Close();
        _event.Dispose();

        _worker.DoWork -= Worker_DoWork;
        _worker.ProgressChanged -= Worker_ProgressChanged;
    }

    private void Worker_DoWork(object sender, DoWorkEventArgs e)
    {
        while (!e.Cancel)
        {
            _event.WaitOne();

            //TODO: Send crop params, to render in time.
            foreach (var track in Project.Tracks)
                track.RenderAt(_renderedImageBackBuffer, Project.Width, Project.Height, CurrentTime, 100);

            _worker.ReportProgress(1);
        }
    }

    private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            RenderedImage.Lock();
            RenderedImage.AddDirtyRect(new Int32Rect(0, 0, RenderedImage.PixelWidth, RenderedImage.PixelHeight));
            RenderedImage.Unlock();
        }, DispatcherPriority.Render);
    }
}