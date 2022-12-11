using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.Video.Webm;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Webm;

public class FfmpegWebmPresetViewModel : WebmPresetViewModel, IFfmpegPreset
{
    public FfmpegWebmPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;

        VideoCodec = VideoCodecs.Vp9;
        CodecPreset = VideoCodecPresets.Fast;
        HardwareAcceleration = HardwareAccelerationModes.Auto;
        Pass = 1;
        ConstantRateFactor = 30;
        BitRate = 0;
        BitRateUnit = RateUnits.Megabits;
        PixelFormat = VideoPixelFormats.Yuv420p;
        Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libvpx-vp9 \n-tile-columns 6 -frame-parallel 1 \n-auto-alt-ref 1 -lag-in-frames 25 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 30 \n-b:v 0 \n-f webm \n{O}";
    }

    public static List<FfmpegWebmPresetViewModel> Defaults => new()
    {
        new FfmpegWebmPresetViewModel
        {
            TitleKey = "S.Preset.Default.Title",
            DescriptionKey = "S.Preset.Default.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            VideoCodec = VideoCodecs.Vp9,
            CodecPreset = VideoCodecPresets.Fast,
            HardwareAcceleration = HardwareAccelerationModes.Auto,
            Pass = 1,
            ConstantRateFactor = 30,
            BitRate = 0,
            BitRateUnit = RateUnits.Megabits,
            PixelFormat = VideoPixelFormats.Yuv420p,
            Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libvpx-vp9 \n-tile-columns 6 -frame-parallel 1 \n-auto-alt-ref 1 -lag-in-frames 25 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 30 \n-b:v 0 \n-f webm \n{O}"
        },

        new FfmpegWebmPresetViewModel
        {
            TitleKey = "S.Preset.Vp8.Title",
            DescriptionKey = "S.Preset.Vp8.Description",
            HasAutoSave = true,
            IsSelectedForEncoder = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            VideoCodec = VideoCodecs.Vp8,
            CodecPreset = VideoCodecPresets.Fast,
            HardwareAcceleration = HardwareAccelerationModes.Auto,
            Pass = 1,
            ConstantRateFactor = 30,
            BitRate = 0,
            BitRateUnit = RateUnits.Megabits,
            PixelFormat = VideoPixelFormats.Yuv420p,
            Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v libvpx \n-tile-columns 6 -frame-parallel 1 \n-auto-alt-ref 1 -lag-in-frames 25 \n-preset fast \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-crf 30 \n-b:v 0 \n-f webm \n{O}"
        }
    };

    public override FfmpegWebmPreset ToModel()
    {
        return new FfmpegWebmPreset
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