using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;

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

    public override ExportPresetViewModel Reset()
    {
        return Defaults.FirstOrDefault(f => f.TitleKey == TitleKey);
    }
}