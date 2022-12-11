using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Upload;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Project;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.ExportPresets;
using ScreenToGif.ViewModel.Presets.Export;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Webp;
using ScreenToGif.ViewModel.Presets.Export.Image;
using ScreenToGif.ViewModel.Presets.Export.Other;
using ScreenToGif.ViewModel.Presets.Export.Video.Avi;
using ScreenToGif.ViewModel.Presets.Export.Video.Mkv;
using ScreenToGif.ViewModel.Presets.Export.Video.Mov;
using ScreenToGif.ViewModel.Presets.Export.Video.Mp4;
using ScreenToGif.ViewModel.Presets.Export.Video.Webm;
using ScreenToGif.ViewModel.Presets.Upload;
using ScreenToGif.ViewModel.Project;
using ScreenToGif.ViewModel.Project.Sequences;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenToGif.ViewModel;

public class ExporterViewModel : BaseViewModel, IPreviewerViewModel, IDisposable
{
    #region Variables

    private readonly AutoResetEvent _event = new(false);
    private readonly BackgroundWorker _renderWorker = new() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
    private readonly BackgroundWorker _playbackWorker = new() { WorkerSupportsCancellation = true };

    private bool _isLoading = true;
    private bool _hasImprecisePlayback;
    private bool _isPlaybackEnabled;
    private bool _loopedPlayback;
    private int _playbackFramerate;
    private bool _showEditButton;
    private WriteableBitmap _renderedImage;
    private IntPtr _renderedImageBackBuffer;
    private long _minimumTime;
    private long _startTime;
    private long _currentTime;
    private long _endTime;
    private ProjectViewModel _project;
    private RecordingProject _projectSource;
    private bool _isInLayerMode;

    private GridLength _exporterSectionWidth = new(UserSettings.All.ExporterSectionWidth, GridUnitType.Pixel);
    private ExportFormats _exportFormat;
    private List<ExportPresetViewModel> _exportPresets;
    private List<ExportPresetViewModel> _filteredExportPresets;
    private ExportPresetViewModel _selectedExportPreset;
    private List<UploadPresetViewModel> _uploadPresets;
    private ICollectionView _filteredUploadPresets;

    #endregion

    #region Properties

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

