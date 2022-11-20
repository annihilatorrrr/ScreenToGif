using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.Video;

namespace ScreenToGif.ViewModel.Presets.Export.Video;

public class VideoPresetViewModel : ExportPresetViewModel
{
    private VideoSettingsModes _settingsMode;
    private string _parameters;
    private VideoCodecs _videoCodec;
    private VideoCodecPresets _codecPreset;
    private HardwareAccelerationModes _hardwareAcceleration = HardwareAccelerationModes.Auto;
    private int _pass = 1;
    private bool _isVariableBitRate = false;
    private int? _constantRateFactor;
    private decimal _bitRate;
    private int _qualityLevel = 5;
    private RateUnits _bitRateUnit = RateUnits.Megabits;
    private decimal _minimumBitRate;
    private RateUnits _minimumBitRateUnit = RateUnits.Megabits;
    private decimal _maximumBitRate;
    private RateUnits _maximumBitRateUnit = RateUnits.Megabits;
    private decimal _rateControlBuffer;
    private RateUnits _rateControlBufferUnit = RateUnits.Megabits;
    private VideoPixelFormats _pixelFormat;
    private Framerates _framerate = Framerates.Auto;
    private decimal _customFramerate = 25M;
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

    public VideoCodecs VideoCodec
    {
        get => _videoCodec;
        set => SetProperty(ref _videoCodec, value);
    }

    public VideoCodecPresets CodecPreset
    {
        get => _codecPreset;
        set => SetProperty(ref _codecPreset, value);
    }

    /// <summary>
    /// Hardware acceleration mode.
    /// https://trac.ffmpeg.org/wiki/HWAccelIntro
    /// </summary>
    public HardwareAccelerationModes HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set => SetProperty(ref _hardwareAcceleration, value);
    }

    public int Pass
    {
        get => _pass;
        set => SetProperty(ref _pass, value);
    }

    public bool IsVariableBitRate
    {
        get => _isVariableBitRate;
        set => SetProperty(ref _isVariableBitRate, value);
    }

    public int? ConstantRateFactor
    {
        get => _constantRateFactor;
        set => SetProperty(ref _constantRateFactor, value);
    }

    public decimal BitRate
    {
        get => _bitRate;
        set => SetProperty(ref _bitRate, value);
    }

    /// <summary>
    /// Quality level (-q:v, -qscale:v), in use when having the bitrate mode set to variable.
    /// </summary>
    public int QualityLevel
    {
        get => _qualityLevel;
        set => SetProperty(ref _qualityLevel, value);
    }

    public RateUnits BitRateUnit
    {
        get => _bitRateUnit;
        set => SetProperty(ref _bitRateUnit, value);
    }

    public decimal MinimumBitRate
    {
        get => _minimumBitRate;
        set => SetProperty(ref _minimumBitRate, value);
    }

    public RateUnits MinimumBitRateUnit
    {
        get => _minimumBitRateUnit;
        set => SetProperty(ref _minimumBitRateUnit, value);
    }

    public decimal MaximumBitRate
    {
        get => _maximumBitRate;
        set => SetProperty(ref _maximumBitRate, value);
    }

    public RateUnits MaximumBitRateUnit
    {
        get => _maximumBitRateUnit;
        set => SetProperty(ref _maximumBitRateUnit, value);
    }

    public decimal RateControlBuffer
    {
        get => _rateControlBuffer;
        set => SetProperty(ref _rateControlBuffer, value);
    }

    public RateUnits RateControlBufferUnit
    {
        get => _rateControlBufferUnit;
        set => SetProperty(ref _rateControlBufferUnit, value);
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

    public decimal CustomFramerate
    {
        get => _customFramerate;
        set => SetProperty(ref _customFramerate, value);
    }

    public Vsyncs Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }

    public bool IsAncientContainer => Type == ExportFormats.Avi;

    protected VideoPresetViewModel()
    {
        OutputFilenameKey = "S.Preset.Filename.Video";
    }

    public static VideoPresetViewModel FromModel(VideoPreset preset, IPreviewerViewModel exporterViewModel)
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
            SettingsMode = preset.SettingsMode,
            Parameters = preset.Parameters,
            VideoCodec = preset.VideoCodec,
            CodecPreset = preset.CodecPreset,
            HardwareAcceleration = preset.HardwareAcceleration,
            Pass = preset.Pass,
            IsVariableBitRate = preset.IsVariableBitRate,
            ConstantRateFactor = preset.ConstantRateFactor,
            BitRate = preset.BitRate,
            QualityLevel = preset.QualityLevel,
            BitRateUnit = preset.BitRateUnit,
            MinimumBitRate = preset.MinimumBitRate,
            MinimumBitRateUnit = preset.MinimumBitRateUnit,
            MaximumBitRate = preset.MaximumBitRate,
            MaximumBitRateUnit = preset.MaximumBitRateUnit,
            RateControlBuffer = preset.RateControlBuffer,
            RateControlBufferUnit = preset.RateControlBufferUnit,
            PixelFormat = preset.PixelFormat,
            Framerate = preset.Framerate,
            CustomFramerate = preset.CustomFramerate,
            Vsync = preset.Vsync,
        };
    }

    public override ExportPreset ToModel()
    {
        return new VideoPreset
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
            SettingsMode = SettingsMode,
            Parameters = Parameters,
            VideoCodec = VideoCodec,
            CodecPreset = CodecPreset,
            HardwareAcceleration = HardwareAcceleration,
            Pass = Pass,
            IsVariableBitRate = IsVariableBitRate,
            ConstantRateFactor = ConstantRateFactor,
            BitRate = BitRate,
            QualityLevel = QualityLevel,
            BitRateUnit = BitRateUnit,
            MinimumBitRate = MinimumBitRate,
            MinimumBitRateUnit = MinimumBitRateUnit,
            MaximumBitRate = MaximumBitRate,
            MaximumBitRateUnit = MaximumBitRateUnit,
            RateControlBuffer = RateControlBuffer,
            RateControlBufferUnit = RateControlBufferUnit,
            PixelFormat = PixelFormat,
            Framerate = Framerate,
            CustomFramerate = CustomFramerate,
            Vsync = Vsync,
        };
    }
}