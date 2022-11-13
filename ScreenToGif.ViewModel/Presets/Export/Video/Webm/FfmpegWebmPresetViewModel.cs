using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;

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
}