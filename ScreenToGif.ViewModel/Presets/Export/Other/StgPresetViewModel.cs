using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.Other;
using System.IO.Compression;

namespace ScreenToGif.ViewModel.Presets.Export.Other;

public class StgPresetViewModel : ExportPresetViewModel
{
    private CompressionLevel _compressionLevel = CompressionLevel.Optimal;
    
    public CompressionLevel CompressionLevel
    {
        get => _compressionLevel;
        set => SetProperty(ref _compressionLevel, value);
    }
    
    public StgPresetViewModel()
    {
        Type = ExportFormats.Stg;
        Encoder = EncoderTypes.ScreenToGif;
        OutputFilenameKey = "S.Preset.Filename.Project";
        Extension = ".stg";
    }
    
    public static StgPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20),
    };

    public static StgPresetViewModel FromModel(StgPreset preset, IPreviewerViewModel exporterViewModel)
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
            CompressionLevel = preset.CompressionLevel,
        };
    }
}