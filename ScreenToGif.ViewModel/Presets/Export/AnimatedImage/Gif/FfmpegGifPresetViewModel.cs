using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

/// <summary>
/// Gif FFmpeg encoder preset.
/// ffmpeg -h muxer=gif
/// ffmpeg -h encoder=gif
/// </summary>
public class FfmpegGifPresetViewModel : GifPresetViewModel, IFfmpegPreset
{
    private VideoSettingsModes _settingsMode = VideoSettingsModes.Normal;
    private string _parameters = "-vsync passthrough \n{I} \n-loop 0 \n-lavfi palettegen=stats_mode=diff[pal],[0:v][pal]paletteuse=new=1:dither=sierra2_4a:diff_mode=rectangle \n-f gif \n{O}";
    private DitherMethods _dither = DitherMethods.Sierra2Lite;
    private int _bayerScale = 2;
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

    public DitherMethods Dither
    {
        get => _dither;
        set => SetProperty(ref _dither, value);
    }

    public int BayerScale
    {
        get => _bayerScale;
        set => SetProperty(ref _bayerScale, value);
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

    public FfmpegGifPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
    
    public static List<FfmpegGifPresetViewModel> Defaults => new()
    {
        new FfmpegGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Ffmpeg.High.Title",
            DescriptionKey = "S.Preset.Gif.Ffmpeg.High.Description",
            HasAutoSave = true,
            IsDefault = true,
            IsSelectedForEncoder = true,
            CreationDate = new DateTime(2021, 02, 20)
        },

        new FfmpegGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Ffmpeg.Low.Title",
            DescriptionKey = "S.Preset.Gif.Ffmpeg.Low.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            UseGlobalColorTable = true,
            Dither = DitherMethods.Bayer,
            BayerScale = 3,
            Parameters = "-vsync passthrough \n{I} \n-loop 0 \n-lavfi palettegen=stats_mode=diff[pal],[0:v][pal]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle \n-f gif \n{O}"
        }
    };

    public static FfmpegGifPresetViewModel FromModel(FfmpegGifPreset preset, IPreviewerViewModel exporterViewModel)
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
            SettingsMode = preset.SettingsMode,
            Parameters = preset.Parameters,
            Dither = preset.Dither,
            BayerScale = preset.BayerScale,
            PixelFormat = preset.PixelFormat,
            Framerate = preset.Framerate,
            CustomFramerate = preset.CustomFramerate,
            Vsync = preset.Vsync,
        };
    }
}