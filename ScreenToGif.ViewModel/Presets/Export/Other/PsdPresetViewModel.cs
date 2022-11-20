using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.Other;

namespace ScreenToGif.ViewModel.Presets.Export.Other;

public class PsdPresetViewModel : ExportPresetViewModel
{
    private bool _compressImage = true;
    private bool _saveTimeline = true;
    private bool _maximizeCompatibility = true;
    
    public bool CompressImage
    {
        get => _compressImage;
        set => SetProperty(ref _compressImage, value);
    }

    public bool SaveTimeline
    {
        get => _saveTimeline;
        set => SetProperty(ref _saveTimeline, value);
    }

    public bool MaximizeCompatibility
    {
        get => _maximizeCompatibility;
        set => SetProperty(ref _maximizeCompatibility, value);
    }
    
    public PsdPresetViewModel()
    {
        Type = ExportFormats.Psd;
        OutputFilenameKey = "S.Preset.Filename.Image";
        Extension = ".psd";
    }
    
    public static PsdPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20),
    };

    public static PsdPresetViewModel FromModel(PsdPreset preset, IPreviewerViewModel exporterViewModel)
    {
        return new()
        {
            Title = preset.Title,
            TitleKey = preset.TitleKey,
            Description = preset.Description,
            DescriptionKey = preset.DescriptionKey,
            IsSelected = preset.IsSelected,
            IsSelectedForEncoder = preset.IsSelectedForEncoder,
            IsDefault = preset.IsDefault,
            HasAutoSave = preset.HasAutoSave,
            CreationDate = preset.CreationDate,
            PickLocation = preset.PickLocation,
            OverwriteMode = preset.OverwriteMode,
            ExportAsProjectToo = preset.ExportAsProjectToo,
            UploadFile = preset.UploadFile,
            UploadService = preset.UploadService,
            SaveToClipboard = preset.SaveToClipboard,
            CopyType = preset.CopyType,
            ExecuteCustomCommands = preset.ExecuteCustomCommands,
            CustomCommands = preset.CustomCommands,
            OutputFolder = preset.OutputFolder,
            OutputFilename = preset.OutputFilename,
            OutputFilenameKey = preset.OutputFilenameKey,
            Extension = preset.Extension,
            PreviewerViewModel = exporterViewModel,
            CompressImage = preset.CompressImage,
            SaveTimeline = preset.SaveTimeline,
            MaximizeCompatibility = preset.MaximizeCompatibility,
        };
    }

    public override ExportPreset ToModel()
    {
        return new PsdPreset
        {
            Title = Title,
            TitleKey = TitleKey,
            Description = Description,
            DescriptionKey = DescriptionKey,
            IsSelected = IsSelected,
            IsSelectedForEncoder = IsSelectedForEncoder,
            IsDefault = IsDefault,
            HasAutoSave = HasAutoSave,
            CreationDate = CreationDate,
            PickLocation = PickLocation,
            OverwriteMode = OverwriteMode,
            ExportAsProjectToo = ExportAsProjectToo,
            UploadFile = UploadFile,
            UploadService = UploadService,
            SaveToClipboard = SaveToClipboard,
            CopyType = CopyType,
            ExecuteCustomCommands = ExecuteCustomCommands,
            CustomCommands = CustomCommands,
            OutputFolder = OutputFolder,
            OutputFilename = OutputFilename,
            OutputFilenameKey = OutputFilenameKey,
            Extension = Extension,
            CompressImage = CompressImage,
            SaveTimeline = SaveTimeline,
            MaximizeCompatibility = MaximizeCompatibility,
        };
    }
}