using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;

/// <summary>
/// Apng FFmpeg encoder preset.
/// ffmpeg -h muxer=apng
/// ffmpeg -h encoder=apng
/// </summary>
public class FfmpegApngPresetViewModel : ApngPresetViewModel, IFfmpegPreset
{
    private VideoSettingsModes _settingsMode = VideoSettingsModes.Normal;
    private string _parameters = "-vsync passthrough \n{I} \n-pred mixed \n-plays 0 \n-pix_fmt rgba \n-f apng \n{O}";
    private PredictionMethods _predictionMethods = PredictionMethods.Mixed;
    private VideoPixelFormats _pixelFormat = VideoPixelFormats.RgbA;
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

    public PredictionMethods PredictionMethod
    {
        get => _predictionMethods;
        set => SetProperty(ref _predictionMethods, value);
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
    
    public FfmpegApngPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;
    }

    public static List<FfmpegApngPresetViewModel> Defaults => new()
    {
        new FfmpegApngPresetViewModel
        {
            TitleKey = "S.Preset.Apng.Ffmpeg.High.Title",
            DescriptionKey = "S.Preset.Apng.Ffmpeg.High.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20)
        },

        new FfmpegApngPresetViewModel
        {
            TitleKey = "S.Preset.Apng.Ffmpeg.Low.Title",
            DescriptionKey = "S.Preset.Apng.Ffmpeg.Low.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            PixelFormat = VideoPixelFormats.Rgb24,
            PredictionMethod = PredictionMethods.None,
            Parameters = "-vsync passthrough \n{I} \n-pred none \n-plays 0 \n-pix_fmt rgb24 \n-f apng \n{O}"
        }
    };

    public static FfmpegApngPresetViewModel FromModel(FfmpegApngPreset preset, IPreviewerViewModel exporterViewModel)
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
            PredictionMethod = preset.PredictionMethod,
            PixelFormat = preset.PixelFormat,
            Framerate = preset.Framerate,
            CustomFramerate = preset.CustomFramerate,
            Vsync = preset.Vsync,
        };
    }

    public override ExportPreset ToModel()
    {
        return new FfmpegApngPreset
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
            PredictionMethod = PredictionMethod,
            PixelFormat = PixelFormat,
            Framerate = Framerate,
            CustomFramerate = CustomFramerate,
            Vsync = Vsync,
        };
    }

    public override ExportPresetViewModel Reset()
    {
        return Defaults.FirstOrDefault(f => f.TitleKey == TitleKey);
    }
}