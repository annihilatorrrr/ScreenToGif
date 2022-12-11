using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.Video.Mkv;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Mkv;

public class FfmpegMkvPresetViewModel : MkvPresetViewModel, IFfmpegPreset
{
    public FfmpegMkvPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;

        VideoCodec = VideoCodecs.X264;
        CodecPreset = VideoCodecPresets.Fast;
        HardwareAcceleration = HardwareAccelerationModes.Auto;
        Pass = 1;
        ConstantRateFactor = 23;
        PixelFormat = VideoPixelFormats.Yuv420p;
        Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libx264 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 23 \n-f matroska \n{O}";
    }

    public static List<FfmpegMkvPresetViewModel> Defaults => new()
    {
        new FfmpegMkvPresetViewModel
        {
            TitleKey = "S.Preset.Default.Title",
            DescriptionKey = "S.Preset.Default.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            VideoCodec = VideoCodecs.X264,
            CodecPreset = VideoCodecPresets.Fast,
            HardwareAcceleration = HardwareAccelerationModes.Auto,
            Pass = 1,
            ConstantRateFactor = 23,
            PixelFormat = VideoPixelFormats.Yuv420p,
            Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libx264 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 23 \n-f matroska \n{O}"
        },

        new FfmpegMkvPresetViewModel
        {
            TitleKey = "S.Preset.Hevc.Title",
            DescriptionKey = "S.Preset.Hevc.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            VideoCodec = VideoCodecs.X265,
            CodecPreset = VideoCodecPresets.Fast,
            HardwareAcceleration = HardwareAccelerationModes.Auto,
            Pass = 1,
            ConstantRateFactor = 28,
            PixelFormat = VideoPixelFormats.Yuv420p,
            Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libx265 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 28 \n-f matroska \n{O}"
        }
    };

    public override FfmpegMkvPreset ToModel()
    {
        return new FfmpegMkvPreset
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
        VideoCodec = preset.VideoCodec;
        CodecPreset = preset.CodecPreset;
        HardwareAcceleration = preset.HardwareAcceleration;
        Pass = preset.Pass;
        IsVariableBitRate = preset.IsVariableBitRate;
        ConstantRateFactor = preset.ConstantRateFactor;
        BitRate = preset.BitRate;
        QualityLevel = preset.QualityLevel;
        BitRateUnit = preset.BitRateUnit;
        MinimumBitRate = preset.MinimumBitRate;
        MinimumBitRateUnit = preset.MinimumBitRateUnit;
        MaximumBitRate = preset.MaximumBitRate;
        MaximumBitRateUnit = preset.MaximumBitRateUnit;
        RateControlBuffer = preset.RateControlBuffer;
        RateControlBufferUnit = preset.RateControlBufferUnit;
        PixelFormat = preset.PixelFormat;
        Framerate = preset.Framerate;
        CustomFramerate = preset.CustomFramerate;
        Vsync = preset.Vsync;
    }
}