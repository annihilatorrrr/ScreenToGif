using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

public class GifskiGifPresetViewModel : GifPresetViewModel
{
    private bool _fast;
    private bool _extraEffort;
    private int _quality = 10;

    public bool Fast
    {
        get => _fast;
        set => SetProperty(ref _fast, value);
    }

    public bool ExtraEffort
    {
        get => _extraEffort;
        set => SetProperty(ref _extraEffort, value);
    }

    public int Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public GifskiGifPresetViewModel()
    {
        Encoder = EncoderTypes.Gifski;
    }

    public static List<GifskiGifPresetViewModel> Defaults => new()
    {
        new GifskiGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Gifski.High.Title",
            DescriptionKey = "S.Preset.Gif.Gifski.High.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quality = 1,
            Fast = false
        },

        new GifskiGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Gifski.Low.Title",
            DescriptionKey = "S.Preset.Gif.Gifski.Low.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quality = 20,
            Fast = false
        },

        new GifskiGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Gifski.Fast.Title",
            DescriptionKey = "S.Preset.Gif.Gifski.Fast.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quality = 20,
            Fast = true
        }
    };

    public static GifskiGifPresetViewModel FromModel(GifskiGifPreset preset, IPreviewerViewModel exporterViewModel)
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
            Fast = preset.Fast,
            Quality = preset.Quality
        };
    }

    public override ExportPreset ToModel()
    {
        return new GifskiGifPreset
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
            Fast = Fast,
            ExtraEffort = ExtraEffort,
            Quality = Quality
        };
    }

    public override void Reset()
    {
        var preset = Defaults.First(f => f.TitleKey == TitleKey);

        Title = LocalizationHelper.Get(preset.TitleKey).Replace("{0}", preset.DefaultExtension);
        Description = LocalizationHelper.Get(preset.DescriptionKey);
        IsSelected = preset.IsSelected;
        IsSelectedForEncoder = preset.IsSelectedForEncoder;
        IsDefault = preset.IsDefault;
        HasAutoSave = preset.HasAutoSave;
        CreationDate = preset.CreationDate;
        PickLocation = preset.PickLocation;
        OverwriteMode = preset.OverwriteMode;
        ExportAsProjectToo = preset.ExportAsProjectToo;
        UploadFile = preset.UploadFile;
        UploadService = preset.UploadService;
        SaveToClipboard = preset.SaveToClipboard;
        CopyType = preset.CopyType;
        ExecuteCustomCommands = preset.ExecuteCustomCommands;
        CustomCommands = preset.CustomCommands;
        OutputFolder = preset.OutputFolder;
        OutputFilename = preset.OutputFilename;
        OutputFilenameKey = preset.OutputFilenameKey;
        Extension = preset.Extension;
        Looped = preset.Looped;
        RepeatForever = preset.RepeatForever;
        RepeatCount = preset.RepeatCount;
        Fast = preset.Fast;
        ExtraEffort = preset.ExtraEffort;
        Quality = preset.Quality;
    }
}