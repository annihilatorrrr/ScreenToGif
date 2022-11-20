using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.Presets.Export;
using System.Collections;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class PresetSettingsViewModel : BaseViewModel
{
    private bool _isLoading = true;
    private bool _isInAddingMode;
    private double _presetSettingsSectionWidth;
    private List<ExportPresetViewModel> _presets;
    private ExportPresetViewModel _selectedPreset;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsInAddingMode
    {
        get => _isInAddingMode;
        set
        {
            SetProperty(ref _isInAddingMode, value);

            OnPropertyChanged(nameof(IsListEnabled));
        }
    }

    public bool IsListEnabled => !IsInAddingMode;

    public double PresetSettingsSectionWidth
    {
        get => _presetSettingsSectionWidth;
        set => SetProperty(ref _presetSettingsSectionWidth, value);
    }

    public List<ExportPresetViewModel> Presets
    {
        get => _presets;
        set => SetProperty(ref _presets, value);
    }

    public ExportPresetViewModel SelectedPreset
    {
        get => _selectedPreset;
        set => SetProperty(ref _selectedPreset, value);
    }

    #region Commands

    public RoutedUICommand AddPresetCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.OemPlus, ModifierKeys.Control) }
    };

    public RoutedUICommand DuplicatePresetCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.D, ModifierKeys.Control) }
    };

    public RoutedUICommand ResetPresetCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.R, ModifierKeys.Control) }
    };

    public RoutedUICommand RemovePresetCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.W, ModifierKeys.Control) }
    };

    #endregion

    public PresetSettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        PresetSettingsSectionWidth = UserSettings.All.PresetSettingsSectionWidth;
        Presets = UserSettings.All.ExportPresets.OfType<ExportPreset>().Select(s => ExportPresetViewModel.FromModel(s, null)).ToList();
    }

    public void PersistSettings()
    {
        UserSettings.All.PresetSettingsSectionWidth = PresetSettingsSectionWidth;
        UserSettings.All.ExportPresets = new ArrayList(Presets.Select(s => s.ToModel()).ToArray());
    }

    public void AddPreset()
    {
        IsInAddingMode = true;

        //Disable list
        //Show Add/Cancel buttons at bottom of the page.

        //After accepting, select item and scroll to it
    }
}