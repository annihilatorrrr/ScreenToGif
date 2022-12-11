using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.Video.Avi;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Avi;

public class FfmpegAviPresetViewModel : AviPresetViewModel, IFfmpegPreset
{
    public FfmpegAviPresetViewModel()
    {
        Encoder = EncoderTypes.FFmpeg;

        //Defaults.
        VideoCodec = VideoCodecs.Mpeg4;
        CodecPreset = VideoCodecPresets.None;
        HardwareAcceleration = HardwareAccelerationModes.Auto;
        Pass = 2;
        BitRate = 5;
        BitRateUnit = RateUnits.Megabits;
        PixelFormat = VideoPixelFormats.Yuv420p;
        Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v mpeg4 -vtag xvid \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-b:v 5M \n-pass 2 \n-f avi \n{O}";
    }

    public static FfmpegAviPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20),

        VideoCodec = VideoCodecs.Mpeg4,
        CodecPreset = VideoCodecPresets.None,
        HardwareAcceleration = HardwareAccelerationModes.Auto,
        Pass = 2,
        BitRate = 5,
        BitRateUnit = RateUnits.Megabits,
        PixelFormat = VideoPixelFormats.Yuv420p,
        Parameters = "-vsync passthrough \n-hwaccel auto \n{I} \n-c:v mpeg4 -vtag xvid \n-pix_fmt yuv420p \n-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" \n-b:v 5M \n-pass 2 \n-f avi \n{O}"
    };

    public override FfmpegAviPreset ToModel()
    {
        return new FfmpegAviPreset
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
        Title = LocalizationHelper.Get(Default.TitleKey).Replace("{0}", Default.DefaultExtension);
        Description = LocalizationHelper.Get(Default.DescriptionKey);
        IsSelected = Default.IsSelected;
        IsSelectedForEncoder = Default.IsSelectedForEncoder;
        IsDefault = Default.IsDefault;
        HasAutoSave = Default.HasAutoSave;
        CreationDate = Default.CreationDate;
        PickLocation = Default.PickLocation;
        OverwriteMode = Default.OverwriteMode;
        ExportAsProjectToo = Default.ExportAsProjectToo;
        UploadFile = Default.UploadFile;
        UploadService = Default.UploadService;
        SaveToClipboard = Default.SaveToClipboard;
        CopyType = Default.CopyType;
        ExecuteCustomCommands = Default.ExecuteCustomCommands;
        CustomCommands = Default.CustomCommands;
        OutputFolder = Default.OutputFolder;
        OutputFilename = Default.OutputFilename;
        OutputFilenameKey = Default.OutputFilenameKey;
        Extension = Default.Extension;
        SettingsMode = Default.SettingsMode;
        Parameters = Default.Parameters;
        VideoCodec = Default.VideoCodec;
        CodecPreset = Default.CodecPreset;
        HardwareAcceleration = Default.HardwareAcceleration;
        Pass = Default.Pass;
        IsVariableBitRate = Default.IsVariableBitRate;
        ConstantRateFactor = Default.ConstantRateFactor;
        BitRate = Default.BitRate;
        QualityLevel = Default.QualityLevel;
        BitRateUnit = Default.BitRateUnit;
        MinimumBitRate = Default.MinimumBitRate;
        MinimumBitRateUnit = Default.MinimumBitRateUnit;
        MaximumBitRate = Default.MaximumBitRate;
        MaximumBitRateUnit = Default.MaximumBitRateUnit;
        RateControlBuffer = Default.RateControlBuffer;
        RateControlBufferUnit = Default.RateControlBufferUnit;
        PixelFormat = Default.PixelFormat;
        Framerate = Default.Framerate;
        CustomFramerate = Default.CustomFramerate;
        Vsync = Default.Vsync;
    }
}