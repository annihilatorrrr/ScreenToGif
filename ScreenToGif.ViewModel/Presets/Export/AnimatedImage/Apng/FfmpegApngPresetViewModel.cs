using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;
using ScreenToGif.Util;
using System.Windows;

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
    private decimal _customFramerate = 25M;
    private Vsyncs _vsync = Vsyncs.Passthrough;
    
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

    public PredictionMethods PredictionMethod
    {
        get => _predictionMethods;
        set => SetProperty(ref _predictionMethods, value);
    }

    public List<EnumItem<VideoPixelFormats>> PixelFormats => new()
    {
        new(VideoPixelFormats.Auto, "S.SaveAs.VideoOptions.PixelFormat.Auto", ""),
        new(VideoPixelFormats.Gray, "", "Gray", "gray"),
        new(VideoPixelFormats.Gray16Be, "", "Gray16Be", "gray16be"),
        new(VideoPixelFormats.MonoB, "", "MonoB", "monob"),
        new(VideoPixelFormats.Pal8, "", "Pal8", "pal8"),
        new(VideoPixelFormats.Rgb24, "", "Rgb24", "rgb24"),
        new(VideoPixelFormats.RgbA, "", "RgbA", "rgba"),
        new(VideoPixelFormats.Rgb48Be, "", "Rgb48Be", "rgb48be"),
        new(VideoPixelFormats.Rgba64Be, "", "Rgba64Be", "rgba64be"),
        new(VideoPixelFormats.Ya8, "", "Ya8", "ya8"),
        new(VideoPixelFormats.Ya16Be, "", "Ya16Be", "ya16be")
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
        SettingsMode = preset.SettingsMode;
        Parameters = preset.Parameters;
        PredictionMethod = preset.PredictionMethod;
        PixelFormat = preset.PixelFormat;
        Framerate = preset.Framerate;
        CustomFramerate = preset.CustomFramerate;
        Vsync = preset.Vsync;
    }
}