    public bool ShowEditButton
    {
        get => _showEditButton;
        set
        {
            SetProperty(ref _showEditButton, value);

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

    public long MinimumTime
    {
        get => _minimumTime;
        set
        {
            SetProperty(ref _minimumTime, value);

            if (_minimumTime > StartTime)
                StartTime = _minimumTime;
        }
    }

    public long StartTime
    {
        get => _startTime;
        set
        {
            SetProperty(ref _startTime, value);

            if (_startTime > CurrentTime)
                CurrentTime = _startTime;

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

            if (StartTime > _endTime)
                StartTime = _endTime;

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

            //MinimumTime = _project.Tracks.Where(w => w.Sequences.OfType<FrameSequenceViewModel>().Any()).Min(s => s.Sequences.OfType<FrameSequenceViewModel>().Min(ss => ss.Frames.Min(sss => sss.TimeStampInTicks)));
            EndTime = _project.Tracks.Any() ? _project.Tracks.Max(s => s.Sequences.Max(ss => ss.EndTime.Ticks)) : 0;

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

    //Has stroke track.

    public Visibility EditButtonVisibility => ShowEditButton ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PlayButtonVisibility => IsPlaybackEnabled ? Visibility.Collapsed : Visibility.Visible;

    public Visibility PauseButtonVisibility => IsPlaybackEnabled ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackVisibility => LoopedPlayback ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LoopedPlaybackOffVisibility => LoopedPlayback ? Visibility.Collapsed : Visibility.Visible;

    //Layers
    public bool IsInLayerMode
    {
        get => _isInLayerMode;
        set => SetProperty(ref _isInLayerMode, value);
    }

    //Exporter

    public GridLength ExporterSectionWidth
    {
        get => _exporterSectionWidth;
        set
        {
            SetProperty(ref _exporterSectionWidth, value);

            if (value.IsAbsolute)
                UserSettings.All.ExporterSectionWidth = value.Value;
        }
    }

    public ExportFormats ExportFormat
    {
        get => _exportFormat;
        set
        {
            SetProperty(ref _exportFormat, value);

            OnPropertyChanged(nameof(Extensions));
            //OnPropertyChanged(nameof(OutputVisibility));
            //OnPropertyChanged(nameof(UploadVisibility));

            FilterPresets();
        }
    }

    internal List<ExportPresetViewModel> ExportPresets
    {
        get => _exportPresets;
        set => SetProperty(ref _exportPresets, value);
    }

    public List<ExportPresetViewModel> FilteredExportPresets
    {
        get => _filteredExportPresets;
        set
        {
            SetProperty(ref _filteredExportPresets, value);

            PersistCurrentPresets();
        }
    }

    public ExportPresetViewModel SelectedExportPreset
    {
        get => _selectedExportPreset;
        set
        {
            SetProperty(ref _selectedExportPreset, value);

            OnPropertyChanged(nameof(Extensions));

            UsePresetSettings();
            LoadUploadPresets();
        }
    }

    internal List<UploadPresetViewModel> UploadPresets
    {
        get => _uploadPresets;
        set => SetProperty(ref _uploadPresets, value);
    }

    public ICollectionView FilteredUploadPresets
    {
        get => _filteredUploadPresets;
        set
        {
            SetProperty(ref _filteredUploadPresets, value);

            OnPropertyChanged(nameof(IsUploadComboBoxEnabled));
        }
    }

    public bool IsUploadComboBoxEnabled => !FilteredUploadPresets.IsEmpty;

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

    #endregion

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

    public RoutedUICommand PresetSettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.P, ModifierKeys.Alt) }
    };

    public RoutedUICommand UploadPresetSettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.U, ModifierKeys.Alt) }
    };

    public RoutedUICommand FileAutomationSettingsCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.F, ModifierKeys.Alt) }
    };

    public RoutedUICommand SelectFolderCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.O, ModifierKeys.Alt) }
    };

    public RoutedUICommand IncreaseFileNumberCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.OemPlus, ModifierKeys.Alt) }
    };

    public RoutedUICommand DecreaseFileNumberCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.OemPlus, ModifierKeys.Alt) }
    };

    public RoutedUICommand OpenExistingFileCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.O, ModifierKeys.Alt | ModifierKeys.Shift) }
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

    public void LoadSettings()
    {
        LoopedPlayback = UserSettings.All.ExporterLoopedPlayback;
        PlaybackFramerate = UserSettings.All.ExporterPlaybackFramerate;

        ExportPresets = UserSettings.All.ExportPresets.OfType<ExportPreset>().Select(s => ExportPresetViewModel.FromModel(s, this)).ToList();
        UploadPresets = UserSettings.All.UploadPresets.OfType<UploadPreset>().Select(s => UploadPresetViewModel.FromModel(s, this)).ToList();

        ExportFormat = UserSettings.All.SaveType;
    }

    public void ImportFromRecording(RecordingProject project)
    {
        IsLoading = true;

        Project = ProjectViewModel.FromModel(project, this);
        ShowEditButton = true;

        InitializePreview();

        IsLoading = false;
    }

    public async Task ImportFromRecording(string path)
    {
        IsLoading = true;

        var project = await Task.Factory.StartNew(() => RecordingProjectHelper.ReadFromPath(path));

        Project = ProjectViewModel.FromModel(project, this);
        ProjectSource = project;
        ShowEditButton = true;

        InitializePreview();

        IsLoading = false;
    }

    public async Task ImportFromLegacyProject(string path, bool deleteOld = true)
    {
        IsLoading = true;

        var project = await Task.Factory.StartNew(() => LegacyProjectHelper.ReadFromPath(path, deleteOld));

        Project = ProjectViewModel.FromModel(project, this);
        ProjectSource = project;
        ShowEditButton = true;

        InitializePreview();

        IsLoading = false;
    }

    public async Task ImportFromEditor(string path)
    {
        IsLoading = true;

        var project = await Task.Factory.StartNew(() => CachedProjectHelper.ReadFromPath(path));
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

    private void FilterPresets()
    {
        var query = ExportPresets.Where(w => w.Type == ExportFormat);

        if (!Environment.Is64BitProcess)
            query = query.Where(w => w.Encoder != EncoderTypes.Gifski);

        var filtered = query.ToList();

        GenerateDefaultPresets(filtered);
        NormalizePresets(filtered);

        FilteredExportPresets = filtered.OrderBy(o => o.Encoder).ThenBy(t => t.Title).ToList();
        SelectedExportPreset = FilteredExportPresets.FirstOrDefault(f => f.IsSelected) ?? FilteredExportPresets.FirstOrDefault();
    }

    private void GenerateDefaultPresets(ICollection<ExportPresetViewModel> presets)
    {
        switch (ExportFormat)
        {
            //Animated images.
            case ExportFormats.Gif:
            {
                AddDistinct(presets, EmbeddedGifPresetViewModel.Defaults);
                AddDistinct(presets, KGySoftGifPresetViewModel.Defaults);
                AddDistinct(presets, FfmpegGifPresetViewModel.Defaults);

                //Gifski only runs on x64.
                if (Environment.Is64BitProcess)
                    AddDistinct(presets, GifskiGifPresetViewModel.Defaults);

                AddDistinct(presets, SystemGifPresetViewModel.Default);
                break;
            }
            case ExportFormats.Apng:
            {
                AddDistinct(presets, EmbeddedApngPresetViewModel.Default);
                AddDistinct(presets, FfmpegApngPresetViewModel.Defaults);
                break;
            }
            case ExportFormats.Webp:
            {
                AddDistinct(presets, FfmpegWebpPresetViewModel.Defaults);
                break;
            }

            //Videos.
            case ExportFormats.Avi:
            {
                AddDistinct(presets, FfmpegAviPresetViewModel.Default);
                break;
            }
            case ExportFormats.Mkv:
            {
                AddDistinct(presets, FfmpegMkvPresetViewModel.Defaults);
                break;
            }
            case ExportFormats.Mov:
            {
                AddDistinct(presets, FfmpegMovPresetViewModel.Defaults);
                break;
            }
            case ExportFormats.Mp4:
            {
                AddDistinct(presets, FfmpegMp4PresetViewModel.Defaults);
                break;
            }
            case ExportFormats.Webm:
            {
                AddDistinct(presets, FfmpegWebmPresetViewModel.Defaults);
                break;
            }

            //Images.
            case ExportFormats.Jpeg:
            {
                AddDistinct(presets, JpegPresetViewModel.Default);
                break;
            }
            case ExportFormats.Png:
            {
                AddDistinct(presets, PngPresetViewModel.Default);
                break;
            }
            case ExportFormats.Bmp:
            {
                AddDistinct(presets, BmpPresetViewModel.Default);
                break;
            }

            //Other.
            case ExportFormats.Stg:
            {
                AddDistinct(presets, StgPresetViewModel.Default);
                break;
            }
            case ExportFormats.Psd:
            {
                AddDistinct(presets, PsdPresetViewModel.Default);
                break;
            }
        }
    }

    private static void AddDistinct(ICollection<ExportPresetViewModel> current, IEnumerable<IExportPreset> newList)
    {
        foreach (var preset in newList.Where(preset => current.Where(w => w.Type == preset.Type).All(a => a.TitleKey != preset.TitleKey)))
            current.Add((ExportPresetViewModel)preset);
    }

    private static void AddDistinct(ICollection<ExportPresetViewModel> current, IExportPreset newPreset)
    {
        if (current.Where(w => w.Type == newPreset.Type).All(a => a.TitleKey != newPreset.TitleKey))
            current.Add((ExportPresetViewModel)newPreset);
    }

    private void NormalizePresets(List<ExportPresetViewModel> presets)
    {
        foreach (var preset in presets.Where(w => w.IsDefault))
        {
            preset.Title = LocalizationHelper.Get(preset.TitleKey).Replace("{0}", preset.DefaultExtension);
            preset.Description = LocalizationHelper.Get(preset.DescriptionKey);
            preset.OutputFolder ??= Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            preset.OutputFilename = (preset.OutputFilenameKey ?? "").Length <= 0 || !string.IsNullOrWhiteSpace(preset.OutputFilename) ? preset.OutputFilename : LocalizationHelper.Get(preset.OutputFilenameKey);
        }
    }

    private void UsePresetSettings()
    {
        if (SelectedExportPreset == null)
            return;

        if (string.IsNullOrWhiteSpace(SelectedExportPreset.Extension))
            SelectedExportPreset.Extension = SelectedExportPreset.DefaultExtension;
    }

    private void PersistCurrentPresets()
    {
        var list = ExportPresets?.Where(w => w.Type != ExportFormat).ToList() ?? new List<ExportPresetViewModel>();

        list.AddRange(FilteredExportPresets);

        ExportPresets = list.ToList();
    }

    private void LoadUploadPresets()
    {
        if (SelectedExportPreset == null)
            return;

        var type = (SelectedExportPreset.Extension ?? SelectedExportPreset.DefaultExtension) == ".zip" ? ExportFormats.Zip : SelectedExportPreset.Type;
        var list = UploadPresets.Where(w => w.AllowedTypes.Count == 0 || w.AllowedTypes.Contains(type)).ToList();

        //No need to adding grouping when there's no item to be displayed.
        //if (list.Count == 0)
        //{
        //    FilteredUploadPresets = list;
        //    return;
        //}

        //Groups by authentication mode.
        //var lcv = new ListCollectionView(list.OrderBy(o => o.IsAnonymous).ThenBy(t => t.Title).ToList());
        //lcv.GroupDescriptions?.Add(new PropertyGroupDescription("Mode"));

        var aa = CollectionViewSource.GetDefaultView(list.OrderBy(o => o.IsAnonymous).ThenBy(t => t.Title).ToList());
        aa.GroupDescriptions.Add(new PropertyGroupDescription("Mode"));

        FilteredUploadPresets = aa;

        //var previous = preset.UploadService;

        //UploadPresetComboBox.IsEnabled = true;
        //UploadPresetComboBox.ItemsSource = lcv;

        //if (uploadPreset != null && list.Contains(uploadPreset))
        //    preset.UploadService = uploadPreset.Title;
        //else
        //    preset.UploadService = previous;
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

        UserSettings.All.ExportPresets = new ArrayList(ExportPresets.Select(s => s.ToModel()).ToArray());
        //UserSettings.All.UploadPresets = UploadPresets.Select(s => UploadPresetViewModel.ToModel(s)).ToList();

        UserSettings.All.SaveType = ExportFormat;
    }

    public void ChangeFileNumber(int change)
    {
        //If there's no filename declared, show the default one.
        if (string.IsNullOrWhiteSpace(SelectedExportPreset.OutputFilename))
        {
            SelectedExportPreset.OutputFilename = LocalizationHelper.Get(SelectedExportPreset.OutputFilenameKey);
            return;
        }

        var index = SelectedExportPreset.OutputFilename.Length;
        int start = -1, end = -1;

        //Detects the last number in a string.
        foreach (var c in SelectedExportPreset.OutputFilename.Reverse())
        {
            if (char.IsNumber(c))
            {
                if (end == -1)
                    end = index;

                start = index - 1;
            }
            else if (start == index)
                break;

            index--;
        }

        //If there's no number.
        if (end == -1)
        {
            SelectedExportPreset.OutputFilename += $" ({change})";
            return;
        }

        //If it's a negative number, include the signal.
        if (start > 0 && SelectedExportPreset.OutputFilename.Substring(start - 1, 1).Equals("-"))
            start--;

        //Cut, convert, merge.
        if (int.TryParse(SelectedExportPreset.OutputFilename.Substring(start, end - start), out var number))
        {
            var offset = start + number.ToString().Length;

            SelectedExportPreset.OutputFilename = SelectedExportPreset.OutputFilename.Substring(0, start) + (number + change) + SelectedExportPreset.OutputFilename.Substring(offset, SelectedExportPreset.OutputFilename.Length - end);
        }
    }

    public void OpenOutputFile()
    {
        try
        {
            ProcessHelper.StartWithShell(SelectedExportPreset.ResolvedOutputPath);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Open file that already exists using the hyperlink");
        }
    }

    public void Dispose()
    {
        _event?.Dispose();
        _renderWorker?.Dispose();
        _playbackWorker?.Dispose();
    }

    private void RenderWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        while (!_renderWorker.CancellationPending)
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