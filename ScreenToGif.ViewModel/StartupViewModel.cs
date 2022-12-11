using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;
using ScreenToGif.Util.Settings;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class StartupViewModel : BaseViewModel, IDisposable
{
    private bool _isLoading = true;
    private CancellationTokenSource _tokenSource;
    private ObservableCollection<RecentProjectViewModel> _recentProjects = new();

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            SetProperty(ref _isLoading, value);

            OnPropertyChanged(nameof(NoItemsTextBlockVisibility));
        }
    }

    public ObservableCollection<RecentProjectViewModel> RecentProjects
    {
        get => _recentProjects;
        set
        {
            SetProperty(ref _recentProjects, value);

            OnPropertyChanged(nameof(NoItemsTextBlockVisibility));
            OnPropertyChanged(nameof(TitleTextBlockVisibility));
        }
    }

    public Visibility NoItemsTextBlockVisibility => !IsLoading && RecentProjects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility TitleTextBlockVisibility => RecentProjects.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public RoutedUICommand ScreenRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewRecording",
        InputGestures = { new KeyGesture(Key.N, ModifierKeys.Control) }
    };

    public RoutedUICommand WebcamRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewWebcamRecording",
        InputGestures = { new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift) }
    };

    public RoutedUICommand BoardRecorderCommand { get; set; } = new()
    {
        Text = "S.Command.NewBoardRecording",
        InputGestures = { new KeyGesture(Key.B, ModifierKeys.Control | ModifierKeys.Shift) }
    };

    public RoutedUICommand EditorCommand { get; set; } = new()
    {
        Text = "S.Command.Editor",
        InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public RoutedUICommand OptionsCommand { get; set; } = new()
    {
        Text = "S.Command.Options",
        InputGestures = { new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public RoutedUICommand UpdateCommand { get; set; } = new()
    {
        Text = "S.Command.Update",
        InputGestures = { new KeyGesture(Key.U, ModifierKeys.Control | ModifierKeys.Alt) }
    };

    public RoutedUICommand LoadRecentCommand { get; set; } = new()
    {
        Text = "S.Command.Load",
        InputGestures = { new KeyGesture(Key.L, ModifierKeys.Control) }
    };

    public RoutedUICommand ExportProjectCommand { get; set; } = new();

    public RoutedUICommand EditProjectCommand { get; set; } = new();

    public RoutedUICommand RemoveProjectCommand { get; set; } = new();

    public async Task LoadProjects()
    {
        _tokenSource?.Cancel();
        _tokenSource = new CancellationTokenSource();

        RecentProjects.Clear();

        await Task.Factory.StartNew(() =>
        {
            var found = new List<RecentProjectViewModel>();

            try
            {
                var path = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif");
                var recordingsPath = Path.Combine(path, "Recordings");
                var projectsPath = Path.Combine(path, "Projects");
                var recordingsOldPath = Path.Combine(path, "Recording");
                
                if (Directory.Exists(recordingsPath))
                {
                    foreach (var file in Directory.GetDirectories(recordingsPath))
                    {
                        var parsed = RecentProjectViewModel.FromPath(file, ExportProjectCommand, EditProjectCommand, RemoveProjectCommand);

                        if (parsed != null)
                            found.Add(parsed);
                    }
                }

                if (Directory.Exists(projectsPath))
                {
                    foreach (var file in Directory.GetDirectories(projectsPath))
                    {
                        var parsed = RecentProjectViewModel.FromPath(file, ExportProjectCommand, EditProjectCommand, RemoveProjectCommand);

                        if (parsed != null)
                            found.Add(parsed);
                    }
                }

                if (Directory.Exists(recordingsOldPath))
                {
                    foreach (var file in Directory.GetDirectories(recordingsOldPath))
                    {
                        var parsed = RecentProjectViewModel.FromPath(file, ExportProjectCommand, EditProjectCommand, RemoveProjectCommand);

                        if (parsed != null)
                            found.Add(parsed);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Loading list of recent projects");
            }
            finally
            {
                RecentProjects = new ObservableCollection<RecentProjectViewModel>(found.OrderByDescending(o => o.CreationDate));

                IsLoading = false;

                OnPropertyChanged(nameof(TitleTextBlockVisibility));
            }
        }, _tokenSource.Token);
    }

    public void RemoveProject(RecentProjectViewModel item)
    {
        try
        {
            Directory.Delete(item.Path, true);

            RecentProjects.Remove(item);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Removing recent project", item?.Path);
        }
    }

    public void Dispose() => _tokenSource?.Dispose();
}