using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Project;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.Project;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenToGif.ViewModel.Editor;

public partial class EditorViewModel : BaseViewModel, IPreviewerViewModel
{
    #region Variables

    private readonly AutoResetEvent _event = new(false);
    private readonly BackgroundWorker _renderWorker = new() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
    private readonly BackgroundWorker _playbackWorker = new() { WorkerSupportsCancellation = true };

    private ProjectViewModel _project;
    private WriteableBitmap _renderedImage;
    private IntPtr _renderedImageBackBuffer;
    private long _currentTime;
    private long _startTime;
    private long _endTime;
    private bool _hasImprecisePlayback;
    private bool _isPlaybackEnabled;
    private bool _loopedPlayback;
    private int _playbackFramerate;
    private double _zoom = 1d;
    private double _quality = 1d;
    private double _viewportTop = 0d;
    private double _viewportLeft = 0d;
    private double _viewportWidth = 0d;
    private double _viewportHeigth = 0d;
    private bool _isLoading;

    private GridLength _timelineHeight = new(UserSettings.All.TimelineHeight, GridUnitType.Pixel);
    private readonly GridLength _minTimelineHeight = new(100, GridUnitType.Pixel);

    private ExportViewModel _export;

    //Erase it later.
    private ObservableCollection<FrameViewModel> _frames = new();

    #endregion

    #region Properties

    public ProjectViewModel Project
    {
        get => _project;
        set
        {
            SetProperty(ref _project, value);

            OnPropertyChanged(nameof(HasProject));
            OnPropertyChanged(nameof(TimelineHeight));
            OnPropertyChanged(nameof(MinTimelineHeight));
        }
    }

