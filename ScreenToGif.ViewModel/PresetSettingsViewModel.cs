using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.Presets.Export;
using ScreenToGif.ViewModel.Presets.Upload;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class PresetSettingsViewModel : BaseViewModel
{
    private bool _isLoading = true;
    private bool _isInAddingMode;
    private ExportFormats _exportFormat;
    private ObservableCollection<ExportPresetViewModel> _presets;
    private ExportPresetViewModel _selectedPreset;
    private GridLength _presetSectionWidth = new(UserSettings.All.PresetSettingsSectionWidth, GridUnitType.Pixel);
    private List<UploadPresetViewModel> _uploadPresets;
    private ICollectionView _filteredUploadPresets;

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

    public ExportFormats ExportFormat
    {
        get => _exportFormat;
        set
        {
            SetProperty(ref _exportFormat, value);

            OnPropertyChanged(nameof(AvailableEncoders));
            OnPropertyChanged(nameof(Extensions));
        }
    }

    public ObservableCollection<ExportPresetViewModel> Presets
    {
        get => _presets;
        set => SetProperty(ref _presets, value);
    }

    public ExportPresetViewModel SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            SetProperty(ref _selectedPreset, value);

            OnPropertyChanged(nameof(ExtendedSettingsVisibility));
        }
    }

    public List<EncoderTypes> AvailableEncoders => ExportFormat switch
    {
        ExportFormats.Gif => new List<EncoderTypes> { EncoderTypes.ScreenToGif, EncoderTypes.KGySoft, EncoderTypes.FFmpeg, EncoderTypes.Gifski, EncoderTypes.System },
        ExportFormats.Apng => new List<EncoderTypes> { EncoderTypes.ScreenToGif, EncoderTypes.FFmpeg },
        ExportFormats.Bmp or ExportFormats.Jpeg or ExportFormats.Png => new List<EncoderTypes> { EncoderTypes.ScreenToGif },
        ExportFormats.Stg or ExportFormats.Psd => new List<EncoderTypes> { EncoderTypes.ScreenToGif },
        _ => new List<EncoderTypes> { EncoderTypes.FFmpeg }
    };

    public GridLength PresetSectionWidth
    {
        get => _presetSectionWidth;
        set
        {
            SetProperty(ref _presetSectionWidth, value);

            if (value.IsAbsolute)
                UserSettings.All.PresetSettingsSectionWidth = value.Value;
        }
    }

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

    public bool IsUploadComboBoxEnabled => FilteredUploadPresets is { IsEmpty: false };

    public Visibility ExtendedSettingsVisibility => SelectedPreset is { Type: ExportFormats.Apng or ExportFormats.Gif or ExportFormats.Webp } ? Visibility.Visible : Visibility.Collapsed;

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

    public RoutedUICommand SelectFolderCommand { get; set; } = new()
    {
        InputGestures = { new KeyGesture(Key.O, ModifierKeys.Alt) }
    };

    #endregion

    public PresetSettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        //Presets = UserSettings.All.ExportPresets.OfType<ExportPreset>().Select(s => ExportPresetViewModel.FromModel(s, null)).ToList();
    }

    public void PersistSettings()
    {
        //UserSettings.All.ExportPresets = new ArrayList(Presets.Select(s => s.ToModel()).ToArray());
    }

    public void AddPreset(ExportPresetViewModel preset)
    {
        Presets.Add(preset);
        SelectedPreset = preset;
    }

    public void DuplicatePreset()
    {
        var duplicated = SelectedPreset.ShallowCopy();

        duplicated.IsDefault = false;
        duplicated.TitleKey = null;
        duplicated.DescriptionKey = null;

        Presets.Add(duplicated);
        SelectedPreset = duplicated;
    }

    public void ResetPreset()
    {
        if (!SelectedPreset.IsDefault)
            return;

        SelectedPreset.Reset();
    }

    public void RemovePreset()
    {
        if (SelectedPreset.IsDefault)
            return;

        var index = Presets.IndexOf(SelectedPreset);

        Presets.Remove(SelectedPreset);

        SelectedPreset = Presets[index == 0 ? 0 : index - 1];
    }
}