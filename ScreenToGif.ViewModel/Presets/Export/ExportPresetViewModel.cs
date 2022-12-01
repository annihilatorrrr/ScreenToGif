using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage;
using ScreenToGif.Domain.Models.Preset.Export.Image;
using ScreenToGif.Domain.Models.Preset.Export.Other;
using ScreenToGif.Domain.Models.Preset.Export.Video;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage;
using ScreenToGif.ViewModel.Presets.Export.Image;
using ScreenToGif.ViewModel.Presets.Export.Other;
using ScreenToGif.ViewModel.Presets.Export.Video;
using System.Windows;
using System.Windows.Threading;

namespace ScreenToGif.ViewModel.Presets.Export;

public abstract class ExportPresetViewModel : BaseViewModel, IExportPreset
{
    private readonly DispatcherTimer _searchTimer = new(DispatcherPriority.Background);
    
    private ExportFormats _type;
    private EncoderTypes _encoder;
    private string _title;
    private string _titleKey;
    private string _description;
    private string _descriptionKey;
    private bool _isSelected;
    private bool _isSelectedForEncoder;
    private bool _isDefault;
    private bool _hasAutoSave;
    private DateTime _creationDate;
    private bool _pickLocation = true;
    private OverwriteModes _overwriteMode;
    private bool _exportAsProjectToo;
    private bool _uploadFile;
    private string _uploadService;
    private bool _saveToClipboard;
    private CopyModes _copyType = CopyModes.File;
    private bool _executeCustomCommands;
    private string _customCommands = "{p}";
    private string _outputFolder;
    private string _outputFilename;
    private string _outputFilenameKey;
    private string _extension;
    private IPreviewerViewModel _previewerViewModel;
    private bool _fileAlreadyExists;

    public ExportFormats Type
    {
        get => _type;
        set
        {
            SetProperty(ref _type, value);

            OnPropertyChanged(nameof(DefaultExtension));
            OnPropertyChanged(nameof(CanExportMultipleFiles));
        }
    }

