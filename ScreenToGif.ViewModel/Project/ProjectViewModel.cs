using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ScreenToGif.ViewModel.Project;

public class ProjectViewModel : BaseViewModel
{
    private string _name = "";
    private int _width = 0;
    private int _height = 0;
    private double _horizontalDpi = 96d;
    private double _verticalDpi = 96d;
    private Brush _background = Brushes.White;
    private ObservableCollection<TrackViewModel> _tracks;
    private readonly IPreviewerViewModel _previewerViewModel;

    public CachedProject Project { get; set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public int Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    public double HorizontalDpi
    {
        get => _horizontalDpi;
        set => SetProperty(ref _horizontalDpi, value);
    }

    public double VerticalDpi
    {
        get => _verticalDpi;
        set => SetProperty(ref _verticalDpi, value);
    }

    public Brush Background
    {
        get => _background;
        set
        {
            SetProperty(ref _background, value);

            PreviewerViewModel?.Render();
        }
    }

    internal IPreviewerViewModel PreviewerViewModel
    {
        get => _previewerViewModel;
        private init => SetProperty(ref _previewerViewModel, value);
    }

    public ObservableCollection<TrackViewModel> Tracks
    {
        get => _tracks;
        set => SetProperty(ref _tracks, value);
    }

    public static ProjectViewModel FromModel(CachedProject project, IPreviewerViewModel previewerViewModel)
    {
        return new ProjectViewModel
        {
            Project = project,
            Name = project.Name,
            Width = project.Width,
            Height = project.Height,
            HorizontalDpi = project.HorizontalDpi,
            VerticalDpi = project.VerticalDpi,
            Background = project.Background,
            PreviewerViewModel = previewerViewModel,
            Tracks = new ObservableCollection<TrackViewModel>(project.Tracks.Select(s => TrackViewModel.FromModel(s, previewerViewModel)).ToList())
        };
    }

    public static ProjectViewModel FromModel(RecordingProject project, IPreviewerViewModel exporterViewModel)
    {
        return new ProjectViewModel
        {
            //Project = project,
            Width = project.Width,
            Height = project.Height,
            HorizontalDpi = project.Dpi,
            VerticalDpi = project.Dpi,
            PreviewerViewModel = exporterViewModel,
            Tracks = TrackViewModel.FromModel(project, exporterViewModel)
        };
    }

    public CachedProject ToModel()
    {
        //This will be used to convert back to the model to save it to disk.
        //It should be called whenever the user does a Ctrl+S or after some time.
        
        return new CachedProject
        {
            Name = Name,
        };
    }
}