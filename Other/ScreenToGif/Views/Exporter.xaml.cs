using ScreenToGif.Controls;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.Util;
using ScreenToGif.ViewModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Views;

//Loading (disable all controls)
//Trim
//Layering
//  Needs to have at least one layer to be able to export.
//  CanExport => layers.Any(a => a.IsVisible);
//Crop
//  Can reduce area to a minimum or 10x10.
//Export options
//  Load/save the settings used.
//Actual exporting experience.
//  Get the ProjectViewModel and start sending frames to the renderer.
//  Display current progress in the UI
//Fix localization (add tooltips).

public partial class Exporter : ExWindow
{
    private readonly ExporterViewModel _viewModel = new();

    public bool OpenInEditor { get; set; }

    public Exporter()
    {
        InitializeComponent();

        DataContext = _viewModel;

        //TODO: Register more Commands
        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.PlayPauseCommand, PlayPause_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.ToggleLoppedPlaybackCommand, ToggleLoppedPlayback_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.SkipBackwardCommand, SkipBackward_Executed, (_, args) => args.CanExecute = _viewModel.CurrentTime > 0),
            new CommandBinding(_viewModel.SkipForwardCommand, SkipForward_Executed, (_, args) => args.CanExecute = _viewModel.CurrentTime < _viewModel.EndTime),

            new CommandBinding(_viewModel.LayersCommand, Layers_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.CropCommand, Crop_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.TrimCommand, Trim_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.EditCommand, Edit_Executed, (_, args) => args.CanExecute = _viewModel.ComesFromRecorder),

            new CommandBinding(_viewModel.SettingsCommand, Settings_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.MouseSettingsCommand, SkipForward_Executed, (_, args) => args.CanExecute = _viewModel.HasMouseTrack),
            new CommandBinding(_viewModel.KeyboardSettingsCommand, SkipForward_Executed, (_, args) => args.CanExecute = _viewModel.HasKeyboardTrack),

            new CommandBinding(_viewModel.PresetSettingsCommand, PresetSettings_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.UploadPresetSettingsCommand, UploadPresetSettings_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.UploadFile == true),
            new CommandBinding(_viewModel.FileAutomationSettingsCommand, FileAutomationSettings_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),
            
