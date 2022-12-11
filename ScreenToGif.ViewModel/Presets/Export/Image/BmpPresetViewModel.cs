using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.Image;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Export.Image;

public class BmpPresetViewModel : ImagePresetViewModel
{
    public BmpPresetViewModel()
    {
        Type = ExportFormats.Bmp;
        Extension = ".bmp";
    }
    
    public static BmpPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20)
    };

    public static BmpPresetViewModel FromModel(BmpPreset preset, IPreviewerViewModel exporterViewModel)
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
            PreviewerViewModel = exporterViewModel
        };
    }

    public override ExportPreset ToModel()
    {
        return new BmpPreset
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
        };
    }

    public override void Reset()
    {
        Title = LocalizationHelper.Get(Default.TitleKey).Replace("{0}", Default.DefaultExtension);
        Description = LocalizationHelper.Get(Default.DescriptionKey);
        IsSelected = Default.IsSelected;
        IsSelectedForEncoder = Default.IsSelectedForEncoder;
        IsDefault = Default.IsDefault;
        HasAutoSave = Default.HasAutoSave;
        CreationDate = Default.CreationDate;
        PickLocation = Default.PickLocation;
        OverwriteMode = Default.OverwriteMode;
        ExportAsProjectToo = Default.ExportAsProjectToo;
        UploadFile = Default.UploadFile;
        UploadService = Default.UploadService;
        SaveToClipboard = Default.SaveToClipboard;
        CopyType = Default.CopyType;
        ExecuteCustomCommands = Default.ExecuteCustomCommands;
        CustomCommands = Default.CustomCommands;
        OutputFolder = Default.OutputFolder;
        OutputFilename = Default.OutputFilename;
        OutputFilenameKey = Default.OutputFilenameKey;
        Extension = Default.Extension;
    }
}