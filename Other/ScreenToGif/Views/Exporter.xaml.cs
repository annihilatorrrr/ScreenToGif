using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Dialogs;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
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

    public RecordingProject Project => _viewModel.ProjectSource;

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
            new CommandBinding(_viewModel.EditCommand, Edit_Executed, (_, args) => args.CanExecute = _viewModel.ShowEditButton),

            new CommandBinding(_viewModel.SettingsCommand, Settings_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.MouseSettingsCommand, SkipForward_Executed, (_, args) => args.CanExecute = _viewModel.HasMouseTrack),
            new CommandBinding(_viewModel.KeyboardSettingsCommand, SkipForward_Executed, (_, args) => args.CanExecute = _viewModel.HasKeyboardTrack),

            new CommandBinding(_viewModel.PresetSettingsCommand, PresetSettings_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.UploadPresetSettingsCommand, UploadPresetSettings_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.UploadFile == true),
            new CommandBinding(_viewModel.FileAutomationSettingsCommand, FileAutomationSettings_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),

            new CommandBinding(_viewModel.SelectFolderCommand, SelectFolder_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),
            new CommandBinding(_viewModel.IncreaseFileNumberCommand, IncreaseFolder_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),
            new CommandBinding(_viewModel.DecreaseFileNumberCommand, DecreaseFolder_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),
            new CommandBinding(_viewModel.OpenExistingFileCommand, OpenExistingFileCommand_Executed, (_, args) => args.CanExecute = _viewModel.SelectedExportPreset?.PickLocation == true),
            
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

        var settings = new PresetSettings(_viewModel.FilteredExportPresets, _viewModel.SelectedExportPreset)
        {
            Owner = this
        };

        if (settings.ShowDialog() == true)
        {
            _viewModel.FilteredExportPresets = settings.Presets;
            _viewModel.PersistSettings();
        }
    }

    private void UploadPresetSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.PersistSettings();

        var settings = new PresetSettings(_viewModel.FilteredExportPresets, _viewModel.SelectedExportPreset);

        if (settings.ShowDialog() == true)
        {
            _viewModel.FilteredExportPresets = settings.Presets;
            _viewModel.PersistSettings();
        }
    }

    private void FileAutomationSettings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //Open window, passing preset.
        //On return, decide how to show data
    }

    private void SelectFolder_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            var output = _viewModel.SelectedExportPreset.OutputFolder ?? "";

            if (output.ToCharArray().Any(x => Path.GetInvalidPathChars().Contains(x)))
                output = "";

            //It's only a relative path if not null/empty and there's no root folder declared.
            var isRelative = !string.IsNullOrWhiteSpace(output) && !Path.IsPathRooted(output);
            var notAlt = !string.IsNullOrWhiteSpace(output) && _viewModel.SelectedExportPreset.OutputFolder.Contains(Path.DirectorySeparatorChar);

            var initial = Directory.Exists(output) ? output : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                #region Select folder

                var fs = new FolderSelector
                {
                    Description = LocalizationHelper.Get("S.SaveAs.File.SelectFolder"),
                    DefaultFolder = isRelative ? Path.GetFullPath(initial) : initial,
                    SelectedPath = isRelative ? Path.GetFullPath(initial) : initial
                };

                if (!fs.ShowDialog())
                    return;

                _viewModel.SelectedExportPreset.OutputFolder = fs.SelectedPath;
                ChooseLocationButton.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                #endregion
            }
            else
            {
                #region Save folder and file

                var sfd = new SaveFileDialog
                {
                    FileName = _viewModel.SelectedExportPreset.OutputFilename,
                    InitialDirectory = isRelative ? Path.GetFullPath(initial) : initial
                };

                #region Extensions

                switch (_viewModel.SelectedExportPreset.Type)
                {
                    //Animated image.
                    case ExportFormats.Apng:
                        sfd.Filter = string.Format("{0}|*.png|{0}|*.apng", LocalizationHelper.Get("S.Editor.File.Apng"));
                        sfd.DefaultExt = _viewModel.SelectedExportPreset.Extension ?? ".png";
                        break;
                    case ExportFormats.Gif:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Gif")} (.gif)|*.gif";
                        sfd.DefaultExt = ".gif";
                        break;
                    case ExportFormats.Webp:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Webp")} (.webp)|*.webp";
                        sfd.DefaultExt = ".webp";
                        break;

                    //Video.
                    case ExportFormats.Avi:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Avi")} (.avi)|*.avi";
                        sfd.DefaultExt = ".avi";
                        break;
                    case ExportFormats.Mkv:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Mkv")} (.mkv)|*.mkv";
                        sfd.DefaultExt = ".mkv";
                        break;
                    case ExportFormats.Mov:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Mov")} (.mov)|*.mov";
                        sfd.DefaultExt = ".mov";
                        break;
                    case ExportFormats.Mp4:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Mp4")} (.mp4)|*.mp4";
                        sfd.DefaultExt = ".mp4";
                        break;
                    case ExportFormats.Webm:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Webm")} (.webm)|*.webm";
                        sfd.DefaultExt = ".webm";
                        break;

                    //Images.
                    case ExportFormats.Bmp:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Image.Bmp")} (.bmp)|*.bmp|{LocalizationHelper.Get("S.Editor.File.Project.Image.Zip")} (.zip)|*.zip";
                        sfd.DefaultExt = _viewModel.SelectedExportPreset.Extension ?? _viewModel.SelectedExportPreset.DefaultExtension ?? ".bmp";
                        break;
                    case ExportFormats.Jpeg:
                        sfd.Filter = string.Format("{0}|*.jpg|{0}|*.jpeg|{1} (.zip)|*.zip", LocalizationHelper.Get("S.Editor.File.Image.Jpeg"), LocalizationHelper.Get("S.Editor.File.Project.Image.Zip"));
                        sfd.DefaultExt = _viewModel.SelectedExportPreset.Extension ?? _viewModel.SelectedExportPreset.DefaultExtension ?? ".jpg";
                        break;
                    case ExportFormats.Png:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Image.Png")} (.png)|*.png|{LocalizationHelper.Get("S.Editor.File.Project.Image.Zip")} (.zip)|*.zip";
                        sfd.DefaultExt = _viewModel.SelectedExportPreset.Extension ?? _viewModel.SelectedExportPreset.DefaultExtension ?? ".png";
                        break;

                    //Other.
                    case ExportFormats.Stg:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Project")} (.stg)|*.stg|{LocalizationHelper.Get("S.Editor.File.Project.Zip")} (.zip)|*.zip";
                        sfd.DefaultExt = _viewModel.SelectedExportPreset.Extension ?? ".stg";
                        break;
                    case ExportFormats.Psd:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Psd")} (.psd)|*.psd";
                        sfd.DefaultExt = ".psd";
                        break;
                }

                #endregion

                var result = sfd.ShowDialog();

                if (!result.HasValue || !result.Value)
                    return;

                //TODO: process output before setting to property?

                _viewModel.SelectedExportPreset.OutputFolder = Path.GetDirectoryName(sfd.FileName);
                _viewModel.SelectedExportPreset.OutputFilename = Path.GetFileNameWithoutExtension(sfd.FileName);
                _viewModel.SelectedExportPreset.OverwriteMode = File.Exists(sfd.FileName) ? OverwriteModes.Prompt : OverwriteModes.Warn;
                _viewModel.SelectedExportPreset.Extension = Path.GetExtension(sfd.FileName);

                //RaiseSaveEvent();
                //_viewModel.Export();

                #endregion
            }

            //Converts to a relative path again.
            if (isRelative && !string.IsNullOrWhiteSpace(_viewModel.SelectedExportPreset.OutputFolder))
            {
                var selected = new Uri(_viewModel.SelectedExportPreset.OutputFolder);
                var baseFolder = new Uri(AppDomain.CurrentDomain.BaseDirectory);
                var relativeFolder = selected.AbsolutePath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar) == baseFolder.AbsolutePath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar) ?
                    "." : Uri.UnescapeDataString(baseFolder.MakeRelativeUri(selected).ToString());

                //This app even returns you the correct slashes/backslashes.
                _viewModel.SelectedExportPreset.OutputFolder = notAlt ? relativeFolder.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) : relativeFolder.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }
        catch (ArgumentException sx)
        {
            LogWriter.Log(sx, "Error while trying to choose the output path and filename.", _viewModel.SelectedExportPreset.OutputFolder + _viewModel.SelectedExportPreset.OutputFilename);

            _viewModel.SelectedExportPreset.OutputFolder = "";
            _viewModel.SelectedExportPreset.OutputFilename = "";
            throw;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Error while trying to choose the output path and filename.", _viewModel.SelectedExportPreset.OutputFolder + _viewModel.SelectedExportPreset.OutputFilename);
            throw;
        }
    }

    private void IncreaseFolder_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.ChangeFileNumber(1);
    }

    private void DecreaseFolder_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.ChangeFileNumber(-1);
    }

    private void OpenExistingFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.OpenOutputFile();
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


    private void Window_Initialized(object sender, EventArgs e)
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

    public async void LoadRecordingProjectPath(string path)
    {
        await _viewModel.ImportFromRecording(path);
    }

    public async void LoadCachedProjectPath(string path)
    {
        await _viewModel.ImportFromEditor(path);
    }
    
    public async void LoadLegacyProjectPath(string path)
    {
        //TODO: Localize.
        var delete = Dialog.AskStatic("Delete old project?",
            "The selected project will be converted to the new format.\r\nDo you want to delete the old one afterwards?",
            "Delete", "Keep");

        await _viewModel.ImportFromLegacyProject(path, delete);
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