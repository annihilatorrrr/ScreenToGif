using ScreenToGif.Controls;
using ScreenToGif.ViewModel;
using ScreenToGif.ViewModel.Presets.Export;
using System.ComponentModel;
using System.Windows.Input;

namespace ScreenToGif.Views;

public partial class PresetSettings : ExWindow
{
    private readonly PresetSettingsViewModel _viewModel = new();

    public PresetSettings(ExportPresetViewModel preset = null)
    {
        InitializeComponent();

        _viewModel.SelectedPreset = preset;

        DataContext = _viewModel;

        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.AddPresetCommand, AddPreset_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.DuplicatePresetCommand, DuplicatePreset_Executed, (_, args) => args.CanExecute = true),
            new CommandBinding(_viewModel.ResetPresetCommand, ResetPreset_Executed, (_, args) => args.CanExecute = _viewModel.SelectedPreset is { IsDefault: true }),
            new CommandBinding(_viewModel.RemovePresetCommand, RemovePreset_Executed, (_, args) => args.CanExecute = _viewModel.SelectedPreset is { IsDefault: false }),
        });
    }

    private void AddPreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _viewModel.AddPreset();

        //Enter in Add mode
        //Add the preset at the bottom of the list.
        //Select that preset
        //
        //In the details section, user must:
        //Select file type
        //Select encoder
        //Set name + description
        //If it should save the filename automatically
    }

    private void DuplicatePreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void ResetPreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void RemovePreset_Executed(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _viewModel.PersistSettings();
    }
}