    public long CurrentTime
    {
        get => _currentTime;
        set
        {
            SetProperty(ref _currentTime, value);

            if (RenderedImage != null)
                _event.Set();
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

    public WriteableBitmap RenderedImage
    {
        get => _renderedImage;
        set
        {
            SetProperty(ref _renderedImage, value);

            _renderedImageBackBuffer = value.BackBuffer;
        }
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

    public double Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, value);
    }

    public double Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public double ViewportTop
    {
        get => _viewportTop;
        set => SetProperty(ref _viewportTop, value);
    }

    public double ViewportLeft
    {
        get => _viewportLeft;
        set => SetProperty(ref _viewportLeft, value);
    }

    public double ViewportWidth
    {
        get => _viewportWidth;
        set => SetProperty(ref _viewportWidth, value);
    }

    public double ViewportHeigth
    {
        get => _viewportHeigth;
        set => SetProperty(ref _viewportHeigth, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasProject => _project != null;

    public GridLength TimelineHeight
    {
        get => HasProject ? _timelineHeight : new GridLength(0);
        set
        {
            SetProperty(ref _timelineHeight, value);

            if (value.IsAbsolute)
                UserSettings.All.TimelineHeight = value.Value;
        }
    }

    public GridLength MinTimelineHeight => HasProject ? _minTimelineHeight : new GridLength(0);

    public Visibility PlayButtonVisibility => IsPlaybackEnabled ? Visibility.Collapsed : Visibility.Visible;

    public Visibility PauseButtonVisibility => IsPlaybackEnabled ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackVisibility => LoopedPlayback ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackOffVisibility => LoopedPlayback ? Visibility.Collapsed : Visibility.Visible;

    public ExportViewModel Export
    {
        get => _export;
        set => SetProperty(ref _export, value);
    }

    #endregion

    #region Methods

    public async Task ImportFromRecording(RecordingProject project)
    {
        IsLoading = true;

        //Show progress.
        //  Create list of progresses.
        //  Pass the created progress reporter.
        //Cancelable.
        //  Pass token.
        //TODO: The conversion is not that difficult anymore.

        var cached = await project.ConvertFromRecordingProject();
        Project = ProjectViewModel.FromModel(cached, this);

        InitializePreview();

        IsLoading = false;
    }

    public async Task ImportFromRecording(string path)
    {
        IsLoading = true;

        //Show progress.
        //  Create list of progresses.
        //  Pass the created progress reporter.
        //Cancelable.
        //  Pass token.
        //TODO: The conversion is not that difficult anymore.

        var cached = await RecordingProjectHelper.ReadFromPath(path);
        Project = ProjectViewModel.FromModel(cached, this);

        InitializePreview();

        IsLoading = false;
    }

    internal void InitializePreview()
    {
        RenderedImage = new WriteableBitmap(Project.Width, Project.Height, Project.HorizontalDpi, Project.VerticalDpi, PixelFormats.Bgra32, null);

        _event.Set();
    }

    public void Render()
    {
        _event.Set();
    }

    private void DrawBackground()
    {
        //Project.Background = new LinearGradientBrush(Colors.Yellow, Colors.Black, new Point(0,0.5), new Point(1,0.5));

        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
            drawingContext.DrawRectangle(Project.Background, null, new Rect(0, 0, Project.Width, Project.Height));

        var target = new RenderTargetBitmap(Project.Width, Project.Height, Project.HorizontalDpi, Project.VerticalDpi, PixelFormats.Pbgra32);
        target.Render(drawingVisual);

        //Size * channels * Bytes per pixel + (height * stride padding?);
        //var buffer = new byte[RenderedImage.BackBufferStride * RenderedImage.PixelHeight];
        //target.CopyPixels(buffer, RenderedImage.BackBufferStride, 0);

        target.CopyPixels(new Int32Rect(0, 0, Project.Width, Project.Height), _renderedImageBackBuffer, RenderedImage.BackBufferStride * RenderedImage.PixelHeight, RenderedImage.BackBufferStride);

        //How to cache this?
        //Maybe simply store in a byte array and leave in memory.
    }

    public void Seek(long timeStamp)
    {
        //Display mode:
        //  By timestamp
        //      Preview is controlled by a timestamp.
        //  By frame selection
        //      Preview is controlled by a timestamp, but users are actually selecting frames (just that each frame has its own frame timestamp).
        //      So the only thing that changes is how the user sees and seeks the recording.

        //By seeking, display the updated info in Statistic tab.
        //Frame count will be hard to know for sure, as multiple sequences can coexist and apart from each other.

        CurrentTime = timeStamp;
    }

    internal void Play()
    {
        //Clock based on a selected fps.
        //Maybe variable? By detecting the sub-sequences.
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

    //How are the frames/data going to be stored in the disk?
    //Project file for the user + opened project should have a cache
    //  Project file for user: I'll need to create a file spec.
    //  Cache folder for the app:

    //As a single cache for each track? (storing as pixel array, to improve performance)
    //I'll need a companion json with positions and other details.
    //I also need to store in memory for faster usage.

    #endregion

    //Pre-render adjustments:
    //  Adjust rendering based on zoom, position, and size.
    //  Quality (maybe, as a plus later).

    //How to render?
    //  Directly to WriteableBitmap address.
    //  Only render what's inside the canvas.
    //  Some sequences can be resized and have a defined rendering size.
    //  Maybe: Viewport details need to be passed along so that the rendering is accurate and within bounds.

    //After rendering?
    //  Cache somehow?
    //      I only need to cache the result frames or the layers.
    //          But since this app will work based on timestamp, how to decide what to render?
    //          Based on changes? Frame event or other event.
    //          Sequences are going to have internal FPS.
    //          Sequence rendering will probably have a high cost, specially because there's tons of data.? 
    //      
    //      MemoryCache
    //      or
    //      CachedContent<T>
    //          Id
    //          IsValid
    //  Invalidate cache
    //      Mark cache list as invalid and request render again.

    //using (var context = RenderedImage.GetBitmapContext())
    //    RenderedImage.DrawRectangle(0, 0, 100, 100, 100);

    //How are previews going to work?
    //  Text rendering
    //  Rendering that needs access to the all layers.
    //  Rendering that changes the size of the canvas.

    //Preview quality.
    //Render the list preview for the frames.

    //Decorator Layer
    //  Needs access to the position, size and angle of the sequence objects.
    //  Altering the size/position/angle needs to directly alter the value of the sequences and subsequences.
    //  Maybe pass Project directly to the decorator layer and let that control read/change the values.

    private void RenderWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        while (!_renderWorker.CancellationPending)
        {
            _event.WaitOne();

            //TODO: Send crop params, to render in time.
            //Send Time + TrimStart
            //Maybe detect if current sequence/subsequence changed, if so rerender frame.

            DrawBackground();

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