            new CommandBinding(_viewModel.ExportCommand, Export_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.CancelCommand, Cancel_Executed, (_, args) => args.CanExecute = true),
        });
    }

    private void PlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.PlayPause();
    }

    private void ToggleLoppedPlayback_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.ToggleLoopedPlayback();
    }

    private void SkipBackward_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.Skip(-10);
    }

    private void SkipForward_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.Skip(10);
    }

    private void Layers_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //_viewModel.L

        //Enter layer selection mode
        //Hide Exporter part
        //Show controls there instead
    }

    private void Crop_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //Enter crop mode
        //Hide Exporter part
        //Show controls there instead
    }

    private void Trim_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //Enter trim mode
        //Hide Exporter part
        //Show controls there instead
    }

    private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        OpenInEditor = true;

        Close();
    }

    private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //Open settings (maybe redirect to exporter tab, if it exists)
        //On return, refresh page.

        _viewModel.PersistSettings();

        var settings = new Options();

        if (settings.ShowDialog() == true)
            _viewModel.LoadSettings();
    }

    private void PresetSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.PersistSettings();

        var settings = new PresetSettings(_viewModel.SelectedExportPreset);

        if (settings.ShowDialog() == true)
            _viewModel.LoadSettings();
    }

    private void UploadPresetSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.PersistSettings();

        var settings = new PresetSettings(_viewModel.SelectedExportPreset);

        if (settings.ShowDialog() == true)
            _viewModel.LoadSettings();
    }

    private void FileAutomationSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //Open window, passing preset.
        //On return, decide how to show data
    }

    private void Export_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //How to export?
        //  Get the frames in order to send?
        //  Maybe just send the whole project view model and let the exporter render each frame.
        //  The exporter needs to know the framerate.
        //      Maybe: let the user pick: Follow main track framerate or set manually.
        //          When setting manually, I can try detecting if there's any change in the frame.
        //  How heavy would be to render on the spot?
    }

    private void Cancel_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //If comming from recorder? (or if only window open)
        //  Call Launch to open the default window? ++
        //  Reopen source window? What if that source window is also closed? --
        //Else
        //  JUst close this window, as the editor will still be there.

        Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //Tries to adjust the position/size of the window, centers on screen otherwise.
        if (!UpdatePositioning())
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void Previewer_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        //Pause/Play?
        _viewModel.Render();
    }

    private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.Pause();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _viewModel.EndPreview();
        _viewModel.PersistSettings();

        UserSettings.All.ExporterTop = Top;
        UserSettings.All.ExporterLeft = Left;
        UserSettings.All.ExporterWidth = Width;
        UserSettings.All.ExporterHeight = Height;
        UserSettings.All.ExporterWindowState = WindowState;
        UserSettings.Save();
    }
    
    public void LoadRecordingProject(RecordingProject project)
    {
        _viewModel.ImportFromRecording(project);
    }

    public async void LoadRecordingProject(string path)
    {
        await _viewModel.ImportFromRecording(path);
    }

    public void LoadCachedProject(CachedProject project)
    {
        _viewModel.ImportFromEditor(project);
    }

    private bool UpdatePositioning(bool onLoad = true)
    {
        //TODO: When the DPI changes, these values are still from the latest dpi.
        var top = onLoad ? UserSettings.All.ExporterTop : Top;
        var left = onLoad ? UserSettings.All.ExporterLeft : Left;
        var width = onLoad ? UserSettings.All.ExporterWidth : Width;
        var height = onLoad ? UserSettings.All.ExporterHeight : Height;
        var state = onLoad ? UserSettings.All.ExporterWindowState : WindowState;

        //If the position was never set, let it center on screen.
        if (double.IsNaN(top) && double.IsNaN(left))
            return false;

        //The catch here is to get the closest monitor from current Top/Left point.
        var monitors = MonitorHelper.AllMonitorsScaled(this.GetVisualScale());
        var closest = monitors.FirstOrDefault(x => x.Bounds.Contains(new Point((int)left, (int)top))) ?? monitors.FirstOrDefault(x => x.IsPrimary);

        if (closest == null)
            return false;

        //To much to the Left.
        if (closest.WorkingArea.Left > left + width - 100)
            left = closest.WorkingArea.Left;

        //Too much to the top.
        if (closest.WorkingArea.Top > top + height - 100)
            top = closest.WorkingArea.Top;

        //Too much to the right.
        if (closest.WorkingArea.Right < left + 100)
            left = closest.WorkingArea.Right - width;

        //Too much to the bottom.
        if (closest.WorkingArea.Bottom < top + 100)
            top = closest.WorkingArea.Bottom - height;

        if (top is > int.MaxValue or < int.MinValue || left is > int.MaxValue or < int.MinValue || width is > int.MaxValue or < 0 || height is > int.MaxValue or < 0)
        {
            var desc = $"On load: {onLoad}\nScale: {this.GetVisualScale()}\n\n" +
                       $"Screen: {closest.AdapterName}\nBounds: {closest.Bounds}\n\nTopLeft: {top}x{left}\nWidthHeight: {width}x{height}\n\n" +
                       $"TopLeft Settings: {UserSettings.All.ExporterTop}x{UserSettings.All.ExporterLeft}\nWidthHeight Settings: {UserSettings.All.ExporterWidth}x{UserSettings.All.ExporterHeight}";
            LogWriter.Log("Wrong exporter window sizing", desc);
            return false;
        }

        //To eliminate the flicker of moving the window to the correct screen, hide and then show it again.
        if (onLoad)
            Opacity = 0;

        //First move the window to the final monitor, so that the UI scale can be adjusted.
        this.MoveToScreen(closest);

        Top = top;
        Left = left;
        Width = width;
        Height = height;
        WindowState = state;

        if (onLoad)
            Opacity = 1;

        return true;
    }
}