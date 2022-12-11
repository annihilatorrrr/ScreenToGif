using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;
using ScreenToGif.Util;

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
        };
    }

    public override ExportPreset ToModel()
    {
        return new SystemGifPreset
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
            Looped = Looped,
            RepeatForever = RepeatForever,
            RepeatCount = RepeatCount,
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
        Looped = Default.Looped;
        RepeatForever = Default.RepeatForever;
        RepeatCount = Default.RepeatCount;
    }
}