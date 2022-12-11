using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;
using ScreenToGif.Util;
using System.Windows;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

/// <summary>
/// Gif FFmpeg encoder preset.
/// ffmpeg -h muxer=gif
/// ffmpeg -h encoder=gif
/// </summary>
public class FfmpegGifPresetViewModel : GifPresetViewModel, IFfmpegPreset
{
    private bool _useGlobalColorTable;
    private VideoSettingsModes _settingsMode = VideoSettingsModes.Normal;
    private string _parameters = "-vsync passthrough \n{I} \n-loop 0 \n-lavfi palettegen=stats_mode=diff[pal],[0:v][pal]paletteuse=new=1:dither=sierra2_4a:diff_mode=rectangle \n-f gif \n{O}";
    private DitherMethods _dither = DitherMethods.Sierra2Lite;
    private int _bayerScale = 2;
    private VideoPixelFormats _pixelFormat = VideoPixelFormats.Auto;
    private Framerates _framerate = Framerates.Auto;
    private decimal _customFramerate = 25M;
    private Vsyncs _vsync = Vsyncs.Passthrough;

    public bool UseGlobalColorTable
    {
        get => _useGlobalColorTable;
        set => SetProperty(ref _useGlobalColorTable, value);
    }

    public VideoSettingsModes SettingsMode
    {
        get => _settingsMode;
        set
        {
            SetProperty(ref _settingsMode, value);

            OnPropertyChanged(nameof(NormalVisibility));
            OnPropertyChanged(nameof(AdvancedVisibility));
            OnPropertyChanged(nameof(CommandResolved));
        }
    }

    public Visibility NormalVisibility => SettingsMode == VideoSettingsModes.Normal ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AdvancedVisibility => SettingsMode == VideoSettingsModes.Advanced ? Visibility.Visible : Visibility.Collapsed;

    public string Parameters
    {
        get => _parameters;
        set
        {
            SetProperty(ref _parameters, value);

            OnPropertyChanged(nameof(CommandResolved));
        }
    }

    public string CommandResolved => "ffmpeg\n" + (Parameters ?? "").Replace("{I}", "-safe 0 -i - ").Replace("{O}", $"-y \"{ResolvedOutputPath}\"");

    public DitherMethods Dither
    {
        get => _dither;
        set
        {
            SetProperty(ref _dither, value);

            OnPropertyChanged(nameof(BayerScaleVisibility));
        }
    }

    public int BayerScale
    {
        get => _bayerScale;
        set => SetProperty(ref _bayerScale, value);
    }

    public Visibility BayerScaleVisibility => Dither == DitherMethods.Bayer ? Visibility.Visible : Visibility.Collapsed;

    public List<EnumItem<VideoPixelFormats>> PixelFormats => new()
    {
        new(VideoPixelFormats.Auto, "S.SaveAs.VideoOptions.PixelFormat.Auto", ""),
        new(VideoPixelFormats.Bgr4Byte, "", "Bgr4Byte", "bgr4byte"),
        new(VideoPixelFormats.Bgr8, "", "Bgr8", "bgr8"),
        new(VideoPixelFormats.Gray, "", "Gray", "gray"),
        new(VideoPixelFormats.Pal8, "", "Pal8", "pal8"),
        new(VideoPixelFormats.Rgb4Byte, "", "Rgb4Byte", "rgb4byte"),
        new(VideoPixelFormats.Rgb8, "", "Rgb8", "rgb8")
    };

    public VideoPixelFormats PixelFormat
    {
        get => _pixelFormat;
        set => SetProperty(ref _pixelFormat, value);
    }
    
    public Vsyncs Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }

    public Framerates Framerate
    {
        get => _framerate;
        set
        {
            SetProperty(ref _framerate, value);

            OnPropertyChanged(nameof(CustomFramerateVisibility));
        }
    }

    public decimal CustomFramerate
    {
        get => _customFramerate;
        set => SetProperty(ref _customFramerate, value);
    }

    public Visibility CustomFramerateVisibility => Framerate == Framerates.Custom ? Visibility.Visible : Visibility.Collapsed;
    
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

    public override ExportPreset ToModel()
    {
        return new FfmpegGifPreset
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
            UseGlobalColorTable = UseGlobalColorTable,
            SettingsMode = SettingsMode,
            Parameters = Parameters,
            Dither = Dither,
            BayerScale = BayerScale,
            PixelFormat = PixelFormat,
            Framerate = Framerate,
            CustomFramerate = CustomFramerate,
            Vsync = Vsync,
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
        UseGlobalColorTable = preset.UseGlobalColorTable;
        SettingsMode = preset.SettingsMode;
        Parameters = preset.Parameters;
        Dither = preset.Dither;
        BayerScale = preset.BayerScale;
        PixelFormat = preset.PixelFormat;
        Framerate = preset.Framerate;
        CustomFramerate = preset.CustomFramerate;
        Vsync = preset.Vsync;
    }
}