using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

public class SystemGifPresetViewModel : GifPresetViewModel
{
    public SystemGifPresetViewModel()
    {
        Encoder = EncoderTypes.System;
    }
    
    public static SystemGifPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Gif.System.Low.Title",
        DescriptionKey = "S.Preset.Gif.System.Low.Description",
        HasAutoSave = true,
        IsDefault = true,
        IsSelectedForEncoder = true,
        CreationDate = new DateTime(2021, 02, 20)
    };

    public static SystemGifPresetViewModel FromModel(SystemGifPreset preset, IPreviewerViewModel exporterViewModel)
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
            Looped = preset.Looped,
            RepeatForever = preset.RepeatForever,
            RepeatCount = preset.RepeatCount,
            UseGlobalColorTable = preset.UseGlobalColorTable,
        };
    }
}