using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Dialogs;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util;
using ScreenToGif.ViewModel;
using ScreenToGif.ViewModel.Presets.Export;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenToGif.Views;

public partial class PresetSettings : ExWindow
{
    private readonly PresetSettingsViewModel _viewModel = new();

    public List<ExportPresetViewModel> Presets { get; set; }

    public PresetSettings(List<ExportPresetViewModel> presets, ExportPresetViewModel selected)
    {
        InitializeComponent();

        DataContext = _viewModel;

        _viewModel.Presets = new ObservableCollection<ExportPresetViewModel>(presets);
        _viewModel.ExportFormat = selected.Type;
        _viewModel.SelectedPreset = selected;
        _viewModel.IsLoading = false;

        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.AddPresetCommand, AddPreset_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.DuplicatePresetCommand, DuplicatePreset_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.ResetPresetCommand, ResetPreset_Executed, (_, args) => args.CanExecute = _viewModel.SelectedPreset is { IsDefault: true }),
            new CommandBinding(_viewModel.RemovePresetCommand, RemovePreset_Executed, (_, args) => args.CanExecute = _viewModel.SelectedPreset is { IsDefault: false }),

            new CommandBinding(_viewModel.SelectFolderCommand, SelectFolder_Executed, (_, args) => args.CanExecute = _viewModel.SelectedPreset?.PickLocation == true),
        });
    }

    private void SelectFolder_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            var output = _viewModel.SelectedPreset.OutputFolder ?? "";

            if (output.ToCharArray().Any(x => Path.GetInvalidPathChars().Contains(x)))
                output = "";

            //It's only a relative path if not null/empty and there's no root folder declared.
            var isRelative = !string.IsNullOrWhiteSpace(output) && !Path.IsPathRooted(output);
            var notAlt = !string.IsNullOrWhiteSpace(output) && _viewModel.SelectedPreset.OutputFolder.Contains(Path.DirectorySeparatorChar);

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

                _viewModel.SelectedPreset.OutputFolder = fs.SelectedPath;

                #endregion
            }
            else
            {
                #region Select folder and file

                var sfd = new SaveFileDialog
                {
                    FileName = _viewModel.SelectedPreset.OutputFilename,
                    InitialDirectory = isRelative ? Path.GetFullPath(initial) : initial
                };

                #region Extensions

                switch (_viewModel.SelectedPreset.Type)
                {
                    //Animated image.
                    case ExportFormats.Apng:
                        sfd.Filter = string.Format("{0}|*.png|{0}|*.apng", LocalizationHelper.Get("S.Editor.File.Apng"));
                        sfd.DefaultExt = _viewModel.SelectedPreset.Extension ?? ".png";
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
                        sfd.DefaultExt = _viewModel.SelectedPreset.Extension ?? _viewModel.SelectedPreset.DefaultExtension ?? ".bmp";
                        break;
                    case ExportFormats.Jpeg:
                        sfd.Filter = string.Format("{0}|*.jpg|{0}|*.jpeg|{1} (.zip)|*.zip", LocalizationHelper.Get("S.Editor.File.Image.Jpeg"), LocalizationHelper.Get("S.Editor.File.Project.Image.Zip"));
                        sfd.DefaultExt = _viewModel.SelectedPreset.Extension ?? _viewModel.SelectedPreset.DefaultExtension ?? ".jpg";
                        break;
                    case ExportFormats.Png:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Image.Png")} (.png)|*.png|{LocalizationHelper.Get("S.Editor.File.Project.Image.Zip")} (.zip)|*.zip";
                        sfd.DefaultExt = _viewModel.SelectedPreset.Extension ?? _viewModel.SelectedPreset.DefaultExtension ?? ".png";
                        break;

                    //Other.
                    case ExportFormats.Stg:
                        sfd.Filter = $"{LocalizationHelper.Get("S.Editor.File.Project")} (.stg)|*.stg|{LocalizationHelper.Get("S.Editor.File.Project.Zip")} (.zip)|*.zip";
                        sfd.DefaultExt = _viewModel.SelectedPreset.Extension ?? ".stg";
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

                _viewModel.SelectedPreset.OutputFolder = Path.GetDirectoryName(sfd.FileName);
                _viewModel.SelectedPreset.OutputFilename = Path.GetFileNameWithoutExtension(sfd.FileName);
                _viewModel.SelectedPreset.OverwriteMode = File.Exists(sfd.FileName) ? OverwriteModes.Prompt : OverwriteModes.Warn;
                _viewModel.SelectedPreset.Extension = Path.GetExtension(sfd.FileName);

                #endregion
            }

            //Converts to a relative path again.
            if (isRelative && !string.IsNullOrWhiteSpace(_viewModel.SelectedPreset.OutputFolder))
            {
                var selected = new Uri(_viewModel.SelectedPreset.OutputFolder);
                var baseFolder = new Uri(AppDomain.CurrentDomain.BaseDirectory);
                var relativeFolder = selected.AbsolutePath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar) == baseFolder.AbsolutePath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar) ?
                    "." : Uri.UnescapeDataString(baseFolder.MakeRelativeUri(selected).ToString());

                //This app even returns you the correct slashes/backslashes.
                _viewModel.SelectedPreset.OutputFolder = notAlt ? relativeFolder.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) : relativeFolder.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }
        catch (ArgumentException sx)
        {
            LogWriter.Log(sx, "Error while trying to choose the output path and filename.", _viewModel.SelectedPreset.OutputFolder + _viewModel.SelectedPreset.OutputFilename);

            _viewModel.SelectedPreset.OutputFolder = "";
            _viewModel.SelectedPreset.OutputFilename = "";
            throw;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Error while trying to choose the output path and filename.", _viewModel.SelectedPreset.OutputFolder + _viewModel.SelectedPreset.OutputFilename);
            throw;
        }
    }

    private void PresetsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PresetsListView.SelectedItem != null)
            PresetsListView.ScrollIntoView(PresetsListView.SelectedItem);
    }

    private void AddPreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var newPreset = new AddPreset(_viewModel.ExportFormat, _viewModel.AvailableEncoders)
        {
            Owner = this
        };

        if (newPreset.ShowDialog() != true)
            return;

        _viewModel.AddPreset(newPreset.Preset);
    }

    private void DuplicatePreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.DuplicatePreset();
    }

    private void ResetPreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!Dialog.Ask("S.SaveAs.Presets.Ask.Reset.Instruction", "S.SaveAs.Presets.Ask.Reset.Message"))
            return;

        _viewModel.ResetPreset();
    }

    private void RemovePreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!Dialog.Ask("S.SaveAs.Presets.Ask.Delete.Instruction", "S.SaveAs.Presets.Ask.Delete.Message"))
            return;

        _viewModel.RemovePreset();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _viewModel.PersistSettings();

        Presets = _viewModel.Presets.ToList();
    }
}