    public EncoderTypes Encoder
    {
        get => _encoder;
        set
        {
            SetProperty(ref _encoder, value);

            OnPropertyChanged(nameof(Symbol));
            OnPropertyChanged(nameof(RequiresFfmpeg));
            OnPropertyChanged(nameof(RequiresGifski));
        }
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string TitleKey
    {
        get => _titleKey;
        set => SetProperty(ref _titleKey, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string DescriptionKey
    {
        get => _descriptionKey;
        set => SetProperty(ref _descriptionKey, value);
    }

    public FluentSymbols Symbol => Encoder switch
    {
        EncoderTypes.ScreenToGif => FluentSymbols.Window,
        EncoderTypes.FFmpeg => FluentSymbols.ArrowSync,
        EncoderTypes.Gifski => FluentSymbols.Timer,
        EncoderTypes.KGySoft => FluentSymbols.ChevronRight,
        _ => FluentSymbols.Add
    };

    public string DefaultExtension => Type switch
    {
        ExportFormats.Apng => ".png",
        ExportFormats.Gif => ".gif",
        ExportFormats.Webp => ".webp",

        ExportFormats.Avi => ".avi",
        ExportFormats.Mkv => ".mkv",
        ExportFormats.Mov => ".mov",
        ExportFormats.Mp4 => ".mp4",
        ExportFormats.Webm => ".webm",

        ExportFormats.Bmp => ".bmp",
        ExportFormats.Jpeg => ".jpg",
        ExportFormats.Png => ".png",

        ExportFormats.Stg => ".stg",
        ExportFormats.Psd => ".psd",
        _ => ""
    };

    /// <summary>
    /// True if this preset was the latest selected preset for the selected file type.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// True if this preset was the latest selected preset for the selected file type and encoder.
    /// </summary>
    public bool IsSelectedForEncoder
    {
        get => _isSelectedForEncoder;
        set => SetProperty(ref _isSelectedForEncoder, value);
    }

    /// <summary>
    /// True if this preset was provided by the app.
    /// </summary>
    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            SetProperty(ref _isDefault, value);

            OnPropertyChanged(nameof(CanBeEdited));
            OnPropertyChanged(nameof(ReadOnlyWarningVisibility));
        }
    }

    /// <summary>
    /// True if this preset automatically saves it's new property values when the user changes something.
    /// </summary>
    public bool HasAutoSave
    {
        get => _hasAutoSave;
        set => SetProperty(ref _hasAutoSave, value);
    }

    public DateTime CreationDate
    {
        get => _creationDate;
        set => SetProperty(ref _creationDate, value);
    }
    
    public bool PickLocation
    {
        get => _pickLocation;
        set
        {
            SetProperty(ref _pickLocation, value);

            OnPropertyChanged(nameof(PickLocationVisibility));
        }
    }

    public OverwriteModes OverwriteMode
    {
        get => _overwriteMode;
        set => SetProperty(ref _overwriteMode, value);
    }

    public bool ExportAsProjectToo
    {
        get => _exportAsProjectToo;
        set => SetProperty(ref _exportAsProjectToo, value);
    }

    public Visibility PickLocationVisibility => PickLocation ? Visibility.Visible : Visibility.Collapsed;

    public bool UploadFile
    {
        get => _uploadFile;
        set
        {
            SetProperty(ref _uploadFile, value);

            OnPropertyChanged(nameof(UploadFileVisibility));
        }
    }

    public string UploadService
    {
        get => _uploadService;
        set => SetProperty(ref _uploadService, value);
    }

    public Visibility UploadFileVisibility => UploadFile ? Visibility.Visible : Visibility.Collapsed;

    public bool SaveToClipboard
    {
        get => _saveToClipboard;
        set
        {
            SetProperty(ref _saveToClipboard, value);

            OnPropertyChanged(nameof(SaveToClipboardVisibility));
        }
    }

    public CopyModes CopyType
    {
        get => _copyType;
        set => SetProperty(ref _copyType, value);
    }

    public Visibility SaveToClipboardVisibility => SaveToClipboard ? Visibility.Visible : Visibility.Collapsed;

    public bool ExecuteCustomCommands
    {
        get => _executeCustomCommands;
        set
        {
            SetProperty(ref _executeCustomCommands, value);

            OnPropertyChanged(nameof(CustomCommandsVisibility));
        }
    }

    public string CustomCommands
    {
        get => _customCommands;
        set => SetProperty(ref _customCommands, value);
    }

    public Visibility CustomCommandsVisibility => ExecuteCustomCommands ? Visibility.Visible : Visibility.Collapsed;

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            SetProperty(ref _outputFolder, value);

            OnPropertyChanged(nameof(ResolvedOutputPath));
        }
    }

    public string OutputFilename
    {
        get => _outputFilename;
        set
        {
            SetProperty(ref _outputFilename, value);

            OnPropertyChanged(nameof(ResolvedOutputPath));
        }
    }

    public string OutputFilenameKey
    {
        get => _outputFilenameKey;
        set => SetProperty(ref _outputFilenameKey, value);
    }

    public string Extension
    {
        get => _extension;
        set
        {
            SetProperty(ref _extension, value);

            OnPropertyChanged(nameof(ResolvedOutputPath));
            OnPropertyChanged(nameof(CanExportMultipleFiles));
        }
    }

    public string ResolvedOutputPath => Path.Combine(OutputFolder, PathHelper.ReplaceRegexInName(OutputFilename) + Extension);

    public bool FileAlreadyExists
    {
        get => _fileAlreadyExists;
        set
        {
            SetProperty(ref _fileAlreadyExists, value);

            OnPropertyChanged(nameof(FileAlreadyExistsVisibility));
        }
    }

    public Visibility FileAlreadyExistsVisibility => FileAlreadyExists ? Visibility.Visible : Visibility.Collapsed;

    public IPreviewerViewModel PreviewerViewModel
    {
        get => _previewerViewModel;
        set => SetProperty(ref _previewerViewModel, value);
    }

    public bool CanExportMultipleFiles => Type is ExportFormats.Bmp or ExportFormats.Jpeg or ExportFormats.Png && Extension != ".zip";

    public bool RequiresFfmpeg => Encoder == EncoderTypes.FFmpeg;

    public bool RequiresGifski => Encoder == EncoderTypes.Gifski;

    public bool CanBeEdited => !IsDefault;

    public Visibility ReadOnlyWarningVisibility => IsDefault ? Visibility.Visible : Visibility.Collapsed;

    protected ExportPresetViewModel()
    {
        _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
        _searchTimer.Tick += SearchTimer_Tick;
    }

    public static ExportPresetViewModel FromModel(ExportPreset preset, IPreviewerViewModel exporterViewModel)
    {
        switch (preset)
        {
            case AnimatedImagePreset image:
                return AnimatedImagePresetViewModel.FromModel(image, exporterViewModel);

            case VideoPreset video:
                return VideoPresetViewModel.FromModel(video, exporterViewModel);

            case ImagePreset image:
                return ImagePresetViewModel.FromModel(image, exporterViewModel);

            case StgPreset stg:
                return StgPresetViewModel.FromModel(stg, exporterViewModel);

            case PsdPreset psd:
                return PsdPresetViewModel.FromModel(psd, exporterViewModel);
        }

        return null;
    }

    public abstract ExportPreset ToModel();

    public abstract ExportPresetViewModel Reset();

    public ExportPresetViewModel ShallowCopy()
    {
        return (ExportPresetViewModel)MemberwiseClone();
    }

    private void ResetFileVerification()
    {
        _searchTimer?.Stop();

        //If no file will be saved, there's no need to verify.
        if (!PickLocation || CanExportMultipleFiles)
        {
            FileAlreadyExists = false;
            return;
        }

        _searchTimer?.Start();
    }

    private void SearchTimer_Tick(object sender, EventArgs e)
    {
       _searchTimer.Stop();

        try
        {
            //Check if there's a file with the same path.
            var exists = File.Exists(Path.Combine(OutputFolder, PathHelper.ReplaceRegexInName(OutputFilename) + Extension));

            FileAlreadyExists = exists;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Check if exists");

            FileAlreadyExists = false;
        }
    }
}