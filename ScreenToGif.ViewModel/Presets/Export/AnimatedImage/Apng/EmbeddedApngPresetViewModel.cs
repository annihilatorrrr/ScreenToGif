using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;

public class EmbeddedApngPresetViewModel : ApngPresetViewModel
{
    private bool _detectUnchanged = true;
    private bool _paintTransparent = true;
    
    public bool DetectUnchanged
    {
        get => _detectUnchanged;
        set => SetProperty(ref _detectUnchanged, value);
    }

    public bool PaintTransparent
    {
        get => _paintTransparent;
        set => SetProperty(ref _paintTransparent, value);
    }
    
    public EmbeddedApngPresetViewModel()
    {
        Encoder = EncoderTypes.ScreenToGif;
    }

    public static EmbeddedApngPresetViewModel Default = new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20)
    };

    public static EmbeddedApngPresetViewModel FromModel(EmbeddedApngPreset preset, IPreviewerViewModel exporterViewModel)
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
            DetectUnchanged = preset.DetectUnchanged,
            PaintTransparent = preset.PaintTransparent
        };
    }

    public override ExportPreset ToModel()
    {
        return new EmbeddedApngPreset
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
            DetectUnchanged = DetectUnchanged,
            PaintTransparent = PaintTransparent
        };
    }

    public override ExportPresetViewModel Reset()
    {
        return Default;
    }
}