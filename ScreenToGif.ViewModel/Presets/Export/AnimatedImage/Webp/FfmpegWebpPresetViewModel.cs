using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Webp;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Webp;

/// <summary>
/// Webp FFmpeg encoder preset.
/// ffmpeg -h muxer=webp
/// ffmpeg -h encoder=libwebp_anim
/// </summary>
public class FfmpegWebpPresetViewModel : WebpPresetViewModel, IFfmpegPreset
{
    private VideoSettingsModes _settingsMode = VideoSettingsModes.Normal;
    private string _parameters = "-vsync passthrough \n{I} \n-c:v libwebp_anim \n-lossless 0 \n-quality 75 \n-loop 0 \n-f webp \n{O}";
    private VideoCodecPresets _codecPreset = VideoCodecPresets.Default;
    private int _quality = 75;
    private bool _lossless = true;
    private VideoPixelFormats _pixelFormat = VideoPixelFormats.Auto;
    private Framerates _framerate = Framerates.Auto;
    private double _customFramerate = 25d;
    private Vsyncs _vsync = Vsyncs.Passthrough;

    public VideoSettingsModes SettingsMode
    {
        get => _settingsMode;
        set => SetProperty(ref _settingsMode, value);
    }

    public string Parameters
    {
        get => _parameters;
        set => SetProperty(ref _parameters, value);
    }

    public VideoCodecPresets CodecPreset
    {
        get => _codecPreset;
        set => SetProperty(ref _codecPreset, value);
    }

    public int Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public bool Lossless
    {
        get => _lossless;
        set => SetProperty(ref _lossless, value);
    }

    public VideoPixelFormats PixelFormat
    {
        get => _pixelFormat;
        set => SetProperty(ref _pixelFormat, value);
    }

    public Framerates Framerate
    {
        get => _framerate;
        set => SetProperty(ref _framerate, value);
    }

    public double CustomFramerate
    {
        get => _customFramerate;
        set => SetProperty(ref _customFramerate, value);
    }

    public Vsyncs Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }
    
    public FfmpegWebpPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;
    }

    public static List<FfmpegWebpPresetViewModel> Defaults => new()
    {
        new FfmpegWebpPresetViewModel
        {
            TitleKey = "S.Preset.Default.Title",
            DescriptionKey = "S.Preset.Default.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20)
        },

        new FfmpegWebpPresetViewModel
        {
            TitleKey = "S.Preset.Webp.Ffmpeg.High.Title",
            DescriptionKey = "S.Preset.Webp.Ffmpeg.High.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quality = 100,
            Parameters = "-vsync passthrough \n{I} \n-c:v libwebp_anim \n-lossless 0 \n-quality 100 \n-loop 0 \n-f webp \n{O}"
        }
    };

    public static FfmpegWebpPresetViewModel FromModel(FfmpegWebpPreset preset, IPreviewerViewModel exporterViewModel)
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
            SettingsMode = preset.SettingsMode,
            Parameters = preset.Parameters,
            CodecPreset = preset.CodecPreset,
            Quality = preset.Quality,
            Lossless = preset.Lossless,
            PixelFormat = preset.PixelFormat,
            Framerate = preset.Framerate,
            CustomFramerate = preset.CustomFramerate,
            Vsync = preset.Vsync,
        };
    }

    public override ExportPreset ToModel()
    {
        return new FfmpegWebpPreset
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
            SettingsMode = SettingsMode,
            Parameters = Parameters,
            CodecPreset = CodecPreset,
            Quality = Quality,
            Lossless = Lossless,
            PixelFormat = PixelFormat,
            Framerate = Framerate,
            CustomFramerate = CustomFramerate,
            Vsync = Vsync,
        };
    }
}